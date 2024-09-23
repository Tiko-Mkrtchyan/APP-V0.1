// using System;
// using System.Linq;
// using Models;
// using UnityEngine;

// namespace SpatialAssets
// {
//     public abstract class SpatialAsset : MonoBehaviour
//     {
//         [SerializeField] protected ModelServer modelServer;
//         private bool _isRecording;
        
//         protected Transform Camera;
//         protected bool Selected;
//         protected string ItemId;
//         protected PlacementData LastPlacementData = new();

//         public abstract void Setup<T>(T model, MediaResponse media, string id);

//         public abstract void Destroy();

//         public virtual string GetName()
//         {
//             return string.Empty;
//         }

//         protected virtual void Start()
//         {
//             Camera = UnityEngine.Camera.main.transform;
//             EventsController.AddListener<bool>(EventId.OnRecordScreen, OnRecord);
//             EventsController.AddListener<ItemRecord>(EventId.OnModelUpdate, AdminOnTap);
//             EventsController.AddListener(EventId.OnFinishOrCancelPlacement, ClearSelection);
//         }

//         protected virtual void OnDestroy()
//         {
//             EventsController.RemoveListener<bool>(EventId.OnRecordScreen, OnRecord);
//             EventsController.RemoveListener<ItemRecord>(EventId.OnModelUpdate, AdminOnTap);
//             EventsController.RemoveListener(EventId.OnFinishOrCancelPlacement, ClearSelection);
//         }
        
//         private void OnRecord(bool obj)
//         {
//             _isRecording = obj;
//         }

//         public virtual void InvokeUpdate()
//         {
//             var userCredential = modelServer.UserCredential.GetModel();
//             var currentLayer = modelServer.CurrentLayer.GetModel();
//             var loggedIn = modelServer.UserCredential.GetModel() != null;
//             var ownedLayer =
//                 currentLayer.Owner.Equals(userCredential?.Record.Id, StringComparison.InvariantCultureIgnoreCase);
            
//             Extensions.Log($"Tapped on asset, logged in: {loggedIn}");
//             if (!loggedIn || !ownedLayer || _isRecording || ARState.IsPlacing)
//             {
//                 ViewerOnTap();
//                 return;
//             }
//             var assetId = modelServer.SpawnedItems.GetModel().FirstOrDefault(x => x.Value == this).Key;
//             var model = modelServer.ItemsInLayer.GetModel().Items.Find(x => x.Id.Equals(assetId));

//             if (model != null)
//             {
//                 Extensions.Log($"Tapped on asset, model: {model.Id}");
//                 EventsController.TriggerActions(EventId.OnModelUpdate, model);
//             }
//         }

//         public virtual float GetMinScale()
//         {
//             return Values.MinScale;
//         }

//         protected virtual void ViewerOnTap()
//         {
            
//         }

//         protected virtual void AdminOnTap(ItemRecord item)
//         {
//             Selected = (item?.Id.Equals(ItemId, Values.StringCompareMode)).GetValueOrDefault();
//         }
        
//         protected virtual void ClearSelection()
//         {
//             Selected = false;
//         }
        
//         public virtual void SetPosition(PlacementData placementPose, PlacementMode mode)
//         {
//             LastPlacementData = placementPose;
//         }
        
//         protected virtual void UpdateLoaderScale(Transform loader)
//         {
//             var scaleRatio = 0.8f; ;
//             var loaderScale = Vector3.one;
//             loader.localPosition = Vector3.zero;
//             loader.localScale = scaleRatio * loaderScale;
//         }
//     }
// }