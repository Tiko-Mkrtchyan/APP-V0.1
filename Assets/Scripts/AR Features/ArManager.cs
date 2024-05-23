using System.Collections.Generic;
using ConjureKit;
using Datas;
using Gameobjects;
using UI;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace AR_Features
{
    [RequireComponent(typeof(ARRaycastManager))]
    public class ArManager : MonoBehaviour
    {
        [SerializeField] private ARRaycastManager arRaycastManager;
        [SerializeField] private Camera arCamera;
        [SerializeField] private ConjureKitManager conjureKitManager;
        [SerializeField] private PanelController panelController;
        public FurniturePlacer furniturePlacer;

        private List<ARRaycastHit> _hits = new List<ARRaycastHit>();

        private void Start()
        {
            arRaycastManager = GetComponent<ARRaycastManager>();
        }

        private void Update()
        {
#if UNITY_EDITOR
            HandleEditorInput();
#endif
            HandleTouchInput();
        }

#if UNITY_EDITOR
        private void HandleEditorInput()
        {
            if (conjureKitManager.LayersActive && !string.IsNullOrEmpty(panelController.activeLayerId))
            {
                if (Input.GetMouseButtonDown(0) && !string.IsNullOrEmpty(panelController.activeLayerId) &&
                    panelController.chosenLayer != null)
                {
                    Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);
                    if (arRaycastManager.Raycast(ray, _hits, TrackableType.PlaneWithinPolygon))
                    {
                        Pose hitPose = _hits[0].pose;
                        GameObject createdCube =
                            furniturePlacer.PlaceSelectedFurniture(hitPose.position, hitPose.rotation);
                        CubeScript cubeScript = createdCube.GetComponent<CubeScript>();
                        Pose objPose = new Pose(createdCube.transform.position, createdCube.transform.rotation);
                        PocketBaseOperations.UploadToVirtualAssetsDB(conjureKitManager.currentDomainId, objPose,
                            panelController.activeLayerId, createdCube.transform.name);
                    }
                }
            }
        }
#endif

        private void HandleTouchInput()
        {
            if (conjureKitManager.LayersActive && Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (arRaycastManager.Raycast(touch.position, _hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = _hits[0].pose;
                    furniturePlacer.PlaceSelectedFurniture(hitPose.position, hitPose.rotation);
                }
            }
        }
    }
}
