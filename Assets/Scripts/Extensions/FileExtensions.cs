using System.IO;

public static class FileExtensions
{
        public static bool CheckIfFileExist(this string fileName, out string filePath, ImageSize imageSize = ImageSize.Original)
    {
        var path = GeneratePath(fileName, imageSize);
        filePath = path;
        return File.Exists(path);
    }
        
    public static string GeneratePath(this string fileName, ImageSize imageSize)
    {
        if (!Directory.Exists(Values.MediaFolder))
            Directory.CreateDirectory(Values.MediaFolder);

        if (!Directory.Exists($"{Values.MediaFolder}/{imageSize}"))
            Directory.CreateDirectory($"{Values.MediaFolder}/{imageSize}");
            
        return $"{Values.MediaFolder}/{imageSize}/{fileName}";
    }
}