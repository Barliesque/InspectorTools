using UnityEngine;

namespace Barliesque.InspectorTools
{
	public class HideIfAttribute : PropertyAttribute
	{
		public readonly string Target;
		public readonly bool HiddenState;
		public readonly bool Indent;

		public HideIfAttribute(string targetProperty, bool hiddenState, bool indent = true)
		{
			Target = targetProperty;
			HiddenState = hiddenState;
			Indent = indent;
		}
	}
}