using Niantic.Lightship.AR.Utilities;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class Depth_ScreenToWorldPosition : MonoBehaviour
{
    [SerializeField]
    private AROcclusionManager _occlusionManager;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private GameObject _prefabToSpawn;

    private Matrix4x4 _displayMatrix;
    private XRCpuImage? _depthImage;
    private ScreenOrientation? _latestScreenOrientation;

    private void Update()
    {
        UpdateImage();
        UpdateDisplayMatrix();
        HandleTouch();
    }

    private void OnDestroy()
    {
        // Dispose the cached depth image
        _depthImage?.Dispose();
    }

    private void UpdateImage()
    {
        if (!_occlusionManager.subsystem.running)
        {
            return;
        }

        if (_occlusionManager.TryAcquireEnvironmentDepthCpuImage(out var image))
        {
            // Dispose the old image
            _depthImage?.Dispose();

            // Cache the new image
            _depthImage = image;
        }
    }

    private void UpdateDisplayMatrix()
    {
        // Make sure we have a valid depth image
        if (_depthImage is {valid: true})
        {
            // The display matrix only needs to be recalculated if the screen orientation changes
            if (!_latestScreenOrientation.HasValue ||
                _latestScreenOrientation.Value != XRDisplayContext.GetScreenOrientation())
            {
                _latestScreenOrientation = XRDisplayContext.GetScreenOrientation();
                _displayMatrix = CameraMath.CalculateDisplayMatrix(
                    _depthImage.Value.width,
                    _depthImage.Value.height,
                    Screen.width,
                    Screen.height,
                    _latestScreenOrientation.Value,
                    invertVertically: true);
            }
        }
    }

    private void HandleTouch()
    {
        // In the editor we want to use mouse clicks, on phones we want touches.
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            var screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
#else
        // If there is no touch or touch selects UI element
        if (Input.touchCount <= 0)
            return;
        var touch = Input.GetTouch(0);

        // Only count touches that just began
        if (touch.phase == UnityEngine.TouchPhase.Began)
        {
            var screenPosition = touch.position;
#endif
            // Do something with touches
            if (_depthImage is {valid: true})
            {
                // Sample eye depth
                var uv = new Vector2(screenPosition.x / Screen.width, screenPosition.y / Screen.height);
                var eyeDepth = _depthImage.Value.Sample<float>(uv, _displayMatrix);

                // Get world position
                var worldPosition =
                    _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, eyeDepth));

                // Spawn a thing on the depth map
                Instantiate(_prefabToSpawn, worldPosition, Quaternion.identity);
            }
        }
    }
}