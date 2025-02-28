using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using Barliesque.Utils;


namespace Barliesque.InspectorTools.Editor
{
	static public class EditorTools
	{
		//---

		/// <summary>
		/// Controls the display of HelpBoxes created with the HelpBox property attribute.
		/// </summary>
		static public bool HelpBoxesEnabled {
			get { return HelpBoxAttribute.enabled; }
			set { HelpBoxAttribute.enabled = value; }
		}

		const int ARRAY_PAGE_SIZE = 20;
		static Dictionary<int, int> _page = new Dictionary<int, int>();

		static GUIStyle _footnote;

		static public GUIStyle Footnote {
			get {
				if (_footnote == null) {
					_footnote = new GUIStyle(GUI.skin.label);
					_footnote.richText = true;
					_footnote.fontSize = 11;
					_footnote.alignment = TextAnchor.MiddleCenter;
				}
				return _footnote;
			}
		}


		static GUIStyle _richText;
		static public GUIStyle RichText {
			get {
				if (_richText == null) {
					_richText = new GUIStyle(GUI.skin.label);
					_richText.richText = true;
					_richText.fontSize = 11;
					_richText.fontStyle = FontStyle.Normal;
				}
				return _richText;
			}
		}


		static GUIStyle _textArea;
		static public GUIStyle TextArea {
			get {
				if (_textArea == null) {
					_textArea = new GUIStyle(GUI.skin.FindStyle("HelpBox"));
					_textArea.richText = true;
					_textArea.fontSize = 11;
					_textArea.padding = new RectOffset(6, 6, 5, 5);
					_textArea.wordWrap = true;
				}
				return _textArea;
			}
		}


		static GUIStyle _infoBox;
		static public GUIStyle InfoBox {
			get {
				if (_infoBox == null) {
					_infoBox = new GUIStyle(GUI.skin.FindStyle("HelpBox"));
					_infoBox.richText = true;
					_infoBox.fontSize = 11;
					_infoBox.padding = new RectOffset(14, 14, 8, 8);
					_infoBox.wordWrap = true;
				}
				return _infoBox;
			}
		}


		static GUIStyle _stringField;
		static public GUIStyle StringField {
			get {
				if (_stringField == null) {
					_stringField = new GUIStyle(GUI.skin.button);
					_stringField.normal.background = InfoBox.normal.background;
					_stringField.fontStyle = FontStyle.Normal;
					_stringField.normal.textColor = _stringField.onActive.textColor;
					_stringField.alignment = TextAnchor.MiddleLeft;
				}
				return _stringField;
			}
		}

		static GUIStyle _buttonDown;
		static public GUIStyle ButtonDown {
			get {
				if (_buttonDown == null) {
					_buttonDown = new GUIStyle(GUI.skin.button);
					_buttonDown.normal.background = _buttonDown.onActive.background;
					_buttonDown.fontStyle = FontStyle.Bold;
					_buttonDown.normal.textColor = _buttonDown.onActive.textColor;
				}
				return _buttonDown;
			}
		}

		static GUIStyle _buttonUp;
		static public GUIStyle ButtonUp
		{
			get
			{
				if (_buttonUp == null)
				{
					_buttonUp = new GUIStyle(GUI.skin.button);
					_buttonUp.normal.textColor *= 0.8f;
				}
				return _buttonUp;
			}
		}


		static public void BeginInfoBox()
		{
			EditorGUILayout.BeginVertical(InfoBox);
		}

		static public void Info(string message)
		{
			EditorGUILayout.LabelField(message, RichText);
		}

		static public void EndInfoBox()
		{
			EditorGUILayout.EndVertical();
		}
	
		
		//---


		/// <summary>
		/// Create a HelpBox using the HelpBox attribute found on an Enum value
		/// </summary>
		static public void HelpBox(Enum value)
		{
			var type = value.GetType();
			var name = Enum.GetName(type, value);
			foreach (var attr in type.GetField(name).GetCustomAttributes(false)) {
				if (attr is HelpBoxAttribute) {
					var help = (attr as HelpBoxAttribute);
					GUILayout.Space(help.spaceAbove);
					EditorGUILayout.HelpBox(help.text, (MessageType)help.messageType);
					GUILayout.Space(help.spaceBelow);
					return;
				}
			}
		}


