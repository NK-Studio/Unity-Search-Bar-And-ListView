using System;

namespace Data
{
    [Serializable]
    public class Person
    {
        public string key;
        public string message;

        public Person(string key, string message)
        {
            this.key = key;
            this.message = message;
        }
    }
}
