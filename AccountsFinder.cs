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
            var steamApps = SteamClient.GetSteamappsDirectory(library.FullName);
            var appstates = await AppState.GetIDetectedAccounts(steamApps);
            var workshop = SteamClient.GetWorkshopDirectory(steamApps.FullName);
            var appworkshops = await AppWorkshop.GetIDetectedAccounts(workshop);
            accounts.AddRange(appstates);
            accounts.AddRange(appworkshops);
        }

        return accounts;
    }

    public async Task InitializeAccounts()
    {
        var start = Stopwatch.GetTimestamp();
        var task = await Task.WhenAll(InstallConfigStore.GetIDetectedAccounts(_steamClient),
            Loginusers.GetIDetectedAccounts(_steamClient),
            RegistryUsers.GetIDetectedAccounts(),
            Userdata.GetIDetectedAccounts(_steamClient),
            ScanSteamLibraries());

        foreach (var collection in task)
        foreach (var account in collection)
            account?.Attach();

        Console.WriteLine(
            $"Local Accounts were initialized for {(Stopwatch.GetTimestamp() - start) / 10000f / 1000f:F} sec.");
    }
}