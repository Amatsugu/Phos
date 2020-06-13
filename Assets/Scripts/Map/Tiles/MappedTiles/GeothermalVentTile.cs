
using Amatsugu.Phos.TileEntities;

using Unity.Entities;
using Unity.Rendering;

using UnityEngine;

namespace Amatsugu.Phos.Tiles
{
	public class GeothermalVentTile : ResourceTile
	{
		public VentTileInfo ventInfo;

		private Entity _core;
		private GameObject _gyser;

		public GeothermalVentTile(HexCoords coords, float height, Map map, VentTileInfo tInfo = null) : base(coords, height, map, tInfo)
		{
			ventInfo = tInfo;
		}

		public override TileEntity GetMeshEntity()
		{
			return originalTile;
		}

		public override Entity Render()
		{
			_core = ventInfo.core.Instantiate(new Vector3(Coords.WorldPos.x, Height, Coords.WorldPos.z));
			_gyser = GameObject.Instantiate(ventInfo.gyser, new Vector3(Coords.WorldPos.x, Height, Coords.WorldPos.z), Quaternion.identity);
			return base.Render();
		}

		public override void OnShow()
		{
			base.OnShow();
			Map.EM.RemoveComponent<FrozenRenderSceneTag>(_core);
			_gyser.SetActive(true);
		}

		public override void OnHide()
		{
			base.OnHide();
			Map.EM.AddComponent(_core, typeof(FrozenRenderSceneTag));
			_gyser.SetActive(false);
		}

		public override void Destroy()
		{
			GameObject.Destroy(_gyser);
			Map.EM.DestroyEntity(_core);
			base.Destroy();
		}
	}

	public class GeothermalVentShellTile : ResourceTile
	{
		public VentTileInfo ventInfo;
		public float angle;
		public Entity _shell;

		public GeothermalVentShellTile(HexCoords coords, float height, float angle, Map map, VentTileInfo tInfo = null) : base(coords, height, map, tInfo)
		{
			ventInfo = tInfo;
			this.angle = angle;
		}

		public override TileEntity GetMeshEntity()
		{
			return originalTile;
		}

		public override Entity Render()
		{
			_shell = ventInfo.shell.Instantiate(new Vector3(Coords.WorldPos.x, Height, Coords.WorldPos.z), Vector3.one, Quaternion.Euler(0, angle - 60, 0));
			return base.Render();
		}

		public override void OnHide()
		{
			base.OnHide();
			Map.EM.AddComponent(_shell, typeof(FrozenRenderSceneTag));
		}

		public override void OnShow()
		{
			base.OnShow();
			Map.EM.RemoveComponent<FrozenRenderSceneTag>(_shell);
		}

		public override void Destroy()
		{
			base.Destroy();
			if (Map.EM.Exists(_shell))
				Map.EM.DestroyEntity(_shell);
		}
	}
}