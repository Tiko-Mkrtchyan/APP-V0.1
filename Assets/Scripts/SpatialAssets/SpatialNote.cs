// using System;
// using System.Collections;
// using System.Text;
// using McKenna.Models;
// using McKenna.Views;
// using UnityEngine;
// using UnityEngine.UI;
// using Object = UnityEngine.Object;

// namespace McKenna.SpatialAssets
// {
//     public class SpatialNote : SpatialAsset
//     {
//         [SerializeField] private ComponentServer componentServer;
//         [SerializeField] private NoteUi noteUi;
        
//         private NoteModel _currentModel;
//         private string _itemId;
//         private readonly Color darkGrey = Color.black;// new Color(15f / 255f, 15f / 255f, 15f / 255f, 1);

//         public NoteModel CurrentModel => _currentModel;

//         public override void Setup<T>(T model, MediaResponse media, string id)
//         {
//             if (model is not NoteModel noteModel)
//             {
//                 Extensions.LogWarning($"Signage mismatch model type, skipping");
//                 return;
//             }

//             _itemId = id;
//             _currentModel = noteModel;
//             noteUi?.Setup(_currentModel, this, componentServer.notesGroup, componentServer.notesScale);
//             SetColorFromId(_itemId);
            
//             EventsController.AddListener(EventId.OnFinishOrCancelPlacement, OnFinishEditing);
//         }

//         protected override void OnDestroy()
//         {
//             EventsController.RemoveListener(EventId.OnFinishOrCancelPlacement, OnFinishEditing);
//             base.OnDestroy();
//         }

//         private void SetColorFromId(string id)
//         {
//             ColorOption color;
//             if (componentServer.contrastNotes)
//             {
//                 color = new ColorOption
//                 {
//                     ColorFrom = darkGrey,
//                     ColorTo = darkGrey
//                 };
//             }
//             else
//             {
//                 var colorId = (int)StringHelper.ConvertBaseToInt(id.Substring(0,4), 36) % componentServer.Colors.Length;
//                 Extensions.Log(colorId.ToString());
//                 color = componentServer.Colors[colorId];
//             }

//             noteUi.UpdateColor(color);
//         }

//         public override void Destroy()
//         {
//             noteUi.DestroyNote();
//             Destroy(gameObject);
//         }

//         public override string GetName()
//         {
//             return _currentModel.Message;
//         }

//         private void Update()
//         {
//             noteUi?.Update();
//         }

//         protected override void AdminOnTap(ItemRecord item)
//         {
//             var selected = item?.Id.Equals(_itemId);
//             if (noteUi.IsSelected) selected = false;
//             noteUi.SetSelected(selected.GetValueOrDefault());
//             base.AdminOnTap(item);
//         }
        
//         protected override void ClearSelection()
//         {
//             noteUi.SetSelected(false);
//         }
        
//         private void OnFinishEditing()
//         {
//             AdminOnTap(null);
//         }

//         public void UpdateNoteTemporary(string value)
//         {
//             noteUi.UpdateNoteTemporary(value);
//         }
//     }

//     [Serializable]
//     public class NoteUi
//     {
//         [SerializeField] private RectTransform uiRoot;
//         [SerializeField] private TMPro.TMP_Text titleText;
//         [SerializeField] private TMPro.TMP_Text descriptionText;
//         [SerializeField] private TMPro.TMP_Text distanceText;
//         [SerializeField] private CanvasGroup canvasGroup, hiddenGroup;
//         [SerializeField] private IndependentRoundedCorners panel;
//         [SerializeField] private GameObject selectedFrame;
        
//         private MonoBehaviour _mb;
//         private Transform _t;
//         private Camera _camera;
//         private Transform _cameraT;
//         private NoteModel _model;
//         private WaitForEndOfFrame _endOfFrame;
//         private VerticalLayoutGroup _vlg;
//         private float _distToCamera;
//         private StringBuilder _distanceString = new();
//         private Material mat;

//         private bool _initialized;
        
