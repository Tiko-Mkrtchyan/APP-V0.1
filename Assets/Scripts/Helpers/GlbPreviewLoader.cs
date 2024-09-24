using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading.Tasks;
using GLTFast;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class GlbPreviewLoader : MonoBehaviour
{
    private static GlbPreviewLoader _instance;

    public static GlbPreviewLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GlbPreviewLoader>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    _instance = singletonObject.AddComponent<GlbPreviewLoader>();
                    singletonObject.name = typeof(GlbPreviewLoader).ToString() + " (Singleton)";
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    
    private const int SnapShotLayer = 7;
    private Transform _transform;

    public string glbURL;
    public RawImage image;

    [ContextMenu("textGLB")]
    public async void TestGLB()
    {
        Texture2D Tex = await TakeGLBSnapShot(glbURL);
        image.texture = Tex;
    }

    public async Task<Texture2D> TakeGLBSnapShot(string url)
    {
        Extensions.Log($"Taking GLB Snapshot, URL {url}");
        byte[] glbByteArray;
        if (url.Contains("http"))
        {
            var request = new HttpClient();
            var response = await request.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            glbByteArray = await response.Content.ReadAsByteArrayAsync();
        }
        else
        {
            glbByteArray = await System.IO.File.ReadAllBytesAsync(url);
        }
        
        return await TakeGLBSnapShot(glbByteArray);
    }

    public async Task<Texture2D> TakeGLBSnapShot(byte[] glbByteArray)
    {
        var gltf = new GltfImport();
        bool success = false;
        try
        {
            success = await gltf.LoadGltfBinary(glbByteArray);
        }
        catch (Exception e)
        {
            Extensions.LogError(e.Message);
            return null;
        }

        if (!success)
        {
            return null;
        }

        GameObject pivot = new GameObject("Glb");
        pivot.layer = SnapShotLayer;
        var glbGameObject = new GameObjectInstantiator(gltf, pivot.transform);
        success = await gltf.InstantiateSceneAsync(glbGameObject);
        if (!success)
        {
            return null;
        }

        GameObject cameraObject = new GameObject("Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.cullingMask = 1 << SnapShotLayer;
        camera.backgroundColor = Color.gray;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.farClipPlane = 1000000;

        Extensions.Log("[GLB] success loaded GLB", gameObject);

        _transform = glbGameObject.SceneTransform;
        glbGameObject.SceneTransform.gameObject.layer = SnapShotLayer;
        // _transform.gameObject.layer = SnapShotLayer;
        Bounds bounds = new Bounds(_transform.position, Vector3.zero);
        foreach (Renderer renderer in _transform.GetComponentsInChildren<Renderer>())
        {
            renderer.gameObject.layer = SnapShotLayer;
            bounds.Encapsulate(renderer.bounds);
        }
        
        CenterGlb(_transform, bounds);
        // Calculate the center of the bounds
        Vector3 center = bounds.center;
        // Move the camera to the center of the bounds and back away in the z-axis
        bounds = new Bounds(center, Vector3.zero);
        foreach (Renderer renderer in _transform.GetComponentsInChildren<Renderer>())
        {
            renderer.gameObject.layer = SnapShotLayer;
            bounds.Encapsulate(renderer.bounds);
        }
        camera.transform.position = bounds.center;
        camera.transform.position += new Vector3(1f, 1f, 1f) * bounds.extents.magnitude;
        camera.transform.LookAt(bounds.center);

        Debug.Log($"Bounds center: {center}, extent: {bounds.extents}");
        
        // Create a new RenderTexture with the desired resolution
        RenderTexture rt = new RenderTexture(256, 256, 24);
        camera.targetTexture = rt;

        // Render the camera's view
        camera.Render();

        // Read the pixels from the RenderTexture into a new Texture2D
        Texture2D screenshot = new Texture2D(256, 256, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
        screenshot.Apply();
        
        // Clean up
        RenderTexture.active = null;
        camera.targetTexture = null;
        Destroy(rt);
        Destroy(cameraObject);
        Destroy(pivot);
        Destroy(_transform.gameObject);
        
        return screenshot;
    }
    
    private void CenterGlb(Transform glbPivot, Bounds sceneBounds)
    {
        var colPosition = sceneBounds.center;
        glbPivot.transform.position -= colPosition;
    }
}