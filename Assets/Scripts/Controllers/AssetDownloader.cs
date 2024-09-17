using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BoilerplateRomi.Enums;
using McKenna.Models;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using McKenna.Helpers;
using UnityEngine;

namespace McKenna.Controllers
{
    public interface IAssetDownloader
    {
        UniTask<string> AsyncDownloadMedia(string url, MediaImageSize imgSize, int priority = 1);
        UniTask<string> AsyncDownloadNft(string url, int priority = 1);
    }

    public class AssetDownloader : IAssetDownloader
    {
        private readonly SemaphoreSlim _downloadSemaphore = new SemaphoreSlim(1, 1);
        private readonly PriorityQueue<(string url, AssetEnum type, MediaImageSize imgSize, UniTaskCompletionSource<string> tcs)> _downloadQueue = new();
        private readonly ModelServer _modelServer;
        private float _startTime;
        private bool _isAppQuiting;

        private const int Timeout = 5;

        private readonly HashSet<string> _queuedUrls = new();

        public AssetDownloader(ModelServer modelServer)
        {
            _modelServer = modelServer;
            Application.quitting += () => _isAppQuiting = true;
            ProcessQueue();
        }

        public UniTask<string> AsyncDownloadMedia(string url, MediaImageSize imgSize, int priority = 1)
        {
            var fileName = url.ExtractFileNameFromUrl();
            if (ImageHelper.CheckIfFileExist(fileName, imgSize, out var path))
            {
                Extensions.Log($"[Downloader]:File exist {path}");
                return UniTask.FromResult(path);
            }

            if (_queuedUrls.Contains(url))
            {
                return UniTask.FromResult(string.Empty);
            }
            
            var tcs = new UniTaskCompletionSource<string>();
            _queuedUrls.Add(url);
            _downloadQueue.Enqueue((url, AssetEnum.Signage, imgSize, tcs), priority);
            return tcs.Task;
        }

        public UniTask<string> AsyncDownloadNft(string url, int priority = 1)
        {
            var fileName = url.ExtractFileNameFromUrl();
            if (ImageHelper.CheckIfFileExist(fileName, MediaImageSize.Original, out var path))
            {
                Extensions.Log($"[Downloader]:File exist {path}");
                return UniTask.FromResult(path);
            }
            
            if (_queuedUrls.Contains(url))
            {
                return UniTask.FromResult(string.Empty);
            }
            
            var tcs = new UniTaskCompletionSource<string>();
            _queuedUrls.Add(url);
            _downloadQueue.Enqueue((url, AssetEnum.Nft, MediaImageSize.Original, tcs), priority);
            return tcs.Task;
        }

        private async void ProcessQueue()
        {
            while (!_isAppQuiting)
            {
                if (!_downloadQueue.IsEmpty)
                {
                    var (url, type, imgSize, tcs) = _downloadQueue.Dequeue();
                    string result;
                    if (type == AssetEnum.Signage)
                        result = await DownloadMedia(url, imgSize);
                    else
                        result = await DownloadNft(url);
                    _queuedUrls.Remove(url);
                    tcs.TrySetResult(result);
                }
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        private async UniTask<string> DownloadMedia(string url, MediaImageSize imgSize)
        {
            var key = (url, imgSize);
            var fileName = url.ExtractFileNameFromUrl();

            if (_isAppQuiting)
            {
                return string.Empty; // Application is quitting, cancel the download.
            }

            if (ImageHelper.CheckIfFileExist(fileName, imgSize, out var path))
            {
                Extensions.Log($"[Downloader]:File exist {path}");
                return path;
            }

            using var  request = UnityWebRequest.Get(url);;
            try
            {
                request.downloadHandler = new DownloadHandlerBuffer();

                Extensions.Log($"[Downloader]:Start Download {fileName}");
                _startTime = Time.time;
                await request.SendWebRequest();

                if (_isAppQuiting)
                {
                    return string.Empty;
                }

                if (request.result is UnityWebRequest.Result.ConnectionError or
                    UnityWebRequest.Result.ProtocolError or UnityWebRequest.Result.DataProcessingError)
                {
                    Extensions.LogError($"[Downloader]:Failed to download from {url}:{request.error}");
                    return string.Empty;
                }

                var model = new CachedMediaModel();
                var data = request.downloadHandler.data;
                path = fileName.GeneratePath(imgSize);
                await UniTask.RunOnThreadPool(() => File.WriteAllBytesAsync(path, data));
                var downloadTime = Time.time - _startTime;
                Extensions.Log($"[Downloader]:Downloaded {fileName} from {url}, time elapsed: {downloadTime} secs");
                model.path = path;
                await UniTask.SwitchToMainThread();
                _modelServer.CachedAssets.AddMember(key, model);
                return path;
            }
            catch (Exception ex)
            {
                Extensions.LogError($"[Downloader]:Exception while downloading from {url}: {ex.Message}");
                return string.Empty;
            }
        }
        
        private async UniTask<string> DownloadNft(string url)
        {
            var imgSize = MediaImageSize.Original;
            var fileName = url.ExtractFileNameFromUrl();
            if (!fileName.Contains(".png", StringComparison.InvariantCultureIgnoreCase))
                fileName += ".png";

            if (_isAppQuiting)
            {
                return string.Empty; // Application is quitting, cancel the download.
            }
            
            if (ImageHelper.CheckIfFileExist(fileName, imgSize, out var path))
            {
                Extensions.Log($"[Downloader]:File exist {path}");
                return path;
            }
            
            using var request = UnityWebRequest.Get(url);
            try
            {
                request.downloadHandler = new DownloadHandlerBuffer();

                Extensions.Log($"[Downloader]:Start Download {fileName}");
                _startTime = Time.time;
                await request.SendWebRequest();

                if (_isAppQuiting)
                {
                    return string.Empty;
                }

                if (request.result is UnityWebRequest.Result.ConnectionError or 
                    UnityWebRequest.Result.ProtocolError or UnityWebRequest.Result.DataProcessingError)
                {
                    Extensions.LogError($"[Downloader]:Failed to download from {url}:{request.error}");
                    return string.Empty;
                }
                
                var data = request.downloadHandler.data;
                path = fileName.GeneratePath(imgSize);
                var hash = data.ComputeHash();
                await UniTask.RunOnThreadPool(() => File.WriteAllBytesAsync(path, data));
                var downloadTime = Time.time - _startTime;
                Extensions.Log($"[Downloader]:Downloaded {fileName} from {url}, time elapsed: {downloadTime} secs");
                await UniTask.SwitchToMainThread();
                _modelServer.CachedNftPath.AddMember(hash, path);
                return path;
            }
            catch (Exception ex)
            {
                // Extensions.LogError($"[Downloader]:Exception while downloading from {url}: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
