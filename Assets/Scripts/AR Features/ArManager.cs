using System.Collections.Generic;
using ConjureKit;
using Model;
using UI;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace AR_Features
{
    [RequireComponent(typeof(ARRaycastManager))]
    public class ArManager : MonoBehaviour
    {
        [SerializeField] private ARRaycastManager arManager;
        [SerializeField] private Camera arCamera;
        [SerializeField] private ConjureKitManager conjureKitManager;
        [SerializeField] private PanelController panelController;
        public FurniturePlacer furniturePlacer;

        private readonly List<ARRaycastHit> _hits = new List<ARRaycastHit>();

        private void Start()
        {
            arManager = GetComponent<ARRaycastManager>();
        }

        private void Update()
        {
            HandleTouchInput();
        }

        private void HandleTouchInput()
        {
            if (conjureKitManager.LayersActive && Input.touchCount > 0 && !string.IsNullOrEmpty(panelController.activeLayerId))
            {
                Touch touch = Input.GetTouch(0);
                if (arManager.Raycast(touch.position, _hits, TrackableType.PlaneWithinPolygon) && touch.phase==TouchPhase.Began&&!string.IsNullOrEmpty(panelController.activeLayerId))
                {
                    Pose hitPose = _hits[0].pose;
                    GameObject createdCube =
                        furniturePlacer.PlaceSelectedFurniture(hitPose.position, hitPose.rotation);
                    Pose objPose = new Pose(createdCube.transform.position, createdCube.transform.rotation);
                    PocketBaseOperations.UploadToVirtualAssetsDB(conjureKitManager.currentDomainId, objPose,
                        panelController.activeLayerId, createdCube.transform.name);
                }
            }
        }
    }
}
