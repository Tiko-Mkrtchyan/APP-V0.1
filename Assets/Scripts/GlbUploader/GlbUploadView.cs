using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using McKenna.Communications;
using Models;
using Newtonsoft.Json;
using UnityEngine;

public class GlbUploadView : MonoBehaviour
{
    [SerializeField] private ModelServer modelServer;
    [SerializeField] private string adminUserName, adminPass;
    [SerializeField] private TMPro.TMP_Dropdown categoryDropDown;
    [SerializeField] private RectTransform scrollContainer;
    [SerializeField] private GlbEntryButton glbButtonPrefab;

    private IServerCommunication _serverCommunication;
    private IAssetDownloader _assetDownloader;
    private GetGlbCategory _glbCategoryHelper = new GetGlbCategory();
    private ResponsesList<GlbResponse> _glbList;

    private async void Start() 
    {
        _serverCommunication = new APICommunication(Values.ApiUrl);
        _assetDownloader = new AssetDownloader(modelServer);
        categoryDropDown.onValueChanged.AddListener(ChangedCategory);

        var userLoginResponse = await Login();
        Debug.Log(userLoginResponse.Token);
        await SetupCategories(userLoginResponse.Token);

        _glbList = await GetAllGlbs();

        if (_glbList != null && _glbList.Items.Count > 0)
        {
            DownloadThumbnails(_glbList.Items);
            PopulateGlbList(_glbList.Items);
        }
    }

    private async UniTask<UserLoginResponse> Login()
    {
        var endPoint = "admins/auth-with-password";
        var req = new UserLoginRequest()
        {
            Identity = adminUserName,
            Password = adminPass
        };

        var jsonReq = JsonConvert.SerializeObject(req);

        Debug.Log(jsonReq);

        var result = await _serverCommunication.Send(endPoint, RestMethod.Post, jsonReq, string.Empty);

        Debug.Log(result.content);

        if (result.status == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            var userLoginResponse = JsonConvert.DeserializeObject<UserLoginResponse>(result.content);
            return userLoginResponse;
        }

        return null;
    }

    private async UniTask SetupCategories(string authToken)
    {
        var endPoint = "collections/glbs";

        var result = await _serverCommunication.Send(endPoint, RestMethod.Get, null, authToken);

        if (result.status == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            var categories = _glbCategoryHelper.GetCategories(result.content);
            categoryDropDown.AddOptions(categories);
            var newSizeDelta = categoryDropDown.template.sizeDelta;
            newSizeDelta.y = Mathf.Min(140*categories.Count, 140*5);
            categoryDropDown.template.sizeDelta = newSizeDelta;
        }
    }   

    private async UniTask<ResponsesList<GlbResponse>> GetAllGlbs()
    {
        var page = 1;
        var response = new ResponsesList<GlbResponse>();
        response.Items = new List<GlbResponse>();

        while (true)
        {
            try
            {
                var tempResponse = await GetGlbPerPage(page);

                if (tempResponse == null)
                {
                    throw new InvalidCastException();
                }

                response.Items.AddUnique(tempResponse.Items);
                page++;

                if (page > response.TotalPages)
                {
                    return response;
                }
            }
            catch (Exception e)
            {
                Extensions.LogError(e.Message);
                return null;
            }
        }

        async UniTask<ResponsesList<GlbResponse>> GetGlbPerPage(int page = 1)
        {
            var endPoint = $"collections/glbs/records?page={page}";
            var result = await _serverCommunication.Send(endPoint, RestMethod.Get, null, null);

            if (result.status == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                var glbList = JsonConvert.DeserializeObject<ResponsesList<GlbResponse>>(result.content, new JsonSerializerSettings(){
                    NullValueHandling = NullValueHandling.Ignore
                });
                return glbList;
            }

            return null;
        }
    }

    private void DownloadThumbnails(List<GlbResponse> glbs)
    {
        foreach (var item in glbs)
        {
            _assetDownloader.AsyncDownloadMedia(item.GetGlbThumbnailUrl());
        }
    }

    private void PopulateGlbList(List<GlbResponse> glbs)
    {
        scrollContainer.DestroyChildren();

        foreach (var item in glbs)
        {
            if (!item.Category.Equals(categoryDropDown.captionText.text, StringComparison.CurrentCultureIgnoreCase)) continue;
            var tempButton = Instantiate(glbButtonPrefab, scrollContainer);
            tempButton.Setup(item);
        }
    }

    private void ChangedCategory(int index)
    {
        PopulateGlbList(_glbList.Items);
    }
}
