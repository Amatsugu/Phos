using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


namespace Amatsugu.Phos.TileEntities
{
	[CreateAssetMenu(menuName = "Map Asset/Tile/Tile Info")]
	[Serializable]
	public class TileEntity : ScriptableObject, ISerializationCallbackReceiver
	{
		[Header("Prefabs")]
		public GameObject tilePrefab;
		[Header("Tile Info")]
		public string description;
		public TileDecorator[] decorators;
		public bool isTraverseable = true;
		[HideInInspector]
		public string assetGuid;

		public virtual void PrepareEntityPrefab(Entity prefab, EntityManager entityManager)
		{

		}

		public virtual Tile CreateTile(Map map, HexCoords pos, float height)
		{
			return new Tile(pos, height, map, this);
		}
		public void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this));
#endif
		}

		public virtual StringBuilder GetNameString()
		{
			return new StringBuilder(name);
		}

		public void OnAfterDeserialize()
		{
		}
	}
}