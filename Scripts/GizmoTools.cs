using UnityEngine;

namespace Barliesque.InspectorTools
{
	static public class GizmoTools
	{
		static private readonly Mesh _quad = new Mesh()
		{
			vertices = new[]
			{
				new Vector3(-0.5f, 0.5f, 0f),
				new Vector3(0.5f, 0.5f, 0f),
				new Vector3(0.5f, -0.5f, 0f),
				new Vector3(-0.5f, -0.5f, 0f),
			},
			normals = new[]
			{
				Vector3.back, Vector3.back, Vector3.back, Vector3.back
			},
			triangles = new[]
			{
				0, 1, 2, 0, 2, 3
			},
			uv = new[]
			{
				new Vector2(0, 1),
				new Vector2(1, 1),
				new Vector2(1, 0),
				new Vector2(0, 0)
			}
		};

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
			Gizmos.color = color == default(Color) ? new Color(0f,0.5f, 1f, 0.75f) : color;
			for (int f = 0; f <= 1; f++)
			{
				for (int i = 1; i < _heel.Length; i++)
				{
					Gizmos.DrawLine(pos + rot * FootPoint2D(_heel[i - 1], f), pos + rot * FootPoint2D(_heel[i], f));
				}
				for (int i = 1; i < _foot.Length; i++)
				{
					Gizmos.DrawLine(pos + rot * FootPoint2D(_foot[i - 1], f), pos + rot * FootPoint2D(_foot[i], f));
				}
			}
		}
		static private Vector3 FootPoint2D(Vector2 point, int foot)
		{
			return new Vector3(point.x * (foot < 1 ? 1 * -1f : 1f), 0f, point.y) * 0.0375f;
		}
		

		static private readonly Color[] _axisColors =
		{
			Color.red * 0.75f, Color.green * 0.5f, Color.blue * 0.75f, Color.red, Color.green, Color.blue
		};



		static public void DrawAxes(Vector3 pos, Quaternion rot, float alpha = 1f)
		{
			Gizmos.color = new Color(1, 0, 0, alpha);
			Gizmos.DrawLine(pos, pos + (rot * Vector3.right) * 0.05f);
			Gizmos.color = new Color(0, 1, 0, alpha);
			Gizmos.DrawLine(pos, pos + (rot * Vector3.up) * 0.05f);
			Gizmos.color = new Color(0, 0, 1, alpha);
			Gizmos.DrawLine(pos, pos + (rot * Vector3.forward) * 0.05f);
		}

		static public void DrawAxes(Vector3 pos, Quaternion rot, float alphaX, float alphaY, float alphaZ)
		{
			Gizmos.color = new Color(1, 0, 0, alphaX);
			Gizmos.DrawLine(pos, pos + (rot * Vector3.right) * 0.05f);
			Gizmos.color = new Color(0, 1, 0, alphaY);
			Gizmos.DrawLine(pos, pos + (rot * Vector3.up) * 0.05f);
			Gizmos.color = new Color(0, 0, 1, alphaZ);
			Gizmos.DrawLine(pos, pos + (rot * Vector3.forward) * 0.05f);
		}


		static private readonly Vector3[] _cubeSides =
		{
			Vector3.right, Vector3.up, Vector3.forward, Vector3.left, Vector3.down, Vector3.back
		};

		static public void DrawRotatedCube(Vector3 pos, Quaternion rot, Vector3 size)
		{
			var scale = new[]
			{
				new Vector3(size.x, size.y, 1f),
				new Vector3(size.z, size.y, 1f),
				new Vector3(size.x, size.z, 1f)
			};
			for (var i = 0; i < 6; i++)
			{
				var side = _cubeSides[i];
				var orientation = rot * Quaternion.LookRotation(side);

				var r = side;
				r.Scale(size);
				var radius = Mathf.Abs(r.x + r.y + r.z) * 0.5f;

				Gizmos.DrawMesh(_quad, pos + (orientation * (Vector3.back * radius)), orientation, scale[i % 3]);
			}
		}


		static public void DrawRotatedCube(Vector3 pos, Quaternion rot, float size, bool useAxisColors = false)
		{
			var radius = size * 0.5f;
			var scale = Vector3.one * size;

			for (var i = 0; i < _cubeSides.Length; i++)
			{
				var side = _cubeSides[i];
				if (useAxisColors) Gizmos.color = _axisColors[i];
				var orientation = rot * Quaternion.LookRotation(side);

				Gizmos.DrawMesh(_quad, pos + (orientation * (Vector3.back * radius)), orientation, scale);
			}
		}

		static public void DrawCircleXZ(Vector3 center, float radius, int segments = 48)
		{
			for (int i = 0; i < segments; i++)
			{
				var a1 = (i * Mathf.PI * 2f / segments);
				var a2 = ((i + 1) * Mathf.PI * 2f / segments);
				var p1 = new Vector3(center.x + Mathf.Cos(a1) * radius, center.y, center.z + Mathf.Sin(a1) * radius);
				var p2 = new Vector3(center.x + Mathf.Cos(a2) * radius, center.y, center.z + Mathf.Sin(a2) * radius);
				Gizmos.DrawLine(p1, p2);
			}
		}

		static public void DrawCircleXY(Vector3 center, float radius, int segments = 48)
		{
			for (int i = 0; i < segments; i++)
			{
				var a1 = (i * Mathf.PI * 2f / segments);
				var a2 = ((i + 1) * Mathf.PI * 2f / segments);
				var p1 = new Vector3(center.x + Mathf.Cos(a1) * radius, center.y + Mathf.Sin(a1) * radius, center.z);
				var p2 = new Vector3(center.x + Mathf.Cos(a2) * radius, center.y + Mathf.Sin(a2) * radius, center.z);
				Gizmos.DrawLine(p1, p2);
			}
		}

		static public void DrawCircleYZ(Vector3 center, float radius, int segments = 48)
		{
			for (int i = 0; i < segments; i++)
			{
				var a1 = (i * Mathf.PI * 2f / segments);
				var a2 = ((i + 1) * Mathf.PI * 2f / segments);
				var p1 = new Vector3(center.x, center.y + Mathf.Cos(a1) * radius, center.z + Mathf.Sin(a1) * radius);
				var p2 = new Vector3(center.x, center.y + Mathf.Cos(a2) * radius, center.z + Mathf.Sin(a2) * radius);
				Gizmos.DrawLine(p1, p2);
			}
		}
	}
}