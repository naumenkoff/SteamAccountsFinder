namespace SteamAccountsFinder;

public class LocationRecipient
{
    public static bool TryGetDirectory(out DirectoryInfo directory, params string[] paths)
    {
        directory = default;
        if (paths.Any(x => x == default)) return false;

        var path = Path.Combine(paths);
        directory = new DirectoryInfo(path);
        return directory.Exists;
    }

    public static DirectoryInfo GetDirectory(params string[] paths)
    {
        if (paths.Any(x => x == default)) return default;

        var path = Path.Combine(paths);
        var directory = new DirectoryInfo(path);
        return directory;
    }

    public static bool TryGetFile(out FileInfo file, params string[] paths)
    {
        file = default;
        if (paths.Any(x => x == default)) return false;

        var path = Path.Combine(paths);
        file = new FileInfo(path);
        return file.Exists;
    }
    
    public static FileInfo GetFile(params string[] paths)
    {
        if (paths.Any(x => x == default)) return default;

        var path = Path.Combine(paths);
        var file = new FileInfo(path);
        return file;
    }

    public static bool TryReadFileContent(out string content, params string[] paths)
    {
        content = default;
        var path = Path.Combine(paths);
        try
        {
            content = File.ReadAllText(path);
            return true;
        }
        catch
        {
            return false;
        }
    }
}