using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace McKenna.Communications
{
    public struct WebRequestResult
    {
        public UnityWebRequest.Result status;
        public string content;
    }
    public interface IServerCommunication
    {
        UniTask<WebRequestResult> Send(string endpoint, string method, string json, string authToken);

        UniTask<WebRequestResult> SendData(string endpoint, string method, List<IMultipartFormSection> formData, string authToken);

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
        
        public async UniTask<WebRequestResult> Send(string endpoint, string method, string json, string authToken)
        {
            using var request = new UnityWebRequest($"{_apiUrl}{endpoint}", method);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(json))
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));

            if (!string.IsNullOrEmpty(authToken))
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");

            var requestResult = new WebRequestResult();

            try
            {
                await request.SendWebRequest();

                Extensions.Log($"Request received code: {request.responseCode} data: {request.downloadHandler.text}");

                requestResult.status = request.result;
                requestResult.content = request.downloadHandler.text;

                return requestResult;
            }
            catch (Exception e)
            {                    
                if (request.result != UnityWebRequest.Result.Success)
                    Extensions.LogError($"Request received code: {request.responseCode} exception: {e.Message}");

                requestResult.status = request.result;
                requestResult.content = request.downloadHandler.text;
                return requestResult;
            }
        }
        
        public async UniTask<WebRequestResult> SendData(string endpoint, string method, List<IMultipartFormSection> formData, string authToken)
        {
            using (var request = UnityWebRequest.Post($"{_apiUrl}{endpoint}", formData))
            {
                request.method = method;
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

                var requestResult = new WebRequestResult();

                try
                {
                    request.SendWebRequest();

                    while (!request.isDone)
                    {
                        OnUploadProgress?.Invoke(UploadStatus.OnGoing, request.uploadProgress);
                        await UniTask.Yield(PlayerLoopTiming.Update);
                    }

                    OnUploadProgress?.Invoke(UploadStatus.Finished, 1f);
                    Extensions.Log(
                        $"Request received code: {request.responseCode} data: {request.downloadHandler.text}");

                    requestResult.status = request.result;
                    requestResult.content = request.downloadHandler.text;

                    return requestResult;
                }
                catch (Exception e)
                {
                    OnUploadProgress?.Invoke(UploadStatus.Cancelled, 1f);
                    requestResult.status = request.result;
                    requestResult.content = request.downloadHandler.text;
                    Extensions.LogError(
                        $"Request received code: {request.responseCode} error: {e.Message}");
                    return requestResult;
                }
                finally
                {
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