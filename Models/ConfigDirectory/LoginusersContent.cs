using System.Text.RegularExpressions;
using SteamAccountsFinder.Helpers;

namespace SteamAccountsFinder.Models.Config;

public partial class Loginusers : ISteamID, IDetectedAccount
{
    private Loginusers(Match match)
    {
        Steam64 = long.Parse(match.Groups["id"].Value);
        Steam32 = SteamDataConverter.GetSteam32(Steam64);
        Login = match.Groups["AccountName"].Value;
        Name = match.Groups["PersonaName"].Value;
        var time = long.Parse(match.Groups["Timestamp"].Value);
        Timestamp = DateTimeOffset.FromUnixTimeSeconds(time).ToLocalTime();
    }

    public string Login { get; }
    public string Name { get; }
    public DateTimeOffset Timestamp { get; }
    public long Steam32 { get; }
    public long Steam64 { get; }

    public void Attach()
    {
        if (SteamDataConverter.IsSteam64(Steam64) is false) return;
        
        var localAccount = LocalAccount.GetAccount(this);
        localAccount.AddLoginusers(this);
    }

    public static IDetectedAccount CreateLoginusers(Match content)
    {
        var match = Pattern().Match(content.Value);
        if (match.Success is false) return default;

        var loginusers = new Loginusers(match);
        return loginusers;
    }

    public static Task<List<IDetectedAccount>> GetIDetectedAccounts(SteamClient steamClient)
    {
        var accounts = new List<IDetectedAccount>();
        
        if (LocationRecipient.TryReadFileContent(out var content, steamClient.LoginusersFile.FullName) is false)
            return Task.FromResult(accounts);

        var matches = Regex.Matches(content, ".\"765.+?\".+?{.+?}", RegexOptions.Singleline).Cast<Match>();
        accounts.AddRange(matches.Select(CreateLoginusers));
        return Task.FromResult(accounts);
    }

    [GeneratedRegex(
        "\"(?<id>765.+?)\".+?{.+?\"AccountName\".+?\"(?<AccountName>.+?)\".+?\"PersonaName\".+?\"(?<PersonaName>.+?)\".+?\"Timestamp\".+?\"(?<Timestamp>.+?)\".+?}",
        RegexOptions.Singleline)]
    private static partial Regex Pattern();
}