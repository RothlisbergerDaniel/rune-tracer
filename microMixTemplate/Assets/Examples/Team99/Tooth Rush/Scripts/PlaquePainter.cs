using UnityEngine;

namespace team99
{
    public class PlaquePainter : MonoBehaviour
    {
        public GameObject prefab;
        private PolygonCollider2D polygonCollider;
        public int instances = 50;

        public bool randomizeRotation = true;
        public bool randomizeScale = true;
        public Transform plaqueHolder;

        public float minScale = 0.5f;
        public float maxScale = 2.0f;

        private void Awake() {
            polygonCollider = GetComponent<PolygonCollider2D>();
            PaintPlaque();
        }

        private void PaintPlaque() {

            Vector3 colliderScale = polygonCollider.transform.localScale;

            for (int i = 0; i < instances; i++) {
                GameObject instance = Instantiate(prefab);       

                // Find a random position that's within the polygon collider
                Vector2 randomPosition = FindValidPosition(polygonCollider);

                // Adjust the position by the collider's scale
                Vector2 adjustedPosition = new Vector2(randomPosition.x, randomPosition.y);

                // Generate a random Z offset within ±0.5f to mitigate rendering order issues
                float randomZOffset = Random.Range(-0.5f, 0.5f);

                // Set world position with random Z offset
                instance.transform.position = new Vector3(adjustedPosition.x, adjustedPosition.y, plaqueHolder.transform.position.z + randomZOffset);

                // Set parent, maintaining the world position
                instance.transform.SetParent(plaqueHolder);

                // Randomize the rotation on the Z-axis
                if (randomizeRotation) {
                    float randomRotation = Random.Range(0f, 360f);
                    instance.transform.rotation = Quaternion.Euler(0f, 0f, randomRotation);
                }

                // Randomize the scale
                if (randomizeScale) {
                    float randomScale = Random.Range(minScale, maxScale);
                    instance.transform.localScale = new Vector3(randomScale, randomScale, 1f);
                }
            }
        }


        // Find a valid position within the polygon collider
        private Vector2 FindValidPosition(PolygonCollider2D collider) {
            Vector2 point;
            int maxAttempts = 100; // Prevents infinite loops
            for (int i = 0; i < maxAttempts; i++) {
                point = new Vector2(
                    Random.Range(collider.bounds.min.x, collider.bounds.max.x),
                    Random.Range(collider.bounds.min.y, collider.bounds.max.y)
                );

                if (collider.OverlapPoint(point)) {
                    // Visualize the valid point for debugging
                    //DebugDrawPoint(point, Color.green);
                    return point;
                } else {
                    // Visualize the invalid point for debugging
                    //DebugDrawPoint(point, Color.red);
                }
            }

            Debug.LogError("No valid point found within the polygon collider after " + maxAttempts + " attempts.");
            return Vector2.zero; // Return a default value
        }

        // Debugging method to draw points in the scene view
        private void DebugDrawPoint(Vector2 point, Color color) {
            Debug.DrawLine(point - Vector2.up * 0.1f, point + Vector2.up * 0.1f, color, 5f);
            Debug.DrawLine(point - Vector2.left * 0.1f, point + Vector2.left * 0.1f, color, 5f);
        }
    }
}