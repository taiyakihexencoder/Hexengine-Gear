using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace com.hexengine.gear.editor {
	public sealed class HexengineGearConfigProvider : SettingsProvider {
		private HexengineGearConfig config;
		private RuntimeHexencoderGearConfig runtimeConfig;

		private int selectedTab = -1;
		private IHexengineGearConfigComponent currentComponent = null;
		private List<System.Type> components;

		private Vector2 contentsScroll;

		public GUIContent[] tabNames {
			get {
				GUIContent[] contents = new GUIContent[components.Count];
				string pattern = "com\\.hexengine\\.gear\\.(.+)\\.editor";
				string replacement = "$1";
				for(int i = 0; i < contents.Length; ++i) {
					contents[i] = new GUIContent(Regex.Replace(components[i].Namespace, pattern, replacement).ToUpper());
				}
				return contents;
			}
		}

		public HexengineGearConfigProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords) {
			config = ScriptableObjectUtility.GetProjectSingleton<HexengineGearConfig>();
			runtimeConfig = ScriptableObjectUtility.GetProjectSingleton<RuntimeHexencoderGearConfig>();

			components = new List<System.Type>();
			foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
				foreach (System.Type type in assembly.GetTypes()) {
					if(
						!type.IsAbstract && !type.IsInterface &&
						typeof(IHexengineGearConfigComponent).IsAssignableFrom(type)
					) {
						components.Add(type);
					}
				}
			}
		}

		[SettingsProvider]
		public static SettingsProvider CreateSettingsProvider() {
			return new HexengineGearConfigProvider("Project/Hexengine Gear", SettingsScope.Project, new string[]{ "Hexengine" });
		}

		public override void OnGUI(string searchContext)
		{
			SerializedObject runtimeSerializedObject = new SerializedObject(runtimeConfig);

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.LabelField(new GUIContent("Runtime"));
			GUIRuntimeStreamingAssetPath(runtimeSerializedObject);

			if (EditorGUI.EndChangeCheck()) {
				runtimeSerializedObject.ApplyModifiedProperties();
			}

			SerializedObject serializedObject = new SerializedObject(config);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.LabelField(new GUIContent("Editor"));
			GUIAutoGeneratePath(serializedObject);
			GUIEncodingType(serializedObject);

			using (new EditorGUILayout.HorizontalScope()) {
				GUILayout.FlexibleSpace();
				int selected = GUILayout.Toolbar(selectedTab, tabNames, GUILayout.ExpandWidth(true));
				if(selected != selectedTab) {
					selectedTab = selected;
					if(selected < 0 || components.Count <= selected) {
						currentComponent = null;
					} else {
						currentComponent = System.Activator.CreateInstance(components[selected]) as IHexengineGearConfigComponent;
					}
					contentsScroll = Vector2.zero;
				}
				GUILayout.FlexibleSpace();
			}

			using(EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(contentsScroll)) {
				contentsScroll = scrollView.scrollPosition;
				using (new EditorGUILayout.VerticalScope())
				{
					GUILayout.Space(16);
					currentComponent?.OnGUI();
					GUILayout.Space(16);
				}
			}

			if (EditorGUI.EndChangeCheck()) {
				serializedObject.ApplyModifiedProperties();
			}
		}

		private void GUIRuntimeStreamingAssetPath(SerializedObject serializedObject) {
			SerializedProperty streamingAssetPathProperty = serializedObject.FindProperty("_streamingAssetPath");
			using (new EditorGUI.IndentLevelScope()) {
				using (new GUILayout.HorizontalScope()) {
					EditorGUILayout.LabelField(new GUIContent("Streaming asset path:"), GUILayout.Width(150.0f));

					using (new EditorGUI.DisabledScope(true)) {
						EditorGUILayout.TextField(
							new GUIContent(""),
							streamingAssetPathProperty.stringValue,
							GUILayout.ExpandWidth(true)
						);
					}

					if (
						GUILayout.Button(
							EditorGUIUtility.IconContent("Folder Icon"),
							GUILayout.Width(EditorGUIUtility.singleLineHeight * 2.0f),
							GUILayout.Height(EditorGUIUtility.singleLineHeight)
						)
					) {
						string path = EditorUtility.SaveFolderPanel("", Application.streamingAssetsPath, "");
						if (!string.IsNullOrEmpty(path)) {
							streamingAssetPathProperty.stringValue = Path.GetRelativePath(Application.streamingAssetsPath, path);
						}
					}
				}
			}

		}

		private void GUIAutoGeneratePath(SerializedObject serializedObject) {
			SerializedProperty autoGeneratePathProperty = serializedObject.FindProperty("_autoGeneratePath");
			using (new EditorGUI.IndentLevelScope()) {
				using (new GUILayout.HorizontalScope()) {
					EditorGUILayout.LabelField(new GUIContent("Auto generate path:"), GUILayout.Width(150.0f));

					using (new EditorGUI.DisabledScope(true)) {
						EditorGUILayout.TextField(
							new GUIContent(""),
							autoGeneratePathProperty.stringValue,
							GUILayout.ExpandWidth(true)
						);
					}

					if (
						GUILayout.Button(
							EditorGUIUtility.IconContent("Folder Icon"),
							GUILayout.Width(EditorGUIUtility.singleLineHeight * 2.0f),
							GUILayout.Height(EditorGUIUtility.singleLineHeight)
						)
					) {
						string path = EditorUtility.SaveFolderPanel("", "", "");
						if (!string.IsNullOrEmpty(path)) {
							autoGeneratePathProperty.stringValue = Path.GetRelativePath(Application.dataPath, path);
						}
					}
				}
			}
		}

		private void GUIEncodingType(SerializedObject serializedObject) {
			SerializedProperty encodingProperty = serializedObject.FindProperty("_encodingType");
			using(new EditorGUI.IndentLevelScope()) {
				EditorGUILayout.PropertyField(encodingProperty);
			}
		}
	}
}