using System;
using UnityEngine;

public static class UrlExtensions
{
    public static string GetImageUrl(this ImageAssetResponse media, ImageSize imageSize = ImageSize.Original)
    {
        Vector2 size = default;
        switch (imageSize)
        {
            case ImageSize.Original:
                size = default;
                break;
            case ImageSize.Thumbnail:
                size = new Vector2(250f, 0f);
                break;
        }

        return GetImageUrl(media, size);
    }

    private static string GetImageUrl(this ImageAssetResponse media, Vector2 thumbnailSize)
    {
        var thumbParam = thumbnailSize != default ? $"?thumb={thumbnailSize.x}x{thumbnailSize.y}" : string.Empty;
        var str = $"{Values.ApiUrl}files/{media.CollectionId}/{media.Id}/{media.Image}{thumbParam}";
        return str;
    }

    public static string GetGlbUrl(this GlbResponse media)
    {
        var str = $"{Values.ApiUrl}files/{media.CollectionId}/{media.Id}/{media.File}";
        return str;
    }

    public static string GetGlbThumbnailUrl(this GlbResponse media)
    {
        var str = $"{Values.ApiUrl}files/{media.CollectionId}/{media.Id}/{media.Thumb}";
        return str;
    }

    public static string GetFileName(this string url)
    {
        try
        {
            var uri = new Uri(url);
            var path = uri.LocalPath;
            var pathSegments = path.Split('/');
    
            if (pathSegments.Length > 0)
            {
                var fileName = $"{pathSegments[^1]}";
                return fileName;
            }
        }
        catch (Exception e)
        {
            Extensions.LogError("Error extracting file name from URL: " + e.Message);
        }
    
        return null;
    }
}