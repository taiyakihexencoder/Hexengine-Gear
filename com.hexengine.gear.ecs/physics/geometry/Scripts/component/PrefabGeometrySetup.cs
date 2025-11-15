using Unity.Entities;

namespace com.hexengine.gear.ecs {
	/// <summary>
	/// Prefab生成時に追加で設定したいときは
	/// このコンポーネントでQueryする
	/// Prefabを検索するのでWithOptions(EntityQueryOptions.IncludePrefab)が必要
	/// </summary>
	public struct PrefabGeometrySetup : IComponentData {
		public long key;
	}
}