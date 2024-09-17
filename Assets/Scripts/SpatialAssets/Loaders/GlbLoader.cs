//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using Cysharp.Threading.Tasks;
//using GLTFast;
//using McKenna.Models;
//using McKenna.SpatialAssets.Helpers;
//using TMPro;
//using UnityEngine;
//using UnityEngine.Networking;
//using UnityEngine.XR.ARFoundation;

//namespace McKenna.SpatialAssets
//{
//    public class GlbLoader : MonoBehaviour, IMediaLoader
//    {
//        [SerializeField] private Transform pivot;
//        [SerializeField] private Transform loader;
//        [SerializeField] private ModelServer modelServer;
//        [SerializeField] private SpatialModel spatialModel;

//        private string _url;
//        private long _timeStamp;
//        private bool _isCopy;
//        private bool _overrideScale;
//        private int _tries;

//        private static readonly object SemaphoreLock = new object();
//        private static readonly SemaphoreSlim LoadGlbSemaphore = new(1, 1);
//        private string _lastPath;
//        private Transform _pivotCorrector;
//        private Transform _glbInstance;
//        private Animation _glbAnimation;
//        private bool _updateGlb, _onDestroy;
//        private string _cachedClip;
//        private BoxCollider _boxCollider;
//        private List<Material> _materials = new();
//        private readonly MaterialSearch _materialSearch = new();
//        private List<Color> _materialColors = new();
//        private readonly BoundsCalculator _boundsCalculator = new();

//        private readonly int _colorShaderPropertyId = Shader.PropertyToID("baseColorFactor");
//        private const string DefaultLayer = "Default";
//        private const string InvisibleLayer = "Invisible";
        
//        private const int MaxRetries = 3;
        
//        public float MinScale { get; private set; }

//        public bool IsSelected { get; private set; }
//        public event Action OnLoaded;

//        public void Setup<T>(T data)
//        {
//            if (modelServer == null || data is not MediaResponse media)
//            {
//                Extensions.LogWarning("No ModelHandler found, skipping");
//                return;
//            }

//            if (DateTimeOffset.TryParse(media.Created, out var dateTimeOffset))
//                _timeStamp = dateTimeOffset.ToUnixTimeMilliseconds();
//            _url = media.GetImageUrl();

//            if (!_pivotCorrector)
//            {
//                _pivotCorrector = new GameObject("PivotCorrector").transform;
//                _pivotCorrector.SetParent(pivot);
//                _pivotCorrector.localPosition = Vector3.zero;
//                _pivotCorrector.localRotation = Quaternion.identity;
//            }
            
//            modelServer.CachedAssets.OnModelUpdate += OnMediaUpdate;
//            OnMediaUpdate(modelServer.CachedAssets.GetModel());
//        }

//        private void OnDestroy()
//        {
//            if (!_isCopy && _glbInstance != null)
//            {
//                _glbInstance.gameObject.SetActive(false);
//                _glbInstance.SetParent(null);
//            }

//            _onDestroy = true;
//        }

//        private void OnMediaUpdate(Dictionary<(string, MediaImageSize), CachedMediaModel> medias)
//        {
//            var exists = medias.TryGetValue((_url, MediaImageSize.Original), out var cachedMedia);
//            if (exists)
//            {
//                _updateGlb = !(_lastPath?.Equals(cachedMedia.path)).GetValueOrDefault();
//                if (!_updateGlb) return;
//                ShowModel(cachedMedia.path);
//                _lastPath = cachedMedia.path;
//            }
//        }
        
//        private async void ShowModel(string path, int tries = 0)
//        {
//            await LoadGlbSemaphore.WaitAsync();

//            try
//            {
//                var localPath = $"file://{path}";

