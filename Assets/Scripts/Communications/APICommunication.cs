using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Cysharp.Threading.Tasks;
using McKenna.Models;
using McKenna.Models.Enums;
using UnityEngine;
using UnityEngine.Networking;

namespace McKenna.Communications
{
    public interface IServerCommunication
    {
        void SendWithCredentials(AppCredential appCredential, string endpoint, string method, string json,
            Action<string> onSuccess, Action<string> onFailure);
        
        void SendThumbnail(string endpoint, string authToken, byte[] byteArray, string filename,string mediaId, Action<string> onSuccess,
            Action<string> onFailure);

        void Send(string endpoint, string method, string json, string authToken, Action<string> onSuccess,
            Action<string> onFailure);
        
        UniTask<string> Send(string endpoint, string method, string json, string authToken);
        
        void SendMedia(string endpoint, string authToken, MediaUploadRequest media, 
            Action<string> onSuccess, Action<string> onFailure);
        
        void UpdateMedia(string endpoint, string authToken, MediaUpdateRequest media, 
            Action<string> onSuccess, Action<string> onFailure);

        void CancelUpload();
        
        event Action<UploadStatus, float> OnUploadProgress;
    }
    public class APICommunication : IServerCommunication
    {
        private readonly string _apiUrl;
        private Action _abortRequest;

        public event Action<UploadStatus, float> OnUploadProgress;

        public APICommunication(string apiUrl)
        {
            _apiUrl = apiUrl;
            Application.quitting += () => _abortRequest?.Invoke();
        }

        public async void SendWithCredentials(AppCredential appCredential, string endpoint, string method, string json, Action<string> onSuccess, Action<string> onFailure)
        {
            Extensions.Log($"Sending {json} to {_apiUrl}/{endpoint}");

            using (var request = new UnityWebRequest($"{_apiUrl}/{endpoint}", method))
            {
                _abortRequest = () =>
                {
                    request?.Abort();
                    _abortRequest = null;
                };
                
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", 
                    $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appCredential.Key}:{appCredential.Secret}"))}");

                if (!string.IsNullOrEmpty(json))
                {
                    request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                }

