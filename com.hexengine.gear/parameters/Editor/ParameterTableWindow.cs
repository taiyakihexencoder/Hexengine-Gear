using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using com.hexengine.gear.editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.hexengine.gear.parameters.editor {
	public sealed class ParameterTableWindow : EditorWindow {
		private const string HEADER_ELEMENT_NAME = "headers";
		private const string TABLE_ELEMENT_NAME = "cells";
		private const string CONTROL_PANEL_ELEMENT_NAME = "controlPanel";
		private const string ROW_NAME = "_records";
		private const string SCROLLER_HORIZONTAL_LEADER = "horizontalScrollLeader";
		private const string SCROLLER_HORIZONTAL_FOLLOWER = "horizontalScrollFollower";
		private const float CELL_WIDTH = 200.0f;
		private static BindingFlags instanceFieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

		private List<ScriptableObject> tables;
		private SerializedObject serializedObject;

		[MenuItem("Hexengine/parameters/Parameter Table")]
		public static void Open() {
			ParameterTableWindow window = GetWindow<ParameterTableWindow>();
		}

		private void OnEnable() {
			tables = FindAllTables(typeof(ParameterTable<>));

			TwoPaneSplitView splitView = new TwoPaneSplitView(0, 250.0f, TwoPaneSplitViewOrientation.Horizontal);

			splitView.Add(CreateLeftPane(tables));
			splitView.Add(CreateRightPane());
			rootVisualElement.Add(splitView);
		}

		private List<ScriptableObject> FindAllTables(System.Type targetType) {
			tables = new List<ScriptableObject>();
			tables.Add(null);
			foreach(Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
				foreach(System.Type type in assembly.GetTypes()) {
					if(
						type.BaseType != null &&
						type.BaseType.IsGenericType &&
						type.BaseType.GetGenericTypeDefinition() == targetType
					) {
						tables.Add(GetParameterTable(type));
					}
				}
			}
			return tables;
		}

		private VisualElement CreateLeftPane(IEnumerable<ScriptableObject> tables) {
			ScrollView listScrollView = new ScrollView(ScrollViewMode.Vertical);
			listScrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;

			VisualElement listView = new VisualElement();
			listView.style.flexDirection = FlexDirection.Column;
			listScrollView.contentContainer.Add(listView);

			Color selectedColor = new Color(0.0f, 0.0f, 0.0f, 0.75f);
			Color notSelectedColor = new Color(0.0f, 0.0f, 0.0f, 0.01f);
			foreach (ScriptableObject table in tables) {
				if (table == null) { continue; }

				VisualElement listElementView = new VisualElement();
				listElementView.name = table.name;
				listElementView.userData = false;
				listElementView.style.width = Length.Percent(100);
				listElementView.style.height = 50.0f;
				listElementView.CaptureMouse();

				Label label = new Label();
				label.style.unityTextAlign = TextAnchor.MiddleCenter;
				label.text = GetTableName(table);
				listElementView.Add(label);
				listElementView.style.backgroundColor = notSelectedColor;
				listElementView.RegisterCallback<MouseDownEvent>(
					evt => {
						serializedObject = new SerializedObject(table);
						foreach (VisualElement element in listView.Query<VisualElement>().ToList()) {
							if (element.userData is bool selected) {
								element.userData = false;
								element.style.backgroundColor = notSelectedColor;
							}
						}
						listElementView.userData = true;
						listElementView.style.backgroundColor = selectedColor;
						serializedObject.ApplyModifiedProperties();
						EditorApplication.delayCall += () => {
							OnTableChanged();
							rootVisualElement.Q(CONTROL_PANEL_ELEMENT_NAME).style.visibility = Visibility.Visible;
						};
					}
				);
				label.style.position = Position.Absolute;
				label.style.top = 0;
				label.style.left = 0;
				label.style.right = 0;
				label.style.bottom = 0;
				label.pickingMode = PickingMode.Ignore;
				listView.Add(listElementView);
			}
			return listScrollView;
		}

		private VisualElement CreateRightPane() {
			VisualElement tableView = new VisualElement();
			tableView.style.flexDirection = FlexDirection.Column;
			tableView.style.flexGrow = 1.0f;

			ScrollView horizontalScrollView = new ScrollView(ScrollViewMode.Horizontal);
			horizontalScrollView.horizontalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
			horizontalScrollView.name = SCROLLER_HORIZONTAL_LEADER;
			tableView.Add(horizontalScrollView);
			VisualElement columnsView = new VisualElement();
			columnsView.name = HEADER_ELEMENT_NAME;
			columnsView.style.flexDirection = FlexDirection.Row;
			horizontalScrollView.contentContainer.Add(columnsView);

			ScrollView verticalScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
			verticalScrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
			verticalScrollView.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
			verticalScrollView.name = SCROLLER_HORIZONTAL_FOLLOWER;
			tableView.Add(verticalScrollView);
			verticalScrollView.style.flexGrow = 1f;
			VisualElement cellsView = new VisualElement();
			cellsView.name = TABLE_ELEMENT_NAME;
			cellsView.style.minWidth = 1000.0f;
			verticalScrollView.contentContainer.Add(cellsView);

			// 横方向のスクロールはヘッダに準拠し、テーブル側の操作で動かないようにする
			horizontalScrollView.horizontalScroller.valueChanged += _ => {
				verticalScrollView.scrollOffset = new Vector2(
					x: _,
					y: verticalScrollView.scrollOffset.y
				);
			};

			verticalScrollView.RegisterCallback<WheelEvent>(
				evt => {
					evt.StopPropagation();
					if ( 
						!verticalScrollView.verticalScroller.enabledSelf ||
						Mathf.Abs(evt.delta.x) > float.Epsilon
					) {
					}
				}
			);
			verticalScrollView.RegisterCallback<PointerMoveEvent>(
				evt => {
					if (evt.pressedButtons != 0) {
						verticalScrollView.scrollOffset = new Vector2(
							x: verticalScrollView.scrollOffset.x,
							y: verticalScrollView.scrollOffset.y + evt.deltaPosition.y
						);
						evt.StopPropagation();
					}
				}
			);
			
			tableView.Add(CreateControlPanel());
			return tableView;
		}

		private VisualElement CreateControlPanel() {
			VisualElement panel = new VisualElement();
			panel.name = CONTROL_PANEL_ELEMENT_NAME;
			panel.style.flexDirection = FlexDirection.Row;
			panel.style.paddingTop = 10;
			panel.style.paddingBottom = 10;
			panel.style.paddingLeft = 16;
			panel.style.paddingRight = 16;
			panel.style.visibility = Visibility.Hidden;

			Button addButton = new Button(
				clickEvent: () => {
					System.Type tableType = serializedObject.targetObject.GetType();
					System.Type recordType = tableType.BaseType.GetGenericArguments()[0];

					SerializedProperty property = serializedObject.FindProperty(ROW_NAME);
					property.arraySize++;
					SetPropertyDefault(property.GetArrayElementAtIndex(property.arraySize-1), recordType);
					serializedObject.ApplyModifiedProperties();
					OnTableChanged();
				}
			);
			addButton.text = Res.String.add;
			panel.Add(addButton);

			VisualElement spacer = new VisualElement();
			spacer.style.flexGrow = 1f;
			panel.Add(spacer);

			Button exportButton = new Button(
				clickEvent: () => { Export(serializedObject.targetObject as ScriptableObject); }
			);
			exportButton.text = Res.String.export;
			panel.Add(exportButton);
			return panel;
		}

		private void Export(ScriptableObject scriptable) {
			HexengineGearConfig config = ScriptableObjectUtility.GetProjectSingleton<HexengineGearConfig>();
			System.Text.Encoding encoding = config.encoding;

			FieldInfo field = scriptable.GetType().BaseType?.GetField(ROW_NAME, instanceFieldFlags);
			System.Type recordType = field?.FieldType?.GetElementType();
			if(field == null || !typeof(Parcelable).IsAssignableFrom(recordType)) {
				Debug.LogError($"Invalid Table Type: {scriptable.GetType().Name}, recordType:{recordType}");
			} else {
				char sep = Path.DirectorySeparatorChar;
				StreamingAssetUtility.CreateBinaryFile(
					path: $"parameters{sep}data{sep}{recordType.Name}.bytes",
					function: writer => {
						System.Array records = (System.Array)field.GetValue(scriptable);
						writer.Write(records.Length);

						if(records.Length > 0) {
							// 後で設定するので0埋めしておく
							for(int i = 0; i < records.Length * 2; ++i) { writer.Write(0); }

							int[] pointers = new int[records.Length];
							int[] sizes = new int[records.Length];
							pointers[0] = sizeof(int) * (records.Length+1);

							// データ書き込み
							int current = 1;
							foreach (Parcelable record in records){
								record.AppendToBytes(writer, out int size, encoding);
								sizes[current - 1] = size;
								if (current < records.Length) {
									pointers[current] = pointers[current-1]+size;
									current++;
								}
							}
							writer.Flush();

							// 0埋め部分をメモリ位置の情報で埋める
							writer.Seek(sizeof(int), SeekOrigin.Begin);
							for(int i = 0; i < records.Length; ++i) {
								writer.Write(pointers[i]);
								writer.Write(sizes[i]);
							}
						}
					},
					encoding: encoding
				);
			}
		}

		private void SynchronizeHorizontalScroll() {
			ScrollView leader = rootVisualElement.Q<ScrollView>(SCROLLER_HORIZONTAL_LEADER);
			ScrollView follower = rootVisualElement.Q<ScrollView>(SCROLLER_HORIZONTAL_FOLLOWER);
			follower.scrollOffset = new Vector2(
				leader.scrollOffset.x,
				follower.scrollOffset.y
			);
		}

		private void OnTableChanged() {
			VisualElement headers = rootVisualElement.Q(HEADER_ELEMENT_NAME);
			headers.Clear();

			VisualElement table = rootVisualElement.Q(TABLE_ELEMENT_NAME);
			table.Clear();

			System.Type tableType = serializedObject.targetObject.GetType();
			System.Type recordType = tableType.BaseType.GetGenericArguments()[0];

			SerializedProperty dataProperty = serializedObject.FindProperty(ROW_NAME);
			if(dataProperty.arraySize == 0) {
				dataProperty.arraySize++;
			}

			// 行の要素数をindex=0と揃える
			SerializedProperty firstProperty = dataProperty.GetArrayElementAtIndex(0);
			for(int i = 1; i < dataProperty.arraySize; ++i) {
				UpdateRowState(
					src: firstProperty,
					dst: dataProperty.GetArrayElementAtIndex(i),
					type: recordType
				);
			}

			SerializedProperty rowProperty = dataProperty.GetArrayElementAtIndex(0);
			InitializeHeader(rowProperty, headers, recordType, "");

			for (int i = 0; i < dataProperty.arraySize; ++i) {
				SerializedProperty p = dataProperty.GetArrayElementAtIndex(i);
				SetupRowUI(p, recordType);
			}

			EditorApplication.delayCall += () => {
				SynchronizeHorizontalScroll();
			};
		}

		private void UpdateRowState(SerializedProperty src, SerializedProperty dst, System.Type type) {
			if(type.IsArray) {
				System.Type elementType = type.GetElementType();
				dst.arraySize = src.arraySize;
				dst.isExpanded = src.isExpanded;
				for (int i = 0; i < src.arraySize; ++i) {
					UpdateRowState(
						src.GetArrayElementAtIndex(i), 
						dst.GetArrayElementAtIndex(i),
						elementType
					);
				}
			} else if(typeof(Parcelable).IsAssignableFrom(type)) {
				dst.isExpanded = src.isExpanded;
				foreach(FieldInfo field in type.GetFields(instanceFieldFlags)) {
					UpdateRowState(
						src.FindPropertyRelative(field.Name), 
						dst.FindPropertyRelative(field.Name), 
						field.FieldType
					);
				}
			}
		}

		/// <summary>
		/// ヘッダーを初期化
		/// </summary>
		/// <param name="property"></param>
		/// <param name="columns"></param>
		/// <param name="type"></param>
		/// <param name="name"></param>
		private void InitializeHeader(SerializedProperty property, VisualElement columns, System.Type type, string name) {
			if (type.IsArray) {
				VisualElement element = new VisualElement();
				element.style.flexGrow = 1f;
				element.style.flexDirection = FlexDirection.Row;
				element.style.width = CELL_WIDTH;
				columns.Add(element);

				Foldout foldout = CreateFoldout(property, name);
				element.Add(foldout);

				System.Type elementType = type.GetElementType();

				if (property.isExpanded) {
					VisualElement buttonsElement = new VisualElement();
					buttonsElement.style.flexDirection = FlexDirection.Row;
					buttonsElement.style.alignSelf = Align.FlexEnd;
					element.Add(buttonsElement);

					Button buttonAdd = new Button(
						clickEvent: () => {
							property.arraySize++;
							serializedObject.ApplyModifiedProperties();
							OnTableChanged();
						}
					);
					buttonAdd.text = "+";
					buttonsElement.Add(buttonAdd);

					Button buttonSub = new Button(
						clickEvent: () => {
							property.arraySize--;
							serializedObject.ApplyModifiedProperties();
							OnTableChanged();
						}
					);
					buttonSub.text = "-";
					buttonSub.enabledSelf = property.arraySize > 0;
					buttonsElement.Add(buttonSub);

					for (int i = 0; i < property.arraySize; ++i) {
						InitializeHeader(
							property: property.GetArrayElementAtIndex(i),
							columns: columns,
							type: elementType,
							name: $"[{i}]"
						);
					}
				}
			} else if(type == typeof(string)) {
				VisualElement element = new VisualElement();
				element.style.flexGrow = 1f;
				element.style.width = CELL_WIDTH;
				columns.Add(element);

				Label label = new Label();
				label.text = name;
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				element.Add(label);
			} else if(type.IsPrimitive) {
				if (
					type == typeof(bool) ||
					type == typeof(int) ||
					type == typeof(uint) ||
					type == typeof(long) ||
					type == typeof(ulong) ||
					type == typeof(float) ||
					type == typeof(double)
				) {
					VisualElement element = new VisualElement();
					element.style.flexGrow = 1f;
					element.style.width = CELL_WIDTH;
					columns.Add(element);

					Label label = new Label();
					label.text = name;
					label.style.unityTextAlign = TextAnchor.MiddleLeft;
					element.Add(label);
				}
			} else if(typeof(Parcelable).IsAssignableFrom(type)) {
				VisualElement element = new VisualElement();
				element.style.flexGrow = 1f;
				element.style.width = CELL_WIDTH;

				columns.Add(element);
				if (columns.childCount == 1) {
					foreach (FieldInfo field in type.GetFields(instanceFieldFlags)) {
						InitializeHeader(property.FindPropertyRelative(field.Name), columns, field.FieldType, field.Name);
					}
				} else {
					Foldout foldout = CreateFoldout(property, name);
					element.Add(foldout);
					if (foldout.value) {
						foreach(FieldInfo field in type.GetFields(instanceFieldFlags)) {
							InitializeHeader(property.FindPropertyRelative(field.Name), columns, field.FieldType, field.Name);
						}
					}
				}
			} else if(type.IsEnum) {
				VisualElement element = new VisualElement();
				element.style.flexGrow = 1f;
				element.style.width = CELL_WIDTH;
				columns.Add(element);

				Label label = new Label();
				label.text = name;
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				element.Add(label);
			}
		}

		private Foldout CreateFoldout(SerializedProperty property, string name) {
			Foldout foldout = new Foldout();
			foldout.text = name;
			foldout.style.unityTextAlign = TextAnchor.MiddleLeft;
			foldout.RegisterValueChangedCallback(
				_ => {
					OnUpdateFoldoutState(property, _.newValue);
					serializedObject.ApplyModifiedProperties();
					OnTableChanged();
				}
			);
			foldout.SetValueWithoutNotify(property.isExpanded);
			return foldout;
		}

		private void OnUpdateFoldoutState(SerializedProperty property, bool expanded) {
			string propertyPath = property.propertyPath;
			SerializedProperty recordsProperty = serializedObject.FindProperty(ROW_NAME);
			Regex regex = new Regex($"^{ROW_NAME}.Array.data\\[0\\]");
			for(int i = 0; i < recordsProperty.arraySize; ++i) {
				string path = regex.Replace(propertyPath, $"{ROW_NAME}.Array.data[{i}]");
				SerializedProperty recordProperty = serializedObject.FindProperty(path);
				recordProperty.isExpanded = expanded;
			}
		}

		private void SetupRowUI(SerializedProperty property, System.Type type) {
			VisualElement row = new VisualElement();
			VisualElement header = rootVisualElement.Q(HEADER_ELEMENT_NAME);
			row.style.flexDirection = FlexDirection.Row;
			row.style.flexGrow = 1.0f;

			row.schedule.Execute(() => {
				row.style.width = header.resolvedStyle.width;
				AddCells(row, property, type);
			});
			VisualElement table = rootVisualElement.Q(TABLE_ELEMENT_NAME);
			table.Add(row);
		}

		/// <summary>
		/// 追加した要素の初期化
		/// </summary>
		/// <param name="property"></param>
		/// <param name="type"></param>
		private void SetPropertyDefault(SerializedProperty property, System.Type type) {
			if(type.IsArray) {
				System.Type elementType = type.GetElementType();
				// 配列の要素数は一つ前のデータを引き継ぐ
				for(int i = 0; i < property.arraySize; ++i) {
					SetPropertyDefault(property.GetArrayElementAtIndex(i), elementType);
				}
			} else if(type.IsPrimitive) {
				if(type == typeof(bool)) { property.boolValue = default; }
				else if(type == typeof(int)) { property.intValue = default; }
				else if(type == typeof(uint)) { property.uintValue = default; }
				else if(type == typeof(long)) { property.longValue = default; }
				else if(type == typeof(ulong)) { property.ulongValue = default; }
				else if(type == typeof(float)) { property.floatValue = default; }
				else if(type == typeof(double)) { property.doubleValue = default; }
			} else if(type == typeof(string)) {
				property.stringValue = "";
			} else if(type.IsEnum) {
				property.intValue = default;
			} else if(typeof(Parcelable).IsAssignableFrom(type)) {
				foreach(FieldInfo field in type.GetFields(instanceFieldFlags)) {
					SetPropertyDefault(property.FindPropertyRelative(field.Name), field.FieldType);
				}
			}
		}

		/// <summary>
		/// テーブルへの各セルの追加
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="property"></param>
		/// <param name="type"></param>
		private void AddCells(VisualElement parent, SerializedProperty property, System.Type type) {
			if(parent.childCount == 0) {
				Button button = new Button(
					clickEvent: () => {
						Regex regex = new Regex($"^{ROW_NAME}.Array.data\\[(\\d)+\\]");
						Match match = regex.Match(property.propertyPath);
						if(match.Success) {
							int index = int.Parse(match.Groups[1].Value);
							serializedObject.FindProperty(ROW_NAME).DeleteArrayElementAtIndex(index);
							serializedObject.ApplyModifiedProperties();
							OnTableChanged();
						}
					}
				);
				button.style.flexGrow = 1f;
				button.style.flexBasis = 0.0f;
				button.text = "x";
				parent.Add(button);

				if (property.isExpanded) {
					foreach (FieldInfo field in type.GetFields(instanceFieldFlags)) {
						if (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null){
							AddCells(parent, property.FindPropertyRelative(field.Name), field.FieldType);
						}
					}
				}
			} else if (type.IsArray) {
				System.Type elementType = type.GetElementType();
				VisualElement arrayElement = new VisualElement();
				arrayElement.style.flexGrow = 1f;
				arrayElement.style.flexBasis = 0.0f;
				parent.Add(arrayElement);

				if(property.isExpanded) {
					for (int i = 0; i < property.arraySize; ++i) {
						AddCells(parent, property.GetArrayElementAtIndex(i), elementType);
					}
				}
			} else if(typeof(Parcelable).IsAssignableFrom(type)) {
				VisualElement parcelableElement = new VisualElement();
				parcelableElement.style.flexGrow = 1f;
				parcelableElement.style.flexBasis = 0.0f;
				parent.Add(parcelableElement);

				if (property.isExpanded) {
					foreach(FieldInfo field in type.GetFields(instanceFieldFlags)) {
						if(field.IsPublic || field.GetCustomAttribute<SerializeField>() != null) {
							AddCells(parent, property.FindPropertyRelative(field.Name), field.FieldType);
						}
					}
				}
			} else if(type == typeof(string)) {
				TextField textField = new TextField();
				textField.style.flexGrow = 1f;
				textField.style.flexBasis = 0.0f;
				textField.RegisterValueChangedCallback(
					_ => {
						property.stringValue = _.newValue;
						serializedObject.ApplyModifiedProperties();
					}
				);
				textField.SetValueWithoutNotify(property.stringValue);
				parent.Add(textField);
			} else if (type.IsEnum) {
				EnumField enumField = new EnumField((System.Enum)System.Enum.ToObject(type, property.intValue));
				enumField.style.flexGrow = 1f;
				enumField.style.flexBasis = 0.0f;
				enumField.RegisterValueChangedCallback(
					_ => {
						property.intValue = System.Convert.ToInt32(_.newValue);
						serializedObject.ApplyModifiedProperties();
					}
				);
				parent.Add(enumField);
			}else if(type.IsPrimitive) {
				if (type == typeof(bool)) {
					Toggle toggle = new Toggle();
					toggle.style.flexGrow = 1f;
					toggle.style.flexBasis = 0.0f;
					toggle.RegisterValueChangedCallback(
						_ => {
							property.boolValue = _.newValue;
							serializedObject.ApplyModifiedProperties();
						}
					);
					toggle.SetValueWithoutNotify(property.boolValue);
					parent.Add(toggle);
				} else if (type == typeof(int)) {
					IntegerField intField = new IntegerField();
					intField.style.flexGrow = 1f;
					intField.style.flexBasis = 0.0f;
					intField.RegisterValueChangedCallback(
						_ => {
							property.intValue = _.newValue;
							serializedObject.ApplyModifiedProperties();
						}
					);
					intField.SetValueWithoutNotify(property.intValue);
					parent.Add(intField);
				} else if (type == typeof(uint)) {
					UnsignedIntegerField uintField = new UnsignedIntegerField();
					uintField.style.flexGrow = 1f;
					uintField.style.flexBasis = 0.0f;
					uintField.RegisterValueChangedCallback(
						_ => {
							property.uintValue = _.newValue;
							serializedObject.ApplyModifiedProperties();
						}
					);
					uintField.SetValueWithoutNotify(property.uintValue);
					parent.Add(uintField);
				} else if (type == typeof(long)) {
					LongField longField = new LongField();
					longField.style.flexGrow = 1f;
					longField.style.flexBasis = 0.0f;
					longField.RegisterValueChangedCallback(
						_ => {
							property.longValue = _.newValue;
							serializedObject.ApplyModifiedProperties();
						}
					);
					longField.SetValueWithoutNotify(property.longValue);
					parent.Add(longField);
				} else if (type == typeof(ulong)) {
					UnsignedLongField ulongField = new UnsignedLongField();
					ulongField.style.flexGrow = 1f;
					ulongField.style.flexBasis = 0.0f;
					ulongField.RegisterValueChangedCallback(
						_ => {
							property.ulongValue = _.newValue;
							serializedObject.ApplyModifiedProperties();
						}
					);
					ulongField.SetValueWithoutNotify(property.ulongValue);
					parent.Add(ulongField);
				} else if (type == typeof(float)) {
					FloatField floatField = new FloatField();
					floatField.style.flexGrow = 1f;
					floatField.style.flexBasis = 0.0f;
					floatField.RegisterValueChangedCallback(
						_ => {
							property.floatValue = _.newValue;
							serializedObject.ApplyModifiedProperties();
						}
					);
					floatField.SetValueWithoutNotify(property.floatValue);
					parent.Add(floatField);
				} else if (type == typeof(double)) {
					DoubleField doubleField = new DoubleField();
					doubleField.style.flexGrow = 1f;
					doubleField.style.flexBasis = 0.0f;
					doubleField.RegisterValueChangedCallback(
						_ => {
							property.doubleValue = _.newValue;
							serializedObject.ApplyModifiedProperties();
						}
					);
					doubleField.SetValueWithoutNotify(property.doubleValue);
					parent.Add(doubleField);
				}
			}
		}

		private string GetTableName(ScriptableObject table) {
			if (table.name.EndsWith("Table")) {
				return table.name.Substring(0, table.name.Length-5);
			} else {
				return table.name;
			}
		}

		private ScriptableObject GetParameterTable(System.Type type) {
			ScriptableObject instance = ScriptableObjectUtility.GetProjectSingleton(type);
			if(instance == null) { 
				HexengineGearConfig config = ScriptableObjectUtility.GetProjectSingleton<HexengineGearConfig>();
				string path = $"{config.autoGeneratePath}{Path.DirectorySeparatorChar}parameters{Path.DirectorySeparatorChar}Editor{Path.DirectorySeparatorChar}tables";
				instance = ScriptableObjectEditorUtility.Create(type, path);
			}
			return instance;
		}
	}
}
