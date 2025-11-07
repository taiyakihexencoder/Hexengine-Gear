using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace com.hexengine.gear.animation {
	public abstract class AnimationControlGraph {
		public class BasePoseParameter {
			public float weight;
			public double speed;
		}

		public class OverridePoseParameter {
			public bool active;
			public double speed;
		}

		public class AdditivePoseParameter {
			public float weight;
			public double speed;
			public bool changed;
		}

		private PlayableGraph graph;
		private AnimationLayerMixerPlayable root;
		private AnimationPlayableOutput output;

		private const int MIXER_INDEX_BASE_POSE = 0;
		private const int MIXER_INDEX_OVERRIDE_POSE = 1;
		private const int MIXER_INDEX_ADDITIVE_POSE = 2;

		private AnimationMixerPlayable basePose;
		private AnimationMixerPlayable overridePose;
		private AnimationMixerPlayable additivePose;

		public AnimationControlGraph(
			IAnimationClipSource clipSource,
			string[] basePoses,
			string[] overridePoses,
			string[] additivePoses
		) {
			graph = PlayableGraph.Create();
			graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

			root = AnimationLayerMixerPlayable.Create(graph, 3);
			root.SetSpeed(1f);

			// base pose
			basePose = AnimationMixerPlayable.Create(graph, basePoses.Length);
			basePose.SetSpeed(1f);
			for(int i = 0; i < basePoses.Length; ++i) {
				AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, clipSource.GetClip(basePoses[i]));
				playable.SetSpeed(1f);
				basePose.ConnectInput(i, playable, 0, 0f);
			}
			root.ConnectInput(MIXER_INDEX_BASE_POSE, basePose, 0, 1f);
			InitBasePose(basePose);

			// override pose
			overridePose = AnimationMixerPlayable.Create(graph, overridePoses.Length);
			overridePose.SetSpeed(1f);
			for(int i = 0; i < overridePoses.Length; ++i) {
				AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, clipSource.GetClip(overridePoses[i]));
				playable.SetSpeed(1f);
				overridePose.ConnectInput(i, playable, 0, 0f);
			}
			root.ConnectInput(MIXER_INDEX_OVERRIDE_POSE, overridePose, 0, 0f);
			InitOverridePose(overridePose);

			// additive pose
			additivePose = AnimationMixerPlayable.Create(graph, additivePoses.Length);
			additivePose.SetSpeed(1f);
			for(int i = 0; i < additivePoses.Length; ++i) {
				AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, clipSource.GetClip(additivePoses[i]));
				playable.SetSpeed(1f);
				additivePose.ConnectInput(i, playable, 0, 0f);
			}
			root.ConnectInput(MIXER_INDEX_ADDITIVE_POSE, additivePose, 0, 1f);
			root.SetLayerAdditive(MIXER_INDEX_ADDITIVE_POSE, true);
			InitAdditivePose(additivePose);

			output = AnimationPlayableOutput.Create(graph, "", null);
			output.SetSourcePlayable(root);
		}

		~AnimationControlGraph() {
			if (!output.Equals(AnimationPlayableOutput.Null)) {
				output.SetTarget(null);
				output.SetSourcePlayable(Playable.Null);
				output = AnimationPlayableOutput.Null;
			}

			if (root.IsValid()) {
				ClearAllBranch(root);
				root.Destroy();
			}

			if (graph.IsValid()) {
				graph.Destroy();
			}
		}

		public void SetTarget(Animator animator) {
			output.SetTarget(animator);
		}

		public void UpdateMotion() {
			UpdateBasePose(root.GetInput(MIXER_INDEX_BASE_POSE));

			float weight = root.GetInputWeight(MIXER_INDEX_OVERRIDE_POSE);
			UpdateOverridePose(root.GetInput(MIXER_INDEX_OVERRIDE_POSE), ref weight);
			root.SetInputWeight(MIXER_INDEX_OVERRIDE_POSE, weight);

			UpdateAdditivePose(root.GetInput(MIXER_INDEX_ADDITIVE_POSE));

			graph.Evaluate();
		}

		private void ClearAllBranch<T>(T playable) where T : unmanaged, IPlayable {
			Playable input;
			for (int i = playable.GetInputCount()-1; i >= 0; --i) {
				input = playable.GetInput(i);
				if (input.IsValid()) {
					ClearAllBranch(input);
					playable.DisconnectInput(i);
					input.Destroy();
				}
			}
			playable.SetInputCount(0);
		}

		public void AddTime(double deltaTime) {
			AddTime(root, deltaTime);
		}

		private void AddTime<T>(T playable, double dt) where T : unmanaged, IPlayable {
			double t = playable.GetTime();
			double delta = dt * playable.GetSpeed();
			playable.SetTime(t + delta);

			for(int i = 0, iMax = playable.GetInputCount(); i < iMax; ++i) {
				AddTime(playable.GetInput(i), delta);
			}
		}

		protected void RestartClip(Playable parent, int index) {
			Playable playable = parent.GetInput(index);
			playable.SetTime(0);
			playable.Play();
		}

		protected void ApplyBaseDefaultPoseSpeed(double speed) {
			basePose.GetInput(0).SetSpeed(speed);
		}

		protected void ApplyBasePoseParameters(params BasePoseParameter[] parameters) {
			// デフォルトのポーズは他ポーズで1に満たない場合に補完する
			float weightSum = 0f;
			foreach(BasePoseParameter parameter in parameters) {
				weightSum += parameter.weight;
			}
			float defaultWeight = Mathf.Max(0, 1.0f - weightSum);
			float weightBase = Mathf.Max(1.0f, weightSum);

			basePose.SetInputWeight(0, defaultWeight);
			for (int i = 0; i < parameters.Length; ++i) {
				basePose.SetInputWeight(i + 1, parameters[i].weight / weightBase);
				basePose.GetInput(i + 1).SetSpeed(parameters[i].speed);
			}
		}

		protected void ApplyOverridePoseParameters(params OverridePoseParameter[] parameters) {
			int activeIndex = -1;
			for(int i = 0; i < parameters.Length; ++i) {
				if(parameters[i].active) {
					activeIndex = i;
				}
			}
			if (activeIndex >= 0) {
				// active=trueがあるなら他を0にする
				overridePose.GetInput(activeIndex).SetSpeed(parameters[activeIndex].speed);
				if (overridePose.GetInputWeight(activeIndex) < float.Epsilon) {
					RestartClip(overridePose, activeIndex);
				}
				for (int i = 0; i < parameters.Length; ++i) {
					overridePose.SetInputWeight(i, i == activeIndex ? 1f : 0f);
				}
				root.SetInputWeight(MIXER_INDEX_OVERRIDE_POSE, 1f);
			} else {
				bool existsOverride = false;
				for (int i = 0; i < parameters.Length; ++i) {
					if(overridePose.GetInputWeight(i) > float.Epsilon) {
						AnimationClipPlayable playable = (AnimationClipPlayable)overridePose.GetInput(i);

						// 終了チェック
						AnimationClip clip = playable.GetAnimationClip();
						if(playable.GetTime() >= clip.length && !clip.isLooping) {
							overridePose.SetInputWeight(i, 0f);
						} else {
							existsOverride = true;
						}
					}
				}

				root.SetInputWeight(MIXER_INDEX_OVERRIDE_POSE, existsOverride ? 1f : 0f);
			}

		}

		protected void ApplyAdditivePoseParameters(params AdditivePoseParameter[] parameters) {
			for(int i = 0; i < parameters.Length; ++i) {
				if(parameters[i].changed) {
					additivePose.SetInputWeight(i, parameters[i].weight);
					additivePose.GetInput(i).SetSpeed(parameters[i].speed);
					parameters[i].changed = false;
				}
			}
		}

		protected abstract void InitBasePose(Playable playable);
		protected abstract void InitOverridePose(Playable playable);
		protected abstract void InitAdditivePose(Playable playable);
		protected abstract void UpdateBasePose(Playable playable);
		protected abstract void UpdateOverridePose(Playable playable, ref float weight);
		protected abstract void UpdateAdditivePose(Playable playable);
	}
}