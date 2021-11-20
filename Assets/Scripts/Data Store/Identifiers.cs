
[System.Serializable]
public class BuildingIdentifier
{
	public int id = -1;
}

[System.Serializable]
public class TileIdenifier
{
	public int id = -1;
}

[System.Serializable]
public class UnitIdentifier
{
	public int id = -1;

	public static implicit operator int(UnitIdentifier identifier) => identifier.id;
	public static implicit operator UnitIdentifier(int identifier) => new UnitIdentifier { id = identifier };
}