using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using com.hexengine.gear.editor;
using UnityEditor;
using UnityEngine;

namespace com.hexengine.gear.animation.editor {
	public sealed class PoseParameterTableWindow : EditorWindow {

		public interface ICustomProfileDrawer {
			void DrawPropertyGUI(SerializedProperty property);
		}

		[System.AttributeUsage(System.AttributeTargets.Class)]
		public class CustomProfileDrawerPriorityAttribute : PropertyAttribute {
			public readonly int priority;
			public CustomProfileDrawerPriorityAttribute(int priority) {
				this.priority = priority;
			}
		}

		private string exportPath => $"animation{Path.DirectorySeparatorChar}Scripts";
		private static ICustomProfileDrawer profileDrawer;

		private SerializedObject serializedObject;

		private Vector2 listScroll;
		private Vector2 contentsScroll;
		private int selectedIndex = -1;

		[MenuItem("Hexengine/animation/Pose Table")]
		public static void OpenPoseParameterTableWindow() {
			PoseParameterTableWindow window = GetWindow<PoseParameterTableWindow>();
		}

		private void OnEnable() {
			listScroll = Vector2.zero;
			contentsScroll = Vector2.zero;
			selectedIndex = -1;

			PoseParameterTable table = ScriptableObjectUtility.GetProjectSingleton<PoseParameterTable>();
			if(table == null) {
				table = CreateInstance<PoseParameterTable>();
				HexengineProject.CreateAsset(table, $"animation{Path.DirectorySeparatorChar}{typeof(PoseParameterTable).Name}.asset");
			}
			serializedObject = new SerializedObject(table);

			int currentPriority = -1;
			profileDrawer = new DefaultProfileDrawer();

			foreach(Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
				foreach(System.Type type in assembly.GetTypes()) {
					if (
						!type.IsInterface &&
						!type.IsAbstract &&
						typeof(ICustomProfileDrawer).IsAssignableFrom(type)
					) {
						int priority = type.GetCustomAttribute<CustomProfileDrawerPriorityAttribute>()?.priority ?? 0;
						if(currentPriority < priority) {
							currentPriority = priority;
							profileDrawer = (System.Activator.CreateInstance(type) as ICustomProfileDrawer) ?? profileDrawer;
						}
					}
				}
			}
		}

