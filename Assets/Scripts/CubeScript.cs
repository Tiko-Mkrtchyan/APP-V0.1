using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScript : MonoBehaviour
{
    
    void Start()
    {
        Pose cubePose = new Pose(transform.position, transform.rotation);
        ConjureKitManager _conjureKitManager = new ConjureKitManager();
        _conjureKitManager.UploadToDB("Some ID","122",cubePose);
    }

   
}
