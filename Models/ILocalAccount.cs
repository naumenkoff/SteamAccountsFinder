using SteamAccountsFinder.AccountEntries;

namespace SteamAccountsFinder.Models;

public interface ILocalAccount : ISteamID
{
    IEnumerable<IAppmanifestEntry> Appmanifest { get; }
    IEnumerable<IAppworkshopEntry> Appworkshop { get; }
    IUserdataEntry Userdata { get; }
    IConfigEntry Config { get; }
    ILoginusersEntry Loginusers { get; }
    IRegistryEntry Registry { get; }
    int DetectionsCount { get; }
    public string GetLogin();
}