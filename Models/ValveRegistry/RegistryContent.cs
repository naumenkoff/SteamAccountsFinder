using Microsoft.Win32;

namespace SteamAccountsFinder.Models.ValveRegistry;

public class RegistryContent : ISteamID, IDetectedAccount
{
    private RegistryContent(string registryPath, string id)
    {
        Steam32 = long.Parse(id);
        Steam64 = ISteamID.GetSteam64(Steam32);
        RegistryPath = registryPath;
    }

    public string RegistryPath { get; }

    public void Attach()
    {
        if (ISteamID.IsSteam64(Steam64) is false) return;

        var localAccount = LocalAccount.GetAccount(this);
        localAccount.Attach(this);
    }

    public long Steam32 { get; }
    public long Steam64 { get; }

    private static IDetectedAccount CreateIDetectedAccount(string registryPath, string id)
    {
        var account = new RegistryContent(registryPath, id);
        return account;
    }

    public static Task<IDetectedAccount[]> GetIDetectedAccounts()
    {
        using var registryKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Valve")?
            .OpenSubKey("Steam")?.OpenSubKey("Users");
        if (registryKey == default) return Task.FromResult(Array.Empty<IDetectedAccount>());

        var users = registryKey.GetSubKeyNames();
        var accounts =
            (from user in users
                let path = Path.Combine(registryKey.Name, user)
                select CreateIDetectedAccount(path, user)).ToArray();
        return Task.FromResult(accounts);
    }
}