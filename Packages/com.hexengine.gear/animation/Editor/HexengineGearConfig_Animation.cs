using UnityEngine;

namespace com.hexengine.gear.editor {
	public partial class HexengineGearConfig {
		[SerializeField]
		private string _animationScriptNamespace = "com.hexengine.gear.animation";
		public string animationScriptNamespace => _animationScriptNamespace;

		[SerializeField]
		private string _animationScriptAssembly = "-";
		public string animationScriptAssembly => _animationScriptAssembly;
	}
}
