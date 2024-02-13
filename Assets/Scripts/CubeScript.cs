using System.Collections;
using System.Collections.Generic;
using Datas;
using UnityEngine;

public class CubeScript : MonoBehaviour
{
    private ConjureKitManager _conjureKitManager;
    void Start() 
    {
        _conjureKitManager = GameObject.Find("ConjureKitManager").GetComponent<ConjureKitManager>();
        Pose cubePose = new Pose(transform.position, transform.rotation);
        PocketBaseOperations.UploadToVirtualAssetsDB(_conjureKitManager._currentDomainId,"null",cubePose);
        
    }
}
