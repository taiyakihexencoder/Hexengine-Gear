using Unity.Collections;
using Unity.Entities;

namespace com.hexengine.gear.ecs {
	public class EntityObserver<T1> 
		where T1: unmanaged, IComponentData {
		private EntityQuery query;

		public EntityObserver() {
			EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			EntityQueryBuilder eqb = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<T1>();

			query = eqb.Build(entityManager);
			eqb.Dispose();
		}

		public void Observe(System.Action<Entity, T1> action) {
			NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
			NativeArray<T1> t1 = query.ToComponentDataArray<T1>(Allocator.Temp);
			for(int i = 0; i < entities.Length; ++i) {
				action(entities[i], t1[i]);
			}
			entities.Dispose();
			t1.Dispose();
		}
	}

	public class EntityObserver<T1, T2> 
		where T1: unmanaged, IComponentData
		where T2: unmanaged, IComponentData {
		private EntityQuery query;

		public EntityObserver() {
			EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			EntityQueryBuilder eqb = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<T1, T2>();

			query = eqb.Build(entityManager);
			eqb.Dispose();
		}

		public void Observe(System.Action<Entity, T1, T2> action) {
			NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
			NativeArray<T1> t1 = query.ToComponentDataArray<T1>(Allocator.Temp);
			NativeArray<T2> t2 = query.ToComponentDataArray<T2>(Allocator.Temp);
			for (int i = 0; i < entities.Length; ++i) {
				action(entities[i], t1[i], t2[i]);
			}
			entities.Dispose();
			t1.Dispose();
			t2.Dispose();
		}
	}

}