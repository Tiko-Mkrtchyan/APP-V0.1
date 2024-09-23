// using System;
// using McKenna.Models;
// using McKenna.Models.Enums;
// using UnityEngine;

// namespace McKenna.SpatialAssets
// {
//     public interface IMediaLoader
//     {
//         void Setup<T>(T media);
//         void SetSelected(bool selected);
//         bool IsSelected { get; }
//         event Action OnLoaded;
//     }
    
//     public class SpatialSignage : SpatialAsset
//     {
//         [SerializeField] protected Transform targetTransform;
//         [SerializeField] protected Transform childPivot;
//         [SerializeField] protected Transform loader;
//         [SerializeField] protected GameObject frame;
//         [SerializeField] protected AnimationComponent animationComponent = new();
//         [SerializeField] protected AlwaysFacingComponent facingComponent;

//         protected RotationComponent _rotationComponent;
//         protected ScaleComponent _scaleComponent;
//         private SignageModel _currentModel;
//         private MediaResponse _currentMedia;
        
//         protected IMediaLoader _mediaLoaderComponent;

//         protected bool _initialized;

//         public bool IsFacing { get; private set; }
//         public Action<float> OnRotate;
//         public Action<Action<float>> OnSnapRotate;
//         public Action<AnimationMode> OnAnimationModeChange;
//         public Action<float> OnAnimationSpeedChange;
//         public Action<float> OnScaleChange;
//         public Action<bool> OnHorizontalChange;

//         public SignageModel CurrentModel => _currentModel;
//         public VideoLoader VideoLoader => _mediaLoaderComponent as VideoLoader;

//         public override void Setup<T>(T model, MediaResponse media, string id)
//         {
//             if (model is not SignageModel signageModel)
//             {
//                 Extensions.LogWarning($"Signage mismatch model type, skipping");
//                 return;
//             }

//             ItemId = id;
//             _currentModel = signageModel;
//             _currentMedia = media;
            
//             if (!_initialized)
//                 InitComponents();
            
//             UpdateAsset(signageModel, media);
            
//             _initialized = true;
            
//             void InitComponents()
//             {
//                 _rotationComponent = new RotationComponent(childPivot);
//                 _scaleComponent = new ScaleComponent(targetTransform, childPivot);
//                 var timeStamp = DateTimeOffset.Parse(media.Created).ToUnixTimeMilliseconds();
//                 animationComponent.Setup(childPivot, timeStamp);
//                 animationComponent.ChangeAnimationMode(_currentModel.AnimationMode);
//                 animationComponent.ChangeAnimationScale(_currentModel.Scale);
//                 animationComponent.ChangeAnimationSpeed(_currentModel.AnimationSpeed);

//                 OnRotate = _rotationComponent.RotateYLocal;
//                 OnSnapRotate = _rotationComponent.SnapRotation;
//                 OnHorizontalChange = _scaleComponent.SetHorizontal;
//                 OnAnimationModeChange = animationComponent.ChangeAnimationMode;
//                 OnAnimationSpeedChange = animationComponent.ChangeAnimationSpeed;
//                 OnScaleChange = SetScale;
//             }
//         }

//         public override void Destroy()
//         {
//             Destroy(gameObject);
//         }
        
//         public override string GetName()
//         {
//             return _currentModel.Title;
//         }
        
//         private void UpdateAsset(SignageModel component, MediaResponse media)
//         {
//             frame.SetActive(false);
//             if (TryGetComponent(out _mediaLoaderComponent))
//             {
//                 _mediaLoaderComponent.Setup(media);
//             }
            
//             animationComponent?.ChangeAnimationMode(component.AnimationMode);
//             animationComponent?.ChangeAnimationScale(component.Scale);
//             animationComponent?.ChangeAnimationSpeed(component.AnimationSpeed);
            
//             _rotationComponent?.ForceRotateYLocal(component.Rotation);
//             _rotationComponent?.SnapOnOcclusion(component.OcclusionSnap, Vector3.forward);

//             //this has to be last to setup!!
//             _scaleComponent.SetHorizontal(component.Horizontal);
//             _scaleComponent.SetScale(component, media);
//             UpdateLoaderScale(loader);
//         }

//         protected void Update()
//         {
//             animationComponent?.Update();
//             if (animationComponent?.CurrentAnimMode == AnimationMode.Rotating) return;
//             _rotationComponent?.Update();    
//         }

//         public void SetFacing(bool value)
//         {
//             IsFacing = value;
//             facingComponent.SetConstraint(true, !value, true);
//         }

//         protected virtual void SetScale(float scale)
//         {
//             scale = MathF.Max(GetMinScale(), scale);
//             _currentModel.Scale = scale;
//             _scaleComponent.SetScale(_currentModel, _currentMedia);
//         }

//         public void ShowFrame(bool value)
//         {
//             frame.SetActive(value);
//         }

//         protected override void ViewerOnTap()
//         {
//             if (_mediaLoaderComponent is VideoLoader videoLoader)
//             {
//                 videoLoader.SetAudioMute(!videoLoader.IsMuted);
//             }
//         }

//         protected override void AdminOnTap(ItemRecord item)
//         {
//             base.AdminOnTap(item);
//             if (!TryGetComponent(out _mediaLoaderComponent)) return;
//             Extensions.Log($"Is Selected: {_mediaLoaderComponent.IsSelected}, item match: {Selected}");
//             if (_mediaLoaderComponent.IsSelected) Selected = false;
//             _mediaLoaderComponent.SetSelected(Selected);
//             base.AdminOnTap(item);
//         }

//         protected override void ClearSelection()
//         {
//            _mediaLoaderComponent?.SetSelected(false);
//         }

//         public override void SetPosition(PlacementData placementPose, PlacementMode mode)
//         {
//             var t = transform;
//             t.position = placementPose.pose.position;
//             _currentModel.OcclusionSnap = placementPose.snap;
//             _rotationComponent?.SnapOnOcclusion(placementPose.snap, Vector3.forward);
//             if (placementPose.snap)
//             {
//                 t.position += 0.02f * placementPose.normal;
//                 var direction = placementPose.pose.rotation * Vector3.forward;
//                 t.forward = -placementPose.normal;
//                 base.SetPosition(placementPose, mode);
//                 return;
//             }
            
//             var lookDirection = t.position - Camera.position;
//             lookDirection.y = 0;
//             lookDirection = lookDirection.normalized;
//             transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
//             base.SetPosition(placementPose, mode);
//         }
//     }
// }