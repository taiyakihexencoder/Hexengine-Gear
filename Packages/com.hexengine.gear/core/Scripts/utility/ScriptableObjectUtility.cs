using UnityEditor;
using UnityEngine;

namespace com.hexengine.gear {
	public static class ScriptableObjectUtility {
		public static T GetProjectSingleton<T>() where T : ScriptableObject {
			foreach(string guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}")) {
				return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
			}
			return null;
		}

		public static ScriptableObject GetProjectSingleton(System.Type type) {
			foreach(string guid in AssetDatabase.FindAssets($"t:{type.Name}")) {
				return AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid));
			}
			return null;
		}
	}
}