using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;

namespace Datas
    {
        
        public  class PocketBaseOperations: MonoBehaviour
        {
            private static string baseUrl="https://1269-46-71-90-59.ngrok-free.app";
            public static string layersPath = $"{baseUrl}/api/collections/layers/records";
            public static string _virtualAssetsPath = $"{baseUrl}/api/collections/virtual_assets/records";
           public static void UpdateLayerName(LayersData layersData)
           {
               string url = layersPath;
                UnityWebRequest request = new UnityWebRequest($"{url}/{layersData.id}");
                request.method = "PATCH";

                var json = JsonUtility.ToJson(layersData);
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
                        Debug.Log($"{url}/{layersData.id}");
                    }
                    else
                    {
                        Debug.Log("Success");

                    }
                };
            }
           public static void UploadLayerToDB(string name,string domainId)
           {
               
               string url = layersPath;
               UnityWebRequest request = new UnityWebRequest(url);
               request.method = "POST";

               var json = JsonUtility.ToJson(new LayersData
                   {
                       name = name,
                       domainId = domainId,
                      
                   }
                   );

                   
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
                       Debug.LogError(request.downloadHandler.text);
                      
                   }
                   else
                   {
                       Debug.Log("Success");

                   }
               };
           }
            
           
            public static void UploadToVirtualAssetsDB(string domainId, string id, Pose pose)
            {
                string url = _virtualAssetsPath;
                UnityWebRequest request = new UnityWebRequest(url);
                request.method = "POST";
                var json = JsonUtility.ToJson(new MyData
                {
                    domainId = domainId,
                    pose = SerializablePose.FromPose(pose)

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
                    }
                    else
                    {
                        Debug.Log("Success");
               
                    }
                };
                
        
            }
            public static void DeleteLayer(LayersData layersData)
            {
                string url = layersPath+$"/{layersData.id}";
                UnityWebRequest request = new UnityWebRequest(url);
                request.method = "DELETE";
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                request.SendWebRequest().completed += operation =>
                {
                    if (request.result == UnityWebRequest.Result.ConnectionError ||
                        request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError(request.error);
                    }
                    else
                    {
                        Debug.Log("Delete Response: " + request.downloadHandler.text);
                       
                    }
                };
            }

            
           
            public static IEnumerator GetLayersNames( Action<LayersDataList>  onComplete)
            {
                 
                UnityWebRequest request = UnityWebRequest.Get(layersPath);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error fetching data: " + request.error);
                }
                else
                {
                    string jsonResponse = request.downloadHandler.text;
                    LayersDataList _layersDataList = JsonUtility.FromJson<LayersDataList>(jsonResponse);
                    onComplete?.Invoke(_layersDataList);
            
            
                }
            }

            
           
        }
    }
   
    [System.Serializable]
    public class MyData 
    {
        public string id;
        public string domainId;
        public SerializablePose pose;
       
    }


   


    [System.Serializable]
    public  class LayersData
    {
        public string id;
        public   string name;
        public  string domainId;
        
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
