using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class UIRectObject : MonoBehaviour
{
    private RectTransform _rectangleRectTransform;
    [SerializeField] private Text _labelText; // Reference set via Inspector

    public void Awake()
    {
        _rectangleRectTransform = GetComponent<RectTransform>();

        // Optionally find the text if not manually assigned
        if (_labelText == null)
            _labelText = GetComponentInChildren<Text>();
    }

    public void SetRectTransform(Rect rect)
    {
        _rectangleRectTransform.anchoredPosition = new Vector2(rect.x, rect.y);
        _rectangleRectTransform.sizeDelta = new Vector2(rect.width, rect.height);

        if (_labelText != null)
        {
            // Keep the label centered and use fixed size
            RectTransform textTransform = _labelText.GetComponent<RectTransform>();
            textTransform.anchoredPosition = Vector2.zero;
            textTransform.sizeDelta = new Vector2(100, 40); // Fixed size box
            _labelText.alignment = TextAnchor.MiddleCenter;
            _labelText.resizeTextForBestFit = false; // Turn off auto-resize
            _labelText.fontSize = 24; // Set consistent font size
        }
    }

    public void SetText(string text)
    {
        if (_labelText != null)
            _labelText.text = text;
    }

    public RectTransform getRectTransform()
    {
        return _rectangleRectTransform;
    }
}
