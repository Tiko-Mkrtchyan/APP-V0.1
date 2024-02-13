using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Networking;
[RequireComponent(typeof(ARRaycastManager))]
public class ArManager : MonoBehaviour
{
    [SerializeField] private ARRaycastManager _arRaycastManager;
    [SerializeField] private GameObject cube;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    [SerializeField] private Camera _arCamera;
    private void Start()
    {
        _arRaycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {


            Ray ray = _arCamera.ScreenPointToRay(Input.mousePosition);
            if (_arRaycastManager.Raycast(ray, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                Instantiate(cube, hitPose.position, hitPose.rotation);
               
            }
        }
#endif
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (_arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                Instantiate(cube, hitPose.position, hitPose.rotation);
            }
        }

    }

    
}
