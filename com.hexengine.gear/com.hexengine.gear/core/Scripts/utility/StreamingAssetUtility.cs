using System.IO;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace com.hexengine.gear {
	public static class StreamingAssetUtility {
		private static char separator => Path.DirectorySeparatorChar;

#if UNITY_EDITOR
		[MenuItem("Hexengine/Open Streaming Assets Folder")]
		public static void OpenStreamingAssetsFolder() {
			EditorUtility.RevealInFinder(Application.streamingAssetsPath + separator);
		}

		public static void CreateBinaryFile(string path, System.Action<BinaryWriter> function, System.Text.Encoding encoding) {
			RuntimeHexengineGearConfig config = ScriptableObjectUtility.GetProjectSingleton<RuntimeHexengineGearConfig>();
			string savePath = $"{config.streamingAssetPath}{separator}{path}";
			CreateStreamingAssetPath(savePath);

			string absPath = Application.streamingAssetsPath + separator + savePath;
			using (FileStream stream = new FileStream(absPath, FileMode.Create, FileAccess.Write)) {
				using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.UTF8)) {
					try {
						function(writer);
						Debug.Log($"Create File:{savePath}");
					} catch (System.Exception e) {
						Debug.LogError(e);
					}
				}
			}
		}
#endif

		public static void LoadBinaryFile(string path, System.Action<BinaryReader> function, System.Text.Encoding encoding) {
			RuntimeHexengineGearConfig config = ScriptableObjectUtility.GetProjectSingleton<RuntimeHexengineGearConfig>();
			string savePath = $"{config.streamingAssetPath}{separator}{path}";

			string absPath = Application.streamingAssetsPath + separator + savePath;
			using (FileStream stream = new FileStream(absPath, FileMode.Open, FileAccess.Read)) {
				using (BinaryReader reader = new BinaryReader(stream, encoding)) {
					try {
						function(reader);
					} catch (System.Exception e) {
						Debug.LogError(e);
					}
				}
			}
		}

		private static void CreateStreamingAssetPath(string pathFromRoot) {
			string path = Application.streamingAssetsPath;
			string[] splits = pathFromRoot.Split(new char[] { '/', '\\', ':' });
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
