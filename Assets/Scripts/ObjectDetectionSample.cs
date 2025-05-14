using Niantic.Lightship.AR.ObjectDetection;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class ObjectDetectionSample : MonoBehaviour
{
    [SerializeField] private float _probabilityThreshold = 0.5f;
    [SerializeField] private ARObjectDetectionManager _objectDetectionManager;
    [SerializeField] private DrawRect _drawRect;
    [SerializeField] private float _detectionInterval = 2f; // Seconds between detection handling

    private Canvas _canvas;
    private float _lastDetectionTime;

    private Dictionary<string, string> _translationCache = new();

    private void Awake()
    {
        _canvas = FindObjectOfType<Canvas>();
    }

    public void Start()
    {
        _objectDetectionManager.enabled = true;
        _objectDetectionManager.MetadataInitialized += OnMetadataInitialized;
    }

    private void OnDestroy()
    {
        _objectDetectionManager.MetadataInitialized -= OnMetadataInitialized;
        _objectDetectionManager.ObjectDetectionsUpdated -= ObjectDetectionsUpdatedAsync;
    }

    private void OnMetadataInitialized(ARObjectDetectionModelEventArgs args)
    {
        _objectDetectionManager.ObjectDetectionsUpdated += ObjectDetectionsUpdatedAsync;
    }

    private async void ObjectDetectionsUpdatedAsync(ARObjectDetectionsUpdatedEventArgs args)
    {
        // Skip if interval has not passed
        if (Time.time - _lastDetectionTime < _detectionInterval)
            return;

        _lastDetectionTime = Time.time;

        var result = args.Results;
        if (result == null || result.Count == 0)
            return;

        _drawRect.ClearRects();

        int canvasHeight = Mathf.FloorToInt(_canvas.GetComponent<RectTransform>().rect.height);
        int canvasWidth = Mathf.FloorToInt(_canvas.GetComponent<RectTransform>().rect.width);

        foreach (var detection in result)
        {
            var categorizations = detection.GetConfidentCategorizations(_probabilityThreshold);
            if (categorizations.Count == 0)
                continue;

            categorizations.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));
            var bestCategory = categorizations[0];

            string name = bestCategory.CategoryName;

            // Use cached translation if available
            string translatedName;
            if (!_translationCache.TryGetValue(name, out translatedName))
            {
                translatedName = await TranslateAsync(name);
                _translationCache[name] = translatedName;
            }

            Rect rect = detection.CalculateRect(canvasWidth, canvasHeight, Screen.orientation);
            _drawRect.CreateRect(rect, translatedName);
        }
    }

    private async Task<string> TranslateAsync(string original)
    {
        // TODO: Replace this with your actual API call
        await Task.Delay(100); // Simulated latency
        return $"Translated({original})";
    }
}
