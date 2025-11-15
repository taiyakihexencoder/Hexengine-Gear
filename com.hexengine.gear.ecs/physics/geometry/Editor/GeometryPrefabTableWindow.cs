using System.Collections.Generic;
using com.hexengine.gear.editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.hexengine.gear.ecs.editor {
	public sealed class GeometryPrefabTableWindow : EditorWindow {
		private const string INDEX_TRIGGER_NAME = "index_trigger";
		private const string INDEX_COLLISION_NAME = "index_collision";
		private const string INDEX_CHARACTER_NAME = "index_character";
		private const string INDEX_SETTINGS_NAME = "index_settings";
		private const string INDEX_EXPORT_NAME = "index_export";
		private const string CONTENT_PANE_NAME = "content";
		private const float CELL_WIDTH_NAME = 240.0f;
		private const float CELL_WIDTH_KEY = 150.0f;
		private const float CELL_WIDTH_EVENT_TYPE = 150.0f;
		private const float CELL_WIDTH_BELONGS_TO = 100.0f;
		private const float CELL_WIDTH_COLLIDES_WITH = 200.0f;
		private const float MARGIN_HEADER_LABEL = 6.3f;

		private static Color SelectedListItemBackgroundColor => new Color(0.1f, 0.1f, 0.1f, 1.0f);
		private static Color SelectedListItemForegroundColor => new Color(1.0f, 0.2f, 0.2f, 1.0f);
		private static Color NotSelectedListItemBackgroundColor => new Color(0.1f, 0.1f, 0.1f, 0.2f);
		private static Color NotSelectedListItemForegroundColor => new Color(0.8f, 0.8f, 0.8f, 1.0f);

		public enum IndexListElement {
			Trigger,
			Collision,
			Character,
			Settings,
			Export,
		}

		private IndexListElement selectedListElement = IndexListElement.Trigger;

		private SerializedObject serializedObject = null;

		private Dictionary<int, int> maskToValue = null;
		private Dictionary<int, int> valueToMask = null;

		[MenuItem("Hexengine/ECS/Geometry prefab table")]
		public static void OpenGeometryPrefabTable() {
			GeometryPrefabTableWindow window = GetWindow<GeometryPrefabTableWindow>();
		}

		private void OnEnable() {
			GeometryPrefabTable table = ScriptableObjectUtility.GetProjectSingleton<GeometryPrefabTable>();
			if (table == null) {
				table = CreateInstance<GeometryPrefabTable>();
				HexengineProject.CreateAsset(table, $"ecs/{typeof(GeometryPrefabTable).Name}.asset");
			}
			serializedObject = new SerializedObject(table);

			TwoPaneSplitView splitView = new TwoPaneSplitView(0, 200.0f, TwoPaneSplitViewOrientation.Horizontal);
			splitView.Add(IndexListLayout());
			splitView.Add(ContentLayout());

			rootVisualElement.Add(splitView);
		}

		private VisualElement IndexListLayout() {
			VisualElement element = new VisualElement();
			element.style.flexDirection = FlexDirection.Column;

			element.Add(IndexListItemLayout(IndexListElement.Trigger));
			element.Add(IndexListItemLayout(IndexListElement.Collision));
			element.Add(IndexListItemLayout(IndexListElement.Character));
			element.Add(IndexListItemLayout(IndexListElement.Settings));
			element.Add(IndexListItemLayout(IndexListElement.Export));

			return element;
		}

		private VisualElement IndexListItemLayout(IndexListElement listElement) {
			string viewText = "";
			string elementName = "";
			switch (listElement) {
				case IndexListElement.Trigger: {
					viewText = "Trigger";
					elementName = INDEX_TRIGGER_NAME;
					break;
				}
				case IndexListElement.Collision: {
					viewText = "Collision";
					elementName = INDEX_COLLISION_NAME;
					break;
				}
				case IndexListElement.Character: {
					viewText = "Character";
					elementName = INDEX_CHARACTER_NAME;
					break;
				}
				case IndexListElement.Settings: {
					viewText = "Settings";
					elementName = INDEX_SETTINGS_NAME;
					break;
				}
				case IndexListElement.Export: {
					viewText = "Export";
					elementName = INDEX_EXPORT_NAME;
					break;
				}
			}

			VisualElement element = new VisualElement();
			element.name = elementName;
			element.style.width = 200.0f;
			element.style.height = 60.0f;
			element.style.backgroundColor = selectedListElement == listElement ? SelectedListItemBackgroundColor : NotSelectedListItemBackgroundColor;
			element.RegisterCallback<MouseDownEvent>(
				evt => {
					if (evt.button == 0) {
						selectedListElement = listElement;
						OnGUIUpdate();
					}
				}
			);

			Label label = new Label(viewText);
			label.style.flexGrow = 1f;
			label.style.color = selectedListElement == listElement ? SelectedListItemForegroundColor : NotSelectedListItemForegroundColor;
			label.style.unityTextAlign = TextAnchor.MiddleCenter;
			element.Add(label);

			return element;
		}

		private void OnGUIUpdate() {
			// Index List
			VisualElement[] indexListItems = new VisualElement[] {
				rootVisualElement.Q(INDEX_TRIGGER_NAME),
				rootVisualElement.Q(INDEX_COLLISION_NAME),
				rootVisualElement.Q(INDEX_CHARACTER_NAME),
				rootVisualElement.Q(INDEX_SETTINGS_NAME),
				rootVisualElement.Q(INDEX_EXPORT_NAME),
			};

			int selectedListItem = (int)selectedListElement;
			for(int i = 0; i < indexListItems.Length; ++i) {
				if(i == selectedListItem) {
					indexListItems[i].style.backgroundColor = SelectedListItemBackgroundColor;
					indexListItems[i].Q<Label>().style.color = SelectedListItemForegroundColor;
				} else {
					indexListItems[i].style.backgroundColor = NotSelectedListItemBackgroundColor;
					indexListItems[i].Q<Label>().style.color = NotSelectedListItemForegroundColor;
				}
			}

			// ContentLayout
			RequestUpdateContentLayout();
		}

		private VisualElement ContentLayout() {
			VisualElement container = new VisualElement();
			ScrollView scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
			scrollView.style.flexDirection = FlexDirection.Column;
			scrollView.name = CONTENT_PANE_NAME;
			container.Add(scrollView);

			UpdateContentLayout(scrollView);
			return container;
		}

		private void RequestUpdateContentLayout() {
			UpdateContentLayout(rootVisualElement.Q(CONTENT_PANE_NAME));
		}

		private void UpdateContentLayout(VisualElement contentLayout) {
			contentLayout.Clear();
			switch (selectedListElement) {
				case IndexListElement.Trigger: {
					CreateIndexConvertTable();
					contentLayout.Add(TriggerContentLayout());
					break;
				}
				case IndexListElement.Collision: {
					CreateIndexConvertTable();
					contentLayout.Add(CollisionContentLayout());
					break;
				}
				case IndexListElement.Character: {
					CreateIndexConvertTable();
					contentLayout.Add(CharacterContentLayout());
					break;
				}
				case IndexListElement.Settings: {
					contentLayout.Add(SettingsContentLayout());
					break;
				}
				case IndexListElement.Export: {
					contentLayout.Add(ExportContentLayout());
					break;
				}
			}
		}

		private VisualElement TriggerContentLayout() {
			SerializedProperty sphereListProperty = serializedObject.FindProperty("_triggerSphereList");
			SerializedProperty boxListProperty = serializedObject.FindProperty("_triggerBoxList");
			SerializedProperty cylinderListProperty = serializedObject.FindProperty("_triggerCylinderList");

			return CommonGeometryContentLayout(
				sphereListProperty,
				boxListProperty,
				cylinderListProperty
			);
		}

		private VisualElement CharacterContentLayout() {
			SerializedProperty characterGeometryListProperty = serializedObject.FindProperty("_characterGeometryList");

			VisualElement element = new VisualElement();

			VisualElement characterListElement = new VisualElement();
			characterListElement.style.marginLeft = 30.0f;
			element.Add(characterListElement);

			characterListElement.Add(FoldoutLayout(characterGeometryListProperty, $"Character Geometry ({characterGeometryListProperty.arraySize})"));

			if (characterGeometryListProperty.isExpanded) {
				if (characterGeometryListProperty.arraySize > 0) {
					VisualElement header = new VisualElement();
					header.style.flexDirection = FlexDirection.Row;
					header.Add(SpaceHorizontal(100.0f + 2 * MARGIN_HEADER_LABEL));

					header.Add(LabelElement("name", CELL_WIDTH_NAME));
					header.Add(LabelElement("key", CELL_WIDTH_KEY));
					header.Add(LabelElement("belongsTo", CELL_WIDTH_BELONGS_TO));
					header.Add(LabelElement("collidesWith", CELL_WIDTH_COLLIDES_WITH));
					header.Add(LabelElement("radius", 100.0f));
					header.Add(LabelElement("height", 100.0f));

					characterListElement.Add(header);
				}
				for (int i = 0; i < characterGeometryListProperty.arraySize; ++i) {
					int index = i;
					VisualElement column = new VisualElement();
					column.style.flexDirection = FlexDirection.Row;
					column.Add(MoveUpButtonLayout(characterGeometryListProperty, index));
					column.Add(MoveDownButtonLayout(characterGeometryListProperty, index));
					column.Add(CharacterColumnLayout(characterGeometryListProperty.GetArrayElementAtIndex(i)));
					column.Add(DeleteButtonLayout(characterGeometryListProperty, index));
					characterListElement.Add(column);
				}
				characterListElement.Add(AddToListLayout(characterGeometryListProperty));
			}

			return element;
		}

		private VisualElement SettingsContentLayout() {
			VisualElement element = new VisualElement();
			element.style.flexDirection = FlexDirection.Row;
			element.Add(LayerNameListLayout());
			element.Add(SpaceHorizontal(24f));
			element.Add(KeyNameListLayout());
			return element;
		}

		#region GEOMETRY
		private VisualElement CollisionContentLayout() {
			SerializedProperty sphereListProperty = serializedObject.FindProperty("_collisionSphereList");
			SerializedProperty boxListProperty = serializedObject.FindProperty("_collisionBoxList");
			SerializedProperty cylinderListProperty = serializedObject.FindProperty("_collisionCylinderList");

			return CommonGeometryContentLayout(
				sphereListProperty,
				boxListProperty,
				cylinderListProperty
			);
		}


		private VisualElement CommonGeometryContentLayout(
			SerializedProperty sphereListProperty,
			SerializedProperty boxListProperty,
			SerializedProperty cylinderListProperty
		) {
			VisualElement element = new VisualElement();
			element.style.flexDirection = FlexDirection.Column;

			element.Add(FoldoutLayout(sphereListProperty, $"Sphere Geometry ({sphereListProperty.arraySize})"));

			if (sphereListProperty.isExpanded) {
				VisualElement sphereListElement = new VisualElement();
				sphereListElement.style.flexDirection = FlexDirection.Column;
				sphereListElement.style.marginLeft = 32.0f;
				element.Add(sphereListElement);

				if (sphereListProperty.arraySize > 0) {
					VisualElement sphereHeader = new VisualElement();
					sphereHeader.style.flexDirection = FlexDirection.Row;
					sphereHeader.Add(SpaceHorizontal(100.0f + 2 * MARGIN_HEADER_LABEL));
					sphereHeader.Add(HeaderLayout());

					Label sphereLabel1 = new Label("Radius");
					sphereLabel1.style.width = 150.0f;
					sphereLabel1.style.marginRight = MARGIN_HEADER_LABEL;
					sphereLabel1.style.unityTextAlign = TextAnchor.MiddleCenter;
					sphereHeader.Add(sphereLabel1);
					sphereListElement.Add(sphereHeader);
				}

				for (int i = 0; i < sphereListProperty.arraySize; ++i) {
					int index = i;
					VisualElement column = new VisualElement();
					column.style.flexDirection = FlexDirection.Row;
					column.Add(MoveUpButtonLayout(sphereListProperty, index));
					column.Add(MoveDownButtonLayout(sphereListProperty, index));
					column.Add(SphereColumnLayout(sphereListProperty.GetArrayElementAtIndex(i)));
					column.Add(DeleteButtonLayout(sphereListProperty, index));
					sphereListElement.Add(column);
				}
				sphereListElement.Add(AddToListLayout(sphereListProperty));
			}
			element.Add(SpaceVertical(30.0f));

			element.Add(FoldoutLayout(boxListProperty, $"Box Geometry ({boxListProperty.arraySize})"));

			if (boxListProperty.isExpanded) {
				VisualElement boxListElement = new VisualElement();
				boxListElement.style.flexDirection = FlexDirection.Column;
				boxListElement.style.marginLeft = 32.0f;
				element.Add(boxListElement);

				if (boxListProperty.arraySize > 0) {
					VisualElement boxHeader = new VisualElement();
					boxHeader.style.flexDirection = FlexDirection.Row;
					boxHeader.Add(SpaceHorizontal(100.0f + 2 * MARGIN_HEADER_LABEL));
					boxHeader.Add(HeaderLayout());

					Label boxLabel1 = new Label("W");
					boxLabel1.style.width = 40.0f;
					boxLabel1.style.marginRight = MARGIN_HEADER_LABEL;
					boxLabel1.style.unityTextAlign = TextAnchor.MiddleCenter;
					boxHeader.Add(boxLabel1);
					Label boxLabel2 = new Label("H");
					boxLabel2.style.width = 40.0f;
					boxLabel2.style.marginRight = MARGIN_HEADER_LABEL;
					boxLabel2.style.unityTextAlign = TextAnchor.MiddleCenter;
					boxHeader.Add(boxLabel2);
					Label boxLabel3 = new Label("D");
					boxLabel3.style.width = 40.0f;
					boxLabel3.style.marginRight = MARGIN_HEADER_LABEL;
					boxLabel3.style.unityTextAlign = TextAnchor.MiddleCenter;
					boxHeader.Add(boxLabel3);
					boxListElement.Add(boxHeader);
				}

				for (int i = 0; i < boxListProperty.arraySize; ++i) {
					int index = i;
					VisualElement column = new VisualElement();
					column.style.flexDirection = FlexDirection.Row;
					column.Add(MoveUpButtonLayout(boxListProperty, index));
					column.Add(MoveDownButtonLayout(boxListProperty, index));
					column.Add(BoxColumnLayout(boxListProperty.GetArrayElementAtIndex(i)));
					column.Add(DeleteButtonLayout(boxListProperty, index));
					boxListElement.Add(column);
				}
				boxListElement.Add(AddToListLayout(boxListProperty));
			}
			element.Add(SpaceVertical(30.0f));

			element.Add(FoldoutLayout(cylinderListProperty, $"Cylinder Geometry ({cylinderListProperty.arraySize})"));

			if (cylinderListProperty.isExpanded) {
				VisualElement cylinderListElement = new VisualElement();
				cylinderListElement.style.flexDirection = FlexDirection.Column;
				cylinderListElement.style.marginLeft = 32.0f;
				element.Add(cylinderListElement);

				if (cylinderListProperty.arraySize > 0) {
					VisualElement cylinderHeader = new VisualElement();
					cylinderHeader.style.flexDirection = FlexDirection.Row;
					cylinderHeader.Add(SpaceHorizontal(100.0f + 2 * MARGIN_HEADER_LABEL));
					cylinderHeader.Add(HeaderLayout());

					Label cylinderLabel1 = new Label("Radius");
					cylinderLabel1.style.width = 70.0f;
					cylinderLabel1.style.marginRight = MARGIN_HEADER_LABEL;
					cylinderLabel1.style.unityTextAlign = TextAnchor.MiddleCenter;
					cylinderHeader.Add(cylinderLabel1);
					Label cylinderLabel2 = new Label("Height");
					cylinderLabel2.style.width = 70.0f;
					cylinderLabel2.style.marginRight = MARGIN_HEADER_LABEL;
					cylinderLabel2.style.unityTextAlign = TextAnchor.MiddleCenter;
					cylinderHeader.Add(cylinderLabel2);
					cylinderListElement.Add(cylinderHeader);
				}
				for (int i = 0; i < cylinderListProperty.arraySize; ++i) {
					int index = i;
					VisualElement column = new VisualElement();
					column.style.flexDirection = FlexDirection.Row;
					column.Add(MoveUpButtonLayout(cylinderListProperty, index));
					column.Add(MoveDownButtonLayout(cylinderListProperty, index));
					column.Add(CylinderColumnLayout(cylinderListProperty.GetArrayElementAtIndex(i)));
					column.Add(DeleteButtonLayout(cylinderListProperty, index));
					cylinderListElement.Add(column);
				}
				cylinderListElement.Add(AddToListLayout(cylinderListProperty));
			}

			return element;
		}

		private VisualElement FoldoutLayout(SerializedProperty property, string foldoutTitle) {
			Foldout foldout = new Foldout();
			foldout.text = foldoutTitle;
			foldout.RegisterValueChangedCallback(
				_ => {
					property.isExpanded = _.newValue;
					property.serializedObject.ApplyModifiedProperties();
					RequestUpdateContentLayout();
				}
			);
			foldout.SetValueWithoutNotify(property.isExpanded);
			return foldout;
		}

		private VisualElement HeaderLayout() {
			VisualElement column = new VisualElement();
			column.style.flexDirection = FlexDirection.Row;

			column.Add(LabelElement("name", CELL_WIDTH_NAME));
			column.Add(LabelElement("key",CELL_WIDTH_KEY));
			column.Add(LabelElement("belongsTo", CELL_WIDTH_BELONGS_TO));
			column.Add(LabelElement("collidesWith", CELL_WIDTH_COLLIDES_WITH));
			column.Add(LabelElement("eventType", CELL_WIDTH_EVENT_TYPE));

			return column;
		}

		private VisualElement LabelElement(string text, float width) {
			Label label = new Label(text);
			label.style.width = width;
			label.style.marginRight = MARGIN_HEADER_LABEL;
			label.style.unityTextAlign = TextAnchor.MiddleCenter;
			return label;
		}

		private VisualElement SphereColumnLayout(SerializedProperty property) {
			SerializedProperty nameProperty = property.FindPropertyRelative("name");
			SerializedProperty keyProperty = property.FindPropertyRelative("key");
			SerializedProperty radiusProperty = property.FindPropertyRelative("radius");
			SerializedProperty belongsToProperty = property.FindPropertyRelative("belongsTo");
			SerializedProperty collidesWithProperty = property.FindPropertyRelative("collidesWith");
			SerializedProperty eventTypeProperty = property.FindPropertyRelative("eventType");

			SerializedProperty uuidProperty = property.FindPropertyRelative("uuid");
			if (uuidProperty.GetUUIDValue() == UUID.Null()) {
				uuidProperty.SetUUIDValue(UUID.Get());
				uuidProperty.serializedObject.ApplyModifiedProperties();
			}

			VisualElement column = new VisualElement();
			column.style.flexDirection = FlexDirection.Row;
			
			column.Add(NameCellLayout(nameProperty, CELL_WIDTH_NAME));
			column.Add(KeyLayout(keyProperty, CELL_WIDTH_KEY));
			column.Add(BelongsToCellLayout(belongsToProperty, CELL_WIDTH_BELONGS_TO));
			column.Add(CollidesWithCellLayout(collidesWithProperty, CELL_WIDTH_COLLIDES_WITH));
			column.Add(EventTypeCellLayout(eventTypeProperty, CELL_WIDTH_EVENT_TYPE));
			column.Add(FloatCellLayout(radiusProperty, 150.0f));
			return column;
		}

		private VisualElement BoxColumnLayout(SerializedProperty property) {
			SerializedProperty nameProperty = property.FindPropertyRelative("name");
			SerializedProperty keyProperty = property.FindPropertyRelative("key");
			SerializedProperty extentProperty = property.FindPropertyRelative("extent");
			SerializedProperty belongsToProperty = property.FindPropertyRelative("belongsTo");
			SerializedProperty collidesWithProperty = property.FindPropertyRelative("collidesWith");
			SerializedProperty eventTypeProperty = property.FindPropertyRelative("eventType");

			SerializedProperty uuidProperty = property.FindPropertyRelative("uuid");
			if (uuidProperty.GetUUIDValue() == UUID.Null()) {
				uuidProperty.SetUUIDValue(UUID.Get());
				uuidProperty.serializedObject.ApplyModifiedProperties();
			}

			VisualElement column = new VisualElement();
			column.style.flexDirection = FlexDirection.Row;

			column.Add(NameCellLayout(nameProperty, CELL_WIDTH_NAME));
			column.Add(KeyLayout(keyProperty, CELL_WIDTH_KEY));
			column.Add(BelongsToCellLayout(belongsToProperty, CELL_WIDTH_BELONGS_TO));
			column.Add(CollidesWithCellLayout(collidesWithProperty, CELL_WIDTH_COLLIDES_WITH));
			column.Add(EventTypeCellLayout(eventTypeProperty, CELL_WIDTH_EVENT_TYPE));
			column.Add(FloatCellLayout(extentProperty.FindPropertyRelative("x"), 40.0f));
			column.Add(FloatCellLayout(extentProperty.FindPropertyRelative("y"), 40.0f));
			column.Add(FloatCellLayout(extentProperty.FindPropertyRelative("z"), 40.0f));
			return column;
		}

		private VisualElement CylinderColumnLayout(SerializedProperty property) {
			SerializedProperty nameProperty = property.FindPropertyRelative("name");
			SerializedProperty keyProperty = property.FindPropertyRelative("key");
			SerializedProperty radiusProperty = property.FindPropertyRelative("radius");
			SerializedProperty heightProperty = property.FindPropertyRelative("height");
			SerializedProperty belongsToProperty = property.FindPropertyRelative("belongsTo");
			SerializedProperty collidesWithProperty = property.FindPropertyRelative("collidesWith");
			SerializedProperty eventTypeProperty = property.FindPropertyRelative("eventType");

			SerializedProperty uuidProperty = property.FindPropertyRelative("uuid");
			if (uuidProperty.GetUUIDValue() == UUID.Null()) {
				uuidProperty.SetUUIDValue(UUID.Get());
				uuidProperty.serializedObject.ApplyModifiedProperties();
			}

			VisualElement column = new VisualElement();
			column.style.flexDirection = FlexDirection.Row;

			column.Add(NameCellLayout(nameProperty, CELL_WIDTH_NAME));
			column.Add(KeyLayout(keyProperty, CELL_WIDTH_KEY));
			column.Add(BelongsToCellLayout(belongsToProperty, CELL_WIDTH_BELONGS_TO));
			column.Add(CollidesWithCellLayout(collidesWithProperty, CELL_WIDTH_COLLIDES_WITH));
			column.Add(EventTypeCellLayout(eventTypeProperty, CELL_WIDTH_EVENT_TYPE));
			column.Add(FloatCellLayout(radiusProperty, 70.0f));
			column.Add(FloatCellLayout(heightProperty, 70.0f));
			return column;
		}

		private VisualElement CharacterColumnLayout(SerializedProperty property) {
			SerializedProperty nameProperty = property.FindPropertyRelative("name");
			SerializedProperty keyProperty = property.FindPropertyRelative("key");
			SerializedProperty radiusProperty = property.FindPropertyRelative("radius");
			SerializedProperty heightProperty = property.FindPropertyRelative("height");
			SerializedProperty belongsToProperty = property.FindPropertyRelative("belongsTo");
			SerializedProperty collidesWithProperty = property.FindPropertyRelative("collidesWith");

			SerializedProperty uuidProperty = property.FindPropertyRelative("uuid");
			if (uuidProperty.GetUUIDValue() == UUID.Null()) {
				uuidProperty.SetUUIDValue(UUID.Get());
				uuidProperty.serializedObject.ApplyModifiedProperties();
			}

			VisualElement column = new VisualElement();
			column.style.flexDirection = FlexDirection.Row;

			column.Add(NameCellLayout(nameProperty, CELL_WIDTH_NAME));
			column.Add(KeyLayout(keyProperty, CELL_WIDTH_KEY));
			column.Add(BelongsToCellLayout(belongsToProperty, CELL_WIDTH_BELONGS_TO));
			column.Add(CollidesWithCellLayout(collidesWithProperty, CELL_WIDTH_COLLIDES_WITH));
			column.Add(FloatCellLayout(radiusProperty, 100.0f));
			column.Add(FloatCellLayout(heightProperty, 100.0f));
			return column;
		}

		private VisualElement FloatCellLayout(SerializedProperty property, float width) {
			FloatField floatField = new FloatField();
			floatField.style.width = width;
			floatField.RegisterValueChangedCallback(
				_ => {
					property.floatValue = _.newValue;
					property.serializedObject.ApplyModifiedProperties();
				}
			);
			floatField.SetValueWithoutNotify(property.floatValue);
			return floatField;
		}

		private VisualElement NameCellLayout(SerializedProperty property, float width) {
			TextField textField = new TextField();
			textField.style.width = width;
			textField.RegisterValueChangedCallback(
				_ => {
					property.stringValue = _.newValue;
					property.serializedObject.ApplyModifiedProperties();
				}
			);
			textField.SetValueWithoutNotify(property.stringValue);
			return textField;
		}

		private VisualElement BelongsToCellLayout(SerializedProperty property, float width) {
			SerializedProperty layerIndexListProperty = serializedObject.FindProperty("_layerIndexList");
			List<int> choices = new List<int>();
			choices.Add(0);
			for(int i = 0; i < layerIndexListProperty.arraySize; ++i) {
				choices.Add(1 << layerIndexListProperty.GetArrayElementAtIndex(i).intValue);
			}

			PopupField<int> field = new PopupField<int>(
				choices, 
				property.intValue, 
				LayerNameFormatValueCallback,
				LayerNameFormatValueCallback
			);
			field.style.width = width;
			field.RegisterValueChangedCallback(
				_ => {
					property.intValue = _.newValue;
					property.serializedObject.ApplyModifiedProperties();
				}
			);
			field.SetValueWithoutNotify(property.intValue);

			return field;
		}

		private string LayerNameFormatValueCallback(int value) {
			SerializedProperty layerListProperty = serializedObject.FindProperty("_layerList");
			int length = layerListProperty.arraySize;
			for (int i = 1; i <= length; ++i, value >>= 1) {
				if (value == 1) {
					return layerListProperty.GetArrayElementAtIndex(i-1).stringValue;
				}
			}
			return "-";
		}

		private VisualElement CollidesWithCellLayout(SerializedProperty property, float width) {
			List<string> choices = new List<string>();
			SerializedProperty layerIndexListProperty = serializedObject.FindProperty("_layerIndexList");
			SerializedProperty layerListProperty = serializedObject.FindProperty("_layerList");
			for(int i = 0; i < layerIndexListProperty.arraySize; ++i) {
				SerializedProperty layerIndexProperty = layerIndexListProperty.GetArrayElementAtIndex(i);
				SerializedProperty layerProperty = layerListProperty.GetArrayElementAtIndex(layerIndexProperty.intValue);
				choices.Add(layerProperty.stringValue);
			}

			MaskField maskField = new MaskField(choices, ConvertValueToMaskFlag(property.intValue));
			maskField.style.width = width;
			maskField.RegisterValueChangedCallback(
				_ => {
					property.intValue = ConvertMaskFlagToValue(_.newValue);
					property.serializedObject.ApplyModifiedProperties();
				}
			);

			return maskField;
		}

		private int ConvertMaskFlagToValue(int flags) {
			int result = 0;
			for(int i = 0, flag = 1; i < maskToValue.Count; ++i, flag <<= 1) {
				if ( (flag & flags) == flag) {
					result |= maskToValue[flag];
				}
			}
			return result;
		}

		private int ConvertValueToMaskFlag(int value) {
			int result = 0;
			for(int i = 0, flag = 1; i < valueToMask.Count; ++i, flag <<= 1) {
				if ( (flag & value) == flag) {
					result |= valueToMask[flag];
				}
			}
			return result;
		}

		private VisualElement KeyLayout(SerializedProperty property, float width) {
		SerializedProperty keyIndexListProperty = serializedObject.FindProperty("_keyIndexList");
			List<int> choices = new List<int>();
			for(int i = 0; i < keyIndexListProperty.arraySize; ++i) {
				choices.Add(keyIndexListProperty.GetArrayElementAtIndex(i).intValue);
			}

			PopupField<int> field = new PopupField<int>(
				choices, 
				property.intValue, 
				KeyNameFormatValueCallback,
				KeyNameFormatValueCallback
			);
			field.style.width = width;
			field.RegisterValueChangedCallback(
				_ => {
					property.intValue = _.newValue;
					property.serializedObject.ApplyModifiedProperties();
				}
			);
			field.SetValueWithoutNotify(property.intValue);

			return field;
		}

		private string KeyNameFormatValueCallback(int value) {
			SerializedProperty keysProperty = serializedObject.FindProperty("_keys");
			int length = keysProperty.arraySize;
			if (0 <= value && value < keysProperty.arraySize) {
				return keysProperty.GetArrayElementAtIndex(value).stringValue;
			} else {
				return "-";
			}
		}

		private void CreateIndexConvertTable() {
			valueToMask = new Dictionary<int, int>();
			maskToValue = new Dictionary<int, int>();
			SerializedProperty layerIndexProperty = serializedObject.FindProperty("_layerIndexList");
			for (int i = 0; i < layerIndexProperty.arraySize; ++i)
			{
				maskToValue.Add(1 << i, 1 << layerIndexProperty.GetArrayElementAtIndex(i).intValue);
				valueToMask.Add(1 << layerIndexProperty.GetArrayElementAtIndex(i).intValue, 1 << i);
			}
		}

		private VisualElement EventTypeCellLayout(SerializedProperty property, float width) {
			EnumFlagsField field = new EnumFlagsField((ColliderEventType)property.intValue);
			field.style.width = width;
			field.RegisterValueChangedCallback(
				_ => {
					property.intValue = (int)(ColliderEventType)_.newValue;
					property.serializedObject.ApplyModifiedProperties();
				}
			);
			return field;
		}
		#endregion

		#region SETTINGS
		private VisualElement LayerNameListLayout() {
			VisualElement layerListElement = new VisualElement();
			layerListElement.style.flexDirection = FlexDirection.Column;

			Label title = new Label("Layers");
			layerListElement.Add(title);

			SerializedProperty layerListProperty = serializedObject.FindProperty("_layerList");
			SerializedProperty layerIndexListProperty = serializedObject.FindProperty("_layerIndexList");
			for(int i = 0; i < layerIndexListProperty.arraySize; ++i) {
				int index = i;
				int targetIndex = layerIndexListProperty.GetArrayElementAtIndex(index).intValue;
				VisualElement column = new VisualElement();
				column.style.flexDirection = FlexDirection.Row;
				column.Add(LayerNameListItemLayout(layerListProperty.GetArrayElementAtIndex(targetIndex)));
				column.Add(MoveUpButtonLayout(layerIndexListProperty, index));
				column.Add(MoveDownButtonLayout(layerIndexListProperty, index));
				column.Add(
					DeleteButtonLayout(
						layerIndexListProperty, 
						index,
						OnLayerDeleted
					)
				);
				layerListElement.Add(column);
			}
			layerListElement.Add(
				AddToSettingsListLayout(
					layerListProperty, 
					layerIndexListProperty,
					layerIndexListProperty.arraySize < 32
				)
			);

			return layerListElement;
		}

		private void OnLayerDeleted(int index) {
			SerializedProperty layerIndexListProperty = serializedObject.FindProperty("_layerIndexList");
			SerializedProperty layerListProperty = serializedObject.FindProperty("_layerList");
			int deleteIndex = layerIndexListProperty.GetArrayElementAtIndex(index).intValue;
			for(int i = 0; i < layerIndexListProperty.arraySize; ++i) {
				SerializedProperty layerIndexProperty = layerIndexListProperty.GetArrayElementAtIndex(i);
				if(layerIndexProperty.intValue > deleteIndex) {
					layerIndexProperty.intValue--;
				}
			}
			layerListProperty.DeleteArrayElementAtIndex(deleteIndex);

			int lowerFlag = (1 << deleteIndex) - 1;
			int upperFlag = ~((2 << deleteIndex) - 1);
			SerializedProperty triggerSphereListProperty = serializedObject.FindProperty("_triggerSphereList");
			SerializedProperty triggerBoxListProperty = serializedObject.FindProperty("_triggerBoxList");
			SerializedProperty triggerCylinderListProperty = serializedObject.FindProperty("_triggerCylinderList");
			SerializedProperty collisionSphereListProperty = serializedObject.FindProperty("_collisionSphereList");
			SerializedProperty collisionBoxListProperty = serializedObject.FindProperty("_collisionBoxList");
			SerializedProperty collisionCylinderListProperty = serializedObject.FindProperty("_collisionCylinderList");
			SerializedProperty characterGeometryListProperty = serializedObject.FindProperty("_characterGeometryList");

			System.Action<SerializedProperty> action = listProperty => {
				for(int i = 0; i < listProperty.arraySize; ++i) {
					SerializedProperty property = listProperty.GetArrayElementAtIndex(i);
					SerializedProperty belongsToProperty = property.FindPropertyRelative("belongsTo");
					int belongsTo = belongsToProperty.intValue;
					belongsToProperty.intValue = ((upperFlag & belongsTo) >> 1) | (lowerFlag & belongsTo);
					
					SerializedProperty collidesWithProperty = property.FindPropertyRelative("collidesWith");
					int collidesWith = collidesWithProperty.intValue;
					collidesWithProperty.intValue = ((upperFlag & collidesWith) >> 1) | (lowerFlag & collidesWith);
				}
			};
			action(triggerSphereListProperty);
			action(triggerBoxListProperty);
			action(triggerCylinderListProperty);
			action(collisionSphereListProperty);
			action(collisionBoxListProperty);
			action(collisionCylinderListProperty);
			action(characterGeometryListProperty);
		}

		private VisualElement LayerNameListItemLayout(SerializedProperty property)
		{
			TextField field = new TextField();
			field.style.width = 240.0f;
			field.RegisterValueChangedCallback(
				_ => {
					property.stringValue = _.newValue;
					serializedObject.ApplyModifiedProperties();
				}
			);
			field.SetValueWithoutNotify(property.stringValue);
			return field;
		}
		
		private VisualElement KeyNameListLayout() {
			VisualElement keysElement = new VisualElement();
			keysElement.style.flexDirection = FlexDirection.Column;

			Label title = new Label("Keys");
			keysElement.Add(title);

			SerializedProperty keysProperty = serializedObject.FindProperty("_keys");
			SerializedProperty keyIndexListProperty = serializedObject.FindProperty("_keyIndexList");
			for(int i = 0; i < keyIndexListProperty.arraySize; ++i) {
				int index = i;
				int targetIndex = keyIndexListProperty.GetArrayElementAtIndex(index).intValue;
				VisualElement column = new VisualElement();
				column.style.flexDirection = FlexDirection.Row;
				column.Add(KeyNameListItemLayout(keysProperty.GetArrayElementAtIndex(targetIndex)));
				column.Add(MoveUpButtonLayout(keyIndexListProperty, index));
				column.Add(MoveDownButtonLayout(keyIndexListProperty, index));
				column.Add(
					DeleteButtonLayout(
						keyIndexListProperty, 
						index,
						OnKeyDeleted
					)
				);
				keysElement.Add(column);
			}
			keysElement.Add(
				AddToSettingsListLayout(
					keysProperty, 
					keyIndexListProperty,
					keyIndexListProperty.arraySize < int.MaxValue
				)
			);

			return keysElement;
		}
		
		private VisualElement KeyNameListItemLayout(SerializedProperty property) {
			TextField field = new TextField();
			field.style.width = 240.0f;
			field.RegisterValueChangedCallback(
				_ => {
					property.stringValue = _.newValue;
					serializedObject.ApplyModifiedProperties();
				}
			);
			field.SetValueWithoutNotify(property.stringValue);
			return field;
		}

		private void OnKeyDeleted(int index) {
			SerializedProperty keyIndexListProperty = serializedObject.FindProperty("_keyIndexList");
			SerializedProperty keysProperty = serializedObject.FindProperty("_keys");
			int deleteIndex = keyIndexListProperty.GetArrayElementAtIndex(index).intValue;
			for(int i = 0; i < keyIndexListProperty.arraySize; ++i) {
				SerializedProperty keyIndexProperty = keyIndexListProperty.GetArrayElementAtIndex(i);
				if(keyIndexProperty.intValue > deleteIndex) {
					keyIndexProperty.intValue--;
				}
			}
			keysProperty.DeleteArrayElementAtIndex(deleteIndex);

			SerializedProperty triggerSphereListProperty = serializedObject.FindProperty("_triggerSphereList");
			SerializedProperty triggerBoxListProperty = serializedObject.FindProperty("_triggerBoxList");
			SerializedProperty triggerCylinderListProperty = serializedObject.FindProperty("_triggerCylinderList");
			SerializedProperty collisionSphereListProperty = serializedObject.FindProperty("_collisionSphereList");
			SerializedProperty collisionBoxListProperty = serializedObject.FindProperty("_collisionBoxList");
			SerializedProperty collisionCylinderListProperty = serializedObject.FindProperty("_collisionCylinderList");
			SerializedProperty characterGeometryListProperty = serializedObject.FindProperty("_characterGeometryList");

			System.Action<SerializedProperty> action = listProperty => {
				for(int i = 0; i < listProperty.arraySize; ++i) {
					SerializedProperty property = listProperty.GetArrayElementAtIndex(i);
					SerializedProperty keyProperty = property.FindPropertyRelative("key");
					if(keyProperty.intValue > deleteIndex) {
						keyProperty.intValue--;
					}
				}
			};
			action(triggerSphereListProperty);
			action(triggerBoxListProperty);
			action(triggerCylinderListProperty);
			action(collisionSphereListProperty);
			action(collisionBoxListProperty);
			action(collisionCylinderListProperty);
			action(characterGeometryListProperty);
		}


		#endregion

		#region EXPORT
		private VisualElement ExportContentLayout() {
			VisualElement element = new VisualElement();
			element.style.paddingLeft = 30.0f;
			element.style.paddingRight = 30.0f;
			element.style.paddingTop = 20.0f;
			element.style.paddingBottom = 20.0f;

			Button exportButton = new Button(
				clickEvent:() => {
					GeometryDataGenerator.Generate();
				}
			);
			exportButton.style.width = 150.0f;
			exportButton.text = "Export";
			element.Add(exportButton);
			return element;
		}
		#endregion

		#region UTILITY
		private VisualElement SpaceVertical(float height) {
			VisualElement element = new VisualElement();
			element.style.width = new StyleLength(StyleKeyword.Auto);
			element.style.height = height;
			return element;
		}

		private VisualElement SpaceHorizontal(float width) {
			VisualElement element = new VisualElement();
			element.style.width = width;
			element.style.height = new StyleLength(StyleKeyword.Auto);
			return element;
		}

		private VisualElement DeleteButtonLayout(SerializedProperty property, int index, System.Action<int> onUpdate = null) {
			Button button = new Button(
				clickEvent: () => {
					onUpdate?.Invoke(index);
					property.DeleteArrayElementAtIndex(index);
					property.serializedObject.ApplyModifiedProperties();
					RequestUpdateContentLayout();
				}
			);
			button.text = "x";
			button.style.width = 30.0f;
			return button;
		}

		private VisualElement MoveUpButtonLayout(SerializedProperty property, int index, System.Action<int> onUpdate = null) {
			Button button = new Button(
				clickEvent: () => {
					onUpdate?.Invoke(index);
					property.MoveArrayElement(index, index-1);
					property.serializedObject.ApplyModifiedProperties();
					RequestUpdateContentLayout();
				}
			);
			button.enabledSelf = index > 0;
			button.text = "up";
			button.style.width = 50.0f;
			return button;
		}

		private VisualElement MoveDownButtonLayout(SerializedProperty property, int index, System.Action<int> onUpdate = null) {
			Button button = new Button(
				clickEvent: () => {
					onUpdate?.Invoke(index);
					property.MoveArrayElement(index, index+1);
					property.serializedObject.ApplyModifiedProperties();
					RequestUpdateContentLayout();
				}
			);
			button.enabledSelf = index+1 < property.arraySize;
			button.text = "down";
			button.style.width = 50.0f;
			return button;
		}

		private VisualElement AddToSettingsListLayout(
			SerializedProperty property,
			SerializedProperty indexProperty,
			bool enabled = true
		) {
			Button button = new Button(
				clickEvent: () => {
					int index = property.arraySize;
					property.arraySize++;
					property.GetArrayElementAtIndex(index).stringValue = "";
					indexProperty.arraySize++;
					indexProperty.GetArrayElementAtIndex(index).intValue = index;
					property.serializedObject.ApplyModifiedProperties();
					RequestUpdateContentLayout();
				}
			);
			button.text = "Add";
			button.enabledSelf = enabled;
			button.style.width = 80.0f;
			return button;
		}

		private VisualElement AddToListLayout(
			SerializedProperty property, 
			bool enabled = true,
			System.Action onUpdate = null
		) {
			Button button = new Button(
				clickEvent: () => {
					onUpdate?.Invoke();
					property.arraySize++;
					property.GetArrayElementAtIndex(property.arraySize-1)
						.FindPropertyRelative("uuid")
						.SetUUIDValue(UUID.Get());
					property.serializedObject.ApplyModifiedProperties();
					RequestUpdateContentLayout();
				}
			);
			button.text = "Add";
			button.enabledSelf = enabled;
			button.style.width = 80.0f;
			return button;
		}
		#endregion
	}
}