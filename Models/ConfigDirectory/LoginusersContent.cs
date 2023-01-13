using System.Text.RegularExpressions;
using SteamAccountsFinder.Helpers;

namespace SteamAccountsFinder.Models.ConfigDirectory;

public partial class LoginusersContent : ISteamID, IDetectedAccount
{
    private LoginusersContent(Match match)
    {
        Steam64 = long.Parse(match.Groups["id"].Value);
        Steam32 = ISteamID.GetSteam32(Steam64);
        Login = match.Groups["AccountName"].Value;
        Name = match.Groups["PersonaName"].Value;
        var time = long.Parse(match.Groups["Timestamp"].Value);
        Timestamp = DateTimeOffset.FromUnixTimeSeconds(time).ToLocalTime();
    }

    public string Login { get; }
    public string Name { get; }
    public DateTimeOffset Timestamp { get; }

    public void Attach()
    {
        if (ISteamID.IsSteam64(Steam64) is false) return;

        var localAccount = LocalAccount.GetAccount(this);
        localAccount.Attach(this);
    }

    public long Steam32 { get; }
    public long Steam64 { get; }

    private static IDetectedAccount CreateIDetectedAccount(Match content)
    {
        var match = Pattern().Match(content.Value);
        if (match.Success is false) return default;

        var account = new LoginusersContent(match);
        return account;
    }

    public static Task<List<IDetectedAccount>> GetIDetectedAccounts(SteamClient steamClient)
    {
        var accounts = new List<IDetectedAccount>();

        if (steamClient.LoginusersFile == default) return Task.FromResult(accounts);
        if (steamClient.LoginusersFile.Exists is false) return Task.FromResult(accounts);
        if (LocationRecipient.TryReadFileContent(out var content, steamClient.LoginusersFile.FullName) is false)
            return Task.FromResult(accounts);

        var matches = Regex.Matches(content, ".\"765.+?\".+?{.+?}", RegexOptions.Singleline).Cast<Match>();
        accounts.AddRange(matches.Select(CreateIDetectedAccount));

        return Task.FromResult(accounts);
    }

    [GeneratedRegex(
        "\"(?<id>765.+?)\".+?{.+?\"AccountName\".+?\"(?<AccountName>.+?)\".+?\"PersonaName\".+?\"(?<PersonaName>.+?)\".+?\"Timestamp\".+?\"(?<Timestamp>.+?)\".+?}",
        RegexOptions.Singleline)]
    private static partial Regex Pattern();
}