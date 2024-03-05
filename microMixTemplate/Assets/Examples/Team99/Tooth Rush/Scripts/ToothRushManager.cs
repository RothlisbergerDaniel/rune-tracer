using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace team99
{
    public class ToothRushManager : MicrogameEvents
    {
        public List<ToothManager> teeth;

        private AudioSource music;

        private Coroutine pitchCoroutine;
        private Coroutine fadeCoroutine;

        public float fifteenSecondsWarningPitch = 1.1f;
        public float fifteenSecondsLerpDuration = 1f;
        public float fiveSecondsWarningPitch=1.2f;
        public float fiveSecondsLerpDuration=0.7f;

        private void Awake() {
            music = GetComponent<AudioSource>();
        }

        protected override void OnEnable() {
            base.OnEnable();
            ToothManager.CheckAllTeethCleanedEvent += OnToothCleaned;
        }

        protected override void OnDisable() {
            base.OnDisable();
            ToothManager.CheckAllTeethCleanedEvent -= OnToothCleaned;
        }
        void OnToothCleaned() {
            foreach (ToothManager toothManager in teeth) {
                if (!toothManager.isToothClean) {
                    return;
                }
            }
            StopAllCoroutines();
            fadeCoroutine = StartCoroutine(FadeOutMusic(music.volume, 0, 2));
            ReportGameCompletedEarly();
        }

        protected override void OnFifteenSecondsLeft() {
            StopAllCoroutines();
            pitchCoroutine = StartCoroutine(LerpPitchCoroutine(music.pitch, fifteenSecondsWarningPitch, fifteenSecondsLerpDuration));
        }

        protected override void OnFiveSecondsLeft() {
            StopAllCoroutines();
            pitchCoroutine = StartCoroutine(LerpPitchCoroutine(music.pitch, fiveSecondsWarningPitch, fiveSecondsLerpDuration));
        }

        protected override void OnTimesUp() {
            StopAllCoroutines();
            fadeCoroutine = StartCoroutine(FadeOutMusic(music.volume, 0, 2));
        }

        // Coroutine to interpolate pitch value over a specified duration
        private IEnumerator LerpPitchCoroutine(float startPitch, float endPitch, float lerpDuration) {
            float time = 0;
            while (time < lerpDuration) {
                music.pitch = Mathf.Lerp(startPitch, endPitch, time / lerpDuration);
                time += Time.deltaTime;
                yield return null;
            }
            music.pitch = endPitch;
        }

        private IEnumerator FadeOutMusic(float startVolume, float endVolume, float fadeDuration) {
            float time = 0;
            while (time < fadeDuration) {
                music.volume = Mathf.Lerp(startVolume, endVolume, time / fadeDuration);
                time += Time.deltaTime;
                yield return null;
            }
            music.volume = endVolume;
            //Debug.Log("finished fading music");
        }
    }
}