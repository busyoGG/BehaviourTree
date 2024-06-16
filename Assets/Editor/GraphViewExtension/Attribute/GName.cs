using System;

namespace GraphViewExtension
{
    [AttributeUsage(AttributeTargets.Field)]
    public class GName: Attribute
    {
        private string _name;

        public GName(string name)
        {
            _name = name;
        }

        public string GetName()
        {
            return _name;
        }
    }
}