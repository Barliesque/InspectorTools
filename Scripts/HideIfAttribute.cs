using UnityEngine;

namespace Barliesque.InspectorTools
{
	public class HideIfAttribute : PropertyAttribute
	{
		public string Target;
		public bool HiddenState;

		public HideIfAttribute(string targetProperty, bool hiddenState)
		{
			Target = targetProperty;
			HiddenState = hiddenState;
		}
	}
}