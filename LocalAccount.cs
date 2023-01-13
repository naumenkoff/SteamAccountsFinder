using SteamAccountsFinder.Models;
using SteamAccountsFinder.Models.ConfigDirectory;
using SteamAccountsFinder.Models.SteamappsDirectory;
using SteamAccountsFinder.Models.UserdataDirectory;
using SteamAccountsFinder.Models.ValveRegistry;

namespace SteamAccountsFinder;

public class LocalAccount : ISteamID
{
    private static readonly List<LocalAccount> Container = new();
    private readonly List<AppmanifestContent> _dataFromAppmanifest;
    private readonly List<AppworkshopContent> _dataFromAppworkshop;
    private readonly List<UserdataContent> _dataFromUserdata;

    private LocalAccount(ISteamID steamData)
    {
        Steam32 = steamData.Steam32;
        Steam64 = steamData.Steam64;
        _dataFromAppmanifest = new List<AppmanifestContent>();
        _dataFromAppworkshop = new List<AppworkshopContent>();
        _dataFromUserdata = new List<UserdataContent>();
    }

    public static IEnumerable<LocalAccount> Accounts => Container;
    public IReadOnlyCollection<AppmanifestContent> DataFromAppmanifest => _dataFromAppmanifest;
    public IReadOnlyCollection<AppworkshopContent> DataFromAppWorkshop => _dataFromAppworkshop;
    public IReadOnlyCollection<UserdataContent> DataFromUserdata => _dataFromUserdata;
    public ConfigContent DataFromConfig { get; private set; }
    public LoginusersContent DataFromLoginusersContent { get; private set; }
    public RegistryContent DataFromRegistry { get; private set; }
    public int DetectionsCount { get; private set; }
    public string Login => GetLogin();
    public long Steam32 { get; }
    public long Steam64 { get; }

    public static LocalAccount GetAccount(ISteamID steamData)
    {
        var account = Container.FirstOrDefault(x => ISteamID.IsMatch(x, steamData));
        if (account != default) return account;

        account = new LocalAccount(steamData);
        Container.Add(account);
        return account;
    }

    public void Attach(IDetectedAccount account)
    {
        var type = account.GetType();

        if (type == typeof(ConfigContent)) DataFromConfig = account as ConfigContent;
        else if (type == typeof(LoginusersContent)) DataFromLoginusersContent = account as LoginusersContent;
        else if (type == typeof(AppmanifestContent)) _dataFromAppmanifest.Add(account as AppmanifestContent);
        else if (type == typeof(AppworkshopContent)) _dataFromAppworkshop.Add(account as AppworkshopContent);
        else if (type == typeof(UserdataContent)) _dataFromUserdata.Add(account as UserdataContent);
        else if (type == typeof(RegistryContent)) DataFromRegistry = account as RegistryContent;

        DetectionsCount++;
    }

    private string GetLogin()
    {
        return DataFromConfig?.Login == DataFromLoginusersContent?.Login
            ? DataFromConfig?.Login
            : $"{DataFromConfig?.Login} | {DataFromLoginusersContent?.Login}";
    }
}