		/// <summary>
		/// Create a HelpBox using the HelpBox attribute found on a property
		/// </summary>
		static public void HelpBox(Type type, string propName)
		{
			var field = type.GetField(propName);
			var help = Attribute.GetCustomAttribute(field, typeof(HelpBoxAttribute)) as HelpBoxAttribute;
			if (help != null) {
				GUILayout.Space(help.spaceAbove);
				EditorGUILayout.HelpBox(help.text, (MessageType)help.messageType);
				GUILayout.Space(help.spaceBelow);
			}
		}


		/// <summary>
		/// Show a message in a box, optionally with an icon to indicate warning, error or general info.
		/// </summary>
		/// <param name="help">The help message</param>
		/// <param name="type">(Optional)  Type of icon to display.</param>
		/// <returns>Returns true if the mouse has been clicked on this HelpBox</returns>
		static public bool HelpBox(string help, MessageType type = MessageType.None)
		{
			EditorStyles.helpBox.richText = true;
			
			try
			{
				EditorGUILayout.HelpBox(help, type);
			} catch
			{
				return false;
			}

			var curEvent = Event.current;
			if (curEvent == null) return false;
			if (curEvent.type != EventType.MouseUp || curEvent.button != 0) return false;

			var rect = GUILayoutUtility.GetLastRect();
			if (!rect.Contains(curEvent.mousePosition)) return false;
				
			curEvent.Use();
			return true;
		}

		//---

		static public void Header(string message, bool bold = true, bool spaceBefore = true)
		{
			if (spaceBefore) GUILayout.Space(10f);
			EditorGUILayout.LabelField(bold ? $"<b>{message}</b>" : $"{message}", RichText);
		}

		static public void Header(string message, string tooltip, bool bold = true, bool spaceBefore = true)
		{
			if (spaceBefore) GUILayout.Space(10f);
			var label = new GUIContent(bold ? $"<b>{message}</b>" : $"{message}", tooltip);
			EditorGUILayout.LabelField(label, RichText);
		}


		static public void Separator()
		{
			var color = Color.grey;
			color.a = 0.5f;
			DrawUILine(color, 1, 16);
		}
		
		static public void DrawUILine(Color color, int thickness = 2, int padding = 10)
		{
			var r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			r.height = thickness;
			r.y += padding * 0.5f;
			r.x -= 2f;
			r.width  += 6f;
			EditorGUI.DrawRect(r, color);
		}
		//***

		public delegate T ArrayElementDrawer<T>(T value);

		[Obsolete("Not Implemented (Yet)")]
		static public IList<T> ArrayEditor<T>(string label, IList<T> list, ArrayElementDrawer<T> drawer, ref bool foldout, string footer = null, int? fixedSize = null)
		{
			//TODO  Implement this!
			return list;
		}

		[Obsolete("Not Implemented (Yet)")]
		static public IList<T> ArrayEditor<T>(string label, IList<T> list, ArrayElementDrawer<T> drawer, string footer = null, int? fixedSize = null)
		{
			//TODO  Implement this!
			return list;
		}



		static public T[] ArrayEditor<T>(string label, T[] list, ref bool unfolded) where T : UnityEngine.Object
		{
			EditorGUILayout.BeginHorizontal();
			bool enabled = GUI.enabled;
			GUI.enabled = true;
			int count = list?.Length ?? 0;
			var labelCount = (!unfolded && count > 0) ? $"{label} - {count} Element{(count == 1 ? "" : "s")}" : label;
			unfolded = EditorGUILayout.Foldout(unfolded, labelCount);
			GUI.enabled = enabled;
			EditorGUILayout.EndHorizontal();

			if (!unfolded) return list;
			return ArrayEditor(null, list);
		}

		static public IList<T> ArrayEditor<T>(string label, IList<T> list, ref bool unfolded) where T : UnityEngine.Object
		{
			EditorGUILayout.BeginHorizontal();
			bool enabled = GUI.enabled;
			GUI.enabled = true;
			int count = list?.Count ?? 0;
			var labelCount = (!unfolded && count > 0) ? $"{label} - {count} Element{(count == 1 ? "" : "s")}" : label;
			unfolded = EditorGUILayout.Foldout(unfolded, labelCount);
			GUI.enabled = enabled;
			EditorGUILayout.EndHorizontal();

			if (!unfolded) return list;
			return ArrayEditor(null, list);
		}

