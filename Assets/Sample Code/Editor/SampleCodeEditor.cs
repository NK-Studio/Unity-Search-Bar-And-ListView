using System.Collections.Generic;
using UnityEditor;
using Data;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(SampleCode))]
public class SampleCodeEditor : Editor
{
    private VisualTreeAsset _template;
    private VisualTreeAsset _itemTemplate;

    private VisualElement _root;
    private ListView _listView;
    private TextField _listViewSizeField;
    private ToolbarSearchField _searchField;
    private Button _addButton;
    private Button _removeButton;
    private Button _testButton; 

    private SampleCode _sampleCode;

    private List<Person> _targetList;
    private List<Person> _cacheList;

    private SerializedProperty _personProperty;

    private void OnEnable()
    {
        string path = AssetDatabase.GUIDToAssetPath("6da8b582898c2439fbb709b439f9e529");
        _template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);

        string itemPath = AssetDatabase.GUIDToAssetPath("d3d803a738c774c31a46158f2d572cf6");
        _itemTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(itemPath);
    }

    private void InitRoot()
    {
        _root = new VisualElement();
        _template.CloneTree(_root);
    }

    private void FindProperty()
    {
        _personProperty = serializedObject.FindProperty("person");
    }

    public override VisualElement CreateInspectorGUI()
    {
        InitRoot();
        FindProperty();

        _sampleCode = (SampleCode)target;
        _cacheList = new List<Person>();

        // 기본 타겟 리스트는 Person 리스트
        _targetList = _sampleCode.person;

        // Find Object By ID
        _searchField = _root.Q<ToolbarSearchField>("search");
        _listView = _root.Q<ListView>("ItemList");
        _listViewSizeField = _listView.Q<TextField>("unity-list-view__size-field");
        _removeButton = _listView.Q<Button>("unity-list-view__remove-button");
        _addButton = _listView.Q<Button>("unity-list-view__add-button");
        _testButton = _root.Q<Button>("TestButton");

        // Search Bar
        _searchField.RegisterValueChangedCallback(OnSearchCallback);

        // ListView
        _listView.makeItem = OnListMake;
        _listView.bindItem = OnListBind;
        _listView.unbindItem = OnListUnbind;
        _listView.itemsSource = _targetList;

        // Add Button
        _testButton.clicked += OnAutoAdd;
        
        return _root;
    }

    private VisualElement OnListMake()
    {
        TemplateContainer template = _itemTemplate.CloneTree();
        
        // Label 변경
        TextField keyField = template.Q<TextField>("keyField");
        keyField.label = "Key";

        TextField messageField = template.Q<TextField>("messageField");
        messageField.label = "message";

        return template;
    }

    private void OnListBind(VisualElement element, int index)
    {
        serializedObject.Update();

        // _targetList는 알아서 SampleCode의 person이나 cacheList의 person 데이터를 사용함
        string key = _targetList[index].key;
        string message = _targetList[index].message;

        // Data Binding
        TextField keyField = element.Q<TextField>("keyField");
        keyField.value = key;
        keyField.RegisterCallback<ChangeEvent<string>, int>(ChangeKey, index);

        TextField messageField = element.Q<TextField>("messageField");
        messageField.value = message;
        messageField.RegisterCallback<ChangeEvent<string>, int>(ChangeMessage, index);
    }

    private void OnListUnbind(VisualElement element, int arg2)
    {
        // Data Release
        TextField keyField = element.Q<TextField>();
        keyField.UnregisterCallback<ChangeEvent<string>, int>(ChangeKey);

        TextField messageField = element.Q<TextField>();
        messageField.UnregisterCallback<ChangeEvent<string>, int>(ChangeMessage);
    }

    private void OnSearchCallback(ChangeEvent<string> evt)
    {
        if (evt.newValue.Length == 0)
        {
            // 데이터 검색이 없으므로 활성화 처리
            _listViewSizeField.SetEnabled(true);
            _addButton.SetEnabled(true);
            _removeButton.SetEnabled(true);
            _testButton.SetEnabled(true);
            
            // 타겟 리스트를 Person 리스트로 변경
            _targetList = _sampleCode.person;
        }
        else if (evt.newValue.Length > 0)
        {
            // 데이터 검색 중이므로 비활성화 처리
            _listViewSizeField.SetEnabled(false);
            _addButton.SetEnabled(false);
            _removeButton.SetEnabled(false);
            _testButton.SetEnabled(false);

            // 캐시 리스트 초기화
            _cacheList.Clear();

            // 키와 일치하는 데이터를 찾아서 캐시 리스트에 추가
            foreach (Person person in _sampleCode.person)
            {
                if (person.key.Contains(evt.newValue))
                    _cacheList.Add(person);
            }

            // 타겟 리스트를 캐시 리스트로 변경
            _targetList = _cacheList;
        }

        _listView.itemsSource = _targetList;
        _listView.Rebuild();
    }

    private void ChangeKey(ChangeEvent<string> evt, int index)
    {
        _targetList[index].key = evt.newValue;
    }

    private void ChangeMessage(ChangeEvent<string> evt, int index)
    {
        _targetList[index].message = evt.newValue;
    }
    
    private void OnAutoAdd()
    {
        _personProperty.ClearArray();
        _personProperty.arraySize = 2;
        _personProperty.GetArrayElementAtIndex(0).FindPropertyRelative("key").stringValue = "Test01";
        _personProperty.GetArrayElementAtIndex(0).FindPropertyRelative("message").stringValue = "NK Studio 입니다.";

        _personProperty.GetArrayElementAtIndex(1).FindPropertyRelative("key").stringValue = "Test02";
        _personProperty.GetArrayElementAtIndex(1).FindPropertyRelative("message").stringValue = "구독과 좋아요 부탁드립니다.";

        serializedObject.ApplyModifiedProperties();
        _listView.Rebuild();
    }
}
