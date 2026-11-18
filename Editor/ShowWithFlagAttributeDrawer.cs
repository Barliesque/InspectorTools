using System;
using UnityEngine;
using UnityEditor;

namespace Barliesque.InspectorTools.Editor
{
	[CustomPropertyDrawer(typeof(ShowWithFlagAttribute))]
	public class ShowWithFlagAttributeDrawer : PropertyDrawer
	{
		override public float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return IsVisible(property) ? base.GetPropertyHeight(property, label) : 0f;
		}

		override public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!IsVisible(property)) return;
			if (Indent) ++EditorGUI.indentLevel;
			var content = new GUIContent(property.displayName, property.tooltip);
			EditorGUI.PropertyField(position, property, content, property.hasVisibleChildren);
			if (Indent) --EditorGUI.indentLevel;
		}

		private bool IsVisible(SerializedProperty property)
		{
			var a = (ShowWithFlagAttribute)attribute;
			var t = property.serializedObject.FindProperty(a.FlagsProp);

			if (t is not { propertyType: SerializedPropertyType.Enum })
			{
				Debug.LogWarning("[ShowWithFlag] Invalid Property Name for Attribute.", property.serializedObject.targetObject);
				return false;
			}

			var targetFlag = Convert.ToInt32(a.Flag);
			var selectedFlags = t.intValue;
			var isFlagSet = (selectedFlags & targetFlag) == targetFlag;
			return isFlagSet ? a.ShowIfSet : !a.ShowIfSet;
		}

		private bool Indent => ((ShowWithFlagAttribute)attribute).Indent;
		
	}
}