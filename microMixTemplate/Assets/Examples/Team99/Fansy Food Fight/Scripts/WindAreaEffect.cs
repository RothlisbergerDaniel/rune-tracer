using UnityEngine;
namespace team99
{
    public class WindAreaEffect : MonoBehaviour
    {
        public float windForce = 10f;
        public float effectDistance = 3f;
        public AnimationCurve distanceIntensityCurve;


        public bool useProp;
        public PropellerSpin prop;
        public float propVelocityPercent;

        public ParticleSystem windParticles;

        public float windParticleMaxSpeed = 10f;

        public float minStartLifetime = 2f;
        public float maxStartLifetime = 5f;

        public float activationThreshold = 0.2f;

        public ZoneTracker objectsInWindCollider;

        public float colliderDepth;
        private AudioSource windAudioSource;

        public float volumeChangeSpeed = 0.5f;
        public float pitchChangeSpeed = 0.5f;

        private void Awake() {
            windAudioSource = GetComponent<AudioSource>();
        }

        private void Update() {

            if (!useProp) return;

            propVelocityPercent = prop.GetCurrentRotationalVelocityPercentage();

            // Enable or disable the emission module based on the velocity threshold
            var emissionModule = windParticles.emission;
            emissionModule.enabled = (propVelocityPercent > activationThreshold);

            var mainModule = windParticles.main;
            mainModule.startSpeed = windParticleMaxSpeed * propVelocityPercent;

            // Scale the start lifetime of the particles between two constants based on propVelocityPercent
            float lifetime = Mathf.Lerp(minStartLifetime, maxStartLifetime, propVelocityPercent);
            mainModule.startLifetime = new ParticleSystem.MinMaxCurve(minStartLifetime, lifetime);

            float targetVolume = Mathf.Lerp(0.0f, 1f, propVelocityPercent);
            float targetPitch = Mathf.Lerp(0.2f, 1f, propVelocityPercent);

            // Gradually adjust volume and pitch towards their target values
            windAudioSource.volume = Mathf.MoveTowards(windAudioSource.volume, targetVolume, volumeChangeSpeed * Time.deltaTime);
            windAudioSource.pitch = Mathf.MoveTowards(windAudioSource.pitch, targetPitch, pitchChangeSpeed * Time.deltaTime);

        }


        private void FixedUpdate() {
            // Apply wind force
            propVelocityPercent = prop.GetCurrentRotationalVelocityPercentage();
            foreach (Rigidbody go in objectsInWindCollider.objectsWithinTrigger) {
                float distanceToTarget = Vector3.Distance(transform.position, go.transform.position);
                if (distanceToTarget < effectDistance) {
                    float forceApplied = windForce * distanceIntensityCurve.Evaluate(distanceToTarget / effectDistance) * propVelocityPercent;
                    go.AddForce(transform.forward * forceApplied, ForceMode.Force);
                }
            }
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Vector3 direction = transform.forward * effectDistance;
            Gizmos.DrawLine(transform.position, transform.position + direction);
        }
    }

}