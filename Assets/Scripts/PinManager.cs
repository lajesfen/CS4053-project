using System.Collections.Generic;
using Niantic.Lightship.AR.Utilities;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using System;

using System.IO;

public class PinManager : MonoBehaviour
{
    [SerializeField] private AROcclusionManager _occlusionManager;
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _prefabToSpawn;

    public string archivo; // 

    public static PinManager Instance { get; private set; }
    private Matrix4x4 _displayMatrix;
    private XRCpuImage? _depthImage;
    private ScreenOrientation? _latestScreenOrientation;

    private Dictionary<string, ARPin> _pins = new();
    private string _activePinID;

    public Dictionary<string, ARPin> Pins => _pins;
    public ARPin ActivePin => string.IsNullOrEmpty(_activePinID) ? null : _pins.GetValueOrDefault(_activePinID);

    public event Action OnActivePinChanged;

    private bool _pinsVisible = false;

    //private void Awake()
    //{
    //    Instance = this;
    //}

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
        if (Input.touchCount <= 0)
            return;

        var touch = Input.GetTouch(0);

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        if (touch.phase != TouchPhase.Began)
            return;

        var screenPosition = touch.position;

        if (_depthImage is { valid: true })
        {
            var uv = new Vector2(screenPosition.x / Screen.width, screenPosition.y / Screen.height);
            var eyeDepth = _depthImage.Value.Sample<float>(uv, _displayMatrix);
            var worldPosition = _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, eyeDepth));

            var newPinGO = Instantiate(_prefabToSpawn, worldPosition, Quaternion.identity);
            var pinComponent = newPinGO.AddComponent<ARPin>();
            var pinID = System.Guid.NewGuid().ToString();
            pinComponent.Initialize(pinID);

            _pins.Add(pinID, pinComponent);

            SetActivePin(pinID);
        }
    }

    public void SetActivePin(string pinID)
    {
        if (_activePinID == pinID) return;
        _activePinID = pinID;

        foreach (var pin in _pins.Values)
        {
            bool isActive = pin.PinID == pinID;
            pin.SetAlpha(isActive ? 1f : 0.3f);
        }

        _pinsVisible = false;
        OnActivePinChanged?.Invoke();
    }

    public void ToggleAllPins()
    {
        _pinsVisible = !_pinsVisible;

        foreach (var pin in _pins.Values)
        {
            if (pin != ActivePin)
                pin.SetAlpha(_pinsVisible ? 1f : 0.3f);
        }
    }

    // ----------------------------------------------- 
    // Clase auxiliar para representar un pin serializable
    [Serializable]
    public class PinData
    {
        public string id;
        public Vector3 position;
        public Quaternion rotation;
    }

    // Clase contenedora para serializar/deserializar varios pines
    [Serializable]
    public class Datos
    {
        public List<PinData> pins = new();
    }

    private Datos datos;

    private void Awake()
    {
        Instance = this;
        archivo = Application.dataPath + "/datapin.json";
    }

    private void CargarDatos()
    {
        if (File.Exists(archivo))
        {
            string contenido = File.ReadAllText(archivo);
            datos = JsonUtility.FromJson<Datos>(contenido);

            foreach (var pinData in datos.pins)
            {
                var go = Instantiate(_prefabToSpawn, pinData.position, pinData.rotation);
                var pinComponent = go.AddComponent<ARPin>();
                pinComponent.Initialize(pinData.id);
                _pins.Add(pinData.id, pinComponent);
            }
        }
        else
        {
            Debug.Log("El archivo no existe");
            datos = new Datos(); // Inicializado vacío si no existe
        }
    }

    private void Start()
    {
        CargarDatos();
    }

    private void GuardarDatos()
    {
        Datos newDatos = new Datos();

        foreach (var pin in _pins.Values)
        {
            PinData pd = new PinData
            {
                id = pin.PinID,
                position = pin.transform.position,
                rotation = pin.transform.rotation
            };
            newDatos.pins.Add(pd);
        }

        string cadenaJSON = JsonUtility.ToJson(newDatos);
        File.WriteAllText(archivo, cadenaJSON);
    }

    private void OnApplicationQuit()
    {
        GuardarDatos();
    }

}
