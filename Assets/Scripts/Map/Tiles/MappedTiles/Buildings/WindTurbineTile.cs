using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindTurbileTile : TickedTile
{
	public WindTurbineInfo turbineInfo;

	public WindTurbileTile(HexCoords coords, float height, WindTurbineInfo tInfo) : base(coords, height, tInfo)
	{
		turbineInfo = tInfo;
	}
}
