using UnityEngine;

namespace team99

{
    public class MouthAnimation : MonoBehaviour
    {
        private Animator animator;
        private float lastSmileTime = 0f;
        private const float smileDebounceThreshold = 1f;

        private void Awake() {
            animator = GetComponent<Animator>();
        }

        private void Surprise() {
            ResetTriggers();
            lastSmileTime = Time.time;
            animator.SetTrigger("Happy");
        }

        public void TriggerCleanReaction() {
            if (Time.time - lastSmileTime < smileDebounceThreshold) return;
            Surprise();
        }
        public void TriggerSadEnding() {
            string[] triggers = { "End Sad 1", "End Sad 2" };
            int randomIndex = Random.Range(0, triggers.Length);
            animator.SetTrigger(triggers[randomIndex]);
        }

        public void TriggerHappyEnding() {
            string[] triggers = { "End Happy 1", "End Happy 2", "End Happy 3" };
            int randomIndex = Random.Range(0, triggers.Length);
            animator.SetTrigger(triggers[randomIndex]);
        }

        private void ResetTriggers() {
            animator.ResetTrigger("Happy");
        }

    }

}
