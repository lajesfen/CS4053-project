using UnityEngine;

public class ARPin : MonoBehaviour
{
    public string PinID { get; private set; }
    public string PinName { get; set; } = "Untitled";
    private bool isVisible = true;

    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Initialize(string id)
    {
        PinID = id;
        PinName = $"Pin {id.Substring(0, 4)}";
    }

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
        isVisible = visible;
    }

    public void ToggleVisibility()
    {
        SetVisibility(!isVisible);
    }
}
