using System;
using System.Collections;
using System.Collections.Generic;
using Datas;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
public class InputFieldController : MonoBehaviour
{
    private Button confirmButton;
    private TMP_Text buttonText;
    private Button layerButton;
    private string layerName;
    private TMP_InputField userInputField;

    private void Start()
    {

        userInputField = GetComponent<TMP_InputField>();
        Debug.Log("input Field");
        layerButton = transform.parent.GetComponent<Button>();
        confirmButton = transform.Find("Confirm Button").GetComponent<Button>();
        confirmButton.onClick.AddListener(ConfirmRenaming);
        buttonText = layerButton.GetComponentInChildren<TMP_Text>();
        

    }


    

    void ConfirmRenaming()
    {
        
        gameObject.SetActive(false);
    }
    
}
