using Unity.Mathematics;

namespace com.hexengine.gear.ecs {
	public static class Vector {
		public static float3 left => new float3(-1.0f, 0.0f, 0.0f);
		public static float3 right => new float3(1.0f, 0.0f, 0.0f);
		public static float3 up => new float3(0.0f, 1.0f, 0.0f);
		public static float3 down => new float3(0.0f, -1.0f, 0.0f);
		public static float3 forward => new float3(0.0f, 0.0f, 1.0f);
		public static float3 back => new float3(0.0f, 0.0f, -1.0f);
	}
}