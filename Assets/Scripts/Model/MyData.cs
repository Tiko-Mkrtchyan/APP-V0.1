using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace Datas
    {
        public  class PocketBaseOperations: MonoBehaviour
        {
            private static string baseUrl="https://ae9b-87-241-159-60.ngrok-free.app";
            private static string _layersPath = $"{baseUrl}/api/collections/layers/records";
            private static string _virtualAssetsPath = $"{baseUrl}/api/collections/virtual_assets/records";
            private static string _domainId = "WPHVS4OIGE2";
            
           public static void UpdateLayerName(LayersData layersData)
           {
               string url = _layersPath;
                UnityWebRequest request = new UnityWebRequest($"{url}/{layersData.id}");
                request.method = "PATCH";
                var json = JsonUtility.ToJson(layersData);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SendWebRequest().completed += operation =>
                {
                    if (request.result == UnityWebRequest.Result.ConnectionError ||
                        request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError(request.error);
                        Debug.Log("Update Layers Called> Error");
                    }
                    else
                    {
                        Debug.Log("Update Layers Called> Success");
                    }
                };
            }
           
           public static void UploadLayerToDB(string name,string domainId, Action<LayersData> onReceived)
           {
               string url = _layersPath;
               UnityWebRequest request = new UnityWebRequest(url);
               request.method = "POST";
               var json = JsonUtility.ToJson(new LayersData
                   {
                       name = name,
                       domainId = domainId,
                   });
               byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
               request.uploadHandler = new UploadHandlerRaw(bodyRaw);
               request.downloadHandler = new DownloadHandlerBuffer();
               request.SetRequestHeader("Content-Type", "application/json");
               request.SendWebRequest().completed += operation =>
               {
                   if (request.result == UnityWebRequest.Result.ConnectionError ||
                       request.result == UnityWebRequest.Result.ProtocolError)
                   {
                       Debug.LogError(request.error);
                       Debug.LogError(request.downloadHandler.text);
                   }
                   else
                   {
                       LayersData layersData = JsonUtility.FromJson<LayersData>(request.downloadHandler.text);
                       onReceived?.Invoke(layersData);
                   }
               };
           }

           public static void UploadToVirtualAssetsDB(string domainId, Pose pose,string layerId,string name)
           {
                string url =_virtualAssetsPath;
                UnityWebRequest request = new UnityWebRequest(url);
                request.method = "POST";
                var json = JsonUtility.ToJson(new FurnitureData
                {
                    domainId = domainId,
                    pose = SerializablePose.FromPose(pose),
                    layer = layerId,
                    name =  name
                });
                Debug.Log(json);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SendWebRequest().completed += operation =>
                {
                    if (request.result == UnityWebRequest.Result.ConnectionError ||
                        request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError(request.error);
                        Debug.Log("Upload To virtual Assets Called> Error");
                    }
                    else
                    {
                        Debug.Log("Upload To virtual Assets Called> Success");
                    }
                };
            }
           
           public static IEnumerator GetFromVirtualAssetsByLayer(string layerId)
           {
               string filter = $"(layer='{layerId}')";
               filter = HttpUtility.UrlEncode(filter);
               string url = $"{_virtualAssetsPath}?filter={filter}";
               UnityWebRequest request = UnityWebRequest.Get(url);
               yield return request.SendWebRequest();

               if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
               {
                   Debug.LogError("Error fetching data: " + request.error);
               }
               
               else
               {
                   FurnitureDataList myDataList = JsonUtility.FromJson<FurnitureDataList>(request.downloadHandler.text);
                   string[] resourceObjectNames = Resources.LoadAll<GameObject>("Furniture")
                       .Select(obj => obj.name)
                       .ToArray();
                   foreach (FurnitureData item in myDataList.items)
                   {
                       if (resourceObjectNames.Contains(item.name))
                       {
                           var furniture = Resources.Load("Furniture/"+item.name) as GameObject;
                           Vector3 recentPosition = item.pose.position;
                           Quaternion recentRotation = item.pose.rotation;
                           Instantiate(furniture, recentPosition, recentRotation);
                       }
                   }
               }
           }
           
            public static IEnumerator DeleteVirtualAssetsByLayer(string layerId,GameObject layerObject)
            {
                string filter = $"(layer='{layerId}')";
                filter = HttpUtility.UrlEncode(filter);
                string url = $"{_virtualAssetsPath}?filter={filter}";
                UnityWebRequest request = UnityWebRequest.Get(url);
                yield return request.SendWebRequest();
           
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error fetching data: " + request.error);
                }
                
                else
                {
                    layerObject.GetComponent<Button>().interactable = false;
                    FurnitureDataList myDataList = JsonUtility.FromJson<FurnitureDataList>(request.downloadHandler.text);
                    foreach (FurnitureData item in myDataList.items)
                    {
                        string urlPath = $"{_virtualAssetsPath}/{item.id}";
                        UnityWebRequest req = UnityWebRequest.Delete(urlPath);
                        yield return req.SendWebRequest();
                        if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
                        {
                            Debug.LogError(req.error);
                        }
                    }
                    layerObject.gameObject.SetActive(false);
                }
            }
           
            public static IEnumerator DeleteLayer(LayersData layersData)
            {
                yield return new WaitForSeconds(1.5f);
                string url = _layersPath+$"/{layersData.id}";
                UnityWebRequest request = UnityWebRequest.Delete(url);
                yield return request.SendWebRequest();
                request.SendWebRequest().completed += operation =>
                {
                    if (request.result == UnityWebRequest.Result.ConnectionError ||
                        request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError(request.error);
                        Debug.Log("DeleteLayer Called> Error");
                    }
                    else
                    {
                        Debug.Log("Delete Response: " + request.downloadHandler.text);
                        Debug.Log("DeleteLayer Called> Success");
                       
                    }
                };
            }
            
            public static IEnumerator GetLayersNames( Action<LayersDataList>  onComplete)
            {
                string filter = $"(domainId='{_domainId}')";
                filter = HttpUtility.UrlEncode(filter);
                string url = $"{_layersPath}?filter={filter}";
                UnityWebRequest request = UnityWebRequest.Get(url);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error fetching data: " + request.error);
                }
                else
                {
                    string jsonResponse = request.downloadHandler.text;
                    LayersDataList layersDataList = JsonUtility.FromJson<LayersDataList>(jsonResponse);
                    onComplete?.Invoke(layersDataList);
            
                }
            }
        }
    }
   
    [System.Serializable]
    public class FurnitureData
    {
        public string name;
        public string id;
        public string domainId;
        public SerializablePose pose;
        public string layer;
    }
    [System.Serializable]
    public class FurnitureDataList
    {
        public List<FurnitureData> items;
    }

    [System.Serializable]
    public  class LayersData
    {
        public string id;
        public   string name;
        public  string domainId;
        public bool selected;
    }

    [Serializable]
    public class LayersDataList
    {
        public List<LayersData> items;
    }
    [System.Serializable]
    public class SerializablePose
    {
        public Vector3 position;
        public Quaternion rotation;

        public SerializablePose(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public Pose ToPose()
        {
            return new Pose(position, rotation);
        }

        public static SerializablePose FromPose(Pose pose)
        {
            return new SerializablePose(pose.position, pose.rotation);
        }
    }
namespace Auki.Integration.ARFoundation.Manna
{
    public class FrameFeederEditor : FrameFeederBase
    {
        private static readonly int TextureSingle = Shader.PropertyToID("_textureSingle");
        
        /// <summary>
        /// AR Camera Manager necessary to supply Manna with camera feed frames.
        /// </summary>
        [SerializeField] protected ARCameraBackground ArCameraBackground;

        private RenderTexture _videoTexture;

        protected override void Awake()
        {
            base.Awake();

            if (ArCameraBackground == null)
                ArCameraBackground = GetComponent<ARCameraBackground>();
        }
        
        /// <summary>
        /// Manna needs to be supplied with camera feed frames so it can detect QR codes and perform Instant Calibration.
        /// For this particular implementation, we use AR Foundations AR Camera Manager to retrieve the images on CPU side.
        /// </summary>
        protected override void ProcessFrame(ARCameraFrameEventArgs frameInfo)
        {
            CreateOrUpdateVideoTexture();
            if (_videoTexture == null) return;

            CopyVideoTexture();

            MannaInstance.ProcessVideoFrameTexture(
                _videoTexture,
                ArCamera.projectionMatrix,
                ArCamera.worldToCameraMatrix
            );
        }

        private void CreateOrUpdateVideoTexture()
        {
            if (_videoTexture != null) return;

            var textureNames = ArCameraBackground.material.GetTexturePropertyNames();
            for (var i = 0; i < textureNames.Length; i++)
            {
                var texture = ArCameraBackground.material.GetTexture(textureNames[i]);
                if (texture == null) continue;
                Debug.Log(
                    $"Creating video texture based on: {textureNames[i]}, format: {texture.graphicsFormat}, size: {texture.width}x{texture.height}");
                _videoTexture = new RenderTexture(texture.width, texture.height, 0, GraphicsFormat.R8_UNorm);
                break;
            }
        }

        private void CopyVideoTexture()
        {
            // Copy the camera background to a RenderTexture
            var textureY = ArCameraBackground.material.GetTexture(TextureSingle);
            Graphics.Blit(textureY, _videoTexture);
        }
    }
}
