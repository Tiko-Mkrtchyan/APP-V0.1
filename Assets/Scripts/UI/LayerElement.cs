using System;
using Datas;
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
        
            

        private void Start()
        {
            
            layerButton.onClick.AddListener(EnableButtons);
            userInputField.gameObject.SetActive(false);
            renameButton.onClick.AddListener(RenameLayerButton);
            deleteButton.onClick.AddListener(DeleteLayer);
           userInputField.onEndEdit.AddListener(ConfirmRenaming);
           
        }

        private void OnEnable()
        {
            userInputField.gameObject.SetActive(false);
            Debug.Log("Button Enabled"+layerName.text);
        }

        void EnableButtons()
        {
       
        
            renameButton.gameObject.SetActive(!renameButton.gameObject.activeSelf);
            deleteButton.gameObject.SetActive(!deleteButton.gameObject.activeSelf);
       


        }

       
             
        void RenameLayerButton()
        {

            
            userInputField.gameObject.SetActive(true);
            layerName.text = "";
        }
        

        void DeleteLayer()
        { 
            PocketBaseOperations.DeleteLayer(_layersData);
            layerButton.gameObject.SetActive(false);
        }

        public void Init(LayersData data)
        {
            if (data==null)
            return;
            
            _layersData = data;
            layerName.text = data.name;
            Debug.Log("Init Called");
        }

        private void ConfirmRenaming(string name)
        {
            
            layerName.text = userInputField.text;
            _layersData.name = layerName.text;
            PocketBaseOperations.UpdateLayerName(_layersData);
            
            
        }
    }
}