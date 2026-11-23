using System;

namespace Barliesque.InspectorTools
{

    [AttributeUsage(AttributeTargets.Field)]
    public class EnumPathAttribute : Attribute
    {
        public string Path { get; private set; }
        public string Label { get; private set; }
        
        public EnumPathAttribute(string path, string label = null)
        {
            Path = path;
            Label = label;
        }
    }

}