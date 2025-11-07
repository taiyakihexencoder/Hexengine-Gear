using System.IO;
using UnityEngine;

namespace com.hexengine.gear.editor {
	public sealed partial class HexengineGearConfig : ScriptableObject {
		internal enum EncodingType {
			DEFAULT,
			ASCII,
			UTF8,
		}

		[SerializeField]
		private string _autoGeneratePath = $"com.hexengine.gear{Path.DirectorySeparatorChar}auto-generated";
		internal string autoGeneratePath => _autoGeneratePath;

		[SerializeField]
		private EncodingType _encodingType = EncodingType.DEFAULT;
		public System.Text.Encoding encoding {
			get {
				switch(_encodingType) {
					case EncodingType.DEFAULT: return System.Text.Encoding.Default;
					case EncodingType.ASCII: return System.Text.Encoding.ASCII;
					case EncodingType.UTF8: return System.Text.Encoding.UTF8;
					default: return System.Text.Encoding.Default;
				}
			}
		}
	}
}