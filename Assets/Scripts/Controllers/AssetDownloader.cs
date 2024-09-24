using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BoilerplateRomi.Enums;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Models;

public interface IAssetDownloader
{
    UniTask<string> AsyncDownloadMedia(string url, ImageSize imageSize = ImageSize.Original, int priority = 1);
}

public class AssetDownloader : IAssetDownloader
{
    private readonly SemaphoreSlim _downloadSemaphore = new SemaphoreSlim(1, 1);
    private readonly PriorityQueue<(string url, ImageSize imgSize, UniTaskCompletionSource<string> tcs)> _downloadQueue = new();
    // private readonly ModelServer _modelServer;
    private float _startTime;
    private bool _isAppQuiting;
    private ModelServer _modelServer;
    private const int Timeout = 5;

    private readonly HashSet<string> _queuedUrls = new();

    public AssetDownloader(ModelServer modelServer)
    {
        _modelServer = modelServer;
        Application.quitting += () => _isAppQuiting = true;
        ProcessQueue();
    }

    public UniTask<string> AsyncDownloadMedia(string url, ImageSize imageSize = ImageSize.Original, int priority = 1)
    {
        var fileName = url.GetFileName();
        if (fileName.CheckIfFileExist(out var path, imageSize))
        {
            Extensions.Log($"[Downloader]:File exist {path}");
            var key = (url, imageSize);
            _modelServer.CachedImages.AddMember(key, path);
            return UniTask.FromResult(path);
        }

        if (_queuedUrls.Contains(url))
        {
            return UniTask.FromResult(string.Empty);
        }
        
        var tcs = new UniTaskCompletionSource<string>();
        _queuedUrls.Add(url);
        _downloadQueue.Enqueue((url, imageSize, tcs), priority);
        return tcs.Task;
    }

    private async void ProcessQueue()
    {
        while (!_isAppQuiting)
        {
            if (!_downloadQueue.IsEmpty)
            {
                var (url, imgSize, tcs) = _downloadQueue.Dequeue();
                var result = await DownloadMedia(url, imgSize);
                _queuedUrls.Remove(url);
                tcs.TrySetResult(result);
            }
            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }

    private async UniTask<string> DownloadMedia(string url, ImageSize imageSize)
    {
        var key = (url, imageSize);
        var fileName = url.GetFileName();

        if (_isAppQuiting)
        {
            return string.Empty; // Application is quitting, cancel the download.
        }

        if (fileName.CheckIfFileExist(out var path, imageSize))
        {
            Extensions.Log($"[Downloader]:File exist {path}");
            _modelServer.CachedImages.AddMember(key, path);
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

            // var model = new CachedMediaModel();
            var data = request.downloadHandler.data;
            path = fileName.GeneratePath(imageSize);
            await UniTask.RunOnThreadPool(() => File.WriteAllBytesAsync(path, data));
            var downloadTime = Time.time - _startTime;
            Extensions.Log($"[Downloader]:Downloaded {fileName} from {url}, time elapsed: {downloadTime} secs");
            // model.path = path;
            await UniTask.SwitchToMainThread();
            _modelServer.CachedImages.AddMember(key, path);
            return path;
        }
        catch (Exception ex)
        {
            Extensions.LogError($"[Downloader]:Exception while downloading from {url}: {ex.Message}");
            return string.Empty;
        }
    }
}