using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UI
{
    public class FurnitureMenu : MonoBehaviour
    {
        public Furniture[] allPrefabs;
        public GameObject[] furniturePrefabs;

        private Dictionary<int, GameObject> _furnitureDictionary;
        private RectTransform _furniturePanel;
        private bool _panelMove;

        private void Start()
        {
            InitializeFurnitureDictionary();
            _furniturePanel = GetComponent<RectTransform>();
        }

        private void InitializeFurnitureDictionary()
        {
            _furnitureDictionary = new Dictionary<int, GameObject>();
            for (int i = 0; i < allPrefabs.Length; i++)
            {
                allPrefabs[i].furnitureNumber = i;
                if (i < furniturePrefabs.Length)
                {
                    _furnitureDictionary[i] = furniturePrefabs[i];
                }
            }
        }

        public GameObject GetFurniturePrefab(int furnitureNumber)
        {
            return _furnitureDictionary.GetValueOrDefault(furnitureNumber);
        }
        
        public void TogglePanel()
        {
            _panelMove =! _panelMove;
            if (_panelMove)
            {
                _furniturePanel.DOAnchorPosX(200 , 1f);
            }
            else
            {
                _furniturePanel.DOAnchorPosX(-200, 1f);
            }
        }
    }
}
