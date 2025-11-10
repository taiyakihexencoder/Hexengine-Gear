using Unity.Entities;
using Unity.Physics;

namespace com.hexengine.gear.ecs {
	/// <summary>
	/// ICleanupComponentDataはPrefabで複製されない
	/// </summary>
	public struct DisposableGeometry : ICleanupComponentData {
		public BlobAssetReference<Collider> geometry;
	}
}