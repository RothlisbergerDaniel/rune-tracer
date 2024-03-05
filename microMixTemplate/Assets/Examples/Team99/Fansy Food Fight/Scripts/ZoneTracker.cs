using System.Collections.Generic;
using UnityEngine;
namespace team99
{
    public class ZoneTracker : MonoBehaviour
    {
        public string objectTag = "Tag9";
        public List<Rigidbody> objectsWithinTrigger = new List<Rigidbody>();

        void OnTriggerEnter(Collider other) {
            if (other.gameObject.CompareTag(objectTag)) {
                if (!objectsWithinTrigger.Contains(other.attachedRigidbody)) {
                    objectsWithinTrigger.Add(other.attachedRigidbody);
                }
            }
        }

        void OnTriggerExit(Collider other) {
            if (other.gameObject.CompareTag(objectTag)) {
                objectsWithinTrigger.Remove(other.attachedRigidbody);
            }
        }
    }

}