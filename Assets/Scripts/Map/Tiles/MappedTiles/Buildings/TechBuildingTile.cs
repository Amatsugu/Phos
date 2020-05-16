using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechBuildingTile : PoweredBuildingTile
{
    private TechBuildingEntity techInfo;

    public TechBuildingTile(HexCoords coords, float height, Map map, TechBuildingEntity tInfo) : base(coords, height, map, tInfo)
    {
        techInfo = tInfo;
    }

    public override void OnConnected()
    {
        base.OnConnected();
        UnlockBuildings();
    }

    protected override void OnBuilt()
    {
        base.OnBuilt();
        UnlockBuildings();
    }

    private void UnlockBuildings()
    {
        if (!IsBuilt || !HasHQConnection)
            return;
        for (int i = 0; i < techInfo.buildingsToUnlock.Length; i++)
            GameRegistry.UnlockBuilding(techInfo.buildingsToUnlock[i]);
    }

}
