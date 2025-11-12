using UnityEngine;

namespace com.hexengine.gear {
	[System.Serializable]
	public struct UUID {
		[SerializeField]
		private int a;

		[SerializeField]
		private short b;

		[SerializeField]
		private short c;

		[SerializeField]
		private long d;

		public static UUID Null() {
			return Get(0, 0, 0, 0L);
		}

		public static bool IsNull(UUID uuid) {
			return uuid.a == 0 && uuid.b == 0 && uuid.c == 0 && uuid.d == 0L;
		}

		public static UUID Get() {
			System.Guid guid = System.Guid.NewGuid();
			byte[] bytes = guid.ToByteArray();
			return new UUID {
				a = System.BitConverter.ToInt32(bytes, 0),
				b = System.BitConverter.ToInt16(bytes, 4),
				c = System.BitConverter.ToInt16(bytes, 6),
				d = BigEndianBits(bytes, 8),
			};
		}

		public static UUID Get(int a, short b, short c, long d) {
			return new UUID { a = a, b = b, c = c, d = d};
		}

		/// <summary>
		/// 処理系に関係なくGuidの後半の64bitはBig-Endianで解釈されるため、
		/// WindowsなどLittle Endian処理系ではByte列を反転させる
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="startIndex"></param>
		/// <returns></returns>
		public static long BigEndianBits(byte[] bytes, int startIndex) {
			if(System.BitConverter.IsLittleEndian) {
				byte[] array = bytes[startIndex..];
				System.Array.Reverse(array);
				return System.BitConverter.ToInt64(array);
			} else {
				return System.BitConverter.ToInt64(bytes, startIndex);
			}
		}

		public override bool Equals(object obj) {
			return obj is UUID uuid  &&
				uuid.a == a	&&
				uuid.b == b	&&
				uuid.c == c	&&
				uuid.d == d;
		}

		public override int GetHashCode() {
			return a.GetHashCode() ^ b.GetHashCode();
		}

		public static bool operator==(UUID a, UUID b) {
			return a.a == b.a &&
				a.b == b.b &&
				a.c == b.c &&
				a.d == b.d;
		}

		public static bool operator!=(UUID a, UUID b) {
			return a.a != b.a &&
				a.b != b.b &&
				a.c != b.c &&
				a.d != b.d;
		}

		public override string ToString() {
			string text = $"{a:x8}{b:x4}{c:x4}{d:x16}";
			return $"{text[..8]}-{text[8..12]}-{text[12..16]}-{text[16..20]}-{text[20..]}";
		}
	}
}