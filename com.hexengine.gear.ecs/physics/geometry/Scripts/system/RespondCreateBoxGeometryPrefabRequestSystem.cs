using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace com.hexengine.gear.ecs {
	public partial struct RespondCreateBoxGeometryPrefabRequestSystem : ISystem {
		private EntityQuery query;

		private EntityArchetype archetype;

		void ISystem.OnCreate(ref SystemState state) {
			archetype = state.EntityManager.CreateArchetype(
				ComponentType.ReadOnly<Prefab>(),
				ComponentType.ReadOnly<PrefabGeometrySetup>(),
				ComponentType.ReadOnly<InstanceGeometrySetup>(),
				ComponentType.ReadWrite<LocalToWorld>(),
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadWrite<GeometryObject>(),
				ComponentType.ReadWrite<DisposableGeometry>(),
				ComponentType.ReadWrite<PhysicsCollider>()
			);

			query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<CreateBoxGeometryPrefabRequest>()
				.Build(ref state);
			state.RequireForUpdate(query);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			JobHandle job = new Job {
				archetype = archetype,
				commandBuffer = ECBUtility.Get(ref state).AsParallelWriter(),
			}.ScheduleParallel(query, state.Dependency);
			job.Complete();
		}

		partial struct Job : IJobEntity {
			public EntityArchetype archetype;
			public EntityCommandBuffer.ParallelWriter commandBuffer;

			void Execute(in Entity entity, [EntityIndexInQuery]int sortKey, RefRO<CreateBoxGeometryPrefabRequest> request) {
				commandBuffer.DestroyEntity(sortKey, entity);

				BlobAssetReference<Collider> geometry = BoxCollider.Create(
					new BoxGeometry {
						Center = float3.zero,
						Size = request.ValueRO.extent,
						BevelRadius = 0.0f,
						Orientation = quaternion.identity,
					},
					new CollisionFilter {
						BelongsTo = request.ValueRO.belongsTo,
						CollidesWith = request.ValueRO.collidesWith,
					},
					Material.Default
				);
				geometry.Value.SetCollisionResponse(GetResponsePolicy(request.ValueRO.hasBody, request.ValueRO.eventType));

				Entity prefab = commandBuffer.CreateEntity(sortKey, archetype);
#if UNITY_EDITOR
				commandBuffer.SetName(sortKey, prefab, request.ValueRO.name);
#endif
				commandBuffer.SetComponent(sortKey, prefab, new PrefabGeometrySetup{ key = request.ValueRO.key, });
				commandBuffer.SetComponent(sortKey, prefab, new InstanceGeometrySetup{ key = request.ValueRO.key, });
				commandBuffer.SetComponent(sortKey, prefab, new LocalToWorld { Value = float4x4.identity, });
				commandBuffer.SetComponent(sortKey, prefab, LocalTransform.Identity);
				commandBuffer.SetComponent(sortKey, prefab, new DisposableGeometry { geometry = geometry, } );
				commandBuffer.SetComponent(sortKey, prefab, new PhysicsCollider { Value = geometry, });
				commandBuffer.SetComponent(sortKey, prefab, new GeometryObject { id = request.ValueRO.geometryId, });
				commandBuffer.AddSharedComponent(sortKey, prefab, new PhysicsWorldIndex { Value = 0, });

				if (request.ValueRO.hasBody) {
					ColliderEventType type = request.ValueRO.eventType;
					if (type != ColliderEventType.None) {
						commandBuffer.AddBuffer<ColliderCollisionEvent>(sortKey, prefab);
						if (type.HasFlag(ColliderEventType.Enter)) {
							commandBuffer.AddBuffer<ColliderCollisionEnterEvent>(sortKey, prefab);
						}
						if (type.HasFlag(ColliderEventType.Exit)) {
							commandBuffer.AddBuffer<ColliderCollisionExitEvent>(sortKey, prefab);
						}
						if (type.HasFlag(ColliderEventType.Stay)) {
							commandBuffer.AddBuffer<ColliderCollisionStayEvent>(sortKey, prefab);
						}
					}
				} else {
					ColliderEventType type = request.ValueRO.eventType;
					if (type != ColliderEventType.None) {
						commandBuffer.AddBuffer<ColliderTriggerEvent>(sortKey, prefab);
						if (type.HasFlag(ColliderEventType.Enter)) {
							commandBuffer.AddBuffer<ColliderTriggerEnterEvent>(sortKey, prefab);
						}
						if (type.HasFlag(ColliderEventType.Exit)) {
							commandBuffer.AddBuffer<ColliderTriggerExitEvent>(sortKey, prefab);
						}
						if (type.HasFlag(ColliderEventType.Stay)) {
							commandBuffer.AddBuffer<ColliderTriggerStayEvent>(sortKey, prefab);
						}
					}
				}
			}

			private CollisionResponsePolicy GetResponsePolicy(
				bool hasBody,
				ColliderEventType eventType
			) {
				if (hasBody) {
					return eventType == ColliderEventType.None 
						? CollisionResponsePolicy.Collide 
						: CollisionResponsePolicy.CollideRaiseCollisionEvents;
				} else {
					return eventType == ColliderEventType.None
						? CollisionResponsePolicy.None
						: CollisionResponsePolicy.RaiseTriggerEvents;
				}
			}
		}
	}
}