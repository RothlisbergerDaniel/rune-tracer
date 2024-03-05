using UnityEngine;
namespace team99
{
    public class RotateFan : MicrogameInputEvents

    {
        public float maxRotationSpeed = 100f;
        public float accelerationRate = 400f;
        public float decelerationRate = 400f;
        private float currentRotationSpeed = 0f;
        public float rotateInput;

        void Update() {
            rotateInput = 0;

            float button1Value = button1.ReadValue<float>();
            float button2Value = button2.ReadValue<float>();

            rotateInput += button2Value - button1Value;

            float targetSpeed = rotateInput * maxRotationSpeed;

            // Accelerate or decelerate towards the target speed
            if (currentRotationSpeed != targetSpeed) {
                if (Mathf.Abs(targetSpeed) > Mathf.Abs(currentRotationSpeed)) {
                    // Accelerating
                    currentRotationSpeed += rotateInput * accelerationRate * Time.deltaTime;
                    if (rotateInput > 0) {
                        currentRotationSpeed = Mathf.Min(currentRotationSpeed, maxRotationSpeed);
                    } else {
                        currentRotationSpeed = Mathf.Max(currentRotationSpeed, -maxRotationSpeed);
                    }
                } else {
                    // Decelerating
                    if (currentRotationSpeed > 0) {
                        currentRotationSpeed -= decelerationRate * Time.deltaTime;
                        currentRotationSpeed = Mathf.Max(currentRotationSpeed, 0);
                    } else if (currentRotationSpeed < 0) {
                        currentRotationSpeed += decelerationRate * Time.deltaTime;
                        currentRotationSpeed = Mathf.Min(currentRotationSpeed, 0);
                    }
                }
            }

            // Apply rotation
            transform.Rotate(Vector3.up, currentRotationSpeed * Time.deltaTime);
        }
    }
}