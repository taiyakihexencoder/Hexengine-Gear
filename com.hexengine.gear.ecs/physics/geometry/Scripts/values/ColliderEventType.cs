namespace com.hexengine.gear.ecs {
	[System.Flags]
	public enum ColliderEventType {
		None = 0,
		Enter = 1,
		Exit = 2,
		Stay = 4,
	}
}