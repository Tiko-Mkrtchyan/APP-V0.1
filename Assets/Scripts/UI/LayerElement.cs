using AR_Features;
using ConjureKit;
using Datas;
using Gameobjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LayerElement: MonoBehaviour
    {
        [SerializeField] private Button layerButton;
    [SerializeField] private Button renameButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private TMP_InputField userInputField;
    public TextMeshProUGUI layerName;
    public LayersData _layersData;
    public bool thisLayerIsActive;
    [SerializeField] private Button layerSelectorButton;

    private PanelController _panelController;
    private ConjureKitManager _conjureKitManager;
    private ArManager _arManager;

    private void Start()
    {
        InitializeComponents();
        RegisterButtonListeners();
        SetInitialButtonStates();
    }

    private void OnEnable()
    {
        layerSelectorButton = transform.Find("Select").GetComponent<Button>();
        thisLayerIsActive = false;
    }

    private void InitializeComponents()
    {
        _panelController = transform.parent.GetComponent<PanelController>();
        _conjureKitManager = GameObject.Find("ConjureKitManager").GetComponent<ConjureKitManager>();
        _arManager = GameObject.Find("XR Origin").GetComponent<ArManager>();
    }

    private void RegisterButtonListeners()
    {
        layerButton.onClick.AddListener(ToggleRenameAndSelectorButtons);
        renameButton.onClick.AddListener(ShowRenameInputField);
        deleteButton.onClick.AddListener(DeleteLayer);
        userInputField.onEndEdit.AddListener(ConfirmRenaming);
        layerSelectorButton.onClick.AddListener(SelectLayer);
    }

    private void SetInitialButtonStates()
    {
        layerSelectorButton.gameObject.SetActive(false);
        userInputField.gameObject.SetActive(false);
    }

    private void ToggleRenameAndSelectorButtons()
    {
        bool isActive = renameButton.gameObject.activeSelf;
        renameButton.gameObject.SetActive(!isActive);
        layerSelectorButton.gameObject.SetActive(!isActive);
    }

    private void ShowRenameInputField()
    {
        userInputField.gameObject.SetActive(true);
        layerName.text = string.Empty;
    }

    private void DeleteLayer()
    {
        StartCoroutine(PocketBaseOperations.DeleteVirtualAssetsByLayer(_layersData.id, layerButton.gameObject));
        _panelController.activeLayerId = null;
        _panelController.chosenLayer = null;
        StartCoroutine(PocketBaseOperations.DeleteLayer(_layersData));
        DestroyAllObjects();
    }

    public void Init(LayersData data)
    {
        if (data == null) return;

        _layersData = data;
        layerName.text = data.name;
    }

    private void ConfirmRenaming(string newName)
    {
        layerName.text = newName;
        _layersData.name = newName;
        PocketBaseOperations.UpdateLayerName(_layersData);
    }

    private void SelectLayer()
    {
        SetLayerActive();
        DeactivateOtherLayers();
        _panelController.GetActiveLayer();
        DestroyAllObjects();
        StartCoroutine(PocketBaseOperations.GetFromVirtualAssetsByLayer(_panelController.activeLayerId));
    }

    private void SetLayerActive()
    {
        thisLayerIsActive = true;
        deleteButton.gameObject.SetActive(true);
        layerSelectorButton.gameObject.SetActive(!layerSelectorButton.gameObject.activeSelf);
        _conjureKitManager.informationText.gameObject.SetActive(false);
    }

    private void DeactivateOtherLayers()
    {
        foreach (var layer in FindObjectsOfType<LayerElement>())
        {
            if (layer != this)
            {
                layer.thisLayerIsActive = false;
            }
        }
    }

    private void DestroyAllObjects()
    {
        foreach (var obj in FindObjectsOfType<CubeScript>())
        {
            Destroy(obj.gameObject);
        }
    }
    }
}