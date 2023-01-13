using System.Text.RegularExpressions;
using SteamAccountsFinder.Helpers;

namespace SteamAccountsFinder.Models.SteamappsDirectory;

public partial class AppmanifestContent : ISteamID, IDetectedAccount
{
    private AppmanifestContent(FileInfo appmanifest, Match match)
    {
        ContainingFile = appmanifest;
        Name = match.Groups["name"].Value;
        Steam64 = long.Parse(match.Groups["ownerid"].Value);
        Steam32 = ISteamID.GetSteam32(Steam64);
        HasLastOwner = true;
    }

    public FileInfo ContainingFile { get; }
    public string Name { get; }
    public bool HasLastOwner { get; }

    public void Attach()
    {
        if (ISteamID.IsSteam64(Steam64) is false) return;

        var localAccount = LocalAccount.GetAccount(this);
        localAccount.Attach(this);
    }

    public long Steam32 { get; }
    public long Steam64 { get; }

    private static IDetectedAccount CreateIDetectedAccount(FileInfo appmanifest)
    {
        if (LocationRecipient.TryReadFileContent(out var content, appmanifest.FullName) is false) return default;

        var match = Pattern().Match(content);
        if (match.Success is false) return default;

        var account = new AppmanifestContent(appmanifest, match);
        return account;
    }

    public static Task<List<IDetectedAccount>> GetIDetectedAccounts(DirectoryInfo steamApps)
    {
        var accounts = new List<IDetectedAccount>();
        
        if (steamApps == default) return Task.FromResult(accounts);
        if (steamApps.Exists is false) return Task.FromResult(accounts);

        var files = steamApps.GetFiles();
        accounts.AddRange(files.Select(CreateIDetectedAccount));

        return Task.FromResult(accounts);
    }

    [GeneratedRegex("\"name\".+?\"(?<name>.+?)\".+?\"LastOwner\".+?\"(?<ownerid>.+?)\"", RegexOptions.Singleline)]
    private static partial Regex Pattern();
}