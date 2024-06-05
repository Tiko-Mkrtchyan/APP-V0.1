using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InputFieldController : MonoBehaviour
    {
        private Button _confirmButton;

        private void Start()
        {
            InitializeComponents();
            RegisterButtonListeners();
        }

        private void InitializeComponents()
        {
            _confirmButton = transform.Find("Confirm Button").GetComponent<Button>();
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
