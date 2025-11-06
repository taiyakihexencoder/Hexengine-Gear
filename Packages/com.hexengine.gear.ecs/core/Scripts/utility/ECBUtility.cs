using Unity.Entities;

namespace com.hexengine.gear.ecs {
	public static class ECBUtility {
		public static EntityCommandBuffer Get(ref SystemState state) {
			EndSimulationEntityCommandBufferSystem system = state.World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
			return system.CreateCommandBuffer();
		}
	}
}