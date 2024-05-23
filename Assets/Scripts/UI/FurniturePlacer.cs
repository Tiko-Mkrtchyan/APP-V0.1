using UnityEngine;

namespace UI
{
    public class FurniturePlacer: MonoBehaviour
    {
        public FurnitureMenu furnitureManager;

        public GameObject PlaceSelectedFurniture(Vector3 position, Quaternion rotation)
        {
            int selectedFurnitureNumber = Furniture.GetCurrentlySelectedFurnitureNumber();
            if (selectedFurnitureNumber != -1)
            {
                GameObject prefab = furnitureManager.GetFurniturePrefab(selectedFurnitureNumber);
                if (prefab != null)
                {
                    GameObject instantiatedFurniture = Instantiate(prefab, position, rotation);
                    instantiatedFurniture.name = prefab.name;
                    Debug.Log("Placed Furniture Number: " + selectedFurnitureNumber);
                    return instantiatedFurniture;
                }
            }
            else
            {
                Debug.LogWarning("No furniture selected.");
            }
            return null;
        }
    }
}