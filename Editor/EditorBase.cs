using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Barliesque.InspectorTools.Editor
{
	abstract public class EditorBase : UnityEditor.Editor
	{

		Dictionary<string, SerializedProperty> _properties = new Dictionary<string, SerializedProperty>();

		virtual protected bool ShowScriptField => true;

		override public sealed void OnInspectorGUI()
		{
			serializedObject.Update();

			if (ShowScriptField)
			{
				EditorTools.ScriptField(serializedObject);
			}

			EditorGUILayout.Space();
			CustomInspector();

			serializedObject.ApplyModifiedProperties();
			EditorTools.HelpBoxesEnabled = true;
		}

		abstract protected void CustomInspector();

		static public SerializedProperty PropertyField(EditorBase editor, string propName) {
			SerializedProperty prop = editor.GetProperty(propName);
			if (prop != null) {
				EditorGUILayout.PropertyField(prop);
			}
			return prop;
		}

		protected SerializedProperty PropertyField(string propName)
		{
			SerializedProperty prop = GetProperty(propName);
			if (prop != null) {
				EditorGUILayout.PropertyField(prop);
			}
			return prop;
		}

		static public SerializedProperty PropertyField(EditorBase editor, string propName, string label)
		{
			SerializedProperty prop = editor.GetProperty(propName);
			if (prop != null) {
				EditorGUILayout.PropertyField(prop, new GUIContent(label, prop.tooltip));
			}
			return prop;
		}

		protected SerializedProperty PropertyField(string propName, string label)
		{
			SerializedProperty prop = GetProperty(propName);
			if (prop != null) {
				EditorGUILayout.PropertyField(prop, new GUIContent(label, prop.tooltip));
			}
			return prop;
		}

		static public SerializedProperty PropertyField(EditorBase editor, string propName, string label, bool includeChildren)
		{
			SerializedProperty prop = editor.GetProperty(propName);
			if (prop != null) {
				EditorGUILayout.PropertyField(prop, new GUIContent(label, prop.tooltip), includeChildren);
			}
			return prop;
		}

		protected SerializedProperty PropertyField(string propName, string label, bool includeChildren)
		{
			SerializedProperty prop = GetProperty(propName);
			if (prop != null) {
				EditorGUILayout.PropertyField(prop, new GUIContent(label, prop.tooltip), includeChildren);
			}
			return prop;
		}

		static public SerializedProperty GetProperty(EditorBase editor, string propName)
		{
			return editor.GetProperty(propName);
		}
		
		protected SerializedProperty GetProperty(string propName)
		{
			SerializedProperty prop;
			if (!_properties.ContainsKey(propName)) {
				prop = serializedObject.FindProperty(propName);
				if (prop == null) {
					Debug.LogErrorFormat("{0} editor could not find property: \"{1}\"", GetType().ToString(), propName);
				} else {
					_properties.Add(propName, prop);
				}
			}
			return _properties[propName];
		}

		protected void EventsGroup(string label, ref bool unfolded, params string[] props)
		{
			int count = 0;
			foreach (var prop in props)
			{
				var handlers = GetProperty(prop).FindPropertyRelative("m_PersistentCalls.m_Calls");
				count += handlers.arraySize;
			}

			if (EditorTools.BeginFoldout($"{label} - {count} Listener{(count == 1 ? "" : "s")}", ref unfolded))
			{
				foreach (var prop in props)
				{
					PropertyField(prop);
				}
				
				EditorGUILayout.Space();
			}
			EditorTools.EndFoldout(unfolded);
		}
		

	}
}