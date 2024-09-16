using ConjureKit;
using Model;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace UI
{
    public class LayersController : MonoBehaviour
    {
        public Button createNew;
        public Button buttonPrefab;
        public Transform panel;

        private ConjureKitManager _conjureKitManager;

        private void Start()
        {
            _conjureKitManager = FindObjectOfType<ConjureKitManager>();
            createNew.onClick.AddListener(CreateNewButton);
            StartCoroutine(PocketBaseOperations.GetLayersNames(ReceiveLayerData));
        }

        private void ReceiveLayerData(LayersDataList layersDataList)
        {
            foreach (var data in layersDataList.items)
            {
                CreateButton(data);
            }
        }

        private void CreateNewButton()
        {
            Button newButton = Instantiate(buttonPrefab, panel);
            SetButtonPosition(newButton);

            var inputField = newButton.transform.Find("InputField").gameObject;
            inputField.SetActive(true);

            var layerName = inputField.GetComponent<TMP_InputField>().text;
            PocketBaseOperations.UploadLayerToDB(layerName, _conjureKitManager.currentDomainId, layerData =>
            {
                newButton.GetComponent<LayerElement>().Init(layerData);
            });
        }

        private void CreateButton(LayersData data)
        {
            Button newButton = Instantiate(buttonPrefab, panel);
            newButton.GetComponent<LayerElement>().Init(data);
            SetButtonPosition(newButton);

            newButton.transform.Find("InputField").gameObject.SetActive(true);
        }

        private void SetButtonPosition(Button button)
        {
            int childCount = panel.childCount;
            float buttonHeight = buttonPrefab.GetComponent<RectTransform>().rect.height;
            Vector3 newPosition = new Vector3(0f, -childCount * buttonHeight, 0f);
            button.GetComponent<RectTransform>().anchoredPosition = newPosition;
        }
    }
}