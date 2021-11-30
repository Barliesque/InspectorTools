using UnityEditor;
using UnityEngine;

static public class SceneGadgets
{

	static public void DrawWireCapsule(Vector3 from, Vector3 to, float radius, Color color = default(Color))
	{
		var delta = (to - from);
		var dist = delta.magnitude;
		var rot = Quaternion.FromToRotation(Vector3.up, delta/dist);
		DrawWireCapsule(from, rot, radius, dist, color);
	}
	
	static public void DrawWireCapsule(Vector3 pos, Quaternion rot, float radius, float height, Color color = default(Color))
	{
		if (color != default(Color))
		{
			Handles.color = color;
		}

		Matrix4x4 angleMatrix = Matrix4x4.TRS(pos, rot, Handles.matrix.lossyScale);
		using (new Handles.DrawingScope(angleMatrix))
		{
			var pointOffset = height;

			// Draw sideways
			Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180f, radius);
			Handles.DrawLine(new Vector3(0f, pointOffset, -radius), new Vector3(0, 0f, -radius));
			Handles.DrawLine(new Vector3(0f, pointOffset, radius), new Vector3(0, 0f, radius));
			Handles.DrawWireArc(Vector3.zero, Vector3.left, Vector3.back, 180f, radius);

			// Draw front-ways
			Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180f, radius);
			Handles.DrawLine(new Vector3(-radius, pointOffset, 0), new Vector3(-radius, 0f, 0));
			Handles.DrawLine(new Vector3(radius, pointOffset, 0), new Vector3(radius, 0f, 0));
			Handles.DrawWireArc(Vector3.zero, Vector3.back, Vector3.left, -180f, radius);

			// Draw center
			Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, radius);
			Handles.DrawWireDisc(Vector3.zero, Vector3.up, radius);
		}
	}
	
	static private Vector2[] _heel =
	{
		new Vector2(3.75f, -1f), new Vector2(2f, -1f), new Vector2(1.5f, -2f),
		new Vector2(2f, -3f), new Vector2(3f, -3f), new Vector2(4f, -2f), new Vector2(3.75f, -1f)
	};
	static private Vector2[] _foot =
	{
		new Vector2(2.25f, 0f), new Vector2(2f, 1.5f), new Vector2(1.5f, 3f),
		new Vector2(1.5f, 5f), new Vector2(2.5f, 6f), new Vector2(3.5f, 6f),
		new Vector2(4.5f, 4f), new Vector2(4f, 0f), new Vector2(2.25f, 0f)
	};
		
	static public void DrawFootprints(Vector3 pos, Quaternion rot, Color color = default(Color))
	{
		Handles.color = color == default(Color) ? new Color(0f,0.5f, 1f, 0.75f) : color;
		
		Matrix4x4 angleMatrix = Matrix4x4.TRS(pos, rot, Handles.matrix.lossyScale);
		using (new Handles.DrawingScope(angleMatrix))
		{
			for (int f = 0; f <= 1; f++)
			{
				for (int i = 1; i < _heel.Length; i++)
				{
					Handles.DrawLine(pos + rot * FootPoint2D(_heel[i - 1], f), pos + rot * FootPoint2D(_heel[i], f));
				}

				for (int i = 1; i < _foot.Length; i++)
				{
					Handles.DrawLine(pos + rot * FootPoint2D(_foot[i - 1], f), pos + rot * FootPoint2D(_foot[i], f));
				}
			}
		}
	}
	static private Vector3 FootPoint2D(Vector2 point, int foot)
	{
		return new Vector3(point.x * (foot < 1 ? 1 * -1f : 1f), 0f, point.y) * 0.0375f;
	}

	
}