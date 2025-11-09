namespace com.hexengine.gear {
	public class AnimatedBool {
		private bool flag;
		private Easing easing;
		private float elapsed;
		private float seconds;
		private float _value;
		private float min;
		private float max;
		public float value { get => _value; }

		public AnimatedBool(
			bool initial, 
			float seconds,
			Easing easing = Easing.EaseOutCubic, 
			float min = 0.0f, 
			float max = 1.0f
		) {
			this.easing = easing;
			this.seconds = seconds;
			Reset(initial);
		}

		public void Set(bool value) {
			if(flag != value) {
				flag = value;
				elapsed = seconds - elapsed;
			}
		}

		public void Reset(bool value) {
			flag = value;
			elapsed = seconds;
			_value = value ? max : min;
		}

		public void Update(float dt) {
			if (elapsed < seconds) {
				elapsed += dt;
				if (elapsed > seconds) {
					elapsed = seconds;
				}
				_value = min + (max - min) * easing.Evaluate(elapsed / seconds);
			}
		}
	}
}
