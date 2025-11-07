using UnityEngine;

namespace com.hexengine.gear.editor {
	public sealed partial class HexengineGearConfig {
		[SerializeField]
		private string _lastSelectedAsmDef = "-";
		public string lastSelectedAsmdef => _lastSelectedAsmDef;

		[SerializeField]
		private string _lastSelectedEditorAsmDef = "-";
		public string lastSelectedEditorAsmDef => _lastSelectedEditorAsmDef;
	}
}