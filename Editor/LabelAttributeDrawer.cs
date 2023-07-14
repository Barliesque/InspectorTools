using UnityEngine;
using UnityEditor;

namespace Barliesque.InspectorTools.Editor
{
	[CustomPropertyDrawer(typeof(LabelAttribute))]
	public class LabelAttributeDrawer : PropertyDrawer
	{

		override public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property == null) return;

			// Get the LabelAttribute
			var attribute = ((LabelAttribute)fieldInfo.GetCustomAttributes(typeof(LabelAttribute), true)[0]);

			var content = new GUIContent(attribute.Label, property.tooltip);
			EditorGUI.BeginProperty(position, content, property);
			EditorGUI.PropertyField(position, property, content, property.hasVisibleChildren);
			EditorGUI.EndProperty();
		}
	}
}