using Unity.Collections;
using Unity.Entities;

namespace com.hexengine.gear.ecs {
	public partial struct RemoveComponentsJob : IJobEntity {
		public EntityCommandBuffer commandBuffer;
		[ReadOnly] public ComponentType type;
		void Execute(in Entity entity) {
			commandBuffer.RemoveComponent(entity, type);
		}
	}

	public partial struct RemoveComponentsParallelJob : IJobEntity {
		public EntityCommandBuffer.ParallelWriter commandBuffer;
		[ReadOnly] public ComponentType type;
		
		void Execute(in Entity entity, [EntityIndexInQuery] int sortKey) {
			commandBuffer.RemoveComponent(sortKey, entity, type);
		}
	}
}