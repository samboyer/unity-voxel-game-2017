using UnityEngine;

using UnityEngine.PostProcessing;

[RequireComponent(typeof(Camera))]
public class CameraQualityController : MonoBehaviour
{
    public bool isVoxelEditorCamera;

    Camera cam;
    PostProcessingBehaviour postBehaviour;

    private void Start()
    {
        cam = GetComponent<Camera>();
        postBehaviour = GetComponent<PostProcessingBehaviour>();

        ApplyQualitySettings();

        GameQualityControls.QualityChanged += GameQualityControls_QualityChanged;
    }

    private void OnDestroy()
    {
        GameQualityControls.QualityChanged -= GameQualityControls_QualityChanged;
    }

    private void GameQualityControls_QualityChanged()
    {
        ApplyQualitySettings();
    }

    void ApplyQualitySettings()
    {
        GameQualityControls.QualityState state = GameQualityControls.currentState;

        cam.fieldOfView = state.cameraFOV;
        cam.farClipPlane = state.maxRenderDistance;

        postBehaviour.profile.ambientOcclusion.enabled = state.aoEnabled;
        postBehaviour.profile.antialiasing.enabled = state.fxaaEnabled;

        postBehaviour.profile.motionBlur.enabled = state.motionBlurEnabled && !isVoxelEditorCamera;
    }
}

