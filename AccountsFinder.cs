using System.Diagnostics;
using SteamAccountsFinder.Models;
using SteamAccountsFinder.Models.ConfigDirectory;
using SteamAccountsFinder.Models.SteamappsDirectory;
using SteamAccountsFinder.Models.UserdataDirectory;
using SteamAccountsFinder.Models.ValveRegistry;

namespace SteamAccountsFinder;

public class AccountsFinder
{
    private readonly SteamClient _steamClient;

    public AccountsFinder(SteamClient steamClient)
    {
        _steamClient = steamClient;
    }

    private async Task<IDetectedAccount[]> ScanSteamLibraries()
    {
        var accounts = new List<IDetectedAccount>();

        foreach (var library in _steamClient.SteamLibraries)
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

    public async Task InitializeAccounts()
    {
        var start = Stopwatch.GetTimestamp();

        var result = await Task.WhenAll(ConfigContent.GetIDetectedAccounts(_steamClient),
            LoginusersContent.GetIDetectedAccounts(_steamClient), RegistryContent.GetIDetectedAccounts(),
            UserdataContent.GetIDetectedAccounts(_steamClient), ScanSteamLibraries());

        foreach (var detectedAccounts in result)
        foreach (var detectedAccount in detectedAccounts)
            detectedAccount?.Attach();

        Console.WriteLine($"Loading time of Local Accounts: {Stopwatch.GetElapsedTime(start).TotalMilliseconds:F} ms.");
    }
}