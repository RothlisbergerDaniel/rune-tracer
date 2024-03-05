using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace team99
{
    public class ToothManager : MicrogameEvents
    {

        public Transform plaqueHolder;

        public Transform leftEye;
        public Transform rightEye;
        private float minEyeScale;
        public float maxEyeScale = .7f;
        public AnimationCurve eyeScaleCurve;

        public Transform mouth;
        private float minMouthScale;
        public float maxMouthScale = .5f;
        public AnimationCurve mouthScaleCurve;

        private int enabledChildrenCount = 0;
        public float plaquePercent;

        public float cleanThreshold = .95f;

        public event Action ToothCleanedEvent;
        public static event Action CheckAllTeethCleanedEvent;

        public Toothbrush toothbrush;

        public bool isToothClean = false;

        public GameObject bgAnimation;
        private SpriteRenderer bgAnimationRenderer;
        private Animator bgAnimator;
        public AnimationCurve bgAnimationFadeCurve;
        public float bgAnimationFadeDuration = 1f;

        public EyesAnimation eyesAnimation;
        public MouthAnimation mouthAnimation;

        public SpriteRenderer bgGradient;
        public Color failGradientTint;
        public Color sunburstColor;
        public Color spiralColor;

        public List<Plaque> cleanedPlaques;
        public float plaquesToReappearPercentage = 0.4f;
        private int totalPlaquesCount;
        public GameObject sparkles;

        public AudioSource endMusic;
        public AudioClip successSound;
        public AudioClip failureSound;

        public GameObject leftEyeShimmer;
        public GameObject rightEyeShimmer;


        public AnimationCurve endScaleCurve;
        public float endScaleDuration = .5f;


        void Awake() {
            minEyeScale = leftEye.localScale.x;
            minMouthScale = mouth.localScale.x;


            bgAnimator = bgAnimation.GetComponent<Animator>();
            bgAnimationRenderer = bgAnimation.GetComponent<SpriteRenderer>();
        }

        private void Start() {

            totalPlaquesCount = plaqueHolder.childCount;

            foreach (Transform child in plaqueHolder) {
                Plaque plaque = child.GetComponent<Plaque>();
                if (plaque != null) {
                    plaque.OnCleaned += OnPlaqueCleaned;
                    ToothCleanedEvent += plaque.OnToothCleaned;
                }
            }
        }


        private void OnDestroy() {
            foreach (Transform child in plaqueHolder) {
                Plaque plaque = child.GetComponent<Plaque>();
                if (plaque != null) {
                    plaque.OnCleaned -= OnPlaqueCleaned;
                    ToothCleanedEvent -= plaque.OnToothCleaned;
                }
            }

        }


        private void OnPlaqueCleaned(Plaque cleanedPlaque) {
            if (isToothClean) return;
            cleanedPlaques.Add(cleanedPlaque);
            UpdateCounts();
            UpdateEyes();
            UpdateMouth();
            eyesAnimation.TriggerCleanReaction();
            mouthAnimation.TriggerCleanReaction();
        }

        protected override void OnTimesUp() {
            if (isToothClean) return;

            MakeToothDirty();
            string animationTrigger = Random.Range(0, 2) == 0 ? "Swirl Left" : "Swirl Right";
            bgAnimator.SetTrigger(animationTrigger);

            leftEyeShimmer.SetActive(true);
            rightEyeShimmer.SetActive(true);
            mouth.GetComponent<WiggleTransform>().enabled = true;
            eyesAnimation.TriggerSadEnding();
            mouthAnimation.TriggerSadEnding();

            StartCoroutine(LerpSpriteColor(bgGradient, failGradientTint, 3f));
            StartCoroutine(LerpSpriteColor(bgAnimationRenderer, spiralColor, bgAnimationFadeDuration, bgAnimationFadeCurve));

            LerpScaleEyesAndMouth();


            endMusic.clip = failureSound;
            endMusic.Play();
        }


        private void MakeToothDirty() {
            // Calculate the start index for the plaques to reappear
            int plaquesToReappearCount = Mathf.RoundToInt(totalPlaquesCount * plaquesToReappearPercentage);
            int start = Mathf.Max(cleanedPlaques.Count - plaquesToReappearCount, 0);

            for (int i = cleanedPlaques.Count - 1; i >= start; i--) {
                cleanedPlaques[i].TriggerReappear();
            }

        }

        // Update the counts of enabled and disabled children
        private void UpdateCounts() {
            enabledChildrenCount = 0;

            foreach (Transform child in plaqueHolder) {
                if (child.gameObject.activeSelf) {
                    enabledChildrenCount++;
                }
            }

            plaquePercent = 1f - (enabledChildrenCount / (float)plaqueHolder.childCount);
            if (plaquePercent >= cleanThreshold) {
                Clean();
            }
        }

        private void LerpScaleEyesAndMouth() {

            Vector3 maxEyeScaleVec = new Vector3(maxEyeScale, maxEyeScale, 1);
            Vector3 maxMouthScaleVec = new Vector3(maxMouthScale, maxMouthScale, 1);
            StartCoroutine(LerpScale(leftEye, leftEye.localScale, maxEyeScaleVec, endScaleDuration, endScaleCurve));
            StartCoroutine(LerpScale(rightEye, rightEye.localScale, maxEyeScaleVec, endScaleDuration, endScaleCurve));
            StartCoroutine(LerpScale(mouth, mouth.localScale, maxMouthScaleVec, endScaleDuration, endScaleCurve));
        }


        private void Clean() {
            //Debug.Log("Tooth Cleaned");
            isToothClean = true;

            ToothCleanedEvent?.Invoke();
            CheckAllTeethCleanedEvent?.Invoke();

            mouthAnimation.TriggerHappyEnding();
            bgAnimator.SetTrigger("Sunburst");

            StartCoroutine(LerpSpriteColor(bgAnimationRenderer, sunburstColor, bgAnimationFadeDuration, bgAnimationFadeCurve));

            LerpScaleEyesAndMouth();

            sparkles.SetActive(true);
            endMusic.clip = successSound;
            endMusic.Play();
        }

        private IEnumerator LerpSpriteColor(SpriteRenderer spriteRenderer, Color targetColor, float duration, AnimationCurve curve = null) {
            float timer = 0f;
            Color startColor = spriteRenderer.color;

            while (timer < duration) {
                float time = timer / duration;
                Color interpolatedColor;

                if (curve != null) {
                    interpolatedColor = Color.Lerp(startColor, targetColor, curve.Evaluate(time));
                } else {
                    interpolatedColor = Color.Lerp(startColor, targetColor, time);
                }

                spriteRenderer.color = interpolatedColor;

                timer += Time.deltaTime;
                yield return null;
            }

            spriteRenderer.color = targetColor;
        }


        private IEnumerator LerpScale(Transform target, Vector3 startScale, Vector3 endScale, float duration, AnimationCurve curve = null) {
            float timer = 0;
            while (timer < duration) {

                float time = curve.Evaluate(timer / duration);

                if (curve != null) {
                    target.localScale = Vector3.Lerp(startScale, endScale, curve.Evaluate(time));
                } else {
                    target.localScale = Vector3.Lerp(startScale, endScale, time);
                }

                timer += Time.deltaTime;
                yield return null;
            }
            target.localScale = endScale;
        }




        private void UpdateEyes() {
            float eyeScaleValue = eyeScaleCurve.Evaluate(plaquePercent) * (maxEyeScale - minEyeScale) + minEyeScale;
            Vector3 newEyesScale = new Vector3(eyeScaleValue, eyeScaleValue, 1);
            leftEye.localScale = newEyesScale;
            rightEye.localScale = newEyesScale;
        }

        private void UpdateMouth() {
            float mouthScaleValue = mouthScaleCurve.Evaluate(plaquePercent) * (maxMouthScale - minMouthScale) + minMouthScale;
            Vector3 newMouthScale = new Vector3(mouthScaleValue, mouthScaleValue, 1);
            mouth.localScale = newMouthScale;
        }
    }
}
