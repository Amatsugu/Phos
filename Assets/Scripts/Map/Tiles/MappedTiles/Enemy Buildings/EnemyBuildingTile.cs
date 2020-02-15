using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EnemyBuildingTile : BuildingTile
{
	public EnemyBuildingTile(HexCoords coords, float height, BuildingTileInfo tInfo) : base(coords, height, tInfo)
	{
		_isBuilt = true;
	}

	protected override void OnBuilt()
	{
		//base.OnBuilt();
	}
}
