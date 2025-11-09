using Unity.Entities;

namespace com.hexengine.gear.ecs {
	public partial struct DestroyEntitiesJob : IJobEntity {
		public EntityCommandBuffer commandBuffer;
		void Execute(in Entity entity) {
			commandBuffer.DestroyEntity(entity);
		}
	}

	public partial struct DestroyEntitiesParallelJob : IJobEntity {
		public EntityCommandBuffer.ParallelWriter commandBuffer;
		void Execute(in Entity entity, [EntityIndexInQuery] int sortKey) {
			commandBuffer.DestroyEntity(sortKey, entity);
		}
	}
}