using System;
using System.Collections;
using System.Collections.Generic;
using BoilerplateRomi.Models;
using BoilerplateRomi.Views;
using Cysharp.Threading.Tasks;
using McKenna.Communications;
using Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace GlbUploader
{
    public class GlbListView : MonoBehaviour
    {
        [SerializeField] private ModelServer modelServer;
        [SerializeField] private string adminUserName, adminPass;
        [SerializeField] private Button ButtonNewGlb;
        [SerializeField] private TMPro.TMP_Dropdown categoryDropDown;
        [SerializeField] private RectTransform scrollContainer;
        [SerializeField] private GlbEntryButton glbButtonPrefab;
        [SerializeField] private GlbUploadView glbUploadView;
        
        [Space(10)]
        [SerializeField] private ToastModalView toastModalView;

        private IServerCommunication _serverCommunication;
        private IAssetDownloader _assetDownloader;
        private GetGlbCategory _glbCategoryHelper = new GetGlbCategory();
        private GlbUploadController _glbUploadController;
        private ResponsesList<GlbResponse> _glbList;

        private async void Start()
        {
            modelServer.Initialize();
            _serverCommunication = new APICommunication(Values.ApiUrl);
            _assetDownloader = new AssetDownloader(modelServer);
            _glbUploadController = new GlbUploadController(_serverCommunication);

            glbUploadView.Setup(_glbUploadController);

            ButtonNewGlb.onClick.AddListener(AddNewGlb);
            categoryDropDown.onValueChanged.AddListener(ChangedCategory);

            modelServer.GlbCategories.OnModelUpdate += OnCategoryUpdate;
            modelServer.GlbLibrary.OnModelUpdate += UpdateGlbLibrary;

            var userLoginResponse = await Login();
            modelServer.UserCredential.SetModel(userLoginResponse);
            await SetupCategories(userLoginResponse.Token);

            RefreshGlbs();
        }

        private async void RefreshGlbs()
        {
            _glbList = await GetAllGlbs();

            if (_glbList != null && _glbList.Items.Count > 0)
            {
                modelServer.GlbLibrary.SetModel(_glbList);
            }
            else
            {
                Debug.Log("No GLB Library found");
            }
        }

        private void AddNewGlb()
        {
            glbUploadView.Open();
        }

        private void UpdateGlbLibrary(ResponsesList<GlbResponse> glbList)
        {
            _glbList = glbList;
            PopulateGlbList(glbList.Items);
            DownloadThumbnails(glbList.Items);
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
                modelServer.GlbCategories.SetModel(categories);
            }
        }

        private void OnCategoryUpdate(List<string> list)
        {
                categoryDropDown.ClearOptions();
                categoryDropDown.AddOptions(list);
                var newSizeDelta = categoryDropDown.template.sizeDelta;
                newSizeDelta.y = Mathf.Min(140 * list.Count, 140 * 5);
                categoryDropDown.template.sizeDelta = newSizeDelta;
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
                    var glbList = JsonConvert.DeserializeObject<ResponsesList<GlbResponse>>(result.content, new JsonSerializerSettings()
                    {
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
                tempButton.Setup(item, UpdateGlb, DeleteGlb);
            }
        }

        private void ChangedCategory(int index)
        {
            PopulateGlbList(_glbList.Items);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                foreach (var item in modelServer.CachedImages.GetModel())
                {
                    Extensions.Log($"Cache, Key: {item.Key}, path: {item.Value}");
                }
            }
        }

        private void UpdateGlb(GlbResponse model)
        {
            glbUploadView.OpenUpdate(model);
        }

        private void DeleteGlb(GlbResponse model)
        {
            var authToken = modelServer.UserCredential.GetModel()?.Token;
            var option = new ModalOptions()
            {
                Message =$"Do you really want to delete {model.Name}?",
                YesAction = async ()=>{
                    var success = await _glbUploadController.DeleteGlb(model.Id, authToken);

                    if (success)
                    {
                        var toast = new ToastOptions()
                        {
                            Message = $"Successfully deleted {model.Name}",
                            Duration = 2f
                        };
                        toastModalView.ShowToast(toast);
                        RefreshGlbs();
                    }
                    else
                    {
                        var toast = new ToastOptions()
                        {
                            Message = $"Failed to delete {model.Name}",
                            Duration = 2f
                        };
                        toastModalView.ShowToast(toast);
                    }
                }
            };
            toastModalView.ShowDialog(option);
        }
    }
}