                try
                {
                    await request.SendWebRequest();

                    Extensions.Log($"Request received code: {request.responseCode} data: {request.downloadHandler.text}");

                    if (request.responseCode is not (200 or 204))
                    {
                        onFailure?.Invoke(request.downloadHandler.text);
                        return;
                    }

                    onSuccess?.Invoke(request.downloadHandler.text);
                    _abortRequest = null;
                }
                catch
                {
                    onFailure?.Invoke(request.downloadHandler.text);
                    _abortRequest = null;

                }
            }
        }
        
        public async void Send(string endpoint, string method, string json, string authToken,
        Action<string> onSuccess, Action<string> onFailure)
        {
            Extensions.Log($"Sending {json} to {_apiUrl}/{endpoint}");
            if (string.IsNullOrEmpty(endpoint))
            {
                onFailure?.Invoke("Endpoint cannot be null or empty.");
                return;
            }

            if (string.IsNullOrEmpty(method))
            {
                onFailure?.Invoke("HTTP method cannot be null or empty.");
                return;
            }

            var request = new UnityWebRequest($"{_apiUrl}/{endpoint}", method);
            _abortRequest = () =>
            {
                request?.Abort();
                _abortRequest = null;
            };

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(json))
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));

            if (!string.IsNullOrEmpty(authToken))
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");
                
            request.SendWebRequest();
            while (!request.isDone)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            Extensions.Log($"Request received code: {request.responseCode} data: {request.downloadHandler.text}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
            {
                string errorMessage = request.result == UnityWebRequest.Result.ConnectionError || 
                                      request.result == UnityWebRequest.Result.ProtocolError 
                    ? request.error 
                    : "An unexpected error occurred.";
                    
                Extensions.LogError($"Request received code: {request.responseCode} exception: {request.error} error: {errorMessage}");
                onFailure?.Invoke(request.downloadHandler.text);
            }
               
            _abortRequest = null;
        }
        
        public async UniTask<string> Send(string endpoint, string method, string json, string authToken)
        {
            using var request = new UnityWebRequest($"{_apiUrl}/{endpoint}", method);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(json))
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));

            if (!string.IsNullOrEmpty(authToken))
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");

            try
            {
                await request.SendWebRequest();

                Extensions.Log($"Request received code: {request.responseCode} data: {request.downloadHandler.text}");

                return request.downloadHandler.text;
            }
            catch (Exception e)
            {
                string errorMessage = request.result == UnityWebRequest.Result.ConnectionError || 
                                      request.result == UnityWebRequest.Result.ProtocolError 
                    ? request.error 
                    : "An unexpected error occurred.";
                    
                Extensions.LogError($"Request received code: {request.responseCode} exception: {e.Message} error: {errorMessage}");
                return errorMessage;
            }
        }

        public async void SendThumbnail(string endpoint, string authToken, byte[] byteArray,string filename,string mediaId, Action<string> onSuccess, Action<string> onFailure)
        {
            var bytes = byteArray;
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("thumbnail", bytes, filename, string.Empty),
            };
            formData.Add(new MultipartFormDataSection("media",mediaId));


            using (var request = UnityWebRequest.Post($"{_apiUrl}/{endpoint}", formData))
            {
                _abortRequest = () =>
                {
                    request?.Abort();
                    OnUploadProgress?.Invoke(UploadStatus.Cancelled, 0f);
                    _abortRequest = null;
                };
                OnUploadProgress?.Invoke(UploadStatus.Start, 0f);
                request.downloadHandler = new DownloadHandlerBuffer();

                if (!string.IsNullOrEmpty(authToken))
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");

                try
                {
                    request.SendWebRequest();

                    while (!request.isDone)
                    {
                        OnUploadProgress?.Invoke(UploadStatus.OnGoing, request.uploadProgress);
                        await UniTask.Yield(PlayerLoopTiming.Update);
                    }

                    OnUploadProgress?.Invoke(UploadStatus.Finishes, 1f);
                    Extensions.Log(
                        $"Request received code: {request.responseCode} data: {request.downloadHandler.text}");

                    if (request.responseCode is 100)
                    {
                        Extensions.Log($"{request.url} Call aborted");
                        return;
                    }

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        onFailure?.Invoke(request.downloadHandler.text);
                        return;
                    }

                    onSuccess?.Invoke(request.downloadHandler.text);
                }
                catch
                {
                    OnUploadProgress?.Invoke(UploadStatus.Cancelled, 1f);
                    onFailure?.Invoke(request.downloadHandler.text);
                }
                finally
                {
                    _abortRequest = null;
                }
            }
        }
        
        public async void SendMedia(string endpoint, string authToken, 
            MediaUploadRequest media, Action<string> onSuccess, Action<string> onFailure)
        {
            var formData = new List<IMultipartFormSection>();

            if (media != null)
            {
                var bytes = await File.ReadAllBytesAsync(media.Path);
                var fileName = Path.GetFileName(media.Path);
                formData.Add(new MultipartFormDataSection("owner",media.Owner));
                formData.Add(new MultipartFormDataSection("name",media.Name));
                formData.Add(new MultipartFormDataSection("width",media.Width.ToString(Values.DeviceCultureInfo)));
                formData.Add(new MultipartFormDataSection("height", media.Height.ToString(Values.DeviceCultureInfo)));
                formData.Add(new MultipartFormDataSection("distance", media.Distance.ToString(Values.DeviceCultureInfo)));
                formData.Add(new MultipartFormFileSection("media", bytes, fileName, string.Empty));
            }

            using (var request = UnityWebRequest.Post($"{_apiUrl}/{endpoint}", formData))
            {
                _abortRequest = () =>
                {
                    request?.Abort();
                    OnUploadProgress?.Invoke(UploadStatus.Cancelled, 0f);
                    _abortRequest = null;
                };
                OnUploadProgress?.Invoke(UploadStatus.Start, 0f);
                request.downloadHandler = new DownloadHandlerBuffer();

                if (!string.IsNullOrEmpty(authToken))
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");

                try
                {
                    request.SendWebRequest();

                    while (!request.isDone)
                    {
                        OnUploadProgress?.Invoke(UploadStatus.OnGoing, request.uploadProgress);
                        await UniTask.Yield(PlayerLoopTiming.Update);
                    }

                    OnUploadProgress?.Invoke(UploadStatus.Finishes, 1f);
                    Extensions.Log(
                        $"Request received code: {request.responseCode} data: {request.downloadHandler.text}");

                    if (request.responseCode is 100)
                    {
                        Extensions.Log($"{request.url} Call aborted");
                        return;
                    }

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        onFailure?.Invoke(request.downloadHandler.text);
                        return;
                    }

                    onSuccess?.Invoke(request.downloadHandler.text);
                }
                catch
                {
                    OnUploadProgress?.Invoke(UploadStatus.Cancelled, 1f);
                    onFailure?.Invoke(request.downloadHandler.text);
                }
                finally
                {
                    _abortRequest = null;
                }
            }
        }

        public async void UpdateMedia(string endpoint, string authToken, MediaUpdateRequest media, Action<string> onSuccess, Action<string> onFailure)
        {
            var formData = new List<IMultipartFormSection>();

            if (media != null)
            {
                formData.Add(new MultipartFormDataSection("owner",media.Owner));
                formData.Add(new MultipartFormDataSection("name",media.Name));
                formData.Add(new MultipartFormDataSection("width",media.Width.ToString()));
                formData.Add(new MultipartFormDataSection("height", media.Height.ToString()));
                formData.Add(new MultipartFormDataSection("distance", media.Distance.ToString()));
                if (!string.IsNullOrEmpty(media.Path))
                {
                    var bytes = File.ReadAllBytes(media.Path);
                    var fileName = Path.GetFileName(media.Path);
                    formData.Add(new MultipartFormFileSection("media", bytes, fileName, string.Empty));
                }
            }

            using (var request = UnityWebRequest.Post($"{_apiUrl}/{endpoint}/{media.Id}", formData))
            {
                _abortRequest = () =>
                {
                    request?.Abort();
                    OnUploadProgress?.Invoke(UploadStatus.Cancelled, 0f);
                    _abortRequest = null;
                };
                OnUploadProgress?.Invoke(UploadStatus.Start, 0f);
                request.method = Values.PATCH;
                request.downloadHandler = new DownloadHandlerBuffer();
            
                if (!string.IsNullOrEmpty(authToken))
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");

                try
                {
                    request.SendWebRequest();

                    while (!request.isDone)
                    {
                        OnUploadProgress?.Invoke(UploadStatus.OnGoing, request.uploadProgress);
                        await UniTask.Yield(PlayerLoopTiming.Update);
                    }
            
                    OnUploadProgress?.Invoke(UploadStatus.Finishes, 1f);
                    _abortRequest = null;
                    Extensions.Log($"Request received code: {request.responseCode} data: {request.downloadHandler.text}");

                    if (request.responseCode is not (200 or 204))
                    {
                        onFailure?.Invoke(request.downloadHandler.text);
                        return;
                    }

                    onSuccess?.Invoke(request.downloadHandler.text);
                }
                catch
                {
                    OnUploadProgress?.Invoke(UploadStatus.Cancelled, 1f);
                    onFailure?.Invoke(request.downloadHandler.text);
                    _abortRequest = null;
                }
            }
        }

        public void CancelUpload()
        {
            _abortRequest?.Invoke();
        }
    }
}