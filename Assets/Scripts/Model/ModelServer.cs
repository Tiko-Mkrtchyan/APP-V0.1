using System;
using System.Collections.Generic;
using McKenna.SpatialAssets;
using UnityEngine;

namespace Models
{
    [CreateAssetMenu(fileName = "ModelServer", menuName = "Model Server")]
    public class ModelServer : ScriptableObject
    {
        private Observable<UserLoginResponse> _userCredential = new();
        private ObservableDict<(string, ImageSize), string> _cachedImages = new();
        private Observable<List<string>> _glbCategories = new();
        private Observable<ResponsesList<GlbResponse>> _glbLibrary = new();
        // private Observable<ThumbnailListRecord> _Thumbnails = new();
        // private Observable<MediaListResponse> _mediaLibrary = new();
        // private Observable<LayerRecord> _currentLayer = new();
        // private Observable<LayerListResponse> _layerList = new();
        // private Observable<ItemsResponse> _allItemsOnBackend = new();
        // private Observable<ItemsResponse> _itemsInLayer = new();
        // private ObservableDict<string, SpatialAsset> _spawnedItems = new();
        // private ObservableDict<(string, ImageSize), CachedMediaModel> _cachedAssets = new();

        // private ObservableDict<string, string> _cachedNftPath = new();
        // private ObservableDict<string, CachedMediaModel> _cachedNft = new();
        // private ObservableDict<string, CachedMediaModel> _cachedNftThumbnails = new();
        // private ObservableDict<string, GameObject> _cacheGlb = new();

        // private ObservableDict<LocalizeKey, Dictionary<string, string>> _localizations = new();
        // private Observable<float> _placementDistance = new();

        public Observable<UserLoginResponse> UserCredential => _userCredential;
        public ObservableDict<(string, ImageSize), string> CachedImages => _cachedImages;
        public Observable<List<string>> GlbCategories => _glbCategories;
        public Observable<ResponsesList<GlbResponse>> GlbLibrary => _glbLibrary;

        // public Observable<MediaListResponse> MediaLibrary => _mediaLibrary;
        // // public Observable<ThumbnailListRecord> Thumbnails => _Thumbnails;
        // public Observable<LayerRecord> CurrentLayer => _currentLayer;
        // public Observable<LayerListResponse> LayerList => _layerList;
        // public Observable<ItemsResponse> AllItemsOnBackend => _allItemsOnBackend;
        // public Observable<ItemsResponse> ItemsInLayer => _itemsInLayer;
        // public ObservableDict<string, SpatialAsset> SpawnedItems => _spawnedItems;
        // public ObservableDict<(string, MediaImageSize), CachedMediaModel> CachedAssets => _cachedAssets;
        // public ObservableDict<string, string> CachedNftPath => _cachedNftPath;
        // public ObservableDict<string, CachedMediaModel> CachedNft => _cachedNft;
        // public ObservableDict<string, GameObject> CachedGlb => _cacheGlb;
        // public ObservableDict<string, CachedMediaModel> CachedNftThumbnails => _cachedNftThumbnails;
        // public ObservableDict<LocalizeKey, Dictionary<string, string>> Localizations => _localizations;
        // public Observable<float> PlacementDistance => _placementDistance;
        // public ObservableList<WalletInfo> WalletAddresses => _walletAddresses;
        // public ObservableDict<string, NFTCollection> NftsCollection => _nftsCollection;
        
        public void Initialize()
        {
            _userCredential = new();
            _cachedImages = new();
            _glbCategories = new();
            _glbLibrary = new();
            // _mediaLibrary = new();
            // _currentLayer = new();
            // _layerList = new();
            // _allItemsOnBackend = new();
            // _itemsInLayer = new();
            // _spawnedItems = new();
            // _cachedAssets = new();
            // _cachedNftPath = new();
            // _cachedNft = new();
            // _cachedNftThumbnails = new();
            // _localizations = new();
            // _placementDistance = new();
            // _cacheGlb = new();
        }
    }
}