//         private static int _colorFromId = Shader.PropertyToID("_ColorFrom");
//         private static int _colorToId = Shader.PropertyToID("_ColorTo");

//         public bool IsSelected => selectedFrame.activeInHierarchy;

//         public void Setup(NoteModel model, MonoBehaviour mb, RectTransform group, float scale = 1f)
//         {
//             // titleText.SetText(model.Title);
//             descriptionText.SetText(model.Message);

//             if (_initialized) return;
            
//             _mb = mb;
//             _t = _mb.transform;
//             _camera = Camera.main;
//             _cameraT = _camera.transform;
//             _model = model;
//             _endOfFrame = new WaitForEndOfFrame();
//             _vlg = _mb.GetComponentInChildren<VerticalLayoutGroup>();
            
//             var oldParent = uiRoot.parent;
//             uiRoot.SetParent(group);
//             canvasGroup.transform.localScale = scale * Vector3.one;
//             Object.Destroy(oldParent.gameObject);
//             InitVisual();
//             _initialized = true;
//         }
        
//         void InitVisual()
//         {
//             uiRoot.gameObject.SetActive(false);
//             _mb.StartCoroutine(DelayVisual());
//         }
        
//         IEnumerator DelayVisual()
//         {
//             if (uiRoot.gameObject == null) yield break;
//             yield return _endOfFrame;
//             panel.Validate();
//             panel.Refresh();
//             uiRoot.gameObject.SetActive(true);
//             RefreshSize();
//         }
        
//         private void RefreshSize()
//         {
//             _mb.StartCoroutine(DelayRefresh());
//             IEnumerator DelayRefresh()
//             {
//                 yield return _endOfFrame;
//                 _vlg.SetLayoutVertical();
//                 Canvas.ForceUpdateCanvases();
//                 LayoutRebuilder.ForceRebuildLayoutImmediate(uiRoot);
//                 panel.enabled = false;
//             }
//         }
        
//         private bool IsBehind()
//         {
//             return _camera.transform.InverseTransformPoint(_t.position).z < 0f;
//         }
        
//         private void UpdateDistance()
//         {
//             // const float feetConversion = 3.28084f;
//             if (Time.frameCount % 10 != 0) return;
//             _distToCamera = (_cameraT.position - _t.position).magnitude;
//             _distanceString.Clear();

//             // var distanceInFeet = Math.Truncate(_distToCamera * feetConversion);
//             // _distanceString.Append(distanceInFeet);
//             // _distanceString.Append(" ft");

//             _distanceString.Append(Math.Truncate(_distToCamera * 100) / 100);
//             _distanceString.Append(" M");

//             distanceText.SetText(_distanceString);
//         }

//         public void UpdateColor(ColorOption color)
//         {
//             mat = new Material(panel.material);
//             mat.SetColor(_colorFromId, color.ColorFrom);
//             mat.SetColor(_colorToId, color.ColorTo);
//             panel.material = mat;
//         }
        
//         public void Update()
//         {
//             if (_cameraT == null || _t == null)
//                 return;
            
//             var screenPos = _camera.WorldToScreenPoint(_t.position);
//             screenPos.z = 0f;
//             uiRoot.position = screenPos;

//             var angle = _camera.transform.localEulerAngles.z;
//             uiRoot.localEulerAngles = -angle * Vector3.forward;

//             var outOfRange = _distToCamera > _model.RenderDistance;
//             canvasGroup.alpha = IsBehind() || outOfRange ? 0f : 1f;
//             hiddenGroup.alpha = outOfRange && !IsBehind() ? 1f : 0f;
//             UpdateDistance();
//         }

//         public void DestroyNote()
//         {
//             Object.Destroy(uiRoot.gameObject);
//         }

//         public void SetSelected(bool selected)
//         {
//             selectedFrame.SetActive(selected);
//         }

//         public void UpdateNoteTemporary(string value)
//         {
//             descriptionText.SetText(value);
//         }
//     }
// }