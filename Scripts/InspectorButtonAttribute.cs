using System;
using UnityEngine;

namespace Barliesque.InspectorTools
{

	[AttributeUsage(AttributeTargets.Method)]
	public class InspectorButtonAttribute : PropertyAttribute
	{
		public readonly string ButtonLabel;
		public readonly string ButtonTooltip;
		public readonly bool GroupWithNext;

		// Optional: Pass a custom label, otherwise it defaults to the method name
		public InspectorButtonAttribute(string buttonLabel = null, string buttonTooltip = null, bool groupWithNext = false)
		{
			ButtonLabel = buttonLabel;
			ButtonTooltip = buttonTooltip;
			GroupWithNext = groupWithNext;
		}
	}

}