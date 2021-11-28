
using Unity.Entities;

namespace Amatsugu.Phos
{
	[UpdateAfter(typeof(PrefabBufferInitializationSystem))]
	public class ConduitLineRendererConversionSystem : GameObjectConversionSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((ConduitLinesAuthoring lines) =>
			{
				var entity = GetPrimaryEntity(lines);
				var prefabs = GameRegistry.PrefabDatabase;
				var active = GetPrimaryEntity(lines.activeLine);
				var inactive = GetPrimaryEntity(lines.inactiveLine);
				DstEntityManager.AddComponent<NewConduitLineTag>(active);
				DstEntityManager.AddComponent<ConduitLineTag>(active);
				DstEntityManager.AddComponent<NewConduitLineTag>(inactive);
				DstEntityManager.AddComponent<ConduitLineTag>(inactive);
				DstEntityManager.AddComponentData(entity, new ConduitLinePrefabs
				{
					active = active,
					inactive = inactive
				});
			});
		}
	}
}