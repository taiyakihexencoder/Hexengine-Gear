using UnityEngine;

namespace com.hexengine.gear.animation {
	public sealed class AnimationControlBehaviour : MonoBehaviour {
		[SerializeField]
		private Animator animator = null;

		private AnimationControlGraph graph = null;

		private void Reset() {
			animator = GetComponentInChildren<Animator>();
		}

		public void AssignAnimationControlGraph(AnimationControlGraph graph) {
			this.graph = graph;
			graph?.SetTarget(animator);
		}

		private void Update() {
			if (graph != null) {
				graph.AddTime(Time.deltaTime);
				graph.UpdateMotion();
			}
		}

		private void OnDestroy() {
			if (graph != null) {
				graph.Dispose();
			}
		}

		private void OnApplicationQuit() {
			if (graph != null) {
				graph.Dispose();
			}
		}
	}
}
