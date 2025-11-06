using UnityEngine;

namespace com.hexengine.gear {
	public partial class RuntimeHexencoderGearConfig : ScriptableObject {
		[SerializeField]
		private string _streamingAssetPath = "com.hexencoder.gear";
		public string streamingAssetPath => _streamingAssetPath;
	}
}