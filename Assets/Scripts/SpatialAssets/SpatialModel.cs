// using System;
// using McKenna.Models;
// using McKenna.Models.Enums;
// using UnityEngine;

// namespace McKenna.SpatialAssets
// {
//     public class SpatialModel : MonoBehaviour
//     {
//         [SerializeField] private Transform targetTransform;
//         [SerializeField] private Transform childPivot;
//         [SerializeField] private Transform loader;
        
//         [SerializeField] private AnimationComponent animationComponent = new();
//         private RotationComponent _rotationComponent;
//         private GlbAssetResponse _currentModel;
//         private MediaResponse _currentMedia;
//         private GlbLoader _glbLoaderComponent;
//         private bool _initialized;

//         public bool IsFacing { get; private set; }
//         public Action<float> OnRotate;
//         public Action<Action<float>> OnSnapRotate;
//         public Action<bool> OnTurntableChange;
//         public Action<string> OnGlbAnimationChange;
//         public Action<float> OnAnimationSpeedChange;
//         public Action<float> OnScaleChange;

//         public GlbLoader Loader => _glbLoaderComponent;

//         public GlbModel CurrentModel => _currentModel;
//         public string GetItemId => _currentModel.id;

//         public void Setup(GlbAssetResponse model)
//         {
            
//             ItemId = id;
//             _currentModel = glbModel;
//             _currentMedia = media;
            
//             if (!_initialized)
//                 InitComponents();
            
//             UpdateAsset(glbModel, media);
            
//             _initialized = true;
            
//             void InitComponents()
//             {
//                 if (TryGetComponent(out _glbLoaderComponent))
//                 {
//                     _glbLoaderComponent.Setup(media);
//                 }
                
//                 _rotationComponent = new RotationComponent(childPivot);
//                 var timeStamp = DateTimeOffset.Parse(media.Created).ToUnixTimeMilliseconds();
//                 animationComponent.Setup(childPivot, timeStamp);
//                 animationComponent.ChangeAnimationMode(_currentModel.AnimationMode);
//                 animationComponent.ChangeAnimationScale(_currentModel.Scale);
//                 animationComponent.ChangeAnimationSpeed(_currentModel.AnimationSpeed);

//                 OnRotate = _rotationComponent.RotateYLocal;
//                 OnSnapRotate = _rotationComponent.SnapRotation;
//                 OnGlbAnimationChange = _glbLoaderComponent.PlayAnimation;
//                 OnTurntableChange = EnableTurntable;
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

//         private void UpdateAsset(GlbModel component, MediaResponse media)
//         {
//             if (TryGetComponent(out _glbLoaderComponent))
//             {
//                 _glbLoaderComponent.Setup(media);
//                 _glbLoaderComponent.PlayAnimation(component.GlbAnimationClip);
//             }
            
//             animationComponent?.ChangeAnimationMode(component.AnimationMode);
//             animationComponent?.ChangeAnimationScale(component.Scale);
//             animationComponent?.ChangeAnimationSpeed(component.AnimationSpeed);
            
//             _rotationComponent?.ForceRotateYLocal(component.Rotation);
//             _rotationComponent?.SnapOnOcclusion(component.OcclusionSnap, Vector3.up);

//             if (_initialized)
//                 SetScale(component.Scale);
//             base.UpdateLoaderScale(loader);
//         }
        
//         private void EnableTurntable(bool value)
//         {
//             animationComponent?.ChangeAnimationMode(value ? AnimationMode.Rotating : AnimationMode.Static);
//         }

//         private void Update()
//         {
//             animationComponent?.Update();
//             if (animationComponent?.CurrentAnimMode == AnimationMode.Rotating) return;
//             _rotationComponent?.Update();
//         }

//         //For GLB asset, its model.Media.Width value are being used as base scale.
//         private void SetScale(float scale)
//         {
//             scale = Mathf.Max(GetMinScale(), scale);
//             _currentModel.Scale = scale;
//             childPivot.localScale = _currentModel.Scale * _currentMedia.Width * Vector3.one;
//             Extensions.Log($"[GLB] Set Scale: {scale} , initialized:{_initialized}", gameObject);
//             _glbLoaderComponent.GenerateBoxCollider();
//         }
        
//         protected override void AdminOnTap(ItemRecord item)
//         {
//             base.AdminOnTap(item);
//             if (!TryGetComponent(out _glbLoaderComponent)) return;
//             if (_glbLoaderComponent.IsSelected) Selected = false;
//             _glbLoaderComponent.SetSelected(Selected);
//         }
        
//         protected override void ClearSelection()
//         {
//             base.ClearSelection();
//             if (!TryGetComponent(out _glbLoaderComponent)) return;
//             _glbLoaderComponent.SetSelected(false);
//         }

//         public override void SetPosition(PlacementData placementPose, PlacementMode mode)
//         {
//             var t = transform;
//             t.position = placementPose.pose.position;
//             _currentModel.OcclusionSnap = placementPose.snap;
//             _rotationComponent?.SnapOnOcclusion(placementPose.snap, Vector3.up);
//             if (placementPose.snap)
//             {
//                 t.up = placementPose.normal;
//                 base.SetPosition(placementPose, mode);
//                 return;
//             }
            
//             var lookDirection = t.position - Camera.position;
//             lookDirection.y = 0;
//             lookDirection = lookDirection.normalized;
//             transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
//             base.SetPosition(placementPose, mode);
//         }

//         public override float GetMinScale()
//         {
//             return _glbLoaderComponent.MinScale;
//         }

//         public void MarkForScaleOverride()
//         {
//             _glbLoaderComponent.OverrideScale(true);
//         }
//     }
// }
