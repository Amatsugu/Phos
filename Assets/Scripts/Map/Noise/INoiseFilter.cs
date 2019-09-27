using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INoiseFilter
{
	float Evaluate(Vector3 point);

	float Evaluate(Vector3 point, float minValue);

	void SetSeed(int seed);
}
