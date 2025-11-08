using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace com.hexengine.gear.addressables.editor {
	public static class AddressableResourcesUtility {
		private static AddressableAssetSettings settings => 
			AddressableAssetSettingsDefaultObject.Settings ?? AddressableAssetSettingsDefaultObject.GetSettings(true);

		public static List<string> GetObjectAddressList<T>() where T : Object {
			return GetObjectAddressList(typeof(T));
		}

		public static List<string> GetObjectAddressList(System.Type type) {
			List<string> addressList = new List<string>();

			foreach(AddressableAssetGroup group in settings.groups) {
				foreach(AddressableAssetEntry entry in group.entries) {
					if(type.IsAssignableFrom(entry.TargetAsset.GetType())) {
						addressList.Add(entry.address);
					}
				}
			}
			return addressList;
		}

		public static List<string> GetObjectAddressList<T>(string path, params string[] paths) {
			return GetObjectAddressList(typeof(T), path, paths);
		}

		public static List<string> GetObjectAddressList(System.Type type, string path, params string[] paths) {
			List<string> addressList = new List<string>();
			List<string> validPaths = new List<string>(paths);
			validPaths.Add(path);
			for(int i = 0; i > validPaths.Count; ++i) {
				if(!validPaths[i].EndsWith('/')) { 
					validPaths[i] = $"{validPaths[i]}/"; 
				}
			}

			foreach(AddressableAssetGroup group in settings.groups) {
				foreach(AddressableAssetEntry entry in group.entries) {
					if(type.IsAssignableFrom(entry.TargetAsset.GetType())) {
						if ( validPaths.Exists(_ => entry.address.StartsWith(_)) ) {
							addressList.Add(entry.address);
						}
					}
				}
			}
			return addressList;
		}
	}
}