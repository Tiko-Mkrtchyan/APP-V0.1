using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace McKenna.SpatialAssets
{
    public class ImageLoader : MonoBehaviour, IMediaLoader
    {
        [SerializeField] private Renderer imageRenderer;
        [SerializeField] private Transform loader;
        [SerializeField] private string url;
        [SerializeField] private Texture errorTex;
        // [SerializeField] private ModelServer modelServer;

        private static readonly SemaphoreSlim LoadImageSemaphore = new SemaphoreSlim(2, 2);
        private static readonly int ColorShaderPropertyId = Shader.PropertyToID("_Color");
        private readonly int _imageOrientationPropertyId = Shader.PropertyToID("_Orientation");

        private bool _imageLoaded;
        private string _lastUrl;
        private bool _destroyInProgress;
        // private (string url, MediaImageSize Original) _key;
        private float _startTime;

        public bool IsSelected { get; private set; }
        private const int MaxRetries = 3;
        public event Action OnLoaded;

        public void Setup<T>(T data)
        {
            // if (modelServer == null || data is not MediaResponse media)
            // {
            //     Extensions.LogWarning("No ModelHandler found, skipping");
            //     return;
            // }

            // url = media.GetImageUrl();
            // _key = (url, MediaImageSize.Original);
            // modelServer.CachedAssets.OnModelUpdate += OnMediaUpdate;
            // OnMediaUpdate(modelServer.CachedAssets.GetModel());
        }

        public void SetSelected(bool selected)
        {
            var color = selected ? new Color(0.5f, 0.5f, 2f, 1f) : Color.white;
            IsSelected = selected;
            imageRenderer.material.SetColor(ColorShaderPropertyId, color);
        }

        private void OnDestroy()
        {
            _destroyInProgress = true;
            // modelServer.CachedAssets.OnModelUpdate -= OnMediaUpdate;
        }

        // private void OnMediaUpdate(Dictionary<(string, MediaImageSize), CachedMediaModel> mediaDictionary)
        // {
        //     _imageLoaded = (_lastUrl?.Equals(url)).GetValueOrDefault();
        //     if (mediaDictionary.TryGetValue(_key, out var cachedMedia) && !_imageLoaded)
        //     {
        //         Extensions.Log($"[Image] Image found, url: {url}");
        //         SetOrientation(cachedMedia.path);
        //         DisplayImage(cachedMedia);
        //         _lastUrl = url;
        //     }
        //     else
        //     {
        //         Extensions.Log($"[Image] Image not found, url: {url}");
        //     }
        // }

        // private void DisplayImage(CachedMediaModel cachedMedia)
        // {
        //     if (cachedMedia.texture != null)
        //     {
        //         SetTexture(cachedMedia.texture);
        //     }
        //     else
        //     {
        //         LoadAndDisplayImage(cachedMedia.path).Forget();
        //     }
        // }

        private void SetTexture(Texture2D texture)
        {
            if (imageRenderer)
                imageRenderer.material.mainTexture = texture;
            if (loader)
                loader.gameObject.SetActive(false);
        }

        private async UniTaskVoid LoadAndDisplayImage(string path, int tries = 0)
        {
            if (_imageLoaded)
                return;

            _imageLoaded = true;
            await LoadImageSemaphore.WaitAsync();

            try
            {
                var texture = await LoadTextureAsync(path);
                if (texture == null || _destroyInProgress)
                    return;
                var width = texture.width;
                var height = texture.height;
                // var resizedTexture = await texture.ResizeTextureToClosestPowerOfTwo(true, 1024);
                // resizedTexture.name = url.ExtractFileNameFromUrl();

                // SetTexture(resizedTexture);

                // if (modelServer.CachedAssets.TryGetValue(_key, out var model))
                // {
                //     model.width = width;
                //     model.height = height;
                //     model.texture = resizedTexture;
                // }

                Extensions.LogWarning($"[Image] Loaded image: {path}, took: {Time.time - _startTime} secs", gameObject);
                OnLoaded?.Invoke();
            }
            catch (Exception ex)
            {
                if (tries < MaxRetries)
                {
                    Extensions.LogError($"[Image] Error loading image, retrying: {ex.Message}");
                    await UniTask.Delay(TimeSpan.FromSeconds(1));
                    LoadAndDisplayImage(path, tries + 1);
                }
                else
                {
                    Extensions.LogError($"[Image] Error loading glb after {MaxRetries} attempts: {ex.Message}");
                }
            }
            finally
            {
                LoadImageSemaphore.Release();
            }
        }

        private async UniTask<Texture2D> LoadTextureAsync(string path)
        {
            path = $"file://{path}";
            using var uwr = UnityWebRequestTexture.GetTexture(path);
            await uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Extensions.Log(uwr.error);
                return null;
            }

            return DownloadHandlerTexture.GetContent(uwr);
        }

        private void SetOrientation(string path)
        {
            // var orientationEnum = NativeGallery.GetImageProperties(path).orientation;
            // float rotation = 0f;
            // switch (orientationEnum)
            // {
            //     case NativeGallery.ImageOrientation.Rotate90:
            //         rotation = 90f;
            //         break;
            //     case NativeGallery.ImageOrientation.Rotate180:
            //         rotation = 180f;
            //         break;
            //     case NativeGallery.ImageOrientation.Rotate270:
            //         rotation = 270f;
            //         break;
            // }
            // imageRenderer.material.SetFloat(_imageOrientationPropertyId, rotation);
        }
    }
}
