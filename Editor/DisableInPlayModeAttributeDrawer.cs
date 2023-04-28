using UnityEngine;
using UnityEditor;

namespace Barliesque.InspectorTools.Editor
{
	[CustomPropertyDrawer(typeof(DisableInPlayModeAttribute))]
	public class BeginLockInPlayModeDecoratorDrawer : PropertyDrawer
	{
		override public float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		override public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var playing = Application.isPlaying;
			if (playing) GUI.enabled = false;

			var ranges = fieldInfo.GetCustomAttributes(typeof(RangeAttribute), true);
			var range = ranges.Length > 0 ? ranges[0] as RangeAttribute : null; // ranges != null && 
			if (range != null && property.propertyType == SerializedPropertyType.Float)
			{
				EditorGUI.Slider(position, property, range.min, range.max);
			}
			else if (range != null && property.propertyType == SerializedPropertyType.Integer)
			{
				EditorGUI.IntSlider(position, property, (int)range.min, (int)range.max);
			}
			else
			{
				EditorGUI.PropertyField(position, property, label, true);
			}

			if (playing) GUI.enabled = true;
		}
	}
}