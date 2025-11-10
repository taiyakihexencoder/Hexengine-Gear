using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace com.hexengine.gear.ecs {
	public static class ColliderCreateUtility {
		private const int CYLINDER_SIDE_COUNT = 16;

		public static Entity Sphere(
			EntityCommandBuffer commandBuffer,
			float radius,
			uint belongsTo,
			uint collidesWith,
			bool hasBody,
			ColliderEventType eventType
		) {
			BlobAssetReference<Collider> geometry = SphereCollider.Create(
				new SphereGeometry {
					Center = float3.zero,
					Radius = radius,
				},
				new CollisionFilter{
					BelongsTo = belongsTo,
					CollidesWith = collidesWith,
				},
				Material.Default
			);
			geometry.Value.SetCollisionResponse(GetResponsePolicy(hasBody, eventType));
			return Primitive(commandBuffer, geometry, hasBody, eventType);
		}

		public static Entity Box(
			EntityCommandBuffer commandBuffer,
			float3 extent,
			uint belongsTo,
			uint collidesWith,
			bool hasBody,
			ColliderEventType eventType
		) {
			BlobAssetReference<Collider> geometry = BoxCollider.Create(
				new BoxGeometry {
					Center = float3.zero,
					Orientation = quaternion.identity,
					Size = extent,
					BevelRadius = 0.0f
				},
				new CollisionFilter{
					BelongsTo = belongsTo,
					CollidesWith = collidesWith,
				},
				Material.Default
			);
			geometry.Value.SetCollisionResponse(GetResponsePolicy(hasBody, eventType));

			return Primitive(commandBuffer, geometry, hasBody, eventType);
		}

		public static Entity Cylinder(
			EntityCommandBuffer commandBuffer,
			float radius,
			float height,
			uint belongsTo,
			uint collidesWith,
			bool hasBody,
			ColliderEventType eventType
		) {
			BlobAssetReference<Collider> geometry = CylinderCollider.Create(
				new CylinderGeometry {
					Center = float3.zero,
					Orientation = quaternion.identity,
					BevelRadius = 0.0f,
					Radius = radius,
					Height = height,
					SideCount = CYLINDER_SIDE_COUNT
				},
				new CollisionFilter{
					BelongsTo = belongsTo,
					CollidesWith = collidesWith,
				},
				Material.Default
			);
			geometry.Value.SetCollisionResponse(GetResponsePolicy(hasBody, eventType));

			return Primitive(commandBuffer, geometry, hasBody, eventType);
		}

		private static CollisionResponsePolicy GetResponsePolicy(
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

		private static Entity Primitive(
			EntityCommandBuffer commandBuffer,
			BlobAssetReference<Collider> geometry,
			bool hasBody,
			ColliderEventType eventType
		) {
			Entity entity = commandBuffer.CreateEntity();

			commandBuffer.AddComponent(
				entity,
				new Prefab { }
			);

			commandBuffer.AddComponent(
				entity,
				new LocalToWorld { Value = float4x4.identity, }
			);

			commandBuffer.AddComponent(
				entity,
				LocalTransform.Identity
			);

			commandBuffer.AddSharedComponent(
				entity,
				new PhysicsWorldIndex { Value = 0, }
			);

			commandBuffer.AddComponent(
				entity,
				new DisposableGeometry { geometry = geometry, }
			);

			commandBuffer.AddComponent(
				entity,
				new PhysicsCollider { Value = geometry, }
			);

			AddEvent(commandBuffer, entity, hasBody, eventType);

			return entity;
		}

		private static void AddEvent(
			EntityCommandBuffer commandBuffer,
			Entity entity,
			bool hasBody,
			ColliderEventType eventType
		) {
			if (hasBody) {
				if(eventType != ColliderEventType.None) {
					commandBuffer.AddBuffer<ColliderCollisionEvent>(entity);
					if (eventType.HasFlag(ColliderEventType.Enter)) {
						commandBuffer.AddBuffer<ColliderCollisionEnterEvent>(entity);
					}
					if (eventType.HasFlag(ColliderEventType.Exit)) {
						commandBuffer.AddBuffer<ColliderCollisionExitEvent>(entity);
					}
					if (eventType.HasFlag(ColliderEventType.Stay)) {
						commandBuffer.AddBuffer<ColliderCollisionStayEvent>(entity);
					}
				}
			} else {
				if(eventType != ColliderEventType.None) {
					commandBuffer.AddBuffer<ColliderTriggerEvent>(entity);
					if (eventType.HasFlag(ColliderEventType.Enter)) {
						commandBuffer.AddBuffer<ColliderTriggerEnterEvent>(entity);
					}
					if (eventType.HasFlag(ColliderEventType.Exit)) {
						commandBuffer.AddBuffer<ColliderTriggerExitEvent>(entity);
					}
					if (eventType.HasFlag(ColliderEventType.Stay)) {
						commandBuffer.AddBuffer<ColliderTriggerStayEvent>(entity);
					}
				}
			}
		}
	}
}