using Newtonsoft.Json;
using System.Collections.Generic;

public class ImageAssetResponse
{
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id;

        [JsonProperty("collectionId", NullValueHandling = NullValueHandling.Ignore)]
        public string CollectionId;

        [JsonProperty("collectionName", NullValueHandling = NullValueHandling.Ignore)]
        public string CollectionName;

        [JsonProperty("created", NullValueHandling = NullValueHandling.Ignore)]
        public string Created;

        [JsonProperty("updated", NullValueHandling = NullValueHandling.Ignore)]
        public string Updated;

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name;

        [JsonProperty("layer", NullValueHandling = NullValueHandling.Ignore)]
        public string Layer;

        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public string Image;

        [JsonProperty("pose", NullValueHandling = NullValueHandling.Ignore)]
        public string Pose;

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public string Data;
}

public class GlbAssetResponse
{
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id;

        [JsonProperty("collectionId", NullValueHandling = NullValueHandling.Ignore)]
        public string CollectionId;

        [JsonProperty("collectionName", NullValueHandling = NullValueHandling.Ignore)]
        public string CollectionName;

        [JsonProperty("created", NullValueHandling = NullValueHandling.Ignore)]
        public string Created;

        [JsonProperty("updated", NullValueHandling = NullValueHandling.Ignore)]
        public string Updated;

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name;

        [JsonProperty("layer", NullValueHandling = NullValueHandling.Ignore)]
        public string Layer;

        [JsonProperty("glb", NullValueHandling = NullValueHandling.Ignore)]
        public string Glb;

        [JsonProperty("pose", NullValueHandling = NullValueHandling.Ignore)]
        public string Pose;

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public string Data;

        [JsonProperty("expand", NullValueHandling = NullValueHandling.Ignore)]
        public GlbExpand Expand;
}

public class GlbExpand
{
    [JsonProperty("glb", NullValueHandling = NullValueHandling.Ignore)]
    public GlbResponse Glb;
}

public class GlbResponse
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id;

    [JsonProperty("collectionId", NullValueHandling = NullValueHandling.Ignore)]
    public string CollectionId;

    [JsonProperty("collectionName", NullValueHandling = NullValueHandling.Ignore)]
    public string CollectionName;

    [JsonProperty("created", NullValueHandling = NullValueHandling.Ignore)]
    public string Created;

    [JsonProperty("updated", NullValueHandling = NullValueHandling.Ignore)]
    public string Updated;

    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name;

    [JsonProperty("file", NullValueHandling = NullValueHandling.Ignore)]
    public string File;
    
    [JsonProperty("thumb", NullValueHandling = NullValueHandling.Ignore)]
    public string Thumb;

    [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
    public string Category;
}

public class DomainResponse
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id;

    [JsonProperty("collectionId", NullValueHandling = NullValueHandling.Ignore)]
    public string CollectionId;

    [JsonProperty("collectionName", NullValueHandling = NullValueHandling.Ignore)]
    public string CollectionName;

    [JsonProperty("created", NullValueHandling = NullValueHandling.Ignore)]
    public string Created;

    [JsonProperty("updated", NullValueHandling = NullValueHandling.Ignore)]
    public string Updated;

    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name;

    [JsonProperty("domainId", NullValueHandling = NullValueHandling.Ignore)]
    public string DomainId;

    [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
    public string Owner;
}

public class LayerResponse
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id;

    [JsonProperty("collectionId", NullValueHandling = NullValueHandling.Ignore)]
    public string CollectionId;

    [JsonProperty("collectionName", NullValueHandling = NullValueHandling.Ignore)]
    public string CollectionName;

    [JsonProperty("created", NullValueHandling = NullValueHandling.Ignore)]
    public string Created;

    [JsonProperty("updated", NullValueHandling = NullValueHandling.Ignore)]
    public string Updated;

    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name;

    [JsonProperty("domain", NullValueHandling = NullValueHandling.Ignore)]
    public string Domain;
}

public class ResponsesList<T>
{
    [JsonProperty("page", NullValueHandling = NullValueHandling.Ignore)]
    public int Page;

    [JsonProperty("perPage", NullValueHandling = NullValueHandling.Ignore)]
    public int PerPage;

    [JsonProperty("totalPages", NullValueHandling = NullValueHandling.Ignore)]
    public int TotalPages;

    [JsonProperty("totalItems", NullValueHandling = NullValueHandling.Ignore)]
    public int TotalItems;

    [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
    public List<T> Items;
}

    public class UserRecord
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id;

        [JsonProperty("collectionId", NullValueHandling = NullValueHandling.Ignore)]
        public string CollectionId;

        [JsonProperty("collectionName", NullValueHandling = NullValueHandling.Ignore)]
        public string CollectionName;

        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username;

        [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
        public bool Verified;

        [JsonProperty("emailVisibility", NullValueHandling = NullValueHandling.Ignore)]
        public bool EmailVisibility;

        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email;

        [JsonProperty("created", NullValueHandling = NullValueHandling.Ignore)]
        public string Created;

        [JsonProperty("updated", NullValueHandling = NullValueHandling.Ignore)]
        public string Updated;

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name;

        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public string Avatar;
    }

    public class UserLoginResponse
    {
        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token;

        [JsonProperty("record", NullValueHandling = NullValueHandling.Ignore)]
        public UserRecord Record;
    }
