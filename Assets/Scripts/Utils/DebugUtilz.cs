using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
public static class DebugUtilz
{
	public static void DrawCrosshair(Vector3 pos, float size, Color color, float duration)
	{
		Debug.DrawRay(pos, size * Vector3.up, color, duration);
		Debug.DrawRay(pos, size * Vector3.down, color, duration);
		Debug.DrawRay(pos, size * Vector3.left, color, duration);
		Debug.DrawRay(pos, size * Vector3.right, color, duration);
		Debug.DrawRay(pos, size * Vector3.forward, color, duration);
		Debug.DrawRay(pos, size * Vector3.back, color, duration);
	}

	public static void DrawBounds(Bounds bounds, Color color, float duration = 0)
	{
		DrawBounds(bounds.min, bounds.max, color, duration);
	}

	public static void DrawBounds(AABB bounds, Color color, float duration = 0)
	{
		DrawBounds(bounds.Min, bounds.Max, color, duration);
	}
	public static void DrawBounds(Aabb bounds, Color color, float duration = 0)
	{
		DrawBounds(bounds.Min, bounds.Max, color, duration);
	}

	public static void DrawBounds(float3 min, float3 max, Color color, float duration = 0)
	{
		float3 a, b, c, d;
		float3 e, f, g, h;
		a = min;
		b = new float3(max.x, min.yz);
		c = new float3(min.x, max.yz);
		d = new float3(min.xy, max.z);
		c = new float3(min.xy, max.z);
		e = new float3(min.x, max.y, min.z);
		f = new float3(max.x, max.y, min.z);
		g = max;
		h = new float3(min.x, max.y, max.z);

		Debug.DrawLine(a, b, color, duration);
		Debug.DrawLine(b, c, color, duration);
		Debug.DrawLine(c, d, color, duration);
		Debug.DrawLine(d, a, color, duration);

		Debug.DrawLine(a, e, color, duration);
		Debug.DrawLine(b, f, color, duration);
		Debug.DrawLine(c, g, color, duration);
		Debug.DrawLine(d, h, color, duration);

		Debug.DrawLine(e, f, color, duration);
		Debug.DrawLine(f, g, color, duration);
		Debug.DrawLine(g, h, color, duration);
		Debug.DrawLine(h, e, color, duration);
	}
}