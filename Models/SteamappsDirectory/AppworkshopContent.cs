﻿using System.Text.RegularExpressions;
using SteamAccountsFinder.Helpers;

namespace SteamAccountsFinder.Models.SteamappsDirectory;

public partial class AppWorkshop : ISteamID, IDetectedAccount
{
    private AppWorkshop(FileInfo appworkshop, Match match)
    {
        ContainingFile = appworkshop;
        AppID = int.Parse(match.Groups["name"].Value);
        Steam32 = long.Parse(match.Groups["ownerid"].Value);
        Steam64 = ISteamID.GetSteam64(Steam32);
        HasSubscriber = true;
    }

    public bool HasSubscriber { get; }
    public FileInfo ContainingFile { get; }
    public int AppID { get; }

    public void Attach()
    {
        if (ISteamID.IsSteam64(Steam64) is false) return;

        var localAccount = LocalAccount.GetAccount(this);
        localAccount.AddAppWorkshop(this);
    }

    public long Steam32 { get; }
    public long Steam64 { get; }

    private static IDetectedAccount CreateIDetectedAccount(FileInfo appworkshop)
    {
        if (LocationRecipient.TryReadFileContent(out var content, appworkshop.FullName) is false) return default;

        var match = Pattern().Match(content);
        if (match.Success is false) return default;

        var account = new AppWorkshop(appworkshop, match);
        return account;
    }

    public static Task<List<IDetectedAccount>> GetIDetectedAccounts(DirectoryInfo workshop)
    {
        var accounts = new List<IDetectedAccount>();
        if (workshop == default) return Task.FromResult(accounts);
        
        var files = workshop.GetFiles();
        accounts.AddRange(files.Select(CreateIDetectedAccount));
        
        return Task.FromResult(accounts);
    }

    [GeneratedRegex("\"appid\".+?\"(?<name>.+?)\".+?\"subscribedby\".+?\"(?<ownerid>.+?)\"", RegexOptions.Singleline)]
    private static partial Regex Pattern();
}