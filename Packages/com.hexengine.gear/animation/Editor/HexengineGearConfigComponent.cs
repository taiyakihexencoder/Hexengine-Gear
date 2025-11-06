using com.hexengine.gear.editor;
using UnityEditor;
using UnityEngine;

namespace com.hexengine.gear.animation.editor {
	public sealed class HexengineGearConfigComponent : IHexengineGearConfigComponent
	{
		void IHexengineGearConfigComponent.OnGUI() {
			SerializedObject serializedObject = new SerializedObject(ScriptableObjectUtility.GetProjectSingleton<HexengineGearConfig>());

			using(EditorGUI.ChangeCheckScope checkScope = new EditorGUI.ChangeCheckScope()) {

				SerializedProperty animationScriptNamespaceProperty = serializedObject.FindProperty("_animationScriptNamespace");
				animationScriptNamespaceProperty.stringValue = EditorGUILayout.TextField(new GUIContent("Namespace"), animationScriptNamespaceProperty.stringValue);

				SerializedProperty animationScriptAssemblyProperty = serializedObject.FindProperty("_animationScriptAssembly");
				animationScriptAssemblyProperty.stringValue = CustomEditorUtility.AssemblyField(animationScriptAssemblyProperty.stringValue, "Assembly");

				if(checkScope.changed) {
					serializedObject.ApplyModifiedProperties();
				}
			}
		}
	}
}
