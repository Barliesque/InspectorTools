using System;
using UnityEngine;

namespace Barliesque.InspectorTools
{

	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public class InspectorButtonAttribute : PropertyAttribute
	{
		public readonly string ButtonLabel;

		// Optional: Pass a custom label, otherwise it defaults to the method name
		public InspectorButtonAttribute(string buttonLabel = null)
		{
			ButtonLabel = buttonLabel;
		}
	}

}