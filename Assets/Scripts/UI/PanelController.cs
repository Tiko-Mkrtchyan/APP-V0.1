using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class PanelController : MonoBehaviour
    {
         private List<LayerElement> _layerElementList;

        private LayerElement _layerElement;

        public string activeLayerId;

        public GameObject chosenLayer;

        private void OnEnable()
        {
            _layerElementList = new List<LayerElement>();
            StartCoroutine(GetLayersFromChild());
        }

        public void GetActiveLayer()
        {
            foreach (var layer in _layerElementList)
            {
                if (layer.thisLayerIsActive)
                {
                    activeLayerId = layer.layersData.id;
                    chosenLayer = layer.gameObject;
                }
            }
        }

        private IEnumerator GetLayersFromChild()
        {
            yield return new WaitForSeconds(3f);
            for (int i = 0; i < transform.childCount; i++)
            {
                _layerElementList.Add(transform.GetChild(i).GetComponent<LayerElement>());
            }
        }
    }
}
