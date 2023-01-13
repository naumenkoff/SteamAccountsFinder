using SteamAccountsFinder.Helpers;

namespace SteamAccountsFinder.Models.UserdataDirectory;

public class UserdataContent : ISteamID, IDetectedAccount
{
    public readonly FileSystemInfo ContainingDirectory;

    private UserdataContent(FileSystemInfo directory)
    {
        Steam32 = long.Parse(directory.Name);
        Steam64 = ISteamID.GetSteam64(Steam32);
        ContainingDirectory = directory;
    }

    public void Attach()
    {
        if (ISteamID.IsSteam64(Steam64) is false) return;

        var localAccount = LocalAccount.GetAccount(this);
        localAccount.Attach(this);
    }

    public long Steam32 { get; }
    public long Steam64 { get; }

    private static IDetectedAccount CreateIDetectedAccount(FileSystemInfo directory)
    {
        var account = new UserdataContent(directory);
        return account;
    }

    public static Task<IDetectedAccount[]> GetIDetectedAccounts(SteamClient steamClient)
    {
        if (LocationRecipient.DirectoryExists(steamClient.UserdataDirectory) is false)
            return Task.FromResult(Array.Empty<IDetectedAccount>());

        var directories = steamClient.UserdataDirectory.GetDirectories();
        var accounts = directories.Select(CreateIDetectedAccount).ToArray();
        return Task.FromResult(accounts);
    }
}