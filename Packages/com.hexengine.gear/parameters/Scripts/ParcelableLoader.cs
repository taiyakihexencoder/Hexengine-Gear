using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace com.hexengine.gear.parameters {
	public static class ParcelableLoader {
		private static string loadPath;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void initialize() {
			RuntimeHexencoderGearConfig config = ScriptableObjectUtility.GetProjectSingleton<RuntimeHexencoderGearConfig>();
			loadPath = Application.streamingAssetsPath +
				Path.DirectorySeparatorChar + config.streamingAssetPath +
				Path.DirectorySeparatorChar + "parameters" +
				Path.DirectorySeparatorChar + "data";
		}

		public static async Task<T> Load<T>(string path, int index, System.Text.Encoding encoding) where T : Parcelable, new() {
			int bufferSize = 16384;
			string absPath = loadPath + Path.DirectorySeparatorChar + path;
			try {
				using (FileStream stream = new FileStream(absPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true)) {
					// 要素数の取得
					byte[] countBytes = new byte[sizeof(int)];
					await stream.ReadAsync(countBytes, 0, sizeof(int));
					int count = System.BitConverter.ToInt32(countBytes);
					if (index < 0 || count <= index) {
						throw new System.Exception($"IndexOutOfRangeException: index={index}, count={count}");
					}

					// オフセット・サイズの取得
					byte[] bytes = new byte[sizeof(int)*2];
					stream.Seek(sizeof(int)*(index*2+1), SeekOrigin.Begin);
					await stream.ReadAsync(bytes, 0, bytes.Length);

					int offset = System.BitConverter.ToInt32(bytes);
					int size = System.BitConverter.ToInt32(bytes, sizeof(int));

					// データの取得
					byte[] binaryData = new byte[size];
					stream.Seek(offset, SeekOrigin.Begin);
					await stream.ReadAsync(binaryData, 0, size);
					T value = new T();
					int idx = 0;
					value.LoadFromBytes(binaryData, ref idx, encoding);
					return value;
				}
			} catch (System.Exception e) {
				Debug.LogError(e);
				return default;
			}
		}

		public static async Task<T[]> LoadAll<T>(string path, System.Text.Encoding encoding) where T : Parcelable, new() {
			int bufferSize = 16384;
			string absPath = loadPath + Path.DirectorySeparatorChar + path;
			try {
				using (FileStream stream = new FileStream(absPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true)) {
					// 要素数の取得
					byte[] countBytes = new byte[sizeof(int)];
					await stream.ReadAsync(countBytes, 0, sizeof(int));
					int count = System.BitConverter.ToInt32(countBytes);
					int[] sizes = new int[count];
					for(int i = 0; i < count; ++i) {
						byte[] sizeBytes = new byte[sizeof(int)*2];
						await stream.ReadAsync(sizeBytes, 0, sizeof(int)*2);
						sizes[i] = System.BitConverter.ToInt32(sizeBytes, sizeof(int));
					}

					int idx = 0;
					T[] values = new T[count];
					for(int i = 0; i < count; ++i) {
						byte[] data = new byte[sizes[i]];
						await stream.ReadAsync(data, 0, sizes[i]);

						idx = 0;
						values[i] = new T();
						values[i].LoadFromBytes(data, ref idx, encoding);
					}
					return values;
				}
			} catch (System.Exception e) {
				Debug.LogError(e);
				return new T[0];
			}
		}
	}
}