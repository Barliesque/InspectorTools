using UnityEngine;
using UnityEditor;

namespace Barliesque.InspectorTools.Editor
{
	[CustomPropertyDrawer(typeof(HelpBoxAttribute))]
	public class HelpBoxAttributeDrawer : DecoratorDrawer
	{
		static private GUIStyle _style;
		private float _height = HelpBoxAttribute.enabled ? 56 : 0;

		static private GUIStyle HelpBoxStyle
		{
			get
			{
				if (_style != null) return _style;
				_style = EditorStyles.helpBox;
				_style.richText = true;
				return _style;
			}
		}

		override public float GetHeight() => _height;

		private void CalculateHeight()
		{
			if (!HelpBoxAttribute.enabled)
			{
				_height = 0f;
				return;
			}
			var helpBoxAttribute = attribute as HelpBoxAttribute;
			if (helpBoxAttribute == null || GUI.skin == null)
			{
				_height = base.GetHeight();
				return;
			}

			var height = HelpBoxStyle.CalcHeight(new GUIContent($"{helpBoxAttribute.text}----"), EditorGUIUtility.currentViewWidth) +
			             helpBoxAttribute.spaceAbove + helpBoxAttribute.spaceBelow;
			_height = (helpBoxAttribute.messageType == HelpBoxType.None) ? height : Mathf.Max(height, 55f);
		}

		override public void OnGUI(Rect position)
		{
			CalculateHeight();
			if (!HelpBoxAttribute.enabled) return;
			var helpBoxAttribute = attribute as HelpBoxAttribute;
			if (helpBoxAttribute == null) return;

			position.y += helpBoxAttribute.spaceAbove;
			position.height -= (helpBoxAttribute.spaceAbove + helpBoxAttribute.spaceBelow);

			EditorGUI.HelpBox(position, helpBoxAttribute.text, GetHelpBoxType(helpBoxAttribute.messageType));
		}

		private MessageType GetHelpBoxType(HelpBoxType type)
		{
			switch (type)
			{
				case HelpBoxType.Info: return MessageType.Info;
				case HelpBoxType.Warning: return MessageType.Warning;
				case HelpBoxType.Error: return MessageType.Error;
				default: return MessageType.None;
			}
		}
	}
}