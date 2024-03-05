using UnityEditor;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public enum SplitScreenMode
    {
        SingleScreen,
        SideBySide,
        TopAndBottom
    }

    public Camera firstScreenCamera;
    public Camera secondScreenCamera;

    private float virtualScreenWidth = 1280f;
    private float virtualScreenHeight = 1024f;
    private float targetAspectRatio;
    private float currentAspectRatio;

    public SplitScreenMode currentSplitScreenMode = SplitScreenMode.SingleScreen;


    private void OnValidate() {
        if (!firstScreenCamera)
        {
            if (!TryGetComponent(out firstScreenCamera))
            {
                Debug.LogWarning("CameraManager: No Camera component found on the GameObject. Please assign firstScreenCamera manually in the inspector.", this);
            }
        }

        if (currentSplitScreenMode != SplitScreenMode.SingleScreen && !secondScreenCamera) {
            Debug.LogWarning("CameraManager: Split screen mode is set to " + currentSplitScreenMode + " but no secondScreenCamera is assigned. Please assign a secondScreenCamera manually in the inspector.", this);
        }
    }

    void Start() {
        AdjustCameraViewport();
    }

    public void AdjustCameraViewport() {

        if (firstScreenCamera == null) {
            Debug.LogError("CameraManager: firstScreenCamera must be defined. Disabling SplitScreenAspectRatioController script.");
            this.enabled = false;
            return;
        } else if (currentSplitScreenMode != SplitScreenMode.SingleScreen && secondScreenCamera == null) {
            Debug.LogError("CameraManager: Both firstScreenCamera and secondScreenCamera must be defined. Disabling SplitScreenAspectRatioController script.");
            this.enabled = false;
            return;

        }


        targetAspectRatio = virtualScreenWidth / virtualScreenHeight;

        currentAspectRatio = (float)Screen.width / (float)Screen.height;

#if UNITY_EDITOR
        // Get the resolution of the game view in the Unity Editor
        string screenRes = UnityStats.screenRes;
        string[] res = screenRes.Split('x');
        currentAspectRatio = float.Parse(res[0]) / float.Parse(res[1]);
#endif

        float viewportX = 0, viewportY = 0, viewportWidth = 1, viewportHeight = 1;

        switch (currentSplitScreenMode) {
            case SplitScreenMode.SingleScreen:
                if (secondScreenCamera != null) {
                    secondScreenCamera.enabled = false;
                }

                if (currentAspectRatio > targetAspectRatio) {
                    // Wider screen: Adjust width, center horizontally, adds vertical bars.
                    viewportWidth = targetAspectRatio / currentAspectRatio;
                    viewportX = (1f - viewportWidth) * 0.5f;

                } else if (currentAspectRatio < targetAspectRatio) {
                    // Taller screen: Adjust height, center vertically, adds horizontal bars.
                    viewportHeight = currentAspectRatio / targetAspectRatio;
                    viewportY = (1f - viewportHeight) * 0.5f;
                }
                break;

            case SplitScreenMode.SideBySide:
                secondScreenCamera.enabled = true;

                if (currentAspectRatio > targetAspectRatio) {
                    // Wider screen: Adjust width, adjust center, adds vertical bars.
                    viewportWidth = (targetAspectRatio / currentAspectRatio) * 0.5f;
                    viewportX = (1f - 2f * viewportWidth) * 0.5f;
                } else if (currentAspectRatio < targetAspectRatio) {
                    // Taller screen: Adjust height, adjust center, adds horizontal bars.
                    viewportHeight = currentAspectRatio / targetAspectRatio;
                    viewportY = (1f - viewportHeight) * 0.5f;
                    viewportWidth = 0.5f;
                } else {
                    viewportWidth = 0.5f;
                }
                break;

            case SplitScreenMode.TopAndBottom:

                secondScreenCamera.enabled = true;
                if (currentAspectRatio > targetAspectRatio) {
                    // Wider screen: Adjust width, adjust center, adds vertical bars.
                    viewportWidth = targetAspectRatio / currentAspectRatio;
                    viewportX = (1f - viewportWidth) * 0.5f;
                    viewportY = (1f - 2f * viewportHeight) * 0.5f;
                } else if (currentAspectRatio < targetAspectRatio) {
                    // Taller screen: Adjust height, adjust center, adds horizontal bars.
                    viewportWidth = targetAspectRatio / currentAspectRatio;
                    viewportHeight = (currentAspectRatio / targetAspectRatio) * 0.5f;
                    viewportY = (1f - 2f * viewportHeight) * 0.5f;
                } else {
                    viewportHeight = 0.5f;
                }
                break;

        }
        SetCameraViewports(viewportX, viewportY, viewportWidth, viewportHeight);
    }

    private void SetCameraViewports(float x, float y, float width, float height) {
        firstScreenCamera.rect = new Rect(x, y, width, height);

        if (currentSplitScreenMode == SplitScreenMode.SideBySide) {
            secondScreenCamera.rect = new Rect(0.5f, y, width, height);
        } else if (currentSplitScreenMode == SplitScreenMode.TopAndBottom) {
            secondScreenCamera.rect = new Rect(x, 0.5f, width, height);
        }
    }
}
