using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace com.hexengine.gear.ecs {
	public struct ColliderCollisionEvent : IBufferElementData, ISimulationEvent<ColliderCollisionEvent> {
		public Entity EntityA { get; private set; }
		public Entity EntityB { get; private set; }
		public int BodyIndexA { get; private set; }
		public int BodyIndexB { get; private set; }
		public ColliderKey ColliderKeyA { get; private set; }
		public ColliderKey ColliderKeyB { get; private set; }

		public float3 Normal { get; private set; }

		public ColliderCollisionEvent(
			Entity entityA,
			int bodyIndexA,
			ColliderKey colliderKeyA,
			Entity entityB,
			int bodyIndexB,
			ColliderKey colliderKeyB,
			float3 normal
		) {
			EntityA = entityA;
			EntityB = entityB;
			BodyIndexA = bodyIndexA;
			BodyIndexB = bodyIndexB;
			ColliderKeyA = colliderKeyA;
			ColliderKeyB = colliderKeyB;
			Normal = normal;
		}

		public int CompareTo(ColliderCollisionEvent other) {
			return ISimulationEventUtilities.CompareEvents(this, other);
		}
	}
}