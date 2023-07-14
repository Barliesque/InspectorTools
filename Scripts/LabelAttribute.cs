using System;
using UnityEngine;

namespace Barliesque.InspectorTools
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class LabelAttribute : PropertyAttribute
    {
        public string Label { get; private set; }

        public LabelAttribute(string label)
        {
            Label = label;
        }
    }
}

