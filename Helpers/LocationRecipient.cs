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

    public static bool DirectoryExists(FileSystemInfo directory)
    {
        return directory?.Exists == true;
    }

    public static bool TryReadFileContent(out string content, FileSystemInfo file)
    {
        content = default;

        if (file?.Exists is false) return false;

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