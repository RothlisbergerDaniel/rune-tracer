using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;

namespace team99
{
    public class Plaque : MonoBehaviour
    {
        public float alphaDecrement = 0.5f;
        private SpriteRenderer spriteRenderer;

        public Sprite[] plaqueSprites;
        public Color[] plaqueColors;
        public AnimationCurve reappearCurve;
        public float minReappearDuration = 0.2f;
        public float maxReappearDuration = 3f;

        public float minFadeDuration = 0.2f;
        public float maxFadeDuration = 3f;


        public Action<Plaque> OnCleaned;

        private void Awake() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = plaqueSprites[Random.Range(0, plaqueSprites.Length)];
            spriteRenderer.color = plaqueColors[Random.Range(0, plaqueColors.Length)];
        }

        private void Clean() {
            Color currentColor = spriteRenderer.color;
            currentColor.a -= alphaDecrement;
            spriteRenderer.color = currentColor;

            if (currentColor.a <= 0f) {
                gameObject.SetActive(false);
                OnCleaned?.Invoke(this);
            }
        }

        public void OnToothCleaned() {

            if (gameObject.activeSelf) {
                StartCoroutine(AnimateAlpha(0f, minFadeDuration, maxFadeDuration)); 
            }
        }


        public void TriggerReappear() {
            gameObject.SetActive(true);
            StartCoroutine(AnimateAlpha(1f, minReappearDuration, maxReappearDuration)); 
        }

        private IEnumerator AnimateAlpha(float targetAlpha, float minDuration, float maxDuration) {
            float duration = Random.Range(minDuration, maxDuration);
            float time = 0;
            Color startColor = spriteRenderer.color;
            while (time < duration) {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(startColor.a, targetAlpha, time / duration);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
            // Ensure alpha is set to targetAlpha at the end
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            Clean();
        }
    }
}
