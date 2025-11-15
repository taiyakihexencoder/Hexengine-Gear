using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace com.hexengine.gear.ecs {
	public partial struct RespondInstantiateSphereRequestSystem : ISystem {
		private EntityQuery query;
		private EntityQuery prefabQuery;

		void ISystem.OnCreate(ref SystemState state) {
			query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<InstantiateSphereGeometryRequest>()
				.Build(ref state);
			state.RequireForUpdate(query);

			prefabQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithOptions(EntityQueryOptions.IncludePrefab)
				.WithAll<Prefab, GeometryObject, InstanceGeometrySetup, LocalToWorld, LocalTransform>()
				.Build(ref state);
			state.RequireForUpdate(prefabQuery);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			NativeArray<InstantiateSphereGeometryRequest> requests = query.ToComponentDataArray<InstantiateSphereGeometryRequest>(Allocator.TempJob);

			JobHandle job = new Job {
				requests = requests,
				commandBuffer = ECBUtility.Get(ref state).AsParallelWriter(),
			}.ScheduleParallel(prefabQuery, state.Dependency);

			job.Complete();
			requests.Dispose();

			state.Dependency = new DestroyEntitiesJob {
				commandBuffer = ECBUtility.Get(ref state),
			}.Schedule(query, state.Dependency);
		}

		partial struct Job : IJobEntity {
			[ReadOnly] public NativeArray<InstantiateSphereGeometryRequest> requests;
			public EntityCommandBuffer.ParallelWriter commandBuffer;

			void Execute(
				in Entity entity, 
				[EntityIndexInQuery] int sortKey, 
				RefRO<GeometryObject> prefab,
				RefRO<InstanceGeometrySetup> setup
			) {
				foreach(InstantiateSphereGeometryRequest request in requests) {
					if (prefab.ValueRO.id == request.geometryId) {
						Entity instance = commandBuffer.Instantiate(sortKey, entity);
						commandBuffer.SetComponent(
							sortKey,
							instance,
							new InstanceGeometrySetup {
								key = setup.ValueRO.key | request.instanceKey,
							}
						);
						commandBuffer.SetComponent(
							sortKey,
							instance,
							new LocalToWorld { Value = float4x4.TRS(request.position, request.rotation, new float3(1.0f,1.0f,1.0f)), }
						);
						commandBuffer.SetComponent(
							sortKey,
							instance,
							LocalTransform.FromPositionRotationScale(request.position, request.rotation, 1.0f)
						);
#if UNITY_EDITOR
						commandBuffer.SetName(sortKey, instance, new FixedString64Bytes("Sphere"));
#endif
					}
				}
			}
		}
	}
}