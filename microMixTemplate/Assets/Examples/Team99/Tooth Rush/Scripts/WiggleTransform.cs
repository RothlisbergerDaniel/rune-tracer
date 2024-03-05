using UnityEngine;
namespace team99
{
    public class WiggleTransform : MonoBehaviour
    {
        public float shakeAmount = 0.05f;
        public float rotationAmount = 15f;
        public float noiseScale = 15f;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private float noiseOffsetX;
        private float noiseOffsetY;
        private float noiseOffsetRotation;

        void Awake() {
            initialPosition = transform.localPosition;
            initialRotation = transform.localRotation;

            // Generate random noise offsets
            noiseOffsetX = Random.Range(0f, 1000f);
            noiseOffsetY = Random.Range(0f, 1000f);
            noiseOffsetRotation = Random.Range(0f, 1000f);
        }

        void Update() {
            Wiggle();
        }

        void Wiggle() {
            float time = Time.time * noiseScale;

            float perlinX = Mathf.PerlinNoise(time + noiseOffsetX, noiseOffsetX) - 0.5f;
            float perlinY = Mathf.PerlinNoise(time + noiseOffsetY, noiseOffsetY) - 0.5f;
            float perlinRotation = Mathf.PerlinNoise(time + noiseOffsetRotation, noiseOffsetRotation) - 0.5f;

            Vector3 noiseOffset = new Vector3(perlinX, perlinY, 0) * 2.0f * shakeAmount;
            transform.localPosition = new Vector3(
                initialPosition.x + noiseOffset.x,
                initialPosition.y + noiseOffset.y,
                initialPosition.z
            );

            float noiseRotation = perlinRotation * 2.0f * rotationAmount;
            transform.localRotation = initialRotation * Quaternion.Euler(0, 0, noiseRotation);
        }
    }
}