		static public bool ArrayEditor(string label, SerializedProperty arrayProp, ref bool unfolded, string footer = null, int? fixedSize = null)
		{
			EditorGUILayout.BeginHorizontal();
			bool enabled = GUI.enabled;
			GUI.enabled = true;
			int count = arrayProp.arraySize;
			var labelCount = (!unfolded && count > 0) ? $"{label} - {count} Element{(count == 1 ? "" : "s")}" : label;
			unfolded = EditorGUILayout.Foldout(unfolded, labelCount);
			GUI.enabled = enabled;
			EditorGUILayout.EndHorizontal();

			bool changed = false;
			if (unfolded) {
				changed = ArrayEditor(null, arrayProp, footer, fixedSize);
			}
			if (fixedSize.HasValue && arrayProp.arraySize != fixedSize.Value) {
				arrayProp.arraySize = fixedSize.Value;
				changed = true;
			}
			
			return changed;
		}



		static public bool ArrayEditor(string label, SerializedProperty arrayProp, string footer = null, int? fixedSize = null)
		{
			bool changed = false;
			if (fixedSize.HasValue) {
				arrayProp.arraySize = fixedSize.Value;
				changed = true;
			}

			EditorGUILayout.BeginVertical("box");

			EditorGUILayout.BeginHorizontal();
			if (!string.IsNullOrEmpty(label))
				EditorGUILayout.LabelField(label);
			// Button... Force it to the right
			EditorGUILayout.Space();
			if (!fixedSize.HasValue) {
				if (GUI.enabled && AddButton()) {
					++arrayProp.arraySize;
					changed = true;
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			int count = arrayProp.arraySize;
			if (count > 0) {
				EditorGUILayout.BeginVertical();
				EditorGUI.BeginChangeCheck();

				int remove = -1;
				int shift = -1;
				int shiftTo = -1;

				int from, to;
				GetArrayPageFromTo(arrayProp.GetHashCode(), count, out from, out to);

				for (int i = from; i <= to; i++) {
					EditorGUILayout.BeginHorizontal("box");

					// Index...  Shift up or down?
					EditorGUILayout.LabelField(string.Format("[{0}]", i), GUILayout.Width(36f));
					ArrayShiftButtons(i, count, ref shift, ref shiftTo);

					// Show the element
					if (EditorGUILayout.PropertyField(arrayProp.GetArrayElementAtIndex(i), GUIContent.none, true, GUILayout.ExpandWidth(true))) {
						changed = true;
					}

					if (!fixedSize.HasValue) {
						// Add a remove button to the right
						if (GUI.enabled && GUILayout.Button("×", GUILayout.ExpandWidth(false))) {
							remove = i;
						}
					}
					EditorGUILayout.EndHorizontal();
				}

				// Carry out element remove operation
				if (remove >= 0) {
					if (remove < count - 1) {
						arrayProp.DeleteArrayElementAtIndex(remove);
					} else {
						--arrayProp.arraySize;
					}
					--count;
					changed = true;
				}
				if (shift >= 0) {
					arrayProp.MoveArrayElement(shift, shiftTo);
				}

				if (EditorGUI.EndChangeCheck()) changed = true;
				EditorGUILayout.EndVertical();
			}
			ArrayPageButtons(arrayProp.GetHashCode(), count);

			// Footer below list
			EditorGUILayout.BeginHorizontal();
			// Help message?
			if (!string.IsNullOrEmpty(footer)) {
				EditorGUILayout.LabelField(footer, Footnote);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();

			return changed;
		}



		static public IList<T> ArrayEditor<T>(string label, IList<T> list) where T : UnityEngine.Object
		{
			EditorGUILayout.BeginVertical("box");

			// Header and ADD button
			EditorGUILayout.BeginHorizontal();
			if (!string.IsNullOrEmpty(label)) {
				EditorGUILayout.LabelField(label);
			}
			// Force ADD button to the right
			EditorGUILayout.Space();
			if (GUI.enabled && AddButton()) {
				list.Add(default(T));
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			if (!string.IsNullOrEmpty(label))
				EditorGUILayout.LabelField(label);

			if (list == null) list = (T[])Activator.CreateInstance(typeof(T[]));
			if (list.Count > 0) {
				EditorGUILayout.BeginVertical("box");
				int remove = -1;
				int shift = -1;
				int shiftTo = -1;

				int from, to;
				GetArrayPageFromTo(list.GetHashCode(), list.Count, out from, out to);

				for (int i = from; i <= to; i++) {
					EditorGUILayout.BeginHorizontal();

					// Index...  Shift up or down?
					EditorGUILayout.LabelField(string.Format("[{0}]", i), GUILayout.Width(36f));
					ArrayShiftButtons(i, list.Count, ref shift, ref shiftTo);
					list[i] = (T)EditorGUILayout.ObjectField(list[i], typeof(T), true, GUILayout.ExpandWidth(true));
					if (GUI.enabled && GUILayout.Button("×", GUILayout.ExpandWidth(false))) remove = i;
					EditorGUILayout.EndHorizontal();
				}
				if (remove >= 0) {
					list.RemoveAt(remove);
				}
				if (shift >= 0) {
					var temp = list[shift];
					list[shift] = list[shiftTo];
					list[shiftTo] = temp;
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.Space();

			ArrayPageButtons(list.GetHashCode(), list.Count);

			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();

			return list;
		}



		static public T[] ArrayEditor<T>(string label, T[] list) where T : UnityEngine.Object
		{
			EditorGUILayout.BeginVertical("box");

			// Header and ADD button
			EditorGUILayout.BeginHorizontal();
			if (!string.IsNullOrEmpty(label)) {
				EditorGUILayout.LabelField(label);
			}
			// Force ADD button to the right
			EditorGUILayout.Space();
			if (GUI.enabled && AddButton()) {
				var newList = Array.CreateInstance(typeof(T), list.Length + 1);
				Array.Copy(list, newList, list.Length);
				list = (T[])newList;
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			if (!string.IsNullOrEmpty(label))
				EditorGUILayout.LabelField(label);

			if (list == null) list = (T[])Activator.CreateInstance(typeof(T[]));
			if (list.Length > 0) {
				EditorGUILayout.BeginVertical("box");
				int remove = -1;
				int shift = -1;
				int shiftTo = -1;

				int from, to;
				GetArrayPageFromTo(list.GetHashCode(), list.Length, out from, out to);

				for (int i = from; i <= to; i++) {
					EditorGUILayout.BeginHorizontal();

					// Index...  Shift up or down?
					EditorGUILayout.LabelField(string.Format("[{0}]", i), GUILayout.Width(36f));
					ArrayShiftButtons(i, list.Length, ref shift, ref shiftTo);

					list[i] = (T)EditorGUILayout.ObjectField(list[i], typeof(T), true, GUILayout.ExpandWidth(true));
					if (GUI.enabled && GUILayout.Button("×", GUILayout.ExpandWidth(false))) remove = i;
					EditorGUILayout.EndHorizontal();
				}
				if (remove >= 0) {
					var newList = Array.CreateInstance(typeof(T), list.Length - 1);
					if (remove > 0) Array.Copy(list, 0, newList, 0, remove);
					if (remove < list.Length - 1) Array.Copy(list, remove + 1, newList, remove, list.Length - 1 - remove);
					list = (T[])newList;
				}
				if (shift >= 0) {
					var temp = list[shift];
					list[shift] = list[shiftTo];
					list[shiftTo] = temp;
				}
				EditorGUILayout.EndVertical();
			}

			ArrayPageButtons(list.GetHashCode(), list.Length);

			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();

			return list;
		}



		static void ArrayShiftButtons(int i, int length, ref int shift, ref int shiftTo)
		{
			if (!GUI.enabled) return;
			GUI.enabled = (i > 0);

			var top = GUILayoutUtility.GetLastRect().y;

			if (GUI.Button(new Rect(60f, top, 20f, 18f), "▲")) {
				shift = i;
				shiftTo = i - 1;
			}
			GUI.enabled = (i < (length - 1));
			if (GUI.Button(new Rect(80f, top, 20f, 18f), "▼")) {
				shift = i;
				shiftTo = i + 1;
			}
			GUILayout.Space(42f);
			GUI.enabled = true;
		}


		static private int GetArrayPageFromTo(int arrayHash, int count, out int from, out int to)
		{
			int page = 0;
			if (_page.ContainsKey(arrayHash)) {
				page = Mathf.Clamp(_page[arrayHash], 0, (count - 1) / ARRAY_PAGE_SIZE);
			} else {
				_page.Add(arrayHash, 0);
			}

			from = page * ARRAY_PAGE_SIZE;
			to = Mathf.Max(Mathf.Min(from + ARRAY_PAGE_SIZE, count) - 1, 0);

			return page;
		}


		static private void ArrayPageButtons(int arrayHash, int count)
		{
			if (count <= ARRAY_PAGE_SIZE) return;

			int from, to;
			int page = GetArrayPageFromTo(arrayHash, count, out from, out to);

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			GUI.enabled = page > 0;
			GUI.SetNextControlName("Prev");
			if (GUILayout.Button(" < ", GUILayout.ExpandWidth(false))) { //«
				_page[arrayHash] = page - 1;
				GUI.FocusControl("Prev");
			}

			EditorGUILayout.LabelField($"[{from}] to [{to}] of {count}", Footnote, GUILayout.Width(150f));

			GUI.enabled = to < (count - 1);
			GUI.SetNextControlName("Next");
			if (GUILayout.Button(" > ", GUILayout.ExpandWidth(false))) { //»
				_page[arrayHash] = page + 1;
				GUI.FocusControl("Next");
			}

			GUI.enabled = true;
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}


		//***


		static public bool BeginFoldout(string label, ref bool showing, bool boxed = true)
		{
			showing = EditorGUILayout.Foldout(showing, label);
			if (showing) {
				if (boxed)
					EditorGUILayout.BeginVertical("Box");
				else
					EditorGUILayout.BeginVertical();
				++EditorGUI.indentLevel;
			}
			return showing;
		}
		static public void EndFoldout(bool showing)
		{
			if (showing) {
				--EditorGUI.indentLevel;
				EditorGUILayout.EndVertical();
			}
		}


		//***


		static public T ObjectField<T>(GUIContent label, T value, bool allowSceneObjects = true) where T : UnityEngine.Object
		{
			return (T)EditorGUILayout.ObjectField(label, value, typeof(T), allowSceneObjects);
		}
		static public T ObjectField<T>(string label, T value, bool allowSceneObjects = true) where T : UnityEngine.Object
		{
			return (T)EditorGUILayout.ObjectField(label, value, typeof(T), allowSceneObjects);
		}

		static public T PrefabField<T>(GUIContent label, T value) where T : UnityEngine.Object
		{
			return (T)EditorGUILayout.ObjectField(label, value, typeof(T), false);
		}
		static public T PrefabField<T>(string label, T value) where T : UnityEngine.Object
		{
			return (T)EditorGUILayout.ObjectField(label, value, typeof(T), false);
		}

		static public void PropertyField(SerializedObject serial, GUIContent label, string propName, bool includeChildren = false, params GUILayoutOption[] options)
		{
			var prop = serial.FindProperty(propName);
			EditorGUILayout.PropertyField(prop, label, includeChildren, options);
		}
		static public void PropertyField(SerializedObject serial, string propName, bool includeChildren = false, params GUILayoutOption[] options)
		{
			var prop = serial.FindProperty(propName);
			EditorGUILayout.PropertyField(prop, includeChildren, options);
		}


		//***


		static public bool MinMaxSlider(string label, ref float minValue, ref float maxValue, float minLimit, float maxLimit, int decimalDigits = 2)
		{
			float min = minValue;
			float max = maxValue;
			EditorGUILayout.BeginHorizontal();
			min = EditorGUILayout.FloatField(label, min);
			EditorGUILayout.MinMaxSlider(ref min, ref max, minLimit, maxLimit, GUILayout.ExpandWidth(true));
			max = EditorGUILayout.FloatField(max, GUILayout.Width(56f));
			EditorGUILayout.EndHorizontal();
			bool changed = false;
			min = (float)System.Math.Round(min, decimalDigits, MidpointRounding.AwayFromZero);
			changed |= (!min.Equals(minValue));
			minValue = min;
			max = (float)System.Math.Round(max, decimalDigits, MidpointRounding.AwayFromZero);
			changed |= (!max.Equals(maxValue));
			maxValue = max;
			return changed;
		}


		//***


		public class EnumTab<T> : GUIContent where T : struct
		{
			public T value;
			public EnumTab(string name, T value) : base(name)
			{
				this.value = value;
			}
		}


		static public EnumTab<T>[] MakeTabsFromEnum<T>(params T[] values) where T : struct
		{
			int count = values.Length;
			var tabs = new EnumTab<T>[count];
			for (int i = 0; i < count; i++) {
				tabs[i] = new EnumTab<T>(values[i].ToString().SplitCamelCase(), values[i]);
			}
			return tabs;
		}


		static public EnumTab<T>[] MakeTabsFromEnum<T>() where T : struct
		{
			var labels = Enum.GetNames(typeof(T));
			int count = labels.Length;
			var tabs = new EnumTab<T>[count];
			for (int i = 0; i < count; i++) {
				tabs[i] = new EnumTab<T>(labels[i].SplitCamelCase(), (T)Enum.Parse(typeof(T), labels[i]));
			}
			return tabs;
		}


		static public bool BeginTabber(GUIContent[] tabs, ref int selected)
		{
			bool changed = false;
			// Show Tabs in the Inspector
			int newTab = GUILayout.Toolbar(Mathf.Clamp(selected, 0, tabs.Length - 1), tabs);
			// Save change of selected tab in global Editor Preferences
			if (selected != newTab) {
				selected = newTab;
				changed = true;
			}
			EditorGUILayout.BeginVertical("box");
			GUILayout.Space(10f);
			++EditorGUI.indentLevel;
			return changed;
		}


		static public void EndTabber()
		{
			--EditorGUI.indentLevel;
			EditorGUILayout.EndVertical();
		}


		//***

		static public string SelectFile(string label, string path, string extension)
		{
			EditorGUILayout.BeginHorizontal();
			string filepath = EditorGUILayout.TextField(label, path, GUILayout.ExpandWidth(true));
			if (GUILayout.Button("•••", GUILayout.Width(40f))) {
				filepath = EditorUtility.OpenFilePanel(label, path, extension);
			}
			EditorGUILayout.EndHorizontal();
			return string.IsNullOrEmpty(filepath) ? path : filepath;
		}


		static public string SelectFolder(string label, string path)
		{
			EditorGUILayout.BeginHorizontal();
			string filepath = EditorGUILayout.TextField(label, path, GUILayout.ExpandWidth(true));
			if (GUILayout.Button("•••", GUILayout.Width(40))) {
				filepath = EditorUtility.OpenFolderPanel(label, path, "Folder");
			}
			EditorGUILayout.EndHorizontal();
			return string.IsNullOrEmpty(filepath) ? path : filepath;
		}

		/// <summary>
		/// Returns the path (from Assets/) to the currently viewed folder in the Project panel.
		/// </summary>
		/// <returns></returns>
		static public string GetCurrentProjectFolder()
		{
			Type projectWindowUtilType = typeof(ProjectWindowUtil);
			MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
			object obj = getActiveFolderPath.Invoke(null, new object[0]);
			return obj.ToString();
		}


		//***

		static public bool ToggleButton(SerializedProperty prop, GUILayoutOption options = null)
		{
			var state = prop.boolValue;
			var result = ToggleButton(state, prop.displayName, prop.tooltip, options);
			if (result != state) prop.boolValue = result;
			return result;
		}
		
		static public bool ToggleButton(SerializedProperty prop, GUIContent label, GUILayoutOption options = null)
		{
			var state = prop.boolValue;
			var result = ToggleButton(state, label, options);
			if (result != state) prop.boolValue = result;
			return result;
		}
		
		static public bool ToggleButton(bool state, GUIContent label, GUILayoutOption options = null)
		{
			bool pressed = false;
			if (state)
			{
				if (options != null) pressed = GUILayout.Button(label, ButtonDown, options);
				else pressed = GUILayout.Button(label, ButtonDown);
			}
			else
			{
				if (options != null) pressed = GUILayout.Button(label, ButtonUp, options);
				else pressed = GUILayout.Button(label, ButtonUp);
			}
			return (pressed) ? !state : state;
		}
		
		static public bool ToggleButton(bool state, string label, string tooltip = null, GUILayoutOption options = null)
		{
			var guiLabel = new GUIContent(label, tooltip);
			bool pressed = false;
			if (options == null)
			{
				pressed = GUILayout.Button(guiLabel, state ? ButtonDown : ButtonUp);
			}
			else
			{
				pressed = GUILayout.Button(guiLabel, state ? ButtonDown : ButtonUp, options);
			}
			return (pressed) ? !state : state;
		}

		static public int RadioButtons(int state, string[] buttons, GUILayoutOption options = null)
		{
			EditorGUILayout.BeginHorizontal();
			for (int i = 0; i < buttons.Length; i++)
			{
				if (GUILayout.Button(buttons[i], i == state ? ButtonDown : ButtonUp)) state = i;
			}
			EditorGUILayout.EndHorizontal();
			return state;
		}

		static public int RadioButtons(int state, GUIContent[] buttons, GUILayoutOption options = null)
		{
			EditorGUILayout.BeginHorizontal();
			for (int i = 0; i < buttons.Length; i++)
			{
				if (GUILayout.Button(buttons[i], i == state ? ButtonDown : ButtonUp)) state = i;
			}
			EditorGUILayout.EndHorizontal();
			return state;
		}
		

		// https://unitylist.com/p/5c3/Unity-editor-icons
		
		static public bool AddButton(string tooltip = null)
		{
			return GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Plus", tooltip), GUILayout.Width(28f));
		}

		static public bool RemoveButton(string tooltip = null)
		{
			return GUILayout.Button(EditorGUIUtility.TrIconContent("Toolbar Minus", tooltip), GUILayout.Width(28f));
		}


		//***


		static public bool ScriptField(SerializedObject serializedObject, bool editable = false)
		{
			EditorGUI.BeginChangeCheck();
			var wasEnabled = GUI.enabled;
			GUI.enabled = editable;
			SerializedProperty prop = serializedObject.FindProperty("m_Script");
			EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
			bool changed = EditorGUI.EndChangeCheck();
			if (changed) serializedObject.ApplyModifiedProperties();
			GUI.enabled = wasEnabled;
			return changed;
		}

		/// <summary>
		/// Draw a property field for an object reference as well as fields for each of the selected object's child properties.
		/// </summary>
		/// <param name="parent">The parent property</param>
		/// <param name="label">(optional) A label</param>
		/// <param name="tooltip">(optional) A tooltip</param>
		/// <returns>Returns a reference to the SerializedObject of the selected object.  Null if none is selected.</returns>
		static public SerializedObject PropertyFieldWithChildren(SerializedProperty parent, string label = null, string tooltip = null)
		{
			if (string.IsNullOrEmpty(label)) label = parent.displayName;
			if (string.IsNullOrEmpty(tooltip)) tooltip = parent.tooltip;
			EditorGUILayout.PropertyField(parent, new GUIContent(label, tooltip));
			
			if (parent.objectReferenceValue == null) return null;
			var parentSerial = new SerializedObject(parent.objectReferenceValue);
			
			BeginInfoBox();
			var current = parentSerial.GetIterator();
			bool first = true;

			while (current.NextVisible(first))
			{
				if (current.name == "m_Script") continue;
				EditorGUILayout.PropertyField(current);
				first = false;
			}
			
			parentSerial.ApplyModifiedProperties();
			EndInfoBox();
			
			return parentSerial;
		}

		/// <summary>
		/// Show a message in a box, with an icon to indicate warning, error or general info.
		/// If clicked in the Inspector, an object will be pinged.
		/// </summary>
		/// <param name="help">The help message</param>
		/// <param name="context">The object to ping.</param>
		/// <param name="type">Type of icon to display.</param>
		static public void HelpBox(string help, GameObject context, MessageType type = MessageType.Warning)
		{
			if (HelpBox(help, type)) EditorGUIUtility.PingObject(context);
		}
		
	}
}