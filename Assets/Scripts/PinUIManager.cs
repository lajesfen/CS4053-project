using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PinUIManager : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private TMP_Text pinNameText;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private Button hideButton;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button listButton;


    [Header("Edit Panel")]
    [SerializeField] private GameObject editPanel;
    [SerializeField] private TMP_InputField editInputField;
    [SerializeField] private Button editAcceptButton;
    [SerializeField] private Button editCancelButton;

    [Header("Delete Confirmation Panel")]
    [SerializeField] private GameObject deletePanel;
    [SerializeField] private Button deleteYesButton;
    [SerializeField] private Button deleteNoButton;

    [Header("List Panel")]
    [SerializeField] private GameObject listPanel;
    [SerializeField] private Transform listContent;
    [SerializeField] private GameObject listItemPrefab;

    private void Start()
    {
        hideButton.onClick.AddListener(ToggleAllPins);
        editButton.onClick.AddListener(ShowEditPanel);
        deleteButton.onClick.AddListener(ShowDeletePanel);
        listButton.onClick.AddListener(ShowListPanel);

        editAcceptButton.onClick.AddListener(AcceptEdit);
        editCancelButton.onClick.AddListener(() => editPanel.SetActive(false));

        deleteYesButton.onClick.AddListener(ConfirmDelete);
        deleteNoButton.onClick.AddListener(() => deletePanel.SetActive(false));

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

    private void ToggleAllPins()
    {
        PinManager.Instance.ToggleAllPins();
    }

    private void ShowEditPanel()
    {
        var activePin = GetCurrentPin();
        if (activePin != null)
        {
            editInputField.text = activePin.PinName;
            editPanel.SetActive(true);
        }
    }

    private void AcceptEdit()
    {
        var activePin = GetCurrentPin();
        if (activePin != null)
        {
            activePin.PinName = editInputField.text;
            RefreshUI();
        }
        editPanel.SetActive(false);
    }

    private void ShowDeletePanel()
    {
        if (GetCurrentPin() != null)
        {
            deletePanel.SetActive(true);
        }
    }

    private void ConfirmDelete()
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

        deletePanel.SetActive(false);
    }

    private void ShowListPanel()
    {
        listPanel.SetActive(true);

        foreach (Transform child in listContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var pin in PinManager.Instance.Pins.Values)
        {
            var item = Instantiate(listItemPrefab, listContent);
            var text = item.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = pin.PinName;
            }

            var button = item.GetComponentInChildren<Button>();
            if (button != null)
            {
                string pinID = pin.PinID;
                button.onClick.AddListener(() =>
                {
                    PinManager.Instance.SetActivePin(pinID);
                    RefreshUI();
                    listPanel.SetActive(false);
                });
            }
        }
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
