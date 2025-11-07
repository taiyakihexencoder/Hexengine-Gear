using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.hexengine.gear.editor {
	public static class HexengineProject {
		private static char separator => Path.DirectorySeparatorChar;

		[InitializeOnLoadMethod]
		private static void InitializeAssets() {
			HexengineGearConfig instance = ScriptableObjectUtility.GetProjectSingleton<HexengineGearConfig>();
			if (instance == null) {
				instance = ScriptableObject.CreateInstance<HexengineGearConfig>();
				instance = ScriptableObjectEditorUtility.Create<HexengineGearConfig>(instance.autoGeneratePath);
			}

			if(!ScriptableObjectUtility.Exists<RuntimeHexengineGearConfig>()) {
				ScriptableObjectEditorUtility.Create<RuntimeHexengineGearConfig>(instance.autoGeneratePath);
			}
		}

		public static void CreateAsset(Object asset, string path) {
			HexengineGearConfig config = ScriptableObjectUtility.GetProjectSingleton<HexengineGearConfig>();
			string savePath = config.autoGeneratePath + separator + path;
			CreateAssetsFolder(savePath);
			AssetDatabase.CreateAsset(asset, $"Assets{separator}{savePath}");
			Debug.Log($"Create Asset:{savePath}");
		}

		public static void CreateTextFile(string path, System.Action<StreamWriter> function) {
			HexengineGearConfig config = ScriptableObjectUtility.GetProjectSingleton<HexengineGearConfig>();
			string savePath = $"{config.autoGeneratePath}{separator}{path}";
			CreateAssetsFolder(savePath);

			string absPath = Application.dataPath + separator + savePath;
			using (FileStream stream = new FileStream(absPath, FileMode.Create, FileAccess.Write)) {
				using (StreamWriter writer = new StreamWriter(stream, System.Text.Encoding.UTF8)) {
					try {
						function(writer);
						Debug.Log($"Create File:{savePath}");
					} catch (System.Exception e) {
						EditorUtility.DisplayDialog("Error", e.Message, "Close");
						Debug.LogError(e);
					}
				}
			}
			AssetDatabase.ImportAsset($"Assets{Path.DirectorySeparatorChar}{savePath}");
		}

		public static void CreateBinaryFile(string path, System.Action<BinaryWriter> function) {
			HexengineGearConfig config = ScriptableObjectUtility.GetProjectSingleton<HexengineGearConfig>();
			string savePath = $"{config.autoGeneratePath}{separator}{path}";
			CreateAssetsFolder(savePath);

			string absPath = Application.dataPath + separator + savePath;
			using (FileStream stream = new FileStream(absPath, FileMode.Create, FileAccess.Write)) {
				using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.UTF8)) {
					try {
						function(writer);
						Debug.Log($"Create File:{savePath}");
					} catch (System.Exception e) {
						EditorUtility.DisplayDialog("Error", e.Message, "Close");
						Debug.LogError(e);
					}
				}
			}
			AssetDatabase.ImportAsset($"Assets{Path.DirectorySeparatorChar}{savePath}");
		}

		public static void CreateAssemblyReference(string assemblyName, string path) {
			CreateTextFile(
				path, 
				writer => {
					string[] guids = AssetDatabase.FindAssets($"t:{typeof(AssemblyDefinitionAsset).Name}");
					foreach(string guid in guids) {
						string assetPath = AssetDatabase.GUIDToAssetPath(guid);
						AssemblyDefinitionAsset asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assetPath);
						Match match = Regex.Match(asset.text, $"\"name\": \"{assemblyName}\",");
						if(match.Success) {
							writer.WriteLine($"{{");
							writer.WriteLine($"\t\"reference\": \"GUID:{guid}\"");
							writer.WriteLine($"}}");
							return;
						}
					}
					throw new System.Exception($"Failed to Get Assembly:{assemblyName}.");
				}
			);
			
		}
		private static void CreateAssetsFolder(string pathFromAssets) {
			string path = Application.dataPath;
			string[] splits = pathFromAssets.Split(new char[] { '/', '\\', ':' });
			bool created = false;

			for (int i = 0; i < splits.Length; ++i) {
				// 末尾に拡張子があると思われる場合は無視する
				if(i != splits.Length-1 || splits[i].LastIndexOf('.') == -1) {
					path += $"{separator}{splits[i]}";
					if (!Directory.Exists(path)) {
						Directory.CreateDirectory(path);
						created = true;
					}
				}
			}

			if (created) {
				Debug.Log($"Create Folder: {path}");
			}
		}
	}
}