using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Models;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GlbEntryButton : MonoBehaviour
{
    [SerializeField] private ModelServer modelServer;
    [SerializeField] private RawImage thumbView;
    [SerializeField] private TMPro.TMP_Text description;
    [SerializeField] private Button editButton, deleteButton;

    private GlbResponse _glbModel;

    public void Setup(GlbResponse glbModel)
    {
        _glbModel = glbModel;

        var glbUrl = _glbModel.GetGlbThumbnailUrl();

        modelServer.CachedImages.OnModelAdd += ThumbnailAdded;

        var key = (glbUrl, ImageSize.Original);
        if (modelServer.CachedImages.TryGetValue(key, out var path))
        {
            ThumbnailAdded(key, path);
        }

        var description = $"{_glbModel.Name}\n<size=\"22\">Category:{_glbModel.Category}</size>";
        this.description.SetText(description);
    }

    private async void ThumbnailAdded((string, ImageSize) key, string path)
    {
        var localKey = (_glbModel.GetGlbThumbnailUrl(), ImageSize.Original);
        if (!key.Equals(localKey)) return;

        var tex = await LoadTextureAsync(path);
        thumbView.texture = tex;
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
}