//                if (modelServer.CachedGlb.GetModel().TryGetValue(localPath, out var cachedGlb))
//                {
//                    _isCopy = true;
//                    loader.gameObject.SetActive(false);
//                    _glbInstance = Instantiate(cachedGlb, pivot).transform;
//                    _glbInstance.SetLayerRecursively(InvisibleLayer.GetLayerMaskIndex());
//                    _glbInstance.localPosition = Vector3.zero;
//                    _glbInstance.localRotation = Quaternion.identity;
//                    _glbInstance.localScale = Vector3.one;
//                    _glbInstance.gameObject.SetActive(true);
//                    _glbAnimation = _glbInstance.GetComponentInChildren<Animation>(true);
//                    if (_glbAnimation)
//                    {
//                        _glbAnimation.enabled = true;
//                        PlayAnimation(_glbAnimation, _cachedClip);
//                    }
//                    await UniTask.NextFrame();
//                    _materials.AddRange(_materialSearch.SearchMaterial(gameObject));
//                    foreach (var m in _materials)
//                    {
//                        if (!m.HasColor(_colorShaderPropertyId)) continue;
//                        _materialColors.Add(m.GetColor(_colorShaderPropertyId));
//                    }
//                    GenerateBoxCollider();
//                    await AdjustScaleIfModelTooBig(_boxCollider);
//                    CenterGlb(_glbInstance, _boxCollider);
//                }
//                else
//                {
//                    using var uwr = UnityWebRequest.Get(localPath);
//                    await uwr.SendWebRequest();

//                    if (uwr.result != UnityWebRequest.Result.Success)
//                    {
//                        Extensions.Log(uwr.error);
//                    }
//                    else
//                    {
//                        var gltf = new GltfImport();
//                        var data = uwr.downloadHandler.data;
//                        var success = await gltf.LoadGltfBinary(data, new Uri(localPath));
//                        if (success)
//                        {
//                            if (_onDestroy) return;
//                            loader.gameObject.SetActive(false);
//                            var instantiator = new GameObjectInstantiator(gltf, _pivotCorrector, settings: new InstantiationSettings()
//                            {
//                                Layer = InvisibleLayer.GetLayerMaskIndex()
//                            });
//                            instantiator.MeshAdded += OnMeshAdded;
//                            success = await gltf.InstantiateSceneAsync(instantiator);
//                            if (!success) return;
//                            Extensions.Log("[GLB] success loaded GLB", gameObject);
//                            _glbInstance = instantiator.SceneTransform;
//                            _glbAnimation = instantiator.SceneInstance.LegacyAnimation;
//                            OnLoaded?.Invoke();
//                            PlayAnimation(_glbAnimation, _cachedClip);
//                            GenerateBoxCollider();
//                            await AdjustScaleIfModelTooBig(_boxCollider);
//                            CenterGlb(_glbInstance, _boxCollider);
//                            modelServer.CachedGlb.AddMember(localPath, _glbInstance.gameObject);
//                        }
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                if (tries < MaxRetries)
//                {
//                    Extensions.LogError($"[GLB] Error loading glb, retrying: {e.Message}");
//                    await UniTask.Delay(TimeSpan.FromSeconds(1));
//                    if (_onDestroy) return;
//                    ShowModel(path, tries + 1);
//                }
//                else
//                {
//                    Extensions.LogError($"[GLB] Error loading glb after {MaxRetries} attempts: {e.Message}");
//                }
//            }
//            finally
//            {
//                LoadGlbSemaphore.Release();
//            }
//        }


//        private void OnMeshAdded(GameObject gameobject, uint nodeindex, string meshname, MeshResult meshresult, uint[] joints, uint? rootjoint, float[] morphtargetweights, int primitivenumeration)
//        {
//            if (_materialSearch.TryGetMaterial(gameobject, out var m))
//            {
//                m.enableInstancing = true;
//                var sm = _materials.Find(x => x.name.Equals(m.name, Values.StringCompareMode));
//                if (sm == null)
//                {
//                    _materials.Add(m);
//                    if (m.HasColor(_colorShaderPropertyId)) 
//                        _materialColors.Add(m.GetColor(_colorShaderPropertyId));
//                }
//                else
//                {
//                    gameobject.GetComponent<MeshRenderer>().sharedMaterial = sm;
//                }
//            }
//        }

//        public async UniTask<List<TMP_Dropdown.OptionData>> GetAnimationsList()
//        {
//            var list = new List<TMP_Dropdown.OptionData>();
//            await UniTask.WaitUntil(() => _glbInstance != null);

//            if (_glbAnimation == null)
//            {
//                list.Add(new TMP_Dropdown.OptionData("No Animation found"));
//                return list;
//            }

//            foreach (AnimationState anim in _glbAnimation)
//            {
//                list.Add(new TMP_Dropdown.OptionData(anim.name));
//            }
//            return list;
//        }

//        public void PlayAnimation(string clipName)
//        {
//            PlayAnimation(_glbAnimation, clipName);
//            _cachedClip = clipName;
//        }

