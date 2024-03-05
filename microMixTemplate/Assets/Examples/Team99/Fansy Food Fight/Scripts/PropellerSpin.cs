using UnityEngine;

namespace team99
{
    public class PropellerSpin : MicrogameInputEvents
    {
        public float clockwiseRotationVelocityIncrease = -12000f;
        public float counterClockwiseRotationVelocityDecrease = 12000f;
        public float rotationalFrictionCoefficient = 8f;
        public float velocityThresholdForStop = 3f;
        public float maximumRotationalVelocity = 3000f;
        public float timeThresholdForDirectionChange = 0.3f;

        private Vector2 playerInputDirection;

        private Vector2[] possibleDirections = {
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
            new Vector2(1, -1),
            new Vector2(0, -1),
            new Vector2(-1, -1),
            new Vector2(-1, 0),
            new Vector2(-1, 1)
         };

        public int currentInputDirectionIndex = 0;
        private int previousInputDirectionIndex = 0;
        private float currentRotationalVelocity = 0f;
        private float timeSinceLastInputDirectionChange = 0f;
        public enum RotationDirection
        {
            Clockwise,
            CounterClockwise,
            None
        }

        public RotationDirection currentRotationDirection = RotationDirection.None;
        public RotationDirection directionFromInput = RotationDirection.None;

        void Update() {
            playerInputDirection = ((Vector2)stick).normalized;

            currentInputDirectionIndex = GetMappedDirectionFromInput(playerInputDirection);

            // Since the last frame, player is pressing in a new cardinal direction
            if (playerInputDirection != Vector2.zero && currentInputDirectionIndex != previousInputDirectionIndex) {
                // determine if the change is clockwise or counterclockwise
                directionFromInput = DetermineRotationDirection(currentInputDirectionIndex);

                // if moving clockwise already, go faster clockwise
                if (directionFromInput == RotationDirection.Clockwise && (currentRotationDirection == RotationDirection.None || currentRotationDirection == RotationDirection.Clockwise)) {
                    currentRotationalVelocity += clockwiseRotationVelocityIncrease * Time.deltaTime;
                    // Reset the timer since there was a direction change
                    timeSinceLastInputDirectionChange = 0f;
                }
                // if moving counterclockwise already, go faster counterclockwise
                else if (directionFromInput == RotationDirection.CounterClockwise && (currentRotationDirection == RotationDirection.None || currentRotationDirection == RotationDirection.CounterClockwise)) {
                    currentRotationalVelocity += counterClockwiseRotationVelocityDecrease * Time.deltaTime;
                    // Reset the timer since there was a direction change
                    timeSinceLastInputDirectionChange = 0f;
                } else {

                    ApplyRotationalFriction();

                }
                // Clamp the rotational velocity to its maximum limits
                currentRotationalVelocity = Mathf.Clamp(currentRotationalVelocity, -maximumRotationalVelocity, maximumRotationalVelocity);
      
            } else {
                ApplyRotationalFriction();
            }

            // Rotate the propeller
            float rotationDegrees = currentRotationalVelocity * Time.deltaTime;
            transform.Rotate(Vector3.forward, rotationDegrees);


            // Update the rotation direction based on the current rotational velocity
            if (currentRotationalVelocity < 0) {
                currentRotationDirection = RotationDirection.Clockwise;
            } else if (currentRotationalVelocity > 0) {
                currentRotationDirection = RotationDirection.CounterClockwise;
            } else {
                currentRotationDirection = RotationDirection.None;
            }

            // Update the previous input direction index for the next frame
            previousInputDirectionIndex = currentInputDirectionIndex;
        }

        private void ApplyRotationalFriction() {
            // Apply rotational friction if there's been no direction change within the threshold time
            timeSinceLastInputDirectionChange += Time.deltaTime;
            if (timeSinceLastInputDirectionChange > timeThresholdForDirectionChange) {
                currentRotationalVelocity -= currentRotationalVelocity * rotationalFrictionCoefficient * Time.deltaTime;
                // Stop the rotation if the velocity is below the threshold
                if (Mathf.Abs(currentRotationalVelocity) < velocityThresholdForStop) {
                    currentRotationalVelocity = 0f;
                }
            }

        }

        private RotationDirection DetermineRotationDirection(int currentDirectionIndex) {
            int totalDirections = possibleDirections.Length;
            int directionIndexDifference = (currentDirectionIndex - previousInputDirectionIndex + totalDirections) % totalDirections;

            if (directionIndexDifference == 1 || directionIndexDifference == -7) {
                return RotationDirection.Clockwise;
            } else if (directionIndexDifference == -1 || directionIndexDifference == 7) {
                return RotationDirection.CounterClockwise;
            } else {
                return RotationDirection.None;
            }
        }


        private int GetMappedDirectionFromInput(Vector2 playerInputDirection) {
            if (playerInputDirection == Vector2.zero) {
                return 0;
            }
            float angleRadians = Mathf.Atan2(playerInputDirection.y, playerInputDirection.x);
            float angleDegrees = (Mathf.Rad2Deg * angleRadians + 360) % 360;
            int directionIndex = Mathf.RoundToInt(angleDegrees / 45) % 8; 

            return directionIndex;
        }


        public float GetCurrentRotationalVelocityPercentage() {
            return Mathf.Abs(currentRotationalVelocity) / maximumRotationalVelocity;
        }
    }
}