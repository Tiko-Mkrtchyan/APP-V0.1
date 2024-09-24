using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Models;
using UnityEngine;
using UnityEngine.UI;

namespace GlbUploader
{
    public class GlbUploadView : MonoBehaviour
    {
        [SerializeField] private ModelServer modelServer;
        [SerializeField] private TMPro.TMP_InputField nameInput;
        [SerializeField] private TMPro.TMP_Dropdown categoryDropdown;
        [SerializeField] private TMPro.TMP_Text selectedGlbPathText;
        [SerializeField] private Button buttonSelectGlb, buttonCancel, buttonSubmit;

        [Header("Debug")]
        [SerializeField] private RawImage debugThumbnail;

        private GlbUploadRequest _request = new();
        private GlbUploadController _glbUploadController;
        private GlbPreviewLoader _glbPreviewLoader;
        private bool _isUpdating;

        public void Setup(GlbUploadController glbUploadController)
        {
            _glbUploadController = glbUploadController;
            _glbPreviewLoader = GlbPreviewLoader.Instance;
            
            modelServer.GlbCategories.OnModelUpdate += CategoryUpdate;

            nameInput.onEndEdit.AddListener(ValidateName);
            categoryDropdown.onValueChanged.AddListener(ValidateCategory);

            buttonSelectGlb.onClick.AddListener(SelectGlb);
            buttonCancel.onClick.AddListener(Cancel);
            buttonSubmit.onClick.AddListener(Submit);

            gameObject.SetActive(false);
        }

        private void CategoryUpdate(List<string> list)
        {
                categoryDropdown.ClearOptions();
                categoryDropdown.AddOptions(list);
                var newSizeDelta = categoryDropdown.template.sizeDelta;
                newSizeDelta.y = Mathf.Min(140 * list.Count, 140 * 5);
                categoryDropdown.template.sizeDelta = newSizeDelta;
        }

        private void ValidateName(string newName)
        {
            _request.Name = newName;
            ValidateSubmitButton();
        }

        private void ValidateCategory(int index)
        {
            _request.Category = categoryDropdown.options[index].text;
            ValidateSubmitButton();
        }

        public void Open()
        {
            _isUpdating = false;
            gameObject.SetActive(true);
            nameInput.SetTextWithoutNotify(string.Empty);
            CategoryUpdate(modelServer.GlbCategories.GetModel());
            _request = new();
            _request.Category = categoryDropdown.captionText.text;
            ValidateSubmitButton();
        }

        public void OpenUpdate(GlbResponse model)
        {
            _isUpdating = true;
            gameObject.SetActive(true);
            CategoryUpdate(modelServer.GlbCategories.GetModel());
            _request = new();
            _request.Name = model.Name;
            _request.Category = model.Category;

            nameInput.SetTextWithoutNotify(model.Name);
            categoryDropdown.SetValueWithoutNotify(categoryDropdown.options.FindIndex(x => x.text.Equals(model.Category, StringComparison.CurrentCultureIgnoreCase)));
            ValidateSubmitButton();
        }

        private void ValidateSubmitButton()
        {
            var checkForFile = !string.IsNullOrEmpty(_request.File) || !_isUpdating;
            buttonSubmit.interactable = !string.IsNullOrEmpty(_request.Name) && 
                                        !string.IsNullOrEmpty(_request.Category) &&
                                        checkForFile;
        }

        private void SelectGlb()
        {
            var glbExtension = NativeFilePicker.ConvertExtensionToFileType("glb");
            NativeFilePicker.PickFile(file =>{
                if (!file.Contains(".glb"))
                {
                    Extensions.Log("Not a GLB file");
                    return;
                }
                _request.File = file;
                selectedGlbPathText.SetText(file);
            }, glbExtension);
        }

        private async void Submit()
        {
            var authToken = modelServer.UserCredential.GetModel().Token;
            var thumbBytes = await AddGlbThumbnail(_request.File);
            if (thumbBytes?.Length > 0)
            {
                _request.Thumb = thumbBytes;
                if (debugThumbnail != null)
                { 
                    var tex = new Texture2D(2,2);
                    ImageConversion.LoadImage(tex, thumbBytes);
                    debugThumbnail.texture = tex;
                }
            } 
            var glbResponse = await _glbUploadController.UploadGlb(_request, authToken);

            if (glbResponse != null)
            {
                var glbList = modelServer.GlbLibrary.GetModel();
                glbList.Items.Add(glbResponse);
                modelServer.GlbLibrary.SetModel(glbList);
                gameObject.SetActive(false);
            }
        }

        private void Cancel()
        {
            _request = new();
            selectedGlbPathText.SetText(string.Empty);
            gameObject.SetActive(false);
        }

        private async UniTask<byte[]> AddGlbThumbnail(string glbUrl)
        {
            Texture2D glbThumb = await _glbPreviewLoader.TakeGLBSnapShot(glbUrl);
            if (glbThumb == null)
            {
                Extensions.LogError($"Failed generating preview: {glbUrl}");
                return null;
            }

            byte[] jpgBytes = glbThumb.EncodeToJPG();
            // get file name from url
            // string fileName = Path.GetFileName(glbUrl);
            // //change the extension with new png only string
            // fileName = fileName.Replace(".glb", ".png");

            // if (!Directory.Exists(Values.ThumbFolder))
            //     Directory.CreateDirectory(Values.ThumbFolder);

            // // Define the file path
            // string filePath = Path.Combine(Values.ThumbFolder, fileName);
            // // Save the PNG bytes to a local file
            // await File.WriteAllBytesAsync(filePath, pngBytes);
            // // wait a second to make sure the file is saved
            // await UniTask.Delay(1000);
            return jpgBytes;
        }
    }
}