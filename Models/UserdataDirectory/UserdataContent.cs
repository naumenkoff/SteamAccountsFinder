namespace SteamAccountsFinder.Models.UserdataDirectory;

public class UserdataContent : ISteamID, IDetectedAccount
{
    private UserdataContent(DirectoryInfo directory)
    {
        Steam32 = long.Parse(directory.Name);
        Steam64 = ISteamID.GetSteam64(Steam32);
        ContainingDirectory = directory;
    }

    public DirectoryInfo ContainingDirectory { get; }

    public void Attach()
    {
        if (ISteamID.IsSteam64(Steam64) is false) return;

        var localAccount = LocalAccount.GetAccount(this);
        localAccount.Attach(this);
    }

    public long Steam32 { get; }
    public long Steam64 { get; }

    private static IDetectedAccount CreateIDetectedAccount(DirectoryInfo directory)
    {
        var account = new UserdataContent(directory);
        return account;
    }

    public static Task<List<IDetectedAccount>> GetIDetectedAccounts(SteamClient steamClient)
    {
        var accounts = new List<IDetectedAccount>();

        if (steamClient.UserdataDirectory == default) return Task.FromResult(accounts);
        if (steamClient.UserdataDirectory.Exists is false) return Task.FromResult(accounts);

        var directories = steamClient.UserdataDirectory.GetDirectories();
        accounts.AddRange(directories.Select(CreateIDetectedAccount));

        return Task.FromResult(accounts);
    }
}