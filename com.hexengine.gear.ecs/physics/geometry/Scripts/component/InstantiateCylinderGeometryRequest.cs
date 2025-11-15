using Unity.Entities;
using Unity.Mathematics;

namespace com.hexengine.gear.ecs {
	public struct InstantiateCylinderGeometryRequest : IComponentData {
		public long instanceKey;
		public int geometryId;

		public float3 position;
		public quaternion rotation;
	}
}