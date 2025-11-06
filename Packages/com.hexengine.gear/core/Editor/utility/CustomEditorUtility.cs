using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.hexengine.gear.editor {
	public static class CustomEditorUtility {
		private static string[] assemblies;
		static CustomEditorUtility() {
			string[] guids = AssetDatabase.FindAssets($"t:{typeof(AssemblyDefinitionAsset).Name}");
			assemblies = new string[guids.Length];
			for (int i = 0; i < guids.Length; ++i) {
				assemblies[i] = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guids[i]));
			}
		}

		public static string AssemblyField(string current, string label, System.Predicate<string> predicate, params GUILayoutOption[] options) {
			int selectedIndex = 0;
			List<string> targetAssemblies = new List<string>();
			targetAssemblies.Add("-");
			foreach(string assembly in assemblies) {
				if(predicate(assembly)) {
					if (selectedIndex == 0 && current == assembly) {
						selectedIndex = targetAssemblies.Count;
					}
					targetAssemblies.Add(assembly);
				}
			}

			string[] array = targetAssemblies.ToArray();
			for(int i = 0; i < array.Length; ++i) {
				array[i] = array[i].Replace(".", "/");
			}

			selectedIndex = EditorGUILayout.Popup(label, selectedIndex, array, options);
			if (0 < selectedIndex && selectedIndex < targetAssemblies.Count) {
				return targetAssemblies[selectedIndex];
			} else if(selectedIndex == 0) {
				return "";
			} else {
				return current;
			}
		}

		public static string AssemblyField(string current, string label = "", params GUILayoutOption[] options) {
			return AssemblyField(current, label, _ => true, options);
		}
	}
}