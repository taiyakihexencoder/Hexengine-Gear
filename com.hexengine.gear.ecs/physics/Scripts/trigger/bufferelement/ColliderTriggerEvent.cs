using Unity.Entities;
using Unity.Physics;

namespace com.hexengine.gear.ecs {
	public struct ColliderTriggerEvent : IBufferElementData, ISimulationEvent<ColliderTriggerEvent> {
	public Entity EntityA { get; private set; }
	public Entity EntityB { get; private set; }
	public int BodyIndexA { get; private set; }
	public int BodyIndexB { get; private set; }
	public ColliderKey ColliderKeyA{ get; private set; }
	public ColliderKey ColliderKeyB { get; private set; }

	public ColliderTriggerEvent(
		Entity entityA,
		int bodyIndexA,
		ColliderKey colliderKeyA,
		Entity entityB,
		int bodyIndexB,
		ColliderKey colliderKeyB
	) {
		EntityA = entityA;
		EntityB = entityB;
		BodyIndexA = bodyIndexA;
		BodyIndexB = bodyIndexB;
		ColliderKeyA = colliderKeyA;
		ColliderKeyB = colliderKeyB;
	}

	public int CompareTo(ColliderTriggerEvent other) {
		return ISimulationEventUtilities.CompareEvents(this, other);
	}
}
}