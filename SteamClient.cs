using System.Text.RegularExpressions;
using Microsoft.Win32;
using SteamAccountsFinder.Helpers;

namespace SteamAccountsFinder;

public class SteamClient
{
    public SteamClient()
    {
        InstallationDirectory = GetGenuineInstallationPath();
        SteamappsDirectory = GetSteamappsDirectory(InstallationDirectory?.FullName);
        LibraryfoldersFile = GetLibraryfoldersFile();
        SteamLibraries = GetSteamLibraries();

        UserdataDirectory = LocationRecipient.GetDirectory(InstallationDirectory.FullName, "userdata");
        ConfigDirectory = LocationRecipient.GetDirectory(InstallationDirectory.FullName, "config");
        LoginusersFile = LocationRecipient.GetFile(ConfigDirectory.FullName, "loginusers.vdf");
        ConfigFile = LocationRecipient.GetFile(ConfigDirectory.FullName, "config.vdf");
    }

    public DirectoryInfo InstallationDirectory { get; }
    public DirectoryInfo SteamappsDirectory { get; }
    public FileInfo LibraryfoldersFile { get; }
    public DirectoryInfo[] SteamLibraries { get; }

    public DirectoryInfo ConfigDirectory { get; }
    public DirectoryInfo UserdataDirectory { get; }
    public FileInfo LoginusersFile { get; }
    public FileInfo ConfigFile { get; }

    public static DirectoryInfo GetWorkshopDirectory(string steamappsDirectory)
    {
        return LocationRecipient.GetDirectory(steamappsDirectory, "workshop");
    }

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
        using var localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using var steam = localMachine64.OpenSubKey("SOFTWARE")?.OpenSubKey("WOW6432Node")?.OpenSubKey("Valve")
            ?.OpenSubKey("Steam");
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
        if (directory.Exists is false) return false;
        var steamFiles = directory.GetFiles();
        return steamFiles.Count(x => x.Name is "steam.exe" or "Steam.dll") == 2;
    }

    public static DirectoryInfo GetSteamappsDirectory(string steamLibraryPath)
    {
        return LocationRecipient.GetDirectory(steamLibraryPath, "steamapps");
    }

    private FileInfo GetLibraryfoldersFile()
    {
        if (SteamappsDirectory == default) return default;
        if (SteamappsDirectory.Exists is false) return default;

        var files = SteamappsDirectory?.GetFiles();
        return files?.FirstOrDefault(file => file.Name == "libraryfolders.vdf");
    }

    private DirectoryInfo[] GetSteamLibraries()
    {
        if (LibraryfoldersFile == default) return Array.Empty<DirectoryInfo>();
        var fileContent = File.ReadAllText(LibraryfoldersFile.FullName);
        var libraries = Regex.Matches(fileContent, "\"path\".+\"(.+?)\"").Select(x => x.Groups[1].Value);
        return libraries.Select(x => new DirectoryInfo(x)).Where(x => x.Exists).ToArray();
    }
}