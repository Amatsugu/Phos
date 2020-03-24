using Unity.Mathematics;
using Unity.Physics;

public static class MathUtils
{
	public static float Remap(this float value, float min, float max, float a, float b)
	{
		return (value - min) / (max - min) * (b - a) + a;
	}

	public static float Pow(this float value, float e) => math.pow(value, e);

	public static float Lerp(this float a, float b, float t) => math.lerp(a, b, t);

	public static float Range(this System.Random r, float min, float max)
	{
		return Remap(r.NextFloat(), 0, 1, min, max);
	}

	public static float NextFloat(this System.Random r)
	{
		return r.Next(1000) / 1000f;
	}

	public static float EaseOut(this float value)
	{
		value = 1 - value;
		value *= value;
		value = 1 - value;
		return value;
	}

	public static float EaseIn(this float value)
	{
		value *= value;
		return value;
	}

	public static float Ease(this float value)
	{
		return value.EaseIn() * value.EaseOut();
	}

	public static float EaseOut(this float value, int power)
	{
		value = 1 - value;
		value = math.pow(value, power);
		value = 1 - value;
		return value;
	}

	public static float EaseIn(this float value, int power)
	{
		value = math.pow(value, power);
		return value;
	}

	public static float Ease(this float value, int power)
	{
		return value.EaseIn(power) * value.EaseOut(power);
	}

	public static Aabb PhysicsBounds(float3 a, float3 b)
	{
		float3 min = default, max = default;
		if (a.x <= b.x)
		{
			min.x = a.x;
			max.x = b.x;
		}else
		{
			min.x = b.x;
			max.x = a.x;
		}
		if(a.y <= b.y)
		{
			min.y = a.y;
			max.y = b.y;
		}else
		{
			min.y = b.y;
			max.y = a.y;
		}
		if (a.z <= b.z)
		{
			min.z = a.z;
			max.z = b.z;
		}
		else
		{
			min.z = b.z;
			max.z = a.z;
		}

		var bounds = new Aabb()
		{
			Min = min,
			Max = max
		};
		return bounds;
	}
}