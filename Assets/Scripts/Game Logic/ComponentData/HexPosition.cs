using Unity.Entities;

public struct HexPosition : IComponentData
{
	public HexCoords Value;

	public static implicit operator HexCoords(HexPosition position) => position.Value;
	public static implicit operator HexPosition(HexCoords coords) => new HexPosition { Value = coords};

}