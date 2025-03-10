using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Barliesque.InspectorTools.Editor
{

	abstract public class EditorBase<T> : UnityEditor.Editor where T : UnityEngine.Object
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

			var inst = target as T;
			CustomInspector(inst);

			serializedObject.ApplyModifiedProperties();
			EditorTools.HelpBoxesEnabled = true;
		}

		private void OnEnable()
		{
			var inst = target as T;
			if (inst != null) OnEnabled(inst);
		}

		virtual protected void OnEnabled(T inst)
		{ }

		private void OnDisable()
		{
			OnDisabled(target as T);
		}

		virtual protected void OnDisabled(T inst)
		{ }

		abstract protected void CustomInspector(T inst);

		//TODO  Make IncludeChildren an optional parameter, so there aren't so many permutations
		//TODO  Duplicate all remaining PropertyField() methods as RequiredProperty()
		//TODO  Add optional method ShowWarning()
		//TODO  If warning's been set ShowWarning hasn't been called directly, then show it after calling CustomInspector()

		protected SerializedProperty ObjectField<O>(string propName, bool allowSceneObjects)
		{
			SerializedProperty prop = GetProperty(propName);
			if (prop != null)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(prop.displayName);
				prop.objectReferenceValue = EditorGUILayout.ObjectField(prop.objectReferenceValue, typeof(O), allowSceneObjects);
				EditorGUILayout.EndHorizontal();
			}
			return prop;
		}

		static protected SerializedProperty PropertyField(EditorBase<T> editor, string propName)
		{
			SerializedProperty prop = editor.GetProperty(propName);
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop);
			}
			return prop;
		}

		public SerializedProperty PropertyField(string propName)
		{
			SerializedProperty prop = GetProperty(propName);
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop);
			}
			return prop;
		}

		protected SerializedProperty ChildPropertyField(string parentProp, string propName, string label = null)
		{
			SerializedProperty parent = GetProperty(parentProp);
			var prop = parent.FindPropertyRelative(propName);
			if (prop != null)
			{
				if (label == null)
				{
					EditorGUILayout.PropertyField(prop);
				}
				else
				{
					EditorGUILayout.PropertyField(prop, new GUIContent(label));
				}
			}
			return prop;
		}

		static protected SerializedProperty PropertyField(EditorBase<T> editor, string propName, string label)
		{
			SerializedProperty prop = editor.GetProperty(propName);
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, new GUIContent(label, prop.tooltip));
			}
			return prop;
		}

		public SerializedProperty PropertyField(string propName, string label)
		{
			SerializedProperty prop = GetProperty(propName);
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, new GUIContent(label, prop.tooltip));
			}
			return prop;
		}
		
		public SerializedProperty PropertyField(string propName, string label, params GUILayoutOption[] options)
		{
			SerializedProperty prop = GetProperty(propName);
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, new GUIContent(label, prop.tooltip), options);
			}
			return prop;
		}

		public SerializedProperty PropertyField(string propName, GUIContent label)
		{
			SerializedProperty prop = GetProperty(propName);
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, label);
			}
			return prop;
		}

		public SerializedProperty PropertyField(string propName, GUIContent label, params GUILayoutOption[] options)
		{
			SerializedProperty prop = GetProperty(propName);
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, label, options);
			}
			return prop;
		}

		static protected SerializedProperty PropertyField(EditorBase<T> editor, string propName, string label, bool includeChildren)
		{
			SerializedProperty prop = editor.GetProperty(propName);
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, new GUIContent(label, prop.tooltip), includeChildren);
			}
			return prop;
		}

		public SerializedProperty PropertyField(string propName, string label, bool includeChildren)
		{
			SerializedProperty prop = GetProperty(propName);
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, new GUIContent(label, prop.tooltip), includeChildren);
			}
			return prop;
		}

		protected SerializedProperty StringFieldWithDefault(string field, string label, string defaultValue)
		{
			var editField = $"edit_{field}";
			var defaultField = $"default_{field}";
			
			var focus = GUI.GetNameOfFocusedControl();
			var editing = focus == defaultField || focus == editField;
			var property = GetProperty(field);
			var notDefault = !string.IsNullOrEmpty(property.stringValue);
			var fieldLabel = new GUIContent(label, property.tooltip);

			if (editing || notDefault)
			{
				GUI.SetNextControlName(editField);
				EditorGUILayout.PropertyField(property, fieldLabel);
			}
			else
			{
				GUI.color *= 0.75f; // Imitate GUI disabled
				GUI.SetNextControlName(defaultField);
				EditorGUILayout.TextField(fieldLabel, defaultValue, EditorTools.StringField);
				GUI.color /= 0.75f;
			}

			return property;
		}
		
		
		// ReSharper disable Unity.PerformanceAnalysis
		public SerializedProperty GetProperty(string propName)
		{
			SerializedProperty prop;
			if (!_properties.ContainsKey(propName))
			{
				prop = serializedObject.FindProperty(propName);
				if (prop == null)
				{
					Debug.LogErrorFormat("{0} editor could not find property: \"{1}\"", GetType().ToString(), propName);
				}
				else
				{
					_properties.Add(propName, prop);
				}
			}
			return _properties[propName];
		}

		public void EventsGroup(string label, ref bool unfolded, params string[] props)
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