		private void OnGUI() {
			SerializedProperty poseListProperty = serializedObject.FindProperty("_poseList");

			using(new EditorGUILayout.HorizontalScope()) {
				using(EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(listScroll, false, true, horizontalScrollbar: GUIStyle.none, verticalScrollbar: GUI.skin.verticalScrollbar, GUI.skin.scrollView, GUILayout.Width(180.0f))) {
					listScroll.y = scrollView.scrollPosition.y;
					using (new EditorGUILayout.VerticalScope()){
						GUIStyle normalSkin = new GUIStyle(GUI.skin.label);
						normalSkin.alignment = TextAnchor.MiddleCenter;

						GUIStyle indexTextSkin = new GUIStyle(GUI.skin.label);
						indexTextSkin.fontSize = 8;
						indexTextSkin.alignment = TextAnchor.UpperLeft;

						Rect[] controlRects = new Rect[poseListProperty.arraySize];
						for (int i = 0; i < poseListProperty.arraySize; ++i) {
							string name = poseListProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
							if(string.IsNullOrEmpty(name)) {
								name = "Empty";
							}
							controlRects[i] = EditorGUILayout.GetControlRect(GUILayout.Width(160.0f), GUILayout.Height(40.0f));

							GUIStyle skin = normalSkin;
							if (selectedIndex == i) {
								skin = new GUIStyle(normalSkin);
								skin.normal.textColor = new Color(1.0f, 0.2f, 0.2f);
							}

							float backgroundAlpha = i == selectedIndex ? 0.9f : ( i % 2 == 0 ? 0f : 0.25f);
							EditorGUI.DrawRect(controlRects[i], new Color(0f, 0f, 0f, backgroundAlpha));
							EditorGUI.LabelField(controlRects[i], new GUIContent($"{i+1}"), indexTextSkin);
							EditorGUI.LabelField(controlRects[i], new GUIContent(name), skin);
						}

						if(GUILayout.Button("+", GUILayout.Width(50.0f))) {
							poseListProperty.arraySize++;
							ResetProperty(poseListProperty.GetArrayElementAtIndex(poseListProperty.arraySize-1));
							serializedObject.ApplyModifiedProperties();
						}

						if (Event.current.type == EventType.MouseDown) {
							Vector2 mousePosition = Event.current.mousePosition;
							for (int i = 0; i < controlRects.Length; ++i) {
								if (controlRects[i].Contains(mousePosition)) {
									if (Event.current.button == 0) {
										// 左クリック
										selectedIndex = i;
										Event.current.Use();
									} else if (Event.current.button == 1) {
										// 右クリック
										GenericMenu menu = new GenericMenu();

										int index = i;
										menu.AddItem(
											new GUIContent(Res.String.insert),
											false,
											() => {
												poseListProperty.InsertArrayElementAtIndex(index);
												ResetProperty(poseListProperty.GetArrayElementAtIndex(index));
												selectedIndex++;
												serializedObject.ApplyModifiedProperties();
											}
										);

										menu.AddItem(
											new GUIContent(Res.String.duplicate),
											false,
											() => {
												poseListProperty.InsertArrayElementAtIndex(index);
												selectedIndex++;
												serializedObject.ApplyModifiedProperties();
											}
										);

										menu.AddItem(
											new GUIContent(Res.String.delete),
											false,
											() => {
												poseListProperty.DeleteArrayElementAtIndex(index);
												if (selectedIndex >= index) { selectedIndex--; }
												serializedObject.ApplyModifiedProperties();
											}
										);
										menu.AddSeparator("");

										if (index > 0) {
											menu.AddItem(
												new GUIContent(Res.String.move_up), 
												false, 
												() => {
													poseListProperty.MoveArrayElement(index, index - 1);
													selectedIndex--;
													serializedObject.ApplyModifiedProperties();
												}
											);
										}

										if (index < controlRects.Length-1) {
											menu.AddItem(
												new GUIContent(Res.String.move_down), 
												false, 
												() => {
													poseListProperty.MoveArrayElement(index, index + 1);
													selectedIndex++;
													serializedObject.ApplyModifiedProperties();
												}
											);
										}

										menu.ShowAsContext();
									}
								}
							}
						}
					}
				}

				using (new EditorGUILayout.VerticalScope()) {
					using (
						EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(
							contentsScroll, 
							GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)
						)
					) {
						contentsScroll = scrollView.scrollPosition;
						using (new EditorGUILayout.VerticalScope()) {
							if (0 <= selectedIndex && selectedIndex < poseListProperty.arraySize)
							{
								using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope()) {
									SerializedProperty selectedProperty = poseListProperty.GetArrayElementAtIndex(selectedIndex);
									SerializedProperty basePosesProperty = selectedProperty.FindPropertyRelative("basePoseParameters");
									if(basePosesProperty.arraySize <= 0) {
										basePosesProperty.arraySize++;
										SerializedProperty basePoseProperty = basePosesProperty.GetArrayElementAtIndex(0);
										basePoseProperty.FindPropertyRelative("weight").floatValue = 0f;
										basePoseProperty.FindPropertyRelative("speed").doubleValue = 1;
										basePoseProperty.FindPropertyRelative("name").stringValue = "Default";
									}

									if(profileDrawer == null) {
										EditorGUILayout.LabelField("Empty Profile Drawer.");
									} else {
										profileDrawer.DrawPropertyGUI(poseListProperty.GetArrayElementAtIndex(selectedIndex));
									}

									if (check.changed) {
										serializedObject.ApplyModifiedProperties();
									}
								}
							}
						}
					}

					using (new EditorGUI.DisabledScope(selectedIndex < 0)) {
						using (new EditorGUILayout.HorizontalScope()) {
							GUILayout.FlexibleSpace();
							if (GUILayout.Button(Res.String.export, GUILayout.Width(120.0f))) {
								Export(selectedIndex);
							}
						}
					}
				}
			}
		}

		private void Export(int selectedIndex) {
			PoseParameterTable table = serializedObject.targetObject as PoseParameterTable;
			PoseParameterTable.CharacterPoses poses = table.poseList[selectedIndex];

			if (Validation(poses, out string msg)) {
				HexengineGearConfig config = ScriptableObjectUtility.GetProjectSingleton<HexengineGearConfig>();
				HexengineProject.CreateTextFile(
					$"{exportPath}{Path.DirectorySeparatorChar}{poses.name}.cs", 
					writer => {
						AnimationControlGenerator.Generate(
							writer: writer,
							ns: config.animationScriptNamespace,
							className: string.IsNullOrEmpty(poses.name) ? "Empty" : poses.name,
							defaultPoseIndex: poses.defaultPoseIndex,
							basePoseParameters: poses.basePoseParameters,
							overridePoseParameters: poses.overridePoseParameters,
							additivePoseParameters: poses.additivePoseParameters
						);
					}
				);

				if (!string.IsNullOrEmpty(config.animationScriptAssembly)) {
					HexengineProject.CreateAssemblyReference(
						config.animationScriptAssembly,
						$"{exportPath}{Path.DirectorySeparatorChar}{config.animationScriptAssembly}.asmref"
					);
				}
			} else {
				EditorUtility.DisplayDialog(
					Res.String.Animation.validation_export_error,
					msg,
					Res.String.ok
				);
			}
		}

		
		private bool Validation(PoseParameterTable.CharacterPoses poses, out string message) {
			message = "";
			// プロフィール名が空白
			if (string.IsNullOrEmpty(poses.name)) {
				message = Res.String.Animation.validation_failed_profile_name;
				return false;
			}

			// ポーズ名が空白
			foreach(PoseParameterTable.BasePoseParameter pose in poses.basePoseParameters) {
				if(string.IsNullOrEmpty(pose.name)) {
					message = Res.String.Animation.validation_failed_empty_pose_name;
					return false;
				}
			}

			foreach(PoseParameterTable.OverridePoseParameter pose in poses.overridePoseParameters) {
				if(string.IsNullOrEmpty(pose.name)) {
					message = Res.String.Animation.validation_failed_empty_pose_name;
					return false;
				}
			}

			foreach(PoseParameterTable.AdditivePoseParameter pose in poses.additivePoseParameters) {
				if(string.IsNullOrEmpty(pose.name)) {
					message = Res.String.Animation.validation_failed_empty_pose_name;
					return false;
				}
			}

			// 基本ポーズにデフォルトが指定されていない
			if (poses.basePoseParameters.Length == 0 || poses.defaultPoseIndex < 0) {
				message = Res.String.Animation.validation_failed_no_default_pose;
				return false;
			}

			// 名称の重複
			List<string> usedWords = new List<string>();
			List<string> usedClipKey = new List<string>();
			List<string> duplicatedClipKey = new List<string>();
			foreach(PoseParameterTable.BasePoseParameter pose in poses.basePoseParameters) {
				if(usedWords.Contains(pose.name)) {
					message = Res.String.Animation.validation_duplicated_name + pose.name;
					return false;
				}

				if(usedClipKey.Contains(pose.clipName)) {
					if(!duplicatedClipKey.Contains(pose.clipName)) {
						duplicatedClipKey.Add(pose.clipName);
					}
				} else {
					usedClipKey.Add(pose.clipName);
				}
			}
			usedWords.Clear();

			foreach(PoseParameterTable.OverridePoseParameter pose in poses.overridePoseParameters) {
				if(usedWords.Contains(pose.name)) {
					message = Res.String.Animation.validation_duplicated_name + pose.name;
					return false;
				}

				if(usedClipKey.Contains(pose.clipName)) {
					if(!duplicatedClipKey.Contains(pose.clipName)) {
						duplicatedClipKey.Add(pose.clipName);
					}
				} else {
					usedClipKey.Add(pose.clipName);
				}
			}
			usedWords.Clear();

			foreach(PoseParameterTable.AdditivePoseParameter pose in poses.additivePoseParameters) {
				if(usedWords.Contains(pose.name)) {
					message = Res.String.Animation.validation_duplicated_name + pose.name;
					return false;
				}

				if(usedClipKey.Contains(pose.clipName)) {
					if(!duplicatedClipKey.Contains(pose.clipName)) {
						duplicatedClipKey.Add(pose.clipName);
					}
				} else {
					usedClipKey.Add(pose.clipName);
				}
			}
			usedWords.Clear();

			if(duplicatedClipKey.Count > 0) {
				message = Res.String.Animation.validation_duplicated_clip_name + string.Join(',', duplicatedClipKey);
				return false;
			}
			usedClipKey.Clear();

			// 変数として使えない文字列
			Regex regex = new Regex("^[A-Za-z_][A-Za-z0-9_]*$");
			List<string> words = new List<string>();
			words.Add(poses.name);
			foreach(PoseParameterTable.BasePoseParameter basePose in poses.basePoseParameters) { words.Add(basePose.name); }
			foreach(PoseParameterTable.OverridePoseParameter overridePose in poses.overridePoseParameters) { words.Add(overridePose.name); }
			foreach(PoseParameterTable.AdditivePoseParameter additivePose in poses.additivePoseParameters) { words.Add(additivePose.name); }

			List<string> invalidWords = new List<string>();
			foreach(string word in words) {
				if (! regex.IsMatch(word)) {
					invalidWords.Add(word);
				}
			}
			if(invalidWords.Count > 0) {
				message = Res.String.Animation.validation_invalid_name + string.Join(',', invalidWords);
				return false;
			}

			// 予約語
			List<string> reservedWords = new List<string>(
				new string[] {
					"abstract", "as", "base", "bool", "break", "byte", "case", "catch",
					"char", "checked", "class", "const", "continue", "decimal", "default",
					"delegate", "do", "double", "else", "enum", "event", "explicit",
					"extern", "false", "finally", "fixed", "float", "for", "foreach",
					"goto", "if", "implicit", "in", "int", "interface", "internal", "is",
					"lock", "long", "namespace", "new", "null", "object", "operator",
					"out", "override", "params", "private", "protected", "public", "readonly",
					"ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc",
					"static", "string", "struct", "switch", "this", "throw", "true",
					"try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort",
					"using", "virtual", "void", "volatile", "while"
				}
			);
			foreach(string word in words) {
				if(reservedWords.Contains(word)) {
					invalidWords.Add(word);
				}
			}
			if(invalidWords.Count > 0) {
				message = Res.String.Animation.validation_invalid_name + string.Join(',', invalidWords);
				return false;
			}

			return true;
		}


		private void ResetProperty(SerializedProperty property) {
			SerializedProperty nameProperty = property.FindPropertyRelative("name");
			nameProperty.stringValue = "";

			SerializedProperty defaultPoseIndexProperty = property.FindPropertyRelative("defaultPoseIndex");
			defaultPoseIndexProperty.intValue = -1;

			SerializedProperty basePoseParametersProperty = property.FindPropertyRelative("basePoseParameters");
			basePoseParametersProperty.arraySize = 0;

			SerializedProperty overridePoseParametersProperty = property.FindPropertyRelative("overridePoseParameters");
			overridePoseParametersProperty.arraySize = 0;

			SerializedProperty additivePoseParametersProperty = property.FindPropertyRelative("additivePoseParameters");
			additivePoseParametersProperty.arraySize = 0;
		}

		[CustomProfileDrawerPriority(-1)]
		private class DefaultProfileDrawer : ICustomProfileDrawer {

			void ICustomProfileDrawer.DrawPropertyGUI(SerializedProperty property) {
				SerializedProperty nameProperty = property.FindPropertyRelative("name");
				SerializedProperty defaultPoseIndexProperty = property.FindPropertyRelative("defaultPoseIndex");
				SerializedProperty basePoseParametersProperty = property.FindPropertyRelative("basePoseParameters");
				SerializedProperty overridePoseParametersProperty = property.FindPropertyRelative("overridePoseParameters");
				SerializedProperty additivePoseParametersProperty = property.FindPropertyRelative("additivePoseParameters");

				nameProperty.stringValue = EditorGUILayout.TextField(new GUIContent(Res.String.Animation.profile), nameProperty.stringValue);
			
				// -- Base Pose -- //
				using (new EditorGUILayout.HorizontalScope()) {
					basePoseParametersProperty.isExpanded = EditorGUILayout.Foldout(
						basePoseParametersProperty.isExpanded,
						new GUIContent(Res.String.Animation.base_pose)
					);
					GUILayout.FlexibleSpace();
					if (basePoseParametersProperty.isExpanded) {
						if (GUILayout.Button("+", GUILayout.Width(30.0f))) {
							basePoseParametersProperty.arraySize++;
						}
					}
				}

				if(basePoseParametersProperty.isExpanded) {
					using (new EditorGUILayout.HorizontalScope()) {
						EditorGUILayout.LabelField("", GUILayout.Width(30.0f));
						EditorGUILayout.LabelField(Res.String.Animation.pose_name, GUILayout.Width(200.0f));
						EditorGUILayout.LabelField(Res.String.Animation.clip_key, GUILayout.Width(300.0f));
						EditorGUILayout.LabelField(Res.String.Animation.weight, GUILayout.Width(200.0f));
						EditorGUILayout.LabelField(Res.String.Animation.speed, GUILayout.Width(100.0f));
						EditorGUILayout.LabelField(Res.String.Animation.is_default_pose, GUILayout.Width(80.0f));
					}

					if(defaultPoseIndexProperty.intValue < 0 && basePoseParametersProperty.arraySize > 0) {
						defaultPoseIndexProperty.intValue = 0;
					}
					for(int i = 0; i < basePoseParametersProperty.arraySize; ++i) {
						SerializedProperty poseParameterProperty = basePoseParametersProperty.GetArrayElementAtIndex(i);
						SerializedProperty poseNameProperty = poseParameterProperty.FindPropertyRelative("name");
						SerializedProperty poseClipNameProperty = poseParameterProperty.FindPropertyRelative("clipName");
						SerializedProperty poseWeightProperty = poseParameterProperty.FindPropertyRelative("weight");
						SerializedProperty poseSpeedProperty = poseParameterProperty.FindPropertyRelative("speed");
						using (new EditorGUILayout.HorizontalScope()) {
							if(GUILayout.Button("x", GUILayout.Width(30.0f))) {
								basePoseParametersProperty.DeleteArrayElementAtIndex(i);
								if(defaultPoseIndexProperty.intValue == i) {
									defaultPoseIndexProperty.intValue = -1;
								}
								break;
							}
							poseNameProperty.stringValue = EditorGUILayout.TextField(poseNameProperty.stringValue, GUILayout.Width(200.0f));
							poseClipNameProperty.stringValue = EditorGUILayout.TextField(poseClipNameProperty.stringValue, GUILayout.Width(300.0f));

							if (i == defaultPoseIndexProperty.intValue) {
								EditorGUILayout.LabelField(Res.String.Animation.auto, GUILayout.Width(200f));
								poseWeightProperty.floatValue = 0.0f;
							} else {
								poseWeightProperty.floatValue = EditorGUILayout.Slider(poseWeightProperty.floatValue, 0f, 1f, GUILayout.Width(200.0f));
							}
							poseSpeedProperty.doubleValue = EditorGUILayout.DoubleField(poseSpeedProperty.doubleValue, GUILayout.Width(100.0f));
							if (EditorGUILayout.Toggle(i == defaultPoseIndexProperty.intValue, GUILayout.Width(80.0f))) {
								defaultPoseIndexProperty.intValue = i;
							}
						}
					}
				}

				// -- Override Pose -- //
				using (new EditorGUILayout.HorizontalScope()) {
					overridePoseParametersProperty.isExpanded = EditorGUILayout.Foldout(
						overridePoseParametersProperty.isExpanded,
						new GUIContent(Res.String.Animation.override_pose)
					);
					GUILayout.FlexibleSpace();
					if (overridePoseParametersProperty.isExpanded) {
						if (GUILayout.Button("+", GUILayout.Width(30.0f))) {
							overridePoseParametersProperty.arraySize++;
						}
					}
				}

				if(overridePoseParametersProperty.isExpanded) {
					using (new EditorGUILayout.HorizontalScope()) {
						EditorGUILayout.LabelField("", GUILayout.Width(30.0f));
						EditorGUILayout.LabelField(Res.String.Animation.pose_name, GUILayout.Width(200.0f));
						EditorGUILayout.LabelField(Res.String.Animation.clip_key, GUILayout.Width(300.0f));
						EditorGUILayout.LabelField(Res.String.Animation.is_active, GUILayout.Width(50.0f));
						EditorGUILayout.LabelField(Res.String.Animation.speed, GUILayout.Width(100.0f));
					}

					for(int i = 0; i < overridePoseParametersProperty.arraySize; ++i) {
						SerializedProperty poseParameterProperty = overridePoseParametersProperty.GetArrayElementAtIndex(i);
						SerializedProperty poseNameProperty = poseParameterProperty.FindPropertyRelative("name");
						SerializedProperty poseClipNameProperty = poseParameterProperty.FindPropertyRelative("clipName");
						SerializedProperty poseActiveProperty = poseParameterProperty.FindPropertyRelative("active");
						SerializedProperty poseSpeedProperty = poseParameterProperty.FindPropertyRelative("speed");
						using (new EditorGUILayout.HorizontalScope()) {
							if (GUILayout.Button("x", GUILayout.Width(30.0f))){
								overridePoseParametersProperty.DeleteArrayElementAtIndex(i);
								break;
							}
							poseNameProperty.stringValue = EditorGUILayout.TextField(poseNameProperty.stringValue, GUILayout.Width(200.0f));
							poseClipNameProperty.stringValue = EditorGUILayout.TextField(poseClipNameProperty.stringValue, GUILayout.Width(300.0f));
							poseActiveProperty.boolValue = EditorGUILayout.Toggle(poseActiveProperty.boolValue, GUILayout.Width(50.0f));
							poseSpeedProperty.doubleValue = EditorGUILayout.DoubleField(poseSpeedProperty.doubleValue, GUILayout.Width(100.0f));
						}
					}
				}

				// -- Additive Pose -- //
				using (new EditorGUILayout.HorizontalScope()) {
					additivePoseParametersProperty.isExpanded = EditorGUILayout.Foldout(
						additivePoseParametersProperty.isExpanded,
						new GUIContent(Res.String.Animation.additive_pose)
					);
					GUILayout.FlexibleSpace();
					if (additivePoseParametersProperty.isExpanded) {
						if (GUILayout.Button("+", GUILayout.Width(30.0f))) {
							additivePoseParametersProperty.arraySize++;
						}
					}
				}

				if(additivePoseParametersProperty.isExpanded) {
					using (new EditorGUILayout.HorizontalScope()) {
						EditorGUILayout.LabelField("", GUILayout.Width(30.0f));
						EditorGUILayout.LabelField(Res.String.Animation.pose_name, GUILayout.Width(200.0f));
						EditorGUILayout.LabelField(Res.String.Animation.clip_key, GUILayout.Width(300.0f));
						EditorGUILayout.LabelField(Res.String.Animation.weight, GUILayout.Width(200.0f));
						EditorGUILayout.LabelField(Res.String.Animation.speed, GUILayout.Width(100.0f));
					}

					for(int i = 0; i < additivePoseParametersProperty.arraySize; ++i) {
						SerializedProperty poseParameterProperty = additivePoseParametersProperty.GetArrayElementAtIndex(i);
						SerializedProperty poseNameProperty = poseParameterProperty.FindPropertyRelative("name");
						SerializedProperty poseClipNameProperty = poseParameterProperty.FindPropertyRelative("clipName");
						SerializedProperty poseWeightProperty = poseParameterProperty.FindPropertyRelative("weight");
						SerializedProperty poseSpeedProperty = poseParameterProperty.FindPropertyRelative("speed");
						using (new EditorGUILayout.HorizontalScope()) {
							if (GUILayout.Button("x", GUILayout.Width(30.0f))) {
								additivePoseParametersProperty.DeleteArrayElementAtIndex(i);
								break;
							}
							poseNameProperty.stringValue = EditorGUILayout.TextField(poseNameProperty.stringValue, GUILayout.Width(200.0f));
							poseClipNameProperty.stringValue = EditorGUILayout.TextField(poseClipNameProperty.stringValue, GUILayout.Width(300.0f));
							poseWeightProperty.floatValue = EditorGUILayout.Slider(poseWeightProperty.floatValue, 0f, 1f, GUILayout.Width(200.0f));
							poseSpeedProperty.doubleValue = EditorGUILayout.DoubleField(poseSpeedProperty.doubleValue, GUILayout.Width(100.0f));
						}
					}
				}
			}
		}
	}
}