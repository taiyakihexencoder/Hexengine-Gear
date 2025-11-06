using System.Collections.Generic;
using System.IO;
using System.Reflection;
using com.hexengine.gear.editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.hexengine.gear.parameters.editor {
	public sealed class HexengineGearConfigComponent : IHexengineGearConfigComponent {
		private List<System.Type> extendedTypes;
		private List<bool> flags;

		private string[] assemblies;
		private int selectedAssemblies;
		private int selectedEditorAssemblies;

		private GUIContent[] assemblyNames {
			get {
				GUIContent[] list = new GUIContent[assemblies.Length];
				for(int i = 0; i < assemblies.Length; ++i) {
					list[i] = new GUIContent(assemblies[i].Replace('.', '/'));
				}
				return list;
			}
		}

		public HexengineGearConfigComponent() {
			extendedTypes = new List<System.Type>();
			flags = new List<bool>();
			foreach(Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
				foreach(System.Type type in assembly.GetTypes()) {
					if(
						!type.IsAbstract && 
						!type.IsInterface &&
						!type.IsNested &&
						(type.IsClass || (!type.IsPrimitive && type.IsValueType && !type.IsEnum)) &&
						type.GetCustomAttribute<Parcelize>() != null &&
						type.GetCustomAttribute<System.SerializableAttribute>() != null
					) {
						extendedTypes.Add(type);
						flags.Add(false);
					}
				}
			}

			string[] guids = AssetDatabase.FindAssets($"t:{typeof(AssemblyDefinitionAsset).Name}");
			assemblies = new string[guids.Length+1];
			assemblies[0] = "-";
			for(int i = 0; i < guids.Length; ++i) {
				assemblies[i+1] = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guids[i]));
			}

			HexengineGearConfig config = ScriptableObjectUtility.GetProjectSingleton<HexengineGearConfig>();
			string lastSelected = config.lastSelectedAsmdef;
			string lastSelectedEditor = config.lastSelectedEditorAsmDef;
			for (int i = 0; i < assemblies.Length; ++i) {
				if(assemblies[i] == lastSelected) {
					selectedAssemblies = i;
				}

				if(assemblies[i] == lastSelectedEditor) {
					selectedEditorAssemblies = i;
				}
			}
		}

		void IHexengineGearConfigComponent.OnGUI() {
			int row = 4;
			float width = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true)).width * 0.25f;
			for(int i = 0; i < extendedTypes.Count; i += row) {
				using( new EditorGUILayout.HorizontalScope()) {
					for(int j = 0; j < row; ++j) {
						if(i + j < extendedTypes.Count) {
							GUILayout.FlexibleSpace();
							CheckboxElements(i + j, width);
							GUILayout.FlexibleSpace();
						} else {
							GUILayout.FlexibleSpace();
							GUILayout.Space(130.0f);
							GUILayout.FlexibleSpace();
						}
					}
				}
			}

			using(new EditorGUILayout.VerticalScope()) {
				using (new EditorGUI.DisabledScope(!flags.Find(_ => _))) {
					EditorGUILayout.LabelField("Assembly Reference:", GUILayout.Width(120.0f));
					SerializedObject obj = new SerializedObject(ScriptableObjectUtility.GetProjectSingleton<HexengineGearConfig>());

					GUIContent[] names = assemblyNames;
					int selectAsm = EditorGUILayout.Popup(selectedAssemblies, names, GUILayout.Width(160.0f));
					if (selectAsm != selectedAssemblies) {
						if(selectAsm < 0) { selectAsm = 0; }

						obj.FindProperty("_lastSelectedAsmDef").stringValue = assemblies[selectAsm];
						selectedAssemblies = selectAsm;
					}

					EditorGUILayout.Space(16);

					EditorGUILayout.LabelField("Edit Assembly Reference:", GUILayout.Width(150.0f));
					int selectEditorAsm = EditorGUILayout.Popup(selectedEditorAssemblies, names, GUILayout.Width(160.0f));
					if (selectEditorAsm != selectedEditorAssemblies) {
						if(selectEditorAsm < 0) { selectEditorAsm = 0; }

						obj.FindProperty("_lastSelectedEditorAsmDef").stringValue = assemblies[selectEditorAsm];
						selectedEditorAssemblies = selectEditorAsm;
					}
					obj.ApplyModifiedProperties();

					EditorGUILayout.Space(24);

					if (GUILayout.Button(new GUIContent("Generate"), GUILayout.Width(160.0f))) {
						for (int i = 0; i < extendedTypes.Count; ++i) {
							if (flags[i]) {
								ParcelableGenerator.Generate(extendedTypes[i]);

								if(
									extendedTypes[i].GetCustomAttribute<Parcelize>() is Parcelize parcelize &&
									parcelize.createTable
								) {
									ParcelableGenerator.GenerateTable(extendedTypes[i]);
								}
								flags[i] = false;
							}
						}
						if (selectedAssemblies > 0) {
							HexengineProject.CreateAssemblyReference(
								assemblies[selectedAssemblies],
								$"parameters{Path.DirectorySeparatorChar}Scripts{Path.DirectorySeparatorChar}parcelables{Path.DirectorySeparatorChar}{assemblies[selectedAssemblies]}.asmref"
							);
						}

						if (selectedEditorAssemblies > 0) {
							HexengineProject.CreateAssemblyReference(
								assemblies[selectedEditorAssemblies],
								$"parameters{Path.DirectorySeparatorChar}Editor{Path.DirectorySeparatorChar}tables{Path.DirectorySeparatorChar}{assemblies[selectedEditorAssemblies]}.asmref"
							);
						}
					}
				}
			}
		}

		private void CheckboxElements(int index, float width) {
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			flags[index] = EditorGUILayout.Toggle(flags[index], GUILayout.Width(30.0f));
			GUIStyle style = new GUIStyle(GUI.skin.label);
			style.wordWrap = false;
			Rect labelRect = EditorGUILayout.GetControlRect();
			if(Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition)) {
				if(Event.current.button == 0) {
					flags[index] = !flags[index];
				}
			}
			EditorGUI.LabelField(labelRect, new GUIContent(extendedTypes[index].Name, extendedTypes[index].FullName), style);
			EditorGUILayout.EndHorizontal();
		}
	}
}