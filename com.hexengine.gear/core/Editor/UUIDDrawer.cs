using UnityEditor;
using UnityEngine;

namespace com.hexengine.gear.editor {
	[CustomPropertyDrawer(typeof(UUID))]
	public class UUIDDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			UUID uuid = UUID.Get(
				property.FindPropertyRelative("a").intValue,
				(short) property.FindPropertyRelative("b").intValue,
				(short) property.FindPropertyRelative("c").intValue,
				property.FindPropertyRelative("d").longValue
			);
			EditorGUI.LabelField(position, label, new GUIContent(uuid.ToString()));
		}
	}
}