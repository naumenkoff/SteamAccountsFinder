using SteamAccountsFinder.Models;
using SteamAccountsFinder.Models.ConfigDirectory;
using SteamAccountsFinder.Models.SteamappsDirectory;
using SteamAccountsFinder.Models.UserdataDirectory;
using SteamAccountsFinder.Models.ValveRegistry;

namespace SteamAccountsFinder;

public class LocalAccount : ISteamID
{
    private static readonly List<LocalAccount> Container = new();
    private readonly List<AppState> _dataFromAppmanifest;
    private readonly List<AppWorkshop> _dataFromAppworkshop;
    private readonly List<Userdata> _dataFromUserdata;

    private LocalAccount(ISteamID steamData)
    {
        Steam32 = steamData.Steam32;
        Steam64 = steamData.Steam64;
        _dataFromAppmanifest = new List<AppState>();
        _dataFromAppworkshop = new List<AppWorkshop>();
        _dataFromUserdata = new List<Userdata>();
    }

    public static IEnumerable<LocalAccount> Accounts => Container;
    public IReadOnlyCollection<AppState> DataFromAppmanifest => _dataFromAppmanifest;
    public IReadOnlyCollection<AppWorkshop> DataFromAppWorkshop => _dataFromAppworkshop;
    public IReadOnlyCollection<Userdata> DataFromUserdata => _dataFromUserdata;
    public InstallConfigStore DataFromConfig { get; private set; }
    public Loginusers DataFromLoginusers { get; private set; }
    public RegistryUsers DataFromRegistry { get; private set; }
    public int NumberOfDublicates { get; private set; }
    public string Login => GetLogin();
    public long Steam32 { get; }
    public long Steam64 { get; }

    public static LocalAccount GetAccount(ISteamID steamData)
    {
        var account = Container.ToList()
            .FirstOrDefault(x => x.Steam32 == steamData.Steam32 && x.Steam64 == steamData.Steam64);
        if (account != default)
        {
            account.NumberOfDublicates++;
            return account;
        }

        account = new LocalAccount(steamData);
        Container.Add(account);
        return account;
    }

    public void AddAppState(AppState appState)
    {
        _dataFromAppmanifest.Add(appState);
    }

    public void AddAppWorkshop(AppWorkshop appWorkshop)
    {
        _dataFromAppworkshop.Add(appWorkshop);
    }

    public void AddUserdata(Userdata userdata)
    {
        _dataFromUserdata.Add(userdata);
    }

    public void AddInstallConfigStore(InstallConfigStore installConfigStore)
    {
        DataFromConfig = installConfigStore;
    }

    public void AddLoginusers(Loginusers loginusers)
    {
        DataFromLoginusers = loginusers;
    }

    public void AddRegistryUsers(RegistryUsers registryUsers)
    {
        DataFromRegistry = registryUsers;
    }

    private string GetLogin()
    {
        return DataFromConfig?.Login == DataFromLoginusers?.Login
            ? DataFromConfig?.Login
            : $"{DataFromConfig?.Login} | {DataFromLoginusers?.Login}";
    }
}