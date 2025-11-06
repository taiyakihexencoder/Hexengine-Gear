using System.IO;

namespace com.hexengine.gear.parameters {
	public interface Parcelable {
		void LoadFromBytes(byte[] bytes, ref int index, System.Text.Encoding encoding);
		void AppendToBytes(BinaryWriter writer, out int dataSize, System.Text.Encoding encoding);
	}
}