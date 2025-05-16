using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PinUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text pinNameText;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private Button hideButton;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button listButton;

    private void Start()
    {
        hideButton.onClick.AddListener(ToggleCurrentPin);
        editButton.onClick.AddListener(EditCurrentPin);
        deleteButton.onClick.AddListener(DeleteCurrentPin);
        listButton.onClick.AddListener(ListAllPins);

        PinManager.Instance.OnActivePinChanged += RefreshUI;

        RefreshUI();
    }

    private void Update()
    {
        var activePin = GetCurrentPin();
        if (activePin != null)
        {
            distanceText.text = $"{Vector3.Distance(activePin.transform.position, Camera.main.transform.position):F2}";
        }
    }

    private ARPin GetCurrentPin() => PinManager.Instance.ActivePin;

    private void ToggleCurrentPin()
    {
        GetCurrentPin()?.ToggleVisibility();
    }

    private void EditCurrentPin()
    {

    }

    private void DeleteCurrentPin()
    {
        var activePin = GetCurrentPin();
        if (activePin != null)
        {
            var pinsDict = PinManager.Instance.Pins;
            var activeID = activePin.PinID;

            pinsDict.Remove(activeID);
            Destroy(activePin.gameObject);

            string newActiveID = null;
            foreach (var pinID in pinsDict.Keys)
            {
                newActiveID = pinID;
            }

            PinManager.Instance.SetActivePin(newActiveID);
            RefreshUI();
        }
    }

    private void ListAllPins()
    {

    }

    private void RefreshUI()
    {
        var activePin = GetCurrentPin();
        if (activePin != null)
        {
            pinNameText.text = activePin.PinName;
            distanceText.text = $"{Vector3.Distance(activePin.transform.position, Camera.main.transform.position):F2}";
        }
        else
        {
            pinNameText.text = "N/A";
            distanceText.text = "0";
        }
    }

}
