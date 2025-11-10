using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace com.hexengine.gear.ecs {
	public partial struct DisposableGeometrySystem : ISystem {
		private EntityQuery destroyQuery;

		void ISystem.OnCreate(ref SystemState state) {
			destroyQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAllRW<DisposableGeometry>()
				.WithOptions(EntityQueryOptions.IncludePrefab)
				.Build(ref state);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			
		}

		void ISystem.OnDestroy(ref SystemState state) {
			NativeArray<Entity> entities = destroyQuery.ToEntityArray(Allocator.Temp);
			NativeArray<DisposableGeometry> components = destroyQuery.ToComponentDataArray<DisposableGeometry>(Allocator.Temp);

			foreach(DisposableGeometry component in components) {
				if (component.geometry.IsCreated) {
					component.geometry.Dispose();
				}
			}
			components.Dispose();

			EntityCommandBuffer commandBuffer = ECBUtility.Get(ref state);
			foreach(Entity entity in entities) {
				commandBuffer.RemoveComponent<DisposableGeometry>(entity);
			}
		}
	}
}