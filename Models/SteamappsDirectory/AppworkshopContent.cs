using System.Text.RegularExpressions;
using SteamAccountsFinder.Helpers;

namespace SteamAccountsFinder.Models.SteamappsDirectory;

public partial class AppworkshopContent : ISteamID, IDetectedAccount
{
    public readonly int AppID;
    public readonly FileSystemInfo ContainingFile;

    public readonly bool HasSubscriber;

    private AppworkshopContent(FileSystemInfo appworkshop, Match match)
    {
        ContainingFile = appworkshop;
        AppID = int.Parse(match.Groups["name"].Value);
        Steam32 = long.Parse(match.Groups["ownerid"].Value);
        Steam64 = ISteamID.GetSteam64(Steam32);
        HasSubscriber = true;
    }

    public void Attach()
    {
        if (ISteamID.IsSteam64(Steam64) is false) return;

        var localAccount = LocalAccount.GetAccount(this);
        localAccount.Attach(this);
    }

    public long Steam32 { get; }
    public long Steam64 { get; }

    private static IDetectedAccount CreateIDetectedAccount(FileSystemInfo appworkshop)
    {
        if (LocationRecipient.TryReadFileContent(out var content, appworkshop) is false) return default;

        var match = Pattern().Match(content);
        if (match.Success is false) return default;

        var account = new AppworkshopContent(appworkshop, match);
        return account;
    }

    public static Task<IDetectedAccount[]> GetIDetectedAccounts(DirectoryInfo workshop)
    {
        if (LocationRecipient.DirectoryExists(workshop) is false)
            return Task.FromResult(Array.Empty<IDetectedAccount>());

        var files = workshop.GetFiles();
        var accounts = files.Select(CreateIDetectedAccount).ToArray();
        return Task.FromResult(accounts);
    }

    [GeneratedRegex("\"appid\".+?\"(?<name>.+?)\".+?\"subscribedby\".+?\"(?<ownerid>.+?)\"", RegexOptions.Singleline)]
    private static partial Regex Pattern();
}