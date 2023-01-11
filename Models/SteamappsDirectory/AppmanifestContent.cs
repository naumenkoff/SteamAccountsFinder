using System.Text.RegularExpressions;
using SteamAccountsFinder.Helpers;

namespace SteamAccountsFinder.Models.Steamapps;

public partial class AppState : ISteamID, IDetectedAccount
{
    private AppState(FileInfo appmanifest, Match match)
    {
        ContainingFile = appmanifest;
        Name = match.Groups["name"].Value;
        Steam64 = long.Parse(match.Groups["ownerid"].Value);
        Steam32 = SteamDataConverter.GetSteam32(Steam64);
        if (SteamDataConverter.IsSteam64(Steam64)) HasLastOwner = true;
    }

    public FileInfo ContainingFile { get; }
    public string Name { get; }
    public bool HasLastOwner { get; }
    public long Steam32 { get; }
    public long Steam64 { get; }

    public void Attach()
    {
        if (SteamDataConverter.IsSteam64(Steam64) is false) return;
        
        var localAccount = LocalAccount.GetAccount(this);
        localAccount.AddAppState(this);
    }

    private static IDetectedAccount CreateAppState(FileInfo appmanifest)
    {
        if (LocationRecipient.TryReadFileContent(out var content, appmanifest.FullName) is false) return default;

        var match = Pattern().Match(content);
        if (match.Success is false) return default;

        var appState = new AppState(appmanifest, match);
        return appState;
    }
    
    public static Task<List<IDetectedAccount>> GetIDetectedAccounts(DirectoryInfo steamApps)
    {
        var files = steamApps.GetFiles();
        var appStates = files.Select(CreateAppState).ToList();
        return Task.FromResult(appStates);
    }
    
    [GeneratedRegex("\"name\".+?\"(?<name>.+?)\".+?\"LastOwner\".+?\"(?<ownerid>.+?)\"", RegexOptions.Singleline)]
    private static partial Regex Pattern();
}