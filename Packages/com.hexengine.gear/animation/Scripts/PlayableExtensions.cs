using UnityEngine.Playables;

namespace com.hexengine.gear.animation {
	public static class PlayableExtensions {
		/// <summary>
		/// Playable以下のinputをすべて破棄する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="playable"></param>
		public static void ClearAllBranch<T>(this T playable) where T : unmanaged, IPlayable {
			Playable input;
			for (int i = playable.GetInputCount()-1; i >= 0; --i) {
				input = playable.GetInput(i);
				if (input.IsValid()) {
					input.ClearAllBranch();
					playable.DisconnectInput(i);
					input.Destroy();
				}
			}
			playable.SetInputCount(0);
		}

		public static void AddTime<T>(this T playable, double dt) where T : unmanaged, IPlayable {
			double t = playable.GetTime();
			double delta = dt * playable.GetSpeed();
			playable.SetTime(t + delta);

			for(int i = 0, iMax = playable.GetInputCount(); i < iMax; ++i) {
				playable.GetInput(i).AddTime(delta);
			}
		}

		public static void SetElapsed<T>(this T playable, double t) where T : unmanaged, IPlayable {
			double time = t * playable.GetSpeed();
			playable.SetTime(time);
			for(int i = 0, iMax = playable.GetInputCount(); i < iMax; ++i) {
				playable.SetElapsed(time);
			}
		}
	}
}
