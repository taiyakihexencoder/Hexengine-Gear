using System;
using UnityEngine;

namespace com.hexengine.gear {
	public enum Easing {
		Linear = 0,

		EaseIn = 32,
		EaseInSine,
		EaseInQuad,
		EaseInCubic,
		EaseInQuart,
		EaseInQuint,
		EaseInExpo,
		EaseInCirc,
		EaseInBounce,
		EaseInElastic,
		EaseInBack,

		EaseOut = 64,
		EaseOutSine,
		EaseOutQuad,
		EaseOutCubic,
		EaseOutQuart,
		EaseOutQuint,
		EaseOutExpo,
		EaseOutCirc,
		EaseOutBounce,
		EaseOutElastic,
		EaseOutBack,
	}

	public static class EasingExtensions {
		private const float PI2 = 3.1415926535f * 2.0f;

		public static float Evaluate(this Easing type, float alpha) {
			if (type.HasFlag(Easing.EaseIn)) {
				switch (type) {
					case Easing.EaseInSine: return EaseInSine(alpha);
					case Easing.EaseInQuad: return EaseInQuad(alpha);
					case Easing.EaseInCubic: return EaseInCubic(alpha);
					case Easing.EaseInQuart: return EaseInQuart(alpha);
					case Easing.EaseInQuint: return EaseInQuint(alpha);
					case Easing.EaseInExpo: return EaseInExpo(alpha);
					case Easing.EaseInCirc: return EaseInCirc(alpha);
					case Easing.EaseInBounce: return EaseInBounce(alpha);
					case Easing.EaseInElastic: return EaseInElastic(alpha);
					case Easing.EaseInBack: return EaseInBack(alpha);
					default: return alpha;
				}
			}
			else if (type.HasFlag(Easing.EaseOut)) {
				switch (type) {
					case Easing.EaseOutSine: return EaseOutSine(alpha);
					case Easing.EaseOutQuad: return EaseOutQuad(alpha);
					case Easing.EaseOutCubic: return EaseOutCubic(alpha);
					case Easing.EaseOutQuart: return EaseOutQuart(alpha);
					case Easing.EaseOutQuint: return EaseOutQuint(alpha);
					case Easing.EaseOutExpo: return EaseOutExpo(alpha);
					case Easing.EaseOutCirc: return EaseOutCirc(alpha);
					case Easing.EaseOutBounce: return EaseOutBounce(alpha);
					case Easing.EaseOutElastic: return EaseOutElastic(alpha);
					case Easing.EaseOutBack: return EaseOutBack(alpha);
					default: return alpha;
				}
			}
			else {
				return Linear(alpha);
			}
		}

		private static float Linear(float alpha) {
			return alpha;
		}

		private static float EaseInSine(float alpha) {
			float radian = (alpha - 0.25f) * PI2;
			return 1.0f + Mathf.Sin(radian);
		}

		private static float EaseInQuad(float alpha) {
			float t = Mathf.Clamp01(alpha);
			return t * t;
		}

		private static float EaseInCubic(float alpha) {
			float t = Mathf.Clamp01(alpha);
			return t * t * t;
		}

		private static float EaseInQuart(float alpha) {
			float t = Mathf.Clamp01(alpha);
			float tt = t * t;
			return tt * tt;
		}

		private static float EaseInQuint(float alpha) {
			float t = Mathf.Clamp01(alpha);
			float tt = t * t;
			return tt * tt * t;
		}

		private static float EaseInExpo(float alpha) {
			float t = Mathf.Clamp01(alpha);
			return t == 0.0f ? 0.0f : Mathf.Pow(2f, -(1.0f - t) * 10.0f);
		}

		private static float EaseInCirc(float alpha) {
			float t = Mathf.Clamp01(alpha);
			return 1.0f - Mathf.Sqrt(1.0f - t * t);
		}

		private static float EaseInBounce(float alpha) {
			float a = 7.5625f;
			float c = 1.0f / 22.0f;

			float x, y;
			float t = 1.0f - Mathf.Clamp01(alpha);

			if (t < 8.0f * c) { x = t; y = 1.0f; }
			else if (t < 16.0f * c) { x = t - 12.0f * c; y = 0.25f; }
			else if (t < 20.0f * c) { x = t - 18.0f * c; y = 0.0625f; }
			else { x = t - 21.0f * c; y = 0.015625f; }
			return -a * x * x + y;
		}

		private static float EaseInElastic(float alpha) {
			float t = 10.0f * (alpha - 1.0f);
			return Mathf.Pow(2.0f, t) * Mathf.Cos(PI2 * t / 3.0f);
		}

		private static float EaseInBack(float alpha) {
			float t = Mathf.Clamp01(alpha);
			return t * t * (2.70158f * t - 1.70158f);
		}

		private static float EaseOutSine(float alpha) {
			float radian = alpha * PI2;
			return Mathf.Sin(radian);
		}

		private static float EaseOutQuad(float alpha) {
			float t = Mathf.Clamp01(1.0f - alpha);
			return 1.0f - t * t;
		}

		private static float EaseOutCubic(float alpha) {
			float t = Mathf.Clamp01(1.0f - alpha);
			return 1.0f - t * t * t;
		}

		private static float EaseOutQuart(float alpha) {
			float t = Mathf.Clamp01(1.0f - alpha);
			float tt = t * t;
			return 1.0f - tt * tt;
		}

		private static float EaseOutQuint(float alpha) {
			float t = Mathf.Clamp01(1.0f - alpha);
			float tt = t * t;
			return 1.0f - tt * tt * t;
		}

		private static float EaseOutExpo(float alpha) {
			float t = Mathf.Clamp01(1.0f - alpha);
			return t == 1.0f ? 0.0f : 1.0f - Mathf.Pow(2f, -t * 10.0f);
		}

		private static float EaseOutCirc(float alpha) {
			float t = 1.0f - Mathf.Clamp01(alpha);
			return 1.0f - Mathf.Sqrt(1.0f - t * t);
		}

		private static float EaseOutBounce(float alpha) {
			float a = 7.5625f;
			float c = 1.0f / 22.0f;

			float x, y;
			float t = Mathf.Clamp01(alpha);

			if (t < 8.0f * c) { x = t; y = 0.0f; }
			else if (t < 16.0f * c) { x = t - 12.0f * c; y = 0.75f; }
			else if (t < 20.0f * c) { x = t - 18.0f * c; y = 0.9375f; }
			else { x = t - 21.0f * c; y = 0.984375f; }
			return a * x * x + y;
		}

		private static float EaseOutElastic(float alpha) {
			float t = -10.0f * alpha;
			return 1.0f - Mathf.Pow(2.0f, t) * Mathf.Cos(PI2 * t / 3.0f);
		}

		private static float EaseOutBack(float alpha) {
			float t = 1.0f - Mathf.Clamp01(alpha);
			return 1.0f - t * t * (2.70158f * t - 1.70158f);
		}
	}
}