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

    private async Task<List<IDetectedAccount>> ScanSteamLibraries()
    {
        var accounts = new List<IDetectedAccount>();
        foreach (var library in _steamClient.SteamLibraries)
        {
            var steamappsDirectory = SteamClient.GetSteamappsDirectory(library.FullName);
            var appmanifestAccounts = await AppmanifestContent.GetIDetectedAccounts(steamappsDirectory);
            var workshopDirectory = SteamClient.GetWorkshopDirectory(steamappsDirectory.FullName);
            var appworkshopAccounts = await AppworkshopContent.GetIDetectedAccounts(workshopDirectory);
            accounts.AddRange(appmanifestAccounts);
            accounts.AddRange(appworkshopAccounts);
        }

        return accounts;
    }

    public async Task InitializeAccounts()
    {
        var start = Stopwatch.GetTimestamp();
        var task = await Task.WhenAll(ConfigContent.GetIDetectedAccounts(_steamClient),
            LoginusersContent.GetIDetectedAccounts(_steamClient),
            RegistryContent.GetIDetectedAccounts(),
            UserdataContent.GetIDetectedAccounts(_steamClient),
            ScanSteamLibraries());

        foreach (var collection in task)
        foreach (var account in collection)
            account?.Attach();

        Console.WriteLine(
            $"Local Accounts were initialized for {(Stopwatch.GetTimestamp() - start) / 10000f / 1000f:F} sec.");
    }
}