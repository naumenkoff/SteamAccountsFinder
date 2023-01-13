namespace SteamAccountsFinder.Helpers;

public static class LocationRecipient
{
    public static DirectoryInfo GetDirectory(params string[] paths)
    {
        if (paths.Any(x => x == default)) return default;

        var path = Path.Combine(paths);
        var directory = new DirectoryInfo(path);
        return directory;
    }

    public static FileInfo GetFile(params string[] paths)
    {
        if (paths.Any(x => x == default)) return default;

        var path = Path.Combine(paths);
        var file = new FileInfo(path);
        return file;
    }

    public static bool DirectoryExists(DirectoryInfo directory)
    {
        return directory?.Exists == true;
    }

    public static bool FileExists(FileInfo file)
    {
        return file?.Exists == true;
    }

    public static bool TryReadFileContent(out string content, FileInfo file)
    {
        content = default;

        if (FileExists(file) is false) return false;
        
        try
        {
            content = File.ReadAllText(file.FullName);
            return true;
        }
        catch
        {
            return false;
        }
    }
}