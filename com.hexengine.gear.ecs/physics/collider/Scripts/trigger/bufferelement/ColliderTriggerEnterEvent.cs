using Unity.Entities;
using Unity.Physics;

namespace com.hexengine.gear.ecs {
	public struct ColliderTriggerEnterEvent : IBufferElementData {
		public Entity Self { get; private set; }
		public Entity Other { get; private set; }
		public int SelfIndex { get; private set; }
		public int OtherIndex { get; private set; }
		public ColliderKey SelfColliderKey { get; private set; }
		public ColliderKey OtherColliderKey { get; private set; }

		public ColliderTriggerEnterEvent(ColliderTriggerEvent evt, Entity other) {
			if (other == evt.EntityA)
			{
				Self = evt.EntityB;
				SelfIndex = evt.BodyIndexB;
				SelfColliderKey = evt.ColliderKeyB;
				Other = evt.EntityA;
				OtherIndex = evt.BodyIndexA;
				OtherColliderKey = evt.ColliderKeyA;
			}
			else
			{
				Self = evt.EntityA;
				SelfIndex = evt.BodyIndexA;
				SelfColliderKey = evt.ColliderKeyA;
				Other = evt.EntityB;
				OtherIndex = evt.BodyIndexB;
				OtherColliderKey = evt.ColliderKeyB;
			}
		}
	}
}