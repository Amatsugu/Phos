using Unity.Mathematics;

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
}