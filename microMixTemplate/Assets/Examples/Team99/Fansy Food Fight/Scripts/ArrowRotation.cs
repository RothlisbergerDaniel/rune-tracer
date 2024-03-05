using UnityEngine;
namespace team99
{
    public class ArrowRotation : MonoBehaviour
    {
        public ZoneTracker leftZoneTracker;
        public ZoneTracker rightZoneTracker;

        public RectTransform arrow;

        public float minAngle = 0f;
        public float maxAngle = 180f;

        public int leftCount;
        public int rightCount;


        void Update() {
            leftCount = leftZoneTracker.objectsWithinTrigger.Count;
            rightCount = rightZoneTracker.objectsWithinTrigger.Count;

            int difference = rightCount - leftCount;

            int totalObjects = leftCount + rightCount;

            float angle = 0;

            if (totalObjects != 0) {
                float totalAngleRange = maxAngle - minAngle;

                // the total number of angle "slices"
                float anglePerObject = totalAngleRange / (totalObjects * 2f);

                angle = difference * anglePerObject;

            }

            arrow.localEulerAngles = new Vector3(0, 0, angle);
        }
    }
}