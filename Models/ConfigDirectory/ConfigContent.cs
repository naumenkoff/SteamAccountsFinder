using System.Text.RegularExpressions;
using SteamAccountsFinder.Helpers;

namespace SteamAccountsFinder.Models.Config;

public partial class InstallConfigStore : ISteamID, IDetectedAccount
{
    private InstallConfigStore(Match match)
    {
        Steam64 = long.Parse(match.Groups["id"].Value);
        Steam32 = SteamDataConverter.GetSteam32(Steam64);
        Login = match.Groups["login"].Value;
    }

    public string Login { get; }
    public long Steam32 { get; }
    public long Steam64 { get; }

    public void Attach()
    {
        if (SteamDataConverter.IsSteam64(Steam64) is false) return;
        
        var localAccount = LocalAccount.GetAccount(this);
        localAccount.AddInstallConfigStore(this);
    }

    public static IDetectedAccount CreateInstallConfigStore(Match content)
    {
        var match = Pattern().Match(content.Value);
        if (match.Success is false) return default;

        var installConfigStore = new InstallConfigStore(match);
        return installConfigStore;
    }

    public static Task<List<IDetectedAccount>> GetIDetectedAccounts(SteamClient steamClient)
    {
        var accounts = new List<IDetectedAccount>();
        if (LocationRecipient.TryReadFileContent(out var content, steamClient.ConfigFile.FullName) is false)
            return Task.FromResult(accounts);
        var accountsSection = Regex.Match(content, "(?<=\"Accounts\".+?{).+765.+?}", RegexOptions.Singleline).Value;
        var matches = Regex.Matches(accountsSection, "\".+?\".+?{.+?}", RegexOptions.Singleline).Cast<Match>();
        accounts.AddRange(matches.Select(CreateInstallConfigStore));
        return Task.FromResult(accounts);
    }

    [GeneratedRegex("\"(?<login>.+?)\".+?{.+?\"(?<id>7.+?)\".+?}", RegexOptions.Singleline)]
    private static partial Regex Pattern();
}