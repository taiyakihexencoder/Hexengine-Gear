namespace com.hexengine.gear.animation {
	public partial struct ModelProfile {
		public string resourceAddress;
		public string[] clipAddresses;

		public static readonly ModelProfile Empty = new ModelProfile {
			resourceAddress = null,
			clipAddresses = null,
		};
	}
}
