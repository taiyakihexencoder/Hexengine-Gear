using Unity.Collections;
using Unity.Entities;

namespace com.hexengine.gear.ecs {
	public struct CreateCapsuleGeometryPrefabRequest : IComponentData {
		public long key;
		public int geometryId;
		public FixedString64Bytes name;

		public float radius;
		public float height;

		public uint belongsTo;
		public uint collidesWith;
		public bool hasBody;
		public ColliderEventType eventType;
	}
}