using System.Diagnostics;
using SteamAccountsFinder.Models;
using SteamAccountsFinder.Models.ConfigDirectory;
using SteamAccountsFinder.Models.SteamappsDirectory;
using SteamAccountsFinder.Models.UserdataDirectory;
using SteamAccountsFinder.Models.ValveRegistry;

namespace SteamAccountsFinder;

public static class AccountsFinder
{
    private static async Task<IDetectedAccount[]> ScanSteamLibraries()
    {
        if (SteamClient.SteamLibraries == default) return Array.Empty<IDetectedAccount>();
        var accounts = new List<IDetectedAccount>();

        foreach (var library in SteamClient.SteamLibraries)
        {
            var steamappsDirectory = SteamClient.GetSteamappsDirectory(library);
            var appmanifestAccounts = await AppmanifestContent.GetIDetectedAccounts(steamappsDirectory);
            var workshopDirectory = SteamClient.GetWorkshopDirectory(steamappsDirectory);
            var appworkshopAccounts = await AppworkshopContent.GetIDetectedAccounts(workshopDirectory);
            accounts.AddRange(appmanifestAccounts);
            accounts.AddRange(appworkshopAccounts);
        }

        return accounts.ToArray();
    }

    public static async Task InitializeAccounts()
    {
        var start = Stopwatch.GetTimestamp();

        var result = await Task.WhenAll(ConfigContent.GetIDetectedAccounts(),
            LoginusersContent.GetIDetectedAccounts(), RegistryContent.GetIDetectedAccounts(),
            UserdataContent.GetIDetectedAccounts(), ScanSteamLibraries());

        foreach (var detectedAccounts in result)
        foreach (var detectedAccount in detectedAccounts)
            detectedAccount?.Attach();

        Console.WriteLine($"Loading time of Local Accounts: {Stopwatch.GetElapsedTime(start).TotalMilliseconds:F} ms.");
    }
}