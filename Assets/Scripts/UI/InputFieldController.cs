using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InputFieldController : MonoBehaviour
    {
        private Button _confirmButton;
        private TMP_Text _buttonText;
        private Button _layerButton;
        private TMP_InputField _userInputField;

        private void Start()
        {
            InitializeComponents();
            RegisterButtonListeners();
        }

        private void InitializeComponents()
        {
            _userInputField = GetComponent<TMP_InputField>();
            _layerButton = transform.parent.GetComponent<Button>();
            _confirmButton = transform.Find("Confirm Button").GetComponent<Button>();
            _buttonText = _layerButton.GetComponentInChildren<TMP_Text>();
        }

        private void RegisterButtonListeners()
        {
            _confirmButton.onClick.AddListener(ConfirmRenaming);
        }

        private void ConfirmRenaming()
        {
            gameObject.SetActive(false);
        }
    }
}
