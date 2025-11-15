using UnityEngine;

namespace com.hexengine.gear.ecs {
	[System.AttributeUsage(System.AttributeTargets.Method)]
	public sealed class GeometryPrimitiveAttribute : PropertyAttribute {
		public readonly string uuid;

		public GeometryPrimitiveAttribute(string uuid) {
			this.uuid = uuid;
		}
	}
}
