using Unity.Entities;
using UnityEngine;

namespace com.hexengine.gear.ecs {
	public static class WorldExtensions {
		public static void AddSystemsTo<T>(this World world, params System.Type[] systems) 
			where T: ComponentSystemGroup {
			if (world.GetExistingSystemManaged(typeof(T)) is ComponentSystemGroup csg) {
				foreach(System.Type system in systems) {
					csg.AddSystemToUpdateList(world.CreateSystem(system));
				}
			} else {
				Debug.LogError($"System not found: {typeof(T).Name}");
			}
		}

		public static void DisposeSystem<T>(this World world) {
			System.Type group = typeof(ComponentSystemGroup);
			if (!group.IsAssignableFrom(typeof(T))) {
				SystemHandle handle = world.GetExistingSystem(typeof(T));
				if (handle != SystemHandle.Null) {
					world.DestroySystem(handle);
				}
			}
		}

		public static void DisposeSystems(this World world, params System.Type[] systems) {
			System.Type group = typeof(ComponentSystemGroup);
			foreach(System.Type system in systems) {
				if (!group.IsAssignableFrom(system)) {
					SystemHandle handle = world.GetExistingSystem(system);
					if (handle != SystemHandle.Null) {
						world.DestroySystem(handle);
					}
				}
			}
		}
	}
}