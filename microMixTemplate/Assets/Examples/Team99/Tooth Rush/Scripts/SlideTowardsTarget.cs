using UnityEngine;

namespace team99
{
    public class SlideTowardsTarget : MonoBehaviour
    {
        public Transform target;
        private Vector3 initialPosition;
        public float maxRadius = 0.25f;
        public float lerpFactor = 5f;

        void Awake() {
            initialPosition = transform.localPosition;
        }

        void Update() {
            if (target != null) {
                Vector3 direction = target.position - transform.position;
                direction.z = 0;
                Vector3 constrainedPosition = initialPosition + Vector3.ClampMagnitude(direction, maxRadius);
                transform.localPosition = Vector3.Lerp(transform.localPosition, constrainedPosition, lerpFactor * Time.deltaTime);
            }
        }
    }
}
