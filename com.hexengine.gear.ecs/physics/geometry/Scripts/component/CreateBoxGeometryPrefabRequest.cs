using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace com.hexengine.gear.ecs {
	public struct CreateBoxGeometryPrefabRequest : IComponentData {
		public long key;
		public int geometryId;
		public FixedString64Bytes name;

		public float3 extent;

		public uint belongsTo;
		public uint collidesWith;
		public bool hasBody;
		public ColliderEventType eventType;
	}
}