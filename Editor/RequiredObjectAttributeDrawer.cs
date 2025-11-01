using UnityEditor;
using UnityEngine;

namespace Barliesque.InspectorTools.Editor
{
    [CustomPropertyDrawer(typeof(RequiredObjectAttributeDrawer))]
    public class RequiredObjectAttributeDrawer : PropertyDrawer
    {
        override public float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, property.hasVisibleChildren);
        }

        override public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue == null) GUI.color = Color.red;
            var content = new GUIContent(property.displayName, property.tooltip);
            EditorGUI.PropertyField(position, property, content, property.hasVisibleChildren);
            GUI.color = Color.white;
        }
    }
}