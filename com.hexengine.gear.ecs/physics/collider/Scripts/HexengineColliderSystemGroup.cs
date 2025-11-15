using Unity.Entities;
using Unity.Physics.Systems;

namespace com.hexengine.gear.ecs {
	[UpdateInGroup(typeof(PhysicsSystemGroup)), UpdateAfter(typeof(AfterPhysicsSystemGroup))]
	public partial class HexengineColliderSystemGroup : ComponentSystemGroup {
		public static void AddTo(World world, bool trigger = true, bool collision = true) {
			if(trigger || collision) {
				world.AddSystemsTo<PhysicsSystemGroup>(typeof(HexengineColliderSystemGroup));
				if (trigger) {
					world.AddSystemsTo<HexengineColliderSystemGroup>(typeof(ColliderTriggerSystem));
				}
				if (collision) {
					world.AddSystemsTo<HexengineColliderSystemGroup>(typeof(ColliderCollisionSystem));
				}
			}
		}

		public static void DisposeFrom(World world, bool trigger = true, bool collision = true) {
			if (trigger) {
				world.DisposeSystem<ColliderTriggerSystem>();
			}
			if (collision) {
				world.DisposeSystem<ColliderCollisionSystem>();
			}
			world.DisposeSystem<HexengineColliderSystemGroup>();
		}
	}
}