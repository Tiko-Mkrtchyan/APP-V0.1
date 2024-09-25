using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using McKenna.Communications;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace GlbUploader
{
    public class GlbUploadController
    {
        private readonly IServerCommunication _serverCommunication;

        private const string EndPoint = "collections/glbs/records";

        public GlbUploadController(IServerCommunication serverCommunication)
        {
            _serverCommunication = serverCommunication;
        }

        public async UniTask<GlbResponse> UploadGlb(GlbUploadRequest request, string authToken)
        {
            if (request == null) return null;

            var form = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("name", request.Name),
                new MultipartFormDataSection("category", request.Category)
            };
            if (!string.IsNullOrEmpty(request.File))
            {
                var (bytes, fileName) = await GetDataByte(request.File);
                form.Add(new MultipartFormFileSection("file", bytes, fileName, string.Empty));
            }
            if (request.Thumb?.Length > 0)
            {
                var fileName = Path.GetFileName(request.File);
                fileName = $"{fileName.Remove(fileName.Length - 4)}.jpg";
                form.Add(new MultipartFormFileSection("thumb", request.Thumb, fileName, string.Empty));
            }

            var result = await _serverCommunication.SendData(EndPoint, RestMethod.Post, form, authToken);

            if (result.status == UnityWebRequest.Result.Success)
            {
                var glbResponse = JsonConvert.DeserializeObject<GlbResponse>(result.content, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                return glbResponse;
            }

            return null;
        }

        public async UniTask<bool> DeleteGlb(string glbId, string authToken)
        {
            var endPoint = $"{EndPoint}/{glbId}";
            var result = await _serverCommunication.Send(endPoint, RestMethod.Delete, string.Empty, authToken);

            return result.status == UnityWebRequest.Result.Success;
        }

        private async UniTask<(byte[], string)> GetDataByte(string path)
        {
            var bytes = await File.ReadAllBytesAsync(path);
            var fileName = Path.GetFileName(path);
            return (bytes, fileName);
        }
    }
}
