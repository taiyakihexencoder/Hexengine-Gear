using UnityEngine;

namespace com.hexengine.gear {
	public partial class RuntimeHexengineGearConfig : ScriptableObject {
		[SerializeField]
		private string _streamingAssetPath = "com.hexengine.gear";
		public string streamingAssetPath => _streamingAssetPath;
	}
}