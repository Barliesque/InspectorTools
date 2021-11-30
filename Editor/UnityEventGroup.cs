using UnityEngine;
using UnityEditor;

namespace Barliesque.InspectorTools.Editor
{
	/// <summary>
	/// Class to condense numerous UnityEvent properties into a list of events that can be displayed as needed.
	/// </summary>
	public class UnityEventGroup
	{
		private GUIContent _label;
		private SerializedObject _target;
		private string[] _properties;
		private string[] _options;
		private int _showing;


		/// <param name="properties">Names of UnityEvent properties to be condensed into a list.</param>
		public UnityEventGroup(string[] properties)
		{
			_label = new GUIContent("Events", "Select events to display in the Inspector.");
			_properties = properties;
		}

		/// <param name="label">Label for a dropdown selector list of event names.  Default label is "Events"</param>
		/// <param name="properties">Names of UnityEvent properties to be condensed into a list.</param>
		public UnityEventGroup(GUIContent label, string[] properties)
		{
			_label = label;
			_properties = properties;
		}

		/// <param name="label">Label for a dropdown selector list of event names.  Default label is "Events"</param>
		/// <param name="properties">Names of UnityEvent properties to be condensed into a list.</param>
		public UnityEventGroup(string label, string[] properties)
		{
			_label = new GUIContent(label, "Select events to display in the Inspector.");
			_properties = properties;
		}


		/// <summary>
		/// To be called in the OnEnable method of an Inspector editor class.
		/// </summary>
		/// <param name="target">A reference to the</param>
		public void SetTarget(SerializedObject target)
		{
			_target = target;

			var setDisplayOptions = false;
			var count = Mathf.Min(_properties.Length, 32);
			if (_options == null || _options.Length != count)
			{
				_options = new string[count];
				setDisplayOptions = true;
				if (_properties.Length > count)
				{
					Debug.LogError("Exceeded max number of 32 items in UnityEventGroup!");
				}
			}

			_showing = 0;
			for (int i = 0; i < count; i++)
			{
				var property = _properties[i];
				var serial = _target.FindProperty(property);
				if (setDisplayOptions)
				{
					if (serial == null)
					{
						Debug.LogError($"Unknown property: {property}");
						continue;
					}
					_options[i] = serial.displayName;
				}
				if (GotHandlers(serial)) _showing |= (1 << i);
			}
		}


		/// <summary>
		/// To be called in the OnEnable method of an Inspector editor class.
		/// </summary>
		/// <param name="target">A reference to the</param>
		/// <param name="mask">Persistent selection mask</param>
		public void SetTarget(SerializedObject target, int mask)
		{
			_target = target;

			var count = Mathf.Min(_properties.Length, 32);
			if (_options == null || _options.Length != count)
			{
				_options = new string[count];
				if (_properties.Length > count)
				{
					Debug.LogError("Exceeded max number of 32 items in UnityEventGroup!");
				}
			}

			_showing = mask;
		}
		

		private bool GotHandlers(SerializedProperty serial)
		{
			var handlers = serial.FindPropertyRelative("m_PersistentCalls.m_Calls");
			return (handlers.arraySize > 0);
		}
		

		/// <summary>
		/// To be called from the OnInspectorGUI method of an Inspector editor class.
		/// </summary>
		public void EventSelection(ref int mask)
		{
			EditorGUILayout.Space();
			mask = _showing = EditorGUILayout.MaskField(_label, mask, _options);

			for (int i = 0; i < _properties.Length; i++)
			{
				if ((_showing & (1 << i)) == 0) continue;

				var property = _properties[i];
				var serial = _target.FindProperty(property);
				EditorGUILayout.PropertyField(serial);
			}

			EditorGUILayout.Space();
		}
		

		/// <summary>
		/// To be called from the OnInspectorGUI method of an Inspector editor class.
		/// </summary>
		public void EventSelection()
		{
			EditorGUILayout.Space();
			_showing = EditorGUILayout.MaskField(_label, _showing, _options);

			for (int i = 0; i < _properties.Length; i++)
			{
				if ((_showing & (1 << i)) == 0) continue;

				var property = _properties[i];
				var serial = _target.FindProperty(property);
				EditorGUILayout.PropertyField(serial);
			}

			EditorGUILayout.Space();
		}
		
	}
}