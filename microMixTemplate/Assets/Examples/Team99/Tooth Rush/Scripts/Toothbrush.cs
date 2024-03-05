using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace team99
{
    public class Toothbrush : MicrogameInputEvents
    {
        public float lastHorizontalDirection = 0; // 0 indicates no direction has been pressed

        // Movement
        private Vector3 currentVelocity;
        public float movementAcceleration = 50f;
        public float movementFriction = 6f;

        // Burst movement
        public bool isBursting = false;
        public bool isOverTooth = false;
        public float burstDuration = 0.05f;
        public AnimationCurve burstCurve; // Curve controlling the burst speed over time
        public float burstIntensityMultiplier = 20f;
        public float burstCooldownDuration = 0.7f;
        private float timeSinceLastBurst = -Mathf.Infinity;
        private Coroutine burstCoroutine = null;

        // Shake
        public float shakeDuration = 0.5f;
        private Coroutine shakeCoroutine;
        public float shakeAmplitudeMultiplier = .002f;
        public float shakeFrequencyMultiplier = 100.0f;
        public AnimationCurve shakeAmplitudeEnvelopeCurve;

        // Camera
        public Camera playerCamera;
        public Vector2 cameraBottomLeft;
        public Vector2 cameraTopRight;

        // Particle system
        public ParticleSystem foamSpray;

        // Audio
        public AudioSource brushSoundsaudioSource;
        public AudioClip[] brushSounds;

        public float minPitchBrushSounds = 1f;
        public float maxPitchBrushSounds = 1.5f;


        public AudioSource whooshSoundsaudioSource;
        public AudioClip[] whooshSounds;
        public float minPitchWhooshSounds = 1f;
        public float maxPitchWhooshSounds = 1.5f;

        // Rendering and sprites
        public Transform toothbrushSprite;
        private Vector3 initialToothbrushSpritePosition;


        public Animator bristlesAnimator;
        public bool isToothClean = false;

        private Rigidbody2D rb;
        private BoxCollider2D boxCollider;
        public float linecastLength = 1.0f;
        public float edgeSafeZone = 0.10f;

        ParticleSystem.EmissionModule foamEmission;

        public ToothManager toothManager;

        public LayerMask idleLayer;

        public LayerMask brushingLayer;

        void Awake() {
            brushSoundsaudioSource = GetComponent<AudioSource>();
            rb = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();

            cameraBottomLeft = playerCamera.ViewportToWorldPoint(new Vector2(0, 0));
            cameraTopRight = playerCamera.ViewportToWorldPoint(new Vector2(1, 1));

            foamEmission = foamSpray.emission;
        }

        private void Start() {
            initialToothbrushSpritePosition = toothbrushSprite.localPosition;
            foamEmission.enabled = false;
        }

        protected override void OnEnable() {
            base.OnEnable();
            toothManager.ToothCleanedEvent += OnToothCleaned;

        }

        protected override void OnDisable() {
            base.OnDisable();
            toothManager.ToothCleanedEvent -= OnToothCleaned;
        }


        protected override void OnButton1Pressed(InputAction.CallbackContext context) {

            if (isBursting) return;


            // the cooldown has passed, and not already bursting
            if (lastHorizontalDirection != 0f && Time.time - timeSinceLastBurst > burstCooldownDuration) {
                // Check if too close to the edges
                bool tooCloseToLeftEdge = transform.position.x - cameraBottomLeft.x < edgeSafeZone;
                bool tooCloseToRightEdge = cameraTopRight.x - transform.position.x < edgeSafeZone;
                // If not too close to the edges, or moving away from the edge, allow the burst
                if (!((tooCloseToLeftEdge && lastHorizontalDirection < 0) || (tooCloseToRightEdge && lastHorizontalDirection > 0))) {
                    if (burstCoroutine != null) {
                        StopCoroutine(burstCoroutine);
                    }
                    // Start a new shake effect coroutine
                    burstCoroutine = StartCoroutine(BurstCoroutine());
                }
            }
        }
        void Update() {
            ApplyMovement();
        }

        [ContextMenu("TestLayers")] 
        void TestLayers() {
            Debug.Log($"Idle layer: {idleLayer} = {idleLayer.value} -> {idleLayer.FirstLayerId()}");
            Debug.Log($"Brush layer: {brushingLayer} = {brushingLayer.value} -> {brushingLayer.FirstLayerId()}");
        }


        IEnumerator BurstCoroutine() {
            // Flag to indicate currently in a burst movement
            isBursting = true;
            gameObject.layer = brushingLayer.FirstLayerId();
            // Record the start time of the burst for timing calculations
            timeSinceLastBurst = Time.time;

            // Determine the direction of the bristle animation based on movement direction
            ResetAndTriggerBristlesAnimation();

            // Check for contact with the tooth and start the shake effect if contact is made
            bool isContact = CheckForToothContact();
            if (isContact) {
                StartShakeEffect();
                foamEmission.enabled = true;
            }

            // Play the appropriate sound based on whether made contact with a tooth
            PlaySoundBasedOnContact(isContact);

            float burstCurveTime = 0;

            // Main loop for burst movement, interpolating based on the burst curve
            while (burstCurveTime < 1.0f) {
                // Calculate current time along the burst curve
                burstCurveTime = (Time.time - timeSinceLastBurst) / burstDuration;
                // Apply the burst curve to determine the addition to movement
                float burstAddition = burstCurve.Evaluate(burstCurveTime) * burstIntensityMultiplier;
                currentVelocity.x = lastHorizontalDirection * burstAddition;

                // Calculate new position and apply constraints to keep within camera bounds
                Vector2 newPosition = transform.position + currentVelocity * Time.deltaTime;
                newPosition.x = Mathf.Clamp(newPosition.x, cameraBottomLeft.x, cameraTopRight.x);
                newPosition.y = Mathf.Clamp(newPosition.y, cameraBottomLeft.y, cameraTopRight.y);

                // Update the position
                transform.position = newPosition;

                yield return null; // Wait until the next frame
            }

            gameObject.layer = idleLayer.FirstLayerId();
            foamEmission.enabled = false;
            // Mark the burst as complete
            isBursting = false;
        }

        void ResetAndTriggerBristlesAnimation() {
            // Reset animation triggers to ensure a clean start
            bristlesAnimator.ResetTrigger("Left");
            bristlesAnimator.ResetTrigger("Right");

            // Trigger the correct animation based on the last horizontal direction
            if (lastHorizontalDirection > 0) {
                bristlesAnimator.SetTrigger("Left");
            } else if (lastHorizontalDirection < 0) {
                bristlesAnimator.SetTrigger("Right");
            }
        }

        void StartShakeEffect() {
            // Stop any ongoing shake effect to prevent overlapping
            if (shakeCoroutine != null) {
                StopCoroutine(shakeCoroutine);
            }
            // Start a new shake effect coroutine
            shakeCoroutine = StartCoroutine(Shake());
        }


        public void OnToothCleaned() {
            if (burstCoroutine != null) {
                StopCoroutine(burstCoroutine);
                burstCoroutine = null;
            }

            if (shakeCoroutine != null) {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }
            rb.isKinematic = false;
            // fling the toothbrush into the air
            rb.AddForce(Vector2.up * 50, ForceMode2D.Impulse);
            this.enabled = false;
        }

        protected override void OnTimesUp() {
            base.OnTimesUp();

            if (!isToothClean) {
                rb.isKinematic = false;
                this.enabled = false;
            }
        }


        IEnumerator Shake() {
            toothbrushSprite.localPosition = initialToothbrushSpritePosition;

            // Record the start time of the shake
            float shakeStartTime = Time.time;
            float verticalOffset = 0f;
            float shakePhase = 0f;
            while (Time.time - shakeStartTime < shakeDuration) {


                // Calculate the curveTime specifically for shake
                float shakeCurveTime = (Time.time - shakeStartTime) / shakeDuration;
                // shake logic
                float envelopeScale = shakeAmplitudeEnvelopeCurve.Evaluate(shakeCurveTime);
                float amplitude = Mathf.Abs(currentVelocity.x) * shakeAmplitudeMultiplier * envelopeScale;
                verticalOffset = amplitude * Mathf.Sin(shakePhase * shakeFrequencyMultiplier);
                shakePhase += Time.deltaTime;

                toothbrushSprite.localPosition = initialToothbrushSpritePosition + new Vector3(0, verticalOffset, 0);

                yield return null;
            }
            toothbrushSprite.localPosition = initialToothbrushSpritePosition;

        }

        void ApplyMovement() {

            Vector2 direction = ((Vector2)stick).normalized;
            // Check if not bursting and there is significant horizontal input to determine burst direction.
            if (!isBursting && Mathf.Abs(direction.x) > 0.01f) {
                // Update the last horizontal direction
                lastHorizontalDirection = (int)Mathf.Sign(direction.x);
            }


            // Apply acceleration and friction to update the velocity
            Vector3 acceleration = direction * movementAcceleration;
            currentVelocity += acceleration * Time.deltaTime;
            currentVelocity *= 1 - movementFriction * Time.deltaTime;

            // Calculate new position based on whether the toothbrush is bursting
            Vector3 newPosition;
            if (isBursting) {
                // During a burst, only apply the vertical component of the velocity to the new position
                newPosition = transform.position + new Vector3(0, currentVelocity.y, 0) * Time.deltaTime;
            } else {
                // When not bursting, apply both horizontal and vertical components of the velocity
                newPosition = transform.position + currentVelocity * Time.deltaTime;
            }

            // Constrain to camera bounds
            newPosition.x = Mathf.Clamp(newPosition.x, cameraBottomLeft.x, cameraTopRight.x);
            newPosition.y = Mathf.Clamp(newPosition.y, cameraBottomLeft.y, cameraTopRight.y);

            // Apply the constrained position
            transform.position = newPosition;
        }



        void PlaySoundBasedOnContact(bool isContact) {
            if (isContact) {
                PlayRandomBrushSound();

            } else {
                PlayRandomWhooshSound();
            }
        }

        void PlayRandomWhooshSound() {
            int randomIndex = Random.Range(0, whooshSounds.Length);
            whooshSoundsaudioSource.clip = whooshSounds[randomIndex];
            whooshSoundsaudioSource.pitch = Random.Range(minPitchWhooshSounds, maxPitchWhooshSounds);
            whooshSoundsaudioSource.Play();
        }
        void PlayRandomBrushSound() {
            int randomIndex = Random.Range(0, brushSounds.Length);
            brushSoundsaudioSource.clip = brushSounds[randomIndex];
            brushSoundsaudioSource.pitch = Random.Range(minPitchBrushSounds, maxPitchBrushSounds);
            brushSoundsaudioSource.Play();
        }
        private void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.name == "Tooth") {
                isOverTooth = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.name == "Tooth") {
                isOverTooth = false;
            }
        }

        private bool CheckForToothContact() {
            if (isOverTooth) {
                return true;
            }

            // Get the collider's half height
            float colliderHalfHeight = boxCollider.size.y / 2;

            // Calculate the center top and center bottom points of the collider
            Vector2 centerTop = (Vector2)transform.position + Vector2.up * colliderHalfHeight;
            Vector2 centerBottom = (Vector2)transform.position - Vector2.up * colliderHalfHeight;

            // Calculate the end points of the linecasts
            Vector2 topEndPoint = centerTop + Vector2.right * linecastLength * lastHorizontalDirection;
            Vector2 bottomEndPoint = centerBottom + Vector2.right * linecastLength * lastHorizontalDirection;

            
            int layerMask = 1 << LayerMask.NameToLayer("Detect Player");

            // Perform the linecasts with the layer mask
            RaycastHit2D hitTop = Physics2D.Linecast(centerTop, topEndPoint, layerMask);
            RaycastHit2D hitBottom = Physics2D.Linecast(centerBottom, bottomEndPoint, layerMask);

            // Draw debug lines for visualization
            //Debug.DrawLine(centerTop, topEndPoint, Color.blue, 1.0f);
            //Debug.DrawLine(centerBottom, bottomEndPoint, Color.blue, 1.0f);

            // If either linecast hits, it means we have contact with a tooth (or whatever is set on Layer 6)
            return hitTop.collider != null || hitBottom.collider != null;
        }

    }
}