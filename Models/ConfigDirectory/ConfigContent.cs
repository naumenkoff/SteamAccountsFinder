using System.Text.RegularExpressions;
using SteamAccountsFinder.Helpers;

namespace SteamAccountsFinder.Models.ConfigDirectory;

public partial class InstallConfigStore : ISteamID, IDetectedAccount
{
    private InstallConfigStore(Match match)
    {
        Steam64 = long.Parse(match.Groups["id"].Value);
        Steam32 = ISteamID.GetSteam32(Steam64);
        Login = match.Groups["login"].Value;
    }

    public string Login { get; }

    public void Attach()
    {
        if (ISteamID.IsSteam64(Steam64) is false) return;

        var localAccount = LocalAccount.GetAccount(this);
        localAccount.AddInstallConfigStore(this);
    }

    public long Steam32 { get; }
    public long Steam64 { get; }

    private static IDetectedAccount CreateIDetectedAccount(Match content)
    {
        var match = Pattern().Match(content.Value);
        if (match.Success is false) return default;

        var account = new InstallConfigStore(match);
        return account;
    }

    public static Task<List<IDetectedAccount>> GetIDetectedAccounts(SteamClient steamClient)
    {
        var accounts = new List<IDetectedAccount>();
        
        if (steamClient.ConfigFile == default) return Task.FromResult(accounts);
        
        if (LocationRecipient.TryReadFileContent(out var content, steamClient.ConfigFile.FullName) is false)
            return Task.FromResult(accounts);
        
        var accountsSection = Regex.Match(content, "(?<=\"Accounts\".+?{).+765.+?}", RegexOptions.Singleline).Value;
        var matches = Regex.Matches(accountsSection, "\".+?\".+?{.+?}", RegexOptions.Singleline).Cast<Match>();
        accounts.AddRange(matches.Select(CreateIDetectedAccount));
        
        return Task.FromResult(accounts);
    }

    [GeneratedRegex("\"(?<login>.+?)\".+?{.+?\"(?<id>7.+?)\".+?}", RegexOptions.Singleline)]
    private static partial Regex Pattern();
}