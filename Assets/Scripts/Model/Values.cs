using UnityEngine;

public static class Values
{
    public static string ApiUrl = "http://127.0.0.1:8090/api/";

    public static readonly string MediaFolder = $"{Application.persistentDataPath}/Media";
}

public static class RestMethod
{
    public const string Get = "GET";
    public const string Post = "POST";
    public const string Patch = "PATCH";
    public const string Delete = "DELETE";

}