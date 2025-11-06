using System.IO;
using UnityEditor;
using UnityEngine;

namespace com.hexengine.gear.editor {
	public static class ScriptableObjectEditorUtility {
		public static T Create<T>(string folderFromAssets) where T: ScriptableObject{
			T instance = ScriptableObject.CreateInstance<T>();
			CreateAssetsFolder(folderFromAssets);
			AssetDatabase.CreateAsset(instance, $"Assets{Path.DirectorySeparatorChar}{folderFromAssets}{Path.DirectorySeparatorChar}{typeof(T).Name}.asset");
			return instance;
		}

		public static ScriptableObject Create(System.Type type, string folderFromAssets) {
			ScriptableObject instance = ScriptableObject.CreateInstance(type);
			CreateAssetsFolder(folderFromAssets);
			AssetDatabase.CreateAsset(instance, $"Assets{Path.DirectorySeparatorChar}{folderFromAssets}{Path.DirectorySeparatorChar}{type.Name}.asset");
			return instance;
		}

		private static void CreateAssetsFolder(string pathFromAssets) {
			string path = Application.dataPath;
			string[] splits = pathFromAssets.Split(new char[] { '/', '\\', ':' });
			bool created = false;

			for (int i = 0; i < splits.Length; ++i) {
				path += $"{Path.DirectorySeparatorChar}{splits[i]}";
				if (!Directory.Exists(path)) {
					Directory.CreateDirectory(path);
					created = true;
				}
			}

			if (created) {
				Debug.Log($"Create Folder: {path}");
			}
		}
	}
}