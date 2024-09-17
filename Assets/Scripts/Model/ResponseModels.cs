using Newtonsoft.Json;
using System.Collections.Generic;

public class MediaResponse
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

    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public string Type;

    [JsonProperty("file", NullValueHandling = NullValueHandling.Ignore)]
    public string File;

    [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
    public string Owner;
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
