using System.Collections.Generic;
using Niantic.Lightship.AR.Utilities;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PinSpawner : MonoBehaviour
{
    [SerializeField] private AROcclusionManager _occlusionManager;
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _prefabToSpawn;

    private Matrix4x4 _displayMatrix;
    private XRCpuImage? _depthImage;
    private ScreenOrientation? _latestScreenOrientation;

    private Dictionary<string, ARPin> _pins = new();
    private string _activePinID;

    private void Update()
    {
        UpdateImage();
        UpdateDisplayMatrix();
        HandleTouch();
    }

    private void OnDestroy() => _depthImage?.Dispose();

    private void UpdateImage()
    {
        if (!_occlusionManager.subsystem.running)
            return;

        if (_occlusionManager.TryAcquireEnvironmentDepthCpuImage(out var image))
        {
            _depthImage?.Dispose();
            _depthImage = image;
        }
    }

    private void UpdateDisplayMatrix()
    {
        if (_depthImage is { valid: true })
        {
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
        if (Input.touchCount <= 0) return;

        var touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        var screenPosition = touch.position;

        if (_depthImage is { valid: true })
        {
            var uv = new Vector2(screenPosition.x / Screen.width, screenPosition.y / Screen.height);
            var eyeDepth = _depthImage.Value.Sample<float>(uv, _displayMatrix);
            var worldPosition = _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, eyeDepth));

            // Spawn and register pin
            var newPinGO = Instantiate(_prefabToSpawn, worldPosition, Quaternion.identity);
            var pinComponent = newPinGO.AddComponent<ARPin>();
            var pinID = System.Guid.NewGuid().ToString();
            pinComponent.Initialize(pinID);

            _pins.Add(pinID, pinComponent);

            // Optionally activate this pin by default
            SetActivePin(pinID);
        }
    }

    public void SetActivePin(string pinID)
    {
        _activePinID = pinID;

        foreach (var kvp in _pins)
        {
            kvp.Value.SetVisibility(kvp.Key == pinID);
        }
    }

    public void HideAllPins()
    {
        foreach (var pin in _pins.Values)
        {
            pin.SetVisibility(false);
        }
    }

    public void ShowAllPins()
    {
        foreach (var pin in _pins.Values)
        {
            pin.SetVisibility(true);
        }
    }
}