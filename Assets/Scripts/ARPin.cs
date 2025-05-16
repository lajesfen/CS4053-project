using UnityEngine;

public class ARPin : MonoBehaviour
{
    public string PinID { get; private set; }
    public string PinName { get; set; } = "Untitled";

    private Renderer _renderer;
    public void Initialize(string id)
    {
        PinID = id;
        PinName = $"Pin #{id.Substring(0, 4)}";

        _renderer = GetComponentInChildren<Renderer>();
    }

    public void SetColor(Color color)
    {
        if (_renderer != null)
        {
            color.a = _renderer.material.color.a;
            _renderer.material.color = color;
        }
    }

    public void SetAlpha(float alpha)
    {
        if (_renderer != null)
        {
            Color c = _renderer.material.color;
            c.a = alpha;
            _renderer.material.color = c;
        }
    }
}
