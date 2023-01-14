using System.Text.RegularExpressions;
using Microsoft.Win32;
using SteamAccountsFinder.Helpers;

namespace SteamAccountsFinder;

public static class SteamClient
{
    public static FileSystemInfo ConfigFile { get; private set; }
    public static FileSystemInfo LoginusersFile { get; private set; }
    public static FileSystemInfo[] SteamLibraries { get; private set; }
    public static DirectoryInfo UserdataDirectory { get; private set; }

    public static Task InitializeAsync()
    {
        return Task.Run(() =>
        {
            var installationDirectory = GetGenuineInstallationPath();
            var steamappsDirectory = GetSteamappsDirectory(installationDirectory);
            var libraryfoldersFile = LocationRecipient.GetFile(steamappsDirectory?.FullName, "libraryfolders.vdf");
            var configDirectory = LocationRecipient.GetDirectory(installationDirectory.FullName, "config");

            SteamLibraries = GetSteamLibraries(libraryfoldersFile);
            UserdataDirectory = LocationRecipient.GetDirectory(installationDirectory.FullName, "userdata");
            LoginusersFile = LocationRecipient.GetFile(configDirectory.FullName, "loginusers.vdf");
            ConfigFile = LocationRecipient.GetFile(configDirectory.FullName, "config.vdf");
        });
    }

    private static DirectoryInfo GetGenuineInstallationPath()
    {
        var registryPath = GetInstallPathFromRegistry();
        if (IsClientGenuine(registryPath)) return registryPath;

        throw new DirectoryNotFoundException("The Steam installation path from the registry isn't genuine!");
    }

    private static DirectoryInfo GetInstallPathFromRegistry()
    {
        using var steam = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
            .OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?
            .OpenSubKey("Valve")?.OpenSubKey("Steam");
        var path = steam?.GetValue("InstallPath")?.ToString();
        return string.IsNullOrEmpty(path) ? default : new DirectoryInfo(path);
    }

    private static bool IsClientGenuine(DirectoryInfo directory)
    {
        if (LocationRecipient.FileSystemInfoExists(directory) is false) return false;

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

    private static FileSystemInfo[] GetSteamLibraries(FileSystemInfo libraryfoldersFile)
    {
        if (LocationRecipient.TryReadFileContent(out var fileContent, libraryfoldersFile) is false)
            return Array.Empty<FileSystemInfo>();

        var libraries = Regex.Matches(fileContent, "\"path\".+\"(.+?)\"").Select(x => x.Groups[1].Value);
        return libraries.Select(x => new DirectoryInfo(x) as FileSystemInfo).Where(x => x.Exists).ToArray();
    }
}