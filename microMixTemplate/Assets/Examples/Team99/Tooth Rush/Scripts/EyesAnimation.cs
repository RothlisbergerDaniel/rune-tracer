using UnityEngine;
using System.Collections;

namespace team99
{
    public class EyesAnimation : MonoBehaviour
    {
        public float minBlinkDelay = 0.5f;
        public float maxBlinkDelay = 1.5f;

        private float lastSmileTime = 0f;
        private Coroutine blinkRoutine = null;

        private const float smileDebounceThreshold = 0.5f;

        private Animator animator;

        private void Awake() {
            animator = GetComponent<Animator>();
        }

        public void OnLookStart() {
            if (blinkRoutine != null) return;
            blinkRoutine = StartCoroutine(BlinkAfterDelay());
        }

        private IEnumerator BlinkAfterDelay() {
            while (true) {
                float randomDelay = Random.Range(minBlinkDelay, maxBlinkDelay);
                yield return new WaitForSeconds(randomDelay);
                Blink();
            }
        }

        private void StopBlinkingRoutine() {
            if (blinkRoutine != null) {
                StopCoroutine(blinkRoutine);
                blinkRoutine = null;
            }
        }

        private void Blink() {
            ResetTriggers();
            animator.SetTrigger("Blink");
        }

        public void TriggerCleanReaction() {
            if (Time.time - lastSmileTime < smileDebounceThreshold) return;
            Smile();
        }

        public void TriggerSadEnding() {
            StopBlinkingRoutine();
            ResetTriggers();
            animator.SetTrigger("Sad Ending");
        }


        private void Smile() {
            StopBlinkingRoutine();
            ResetTriggers();
            animator.SetTrigger("Smile");
            lastSmileTime = Time.time;
        }

        private void ResetTriggers() {
            animator.ResetTrigger("Smile");
            animator.ResetTrigger("Blink");
        }


    }
}
