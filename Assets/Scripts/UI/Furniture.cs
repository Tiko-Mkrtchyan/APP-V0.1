using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    public class Furniture : MonoBehaviour
    {
        public bool thisFurnitureSelected;
        public int furnitureNumber;

        private Button _button;
        private static Furniture _currentlySelectedFurniture;

        [SerializeField] private GameObject layersPanel;

        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(SelectFurniture);
        }

        private void SelectFurniture()
        {
            if (_currentlySelectedFurniture != null)
            {
                _currentlySelectedFurniture.thisFurnitureSelected = false;
            }
            
            layersPanel.gameObject.SetActive(true);

            thisFurnitureSelected = true;
            _currentlySelectedFurniture = this;
        }

        public static int GetCurrentlySelectedFurnitureNumber()
        {
            if (_currentlySelectedFurniture != null)
            {
                return _currentlySelectedFurniture.furnitureNumber;
            }
            return -1;
        }
    }
}
