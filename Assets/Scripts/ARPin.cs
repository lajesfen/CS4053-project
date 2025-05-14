using UnityEngine;

public class ARPin : MonoBehaviour
{
    public string PinID { get; private set; }

    public void Initialize(string id)
    {
        PinID = id;
    }

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
