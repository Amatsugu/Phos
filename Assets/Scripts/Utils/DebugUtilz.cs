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
}