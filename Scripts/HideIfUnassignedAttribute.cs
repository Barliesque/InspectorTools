using UnityEngine;

namespace Barliesque.InspectorTools
{
	public class HideIfUnassignedAttribute : PropertyAttribute
	{
		public readonly string Target;
		public readonly bool Indent;

		public HideIfUnassignedAttribute(string targetProperty, bool indent = true)
		{
			Target = targetProperty;
			Indent = indent;
		}
	}
}