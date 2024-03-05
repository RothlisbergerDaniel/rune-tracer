using UnityEngine;
using System.Collections;

namespace team99
{
    public class ToothMovement : MicrogameEvents
    {
        public Vector2 minBounds;
        public Vector2 maxBounds;
        public float speed = 1.0f;
        public AnimationCurve perlinCurve;
        public float lerpFactor = 1f;
        public bool gameStarted = false;
        private Vector2 perlinOffset;
        private Coroutine movementCoroutine;
        public ToothManager toothManager;
   

        void Awake() {
            perlinOffset = new Vector2(Random.value * 1000, Random.value * 1000);
        }


        protected override void OnEnable() {
            base.OnEnable();
            toothManager.ToothCleanedEvent += OnToothCleaned;
        }

        protected override void OnDisable() {
            base.OnDisable();
            toothManager.ToothCleanedEvent -= OnToothCleaned;
        }

        protected override void OnGameStart() {
            gameStarted = true;
            movementCoroutine = StartCoroutine(MoveWithPerlinNoise());
        }

        protected override void OnTimesUp() {
            ResetPosition();
        }

        IEnumerator MoveWithPerlinNoise() {
            while (true) {
                float perlinX = Mathf.PerlinNoise(perlinOffset.x + Time.time * speed, 0);
                float perlinY = Mathf.PerlinNoise(0, perlinOffset.y + Time.time * speed);

                float adjustedPerlinX = perlinCurve.Evaluate(perlinX);
                float adjustedPerlinY = perlinCurve.Evaluate(perlinY);

                float mappedX = Mathf.Lerp(minBounds.x, maxBounds.x, adjustedPerlinX);
                float mappedY = Mathf.Lerp(minBounds.y, maxBounds.y, adjustedPerlinY);

                mappedX *= transform.parent.localScale.x;
                mappedY *= transform.parent.localScale.y;
                mappedX += transform.parent.position.x;
                mappedY += transform.parent.position.y;

                Vector3 targetPosition = new Vector3(mappedX, mappedY, transform.position.z);
                transform.position += (targetPosition - transform.position) * lerpFactor * Time.deltaTime;

                yield return null;
            }
        }

        public void OnToothCleaned() {
            ResetPosition();
        }
        private void ResetPosition() {
            if (movementCoroutine != null) {
                StopCoroutine(movementCoroutine);
            }
            StartCoroutine(LerpBackToCenter());
        }

        IEnumerator LerpBackToCenter() {
            float elapsedTime = 0;
            Vector3 startPosition = transform.position;
            Vector3 centerPosition = new Vector3(transform.parent.position.x, transform.parent.position.y, transform.position.z);

            while (elapsedTime < 2f) {
                transform.position = Vector3.Lerp(startPosition, centerPosition, elapsedTime / 2f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = centerPosition;
        }
    }

}