using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Datas;
using UI;

public class LayersController : MonoBehaviour
{
    
    public Button CreateNew;
    public Button buttonPrefab; 
    public Transform panel;
    private ConjureKitManager _conjureKitManager;
    private void Start()
    {
        _conjureKitManager = new ConjureKitManager();
        // CreateNew.onClick.AddListener(()=>CreateNewButton(new LayersData()));
        CreateNew.onClick.AddListener(CreateNewButton);
        
    }
    private void OnEnable()
    {
        StartCoroutine(PocketBaseOperations.GetLayersNames(ReceiveData));
        
    }
    void ReceiveData(LayersDataList _layersDataList)
    {
        
        foreach (var data in _layersDataList.items)
        {
            GetActiveButtons(data);
        }
        Debug.Log("Receive Data Void Called");
    }

    void CreateNewButton()
    {
        string layerName;
        Button newButton = Instantiate(buttonPrefab);
        newButton.transform.SetParent(panel, false);
        int childCount = panel.childCount;
        Vector3 newPosition = new Vector3(0f, -childCount * buttonPrefab.GetComponent<RectTransform>().rect.height, 0f);
        newButton.GetComponent<RectTransform>().anchoredPosition = newPosition;
        layerName = newButton.GetComponentInChildren<TMP_Text>().text;
        PocketBaseOperations.UploadLayerToDB(layerName,_conjureKitManager._currentDomainId);
        newButton.transform.Find("InputField").gameObject.SetActive(true);
    }

    void GetActiveButtons(LayersData datas)
    {
        Button newButton = Instantiate(buttonPrefab);
        newButton.GetComponent<LayerElement>().Init(datas);
        newButton.transform.SetParent(panel, false);
        int childCount = panel.childCount;
        Vector3 newPosition = new Vector3(0f, -childCount * buttonPrefab.GetComponent<RectTransform>().rect.height, 0f);
        newButton.GetComponent<RectTransform>().anchoredPosition = newPosition;
        newButton.transform.Find("InputField").gameObject.SetActive(true);
        Debug.Log("Create Button void Called");
    }

}