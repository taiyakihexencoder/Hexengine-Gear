using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace com.hexengine.gear.addressables {
	public static class AddressableResourceLoader {
		private static SynchronizationContext mainContext;

		private static Dictionary<string, GameObject> loadedModel = new Dictionary<string, GameObject>();
		private static List<string> loadingModel = new List<string>();

		private static Dictionary<string, AnimationClip> loadedAnimationClip = new Dictionary<string, AnimationClip>();
		private static List<string> loadingAnimationClip = new List<string>();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Initialize() {
			mainContext = SynchronizationContext.Current;
		}

		public static bool IsModelLoaded(string address) { return loadedModel.ContainsKey(address); }
		public static bool IsAnimationClipLoaded(string address) { return loadedAnimationClip.ContainsKey(address); }

		public static bool TryGetValue(string address, out GameObject value) { return loadedModel.TryGetValue(address, out value); }
		public static bool TryGetValue(string address, out AnimationClip value) { return loadedAnimationClip.TryGetValue(address, out value); }

		public static void LoadAnimationClips(IEnumerable<string> addressList, System.Action<AnimationClip[]> onCompleted = null) {
			List<string> loadList = CreateLoadingList(addressList, loadingAnimationClip, loadedAnimationClip);
			if (loadList.Count > 0) {
				_ = Task.Run(
					async () => {
						await LoadAsync<AnimationClip>(
							addressList: loadList,
							loaded: loadedAnimationClip.Add,
							completed: list => {
								foreach (string address in loadList) {
									loadingAnimationClip.Remove(address);
								}
								onCompleted?.Invoke(list);
							}
						);
					}
				);
			}
		}
		
		public static async Task LoadAnimationClipsAsync(IEnumerable<string> addressList, System.Action<AnimationClip[]> onCompleted = null) {
			List<string> loadList = CreateLoadingList(addressList, loadingAnimationClip, loadedAnimationClip);
			await LoadAsync<AnimationClip>(
				addressList: loadList,
				loaded: loadedAnimationClip.Add,
				completed: list => {
					foreach (string address in loadList) {
						loadingAnimationClip.Remove(address);
					}
					onCompleted?.Invoke(list);
				}
			);
		}


		public static void LoadModels(IEnumerable<string> addressList, System.Action<GameObject[]> onCompleted = null) {
			List<string> loadList = CreateLoadingList(addressList, loadingModel, loadedModel);

			if(loadList.Count > 0) {
				_ = Task.Run(
					async () => {
						await LoadAsync<GameObject>(
							addressList: loadList,
							loaded: loadedModel.Add,
							completed: list => {
								foreach (string address in loadList) {
									loadingModel.Remove(address);
								}
								onCompleted?.Invoke(list);
							}
						);
					}
				);
			}
		}

		public static async Task LoadModelsAsync(IEnumerable<string> addressList, System.Action<GameObject[]> onCompleted = null) {
			List<string> loadList = CreateLoadingList(addressList, loadingModel, loadedModel);
			await LoadAsync<GameObject>(
				addressList: loadList,
				loaded: loadedModel.Add,
				completed: list => {
					foreach (string address in loadList)
					{
						loadingModel.Remove(address);
					}
					onCompleted?.Invoke(list);
				}
			);
		}

		private static List<string> CreateLoadingList<T>(in IEnumerable<string> list, List<string> loading, Dictionary<string, T> loaded) where T: Object{
			List<string> loadList = new List<string>();
			foreach(string address in list) {
				if(! loading.Contains(address) && !loaded.ContainsKey(address)) {
					loadList.Add(address);
					loading.Add(address);
				}
			}
			return loadList;
		}

		private static async Task LoadAsync<T>(
			IList<string> addressList, 
			System.Action<string, T> loaded, 
			System.Action<T[]> completed
		) where T : Object {
			AsyncOperationHandle<T>[] ops = new AsyncOperationHandle<T>[addressList.Count];
			for (int i = 0; i < addressList.Count; ++i) {
				int index = i;
				mainContext.Send(
					_ => ops[index] = Addressables.LoadAssetAsync<T>(addressList[index]),
					null
				);
			}

			T[] resultList = new T[addressList.Count];
			for (int i = 0; i < addressList.Count; ++i) {
				int index = i;
				T result = await ops[index].Task;
				mainContext.Post(_ => loaded(addressList[index], result), null);
			}
			mainContext.Post( _ => completed(resultList), null);
		}

		public static void UnloadModel(string address) {
			if (IsModelLoaded(address)) {
				Addressables.Release(loadedModel[address]);
				loadedModel.Remove(address);
			}
		}

		public static void UnloadAnimationClip(string address) {
			if (IsAnimationClipLoaded(address)) {
				Addressables.Release(loadedAnimationClip[address]);
				loadedAnimationClip.Remove(address);
			}
		}
	}
}