//        // This is needed because with the public override above, the _glbAnimation component might still be null, so we need to always
//        // cache the clip and use the overload below when glb finished initialized.
//        private void PlayAnimation(Animation player, string clipName)
//        {
//            if (!player)
//            {
//                Extensions.LogWarning("[GLB] Can't find GLB animation component");
//                return;
//            }

//            if (!string.IsNullOrEmpty(clipName) && player.GetClip(clipName) != null)
//            {
//                player[clipName].time = GetAnimTime();
//                player.Play(clipName);
//                _cachedClip = clipName;
//            }
//            else
//            {
//                var clip = player.clip;
//                player[clip.name].time = GetAnimTime();
//                player.Play();
//            }
//        }

//        public void GenerateBoxCollider()
//        {
//            if (!_boxCollider) _boxCollider = gameObject.AddComponent<BoxCollider>();
//            _boundsCalculator.AdjustBoxColliderToFitBounds(gameObject, pivot, _boxCollider);
//        }

//        private async UniTask AdjustScaleIfModelTooBig(BoxCollider boxCollider)
//        {
//            pivot.localScale = Vector3.one;
//            _glbInstance.SetLayerRecursively(InvisibleLayer.GetLayerMaskIndex());
//            await UniTask.NextFrame();
//            var existing = false;
//            try
//            {
//                existing = modelServer.ItemsInLayer.GetModel().Items.Exists(x =>
//                    x.Id.Equals(spatialModel.GetItemId, Values.StringCompareMode));
//            }
//            catch {}
            
//            var size = boxCollider.bounds.size;
//            var widestAxis = GetWidestAxis(size);
//            var massiveSize = widestAxis > 1.5f;
//            var scaleFactor = 1f / (widestAxis / 1.5f);
//            Extensions.Log($"[GLB] widest axis:{widestAxis}\nmassive size flag: {massiveSize}\ninitial scale factor: {scaleFactor}", gameObject);
//            MinScale = massiveSize ? scaleFactor / 10f : Values.MinScale;
//            scaleFactor = existing ? 
//                Mathf.Max(MinScale, spatialModel.CurrentModel.Scale) :
//                massiveSize ? scaleFactor : 1f;
//            if (_overrideScale) scaleFactor = spatialModel.CurrentModel.Scale;
//            Extensions.Log($"[GLB] model scale:{spatialModel.CurrentModel.Scale}\nscale factor: {scaleFactor}\nmin scale: {MinScale}\nexisting asset: {existing}\noverride scale:{_overrideScale}\nid: {spatialModel.GetItemId}", gameObject);
//            spatialModel.OnScaleChange?.Invoke(scaleFactor);
//            _overrideScale = false;

//            if (!existing) //send scale factor to the asset control UI, is placing new asset
//                EventsController.TriggerActions(EventId.ScaleAsset, scaleFactor, MinScale);
            
//            await UniTask.Yield(PlayerLoopTiming.Update);
//            _glbInstance.SetLayerRecursively(DefaultLayer.GetLayerMaskIndex());
//        }

//        private float GetWidestAxis(Vector3 size)
//        {
//            var axis = new[] { size.x, size.y, size.z };
//            return axis.Max();
//        }

//        private void CenterGlb(Transform glbPivot, BoxCollider generatedCollider)
//        {
//            var colPosition = generatedCollider.bounds.center;
//            var offset = transform.position - colPosition;
//            glbPivot.transform.position += offset;
//            generatedCollider.center = Vector3.zero;
//        }

//        public string GetCurrentClip()
//        {
//            return _cachedClip;
//        }

//        private float GetAnimTime()
//        {
//            return (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _timeStamp) / 1000f;
//        }

//        public void SetSelected(bool selected)
//        {
//            if (_materials == null) return;
//            IsSelected = selected;
//            for (int i = 0; i < _materials.Count; i++)
//            {
//                if (selected)
//                {
//                    _materialColors[i] = _materials[i].GetColor(_colorShaderPropertyId);
//                }
                
//                var defaultColor = _materialColors != null && _materialColors.Count == _materials.Count ? _materialColors[i] : Color.white;
//                var color = selected ? new Color(0.5f, 0.5f, 2f, 1f) : defaultColor;
//                _materials[i].SetColor(_colorShaderPropertyId, color);
//            }
//        }

//        public void OverrideScale(bool value)
//        {
//            _overrideScale = true;
//        }

//        private void OnApplicationFocus(bool hasFocus)
//        {
//            if (hasFocus)
//                PlayAnimation(_glbAnimation, _cachedClip);
//        }
//    }
//}
