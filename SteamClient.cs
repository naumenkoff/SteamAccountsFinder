using System.Text.RegularExpressions;
using Microsoft.Win32;
using SteamAccountsFinder.Helpers;

namespace SteamAccountsFinder;

public class SteamClient
{
    private readonly FileSystemInfo _libraryfoldersFile;
    private readonly FileSystemInfo _steamappsDirectory;

    public SteamClient()
    {
        var installationDirectory = GetGenuineInstallationPath();
        _steamappsDirectory = GetSteamappsDirectory(installationDirectory);
        _libraryfoldersFile = GetLibraryfoldersFile();
        SteamLibraries = GetSteamLibraries();

        UserdataDirectory = LocationRecipient.GetDirectory(installationDirectory.FullName, "userdata");
        var configDirectory = LocationRecipient.GetDirectory(installationDirectory.FullName, "config");
        LoginusersFile = LocationRecipient.GetFile(configDirectory.FullName, "loginusers.vdf");
        ConfigFile = LocationRecipient.GetFile(configDirectory.FullName, "config.vdf");
    }

    public FileSystemInfo[] SteamLibraries { get; }
    public DirectoryInfo UserdataDirectory { get; }
    public FileSystemInfo LoginusersFile { get; }
    public FileSystemInfo ConfigFile { get; }

    private static DirectoryInfo GetGenuineInstallationPath()
    {
        if (TryGetInstallPathFromRegistry(out var installPathFromRegistry))
            if (IsClientGenuine(installPathFromRegistry))
                return installPathFromRegistry;

        // is this a weird method? Ofc, no.
        // Here you can add your own methods for getting Steam client paths.
        // For example, by reading the value from app.config,
        // so you will get two checks and you will be able to throw an exception
        // that all the paths that you found are not valid.

        throw new DirectoryNotFoundException("The Steam installation path from the registry isn't genuine!");
    }

    private static bool TryGetInstallPathFromRegistry(out DirectoryInfo steamClientDirectory)
    {
        using var steam = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
            .OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?
            .OpenSubKey("Valve")?.OpenSubKey("Steam");

        var path = steam?.GetValue("InstallPath")?.ToString();
        if (string.IsNullOrEmpty(path))
        {
            steamClientDirectory = default;
            return false;
        }

        steamClientDirectory = new DirectoryInfo(path);
        return true;
    }

    private static bool IsClientGenuine(DirectoryInfo directory)
    {
        if (LocationRecipient.DirectoryExists(directory) is false) return false;

        var steamFiles = directory.GetFiles();
        return steamFiles.Count(x => x.Name is "steam.exe" or "Steam.dll") == 2;
    }

    public static DirectoryInfo GetSteamappsDirectory(FileSystemInfo steamLibraryPath)
    {
        return LocationRecipient.GetDirectory(steamLibraryPath?.FullName, "steamapps");
    }

    public static DirectoryInfo GetWorkshopDirectory(FileSystemInfo steamappsDirectory)
    {
        return LocationRecipient.GetDirectory(steamappsDirectory?.FullName, "workshop");
    }

    private FileSystemInfo GetLibraryfoldersFile()
    {
        return LocationRecipient.GetFile(_steamappsDirectory?.FullName, "libraryfolders.vdf");
    }

    private FileSystemInfo[] GetSteamLibraries()
    {
        if (LocationRecipient.TryReadFileContent(out var fileContent, _libraryfoldersFile) is false)
            return Array.Empty<FileSystemInfo>();

        var libraries = Regex.Matches(fileContent, "\"path\".+\"(.+?)\"").Select(x => x.Groups[1].Value);
        return libraries.Select(x => new DirectoryInfo(x) as FileSystemInfo).Where(x => x.Exists).ToArray();
    }
}