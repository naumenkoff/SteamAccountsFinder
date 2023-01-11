﻿using SteamAccountsFinder.Helpers;
using Microsoft.Win32;

namespace SteamAccountsFinder.Models.Registry;

public class RegistryUsers : ISteamID, IDetectedAccount
{
    private RegistryUsers(string registryPath, string id)
    {
        Steam32 = long.Parse(id);
        Steam64 = SteamDataConverter.GetSteam64(Steam32);
        RegistryPath = registryPath;
    }

    public string RegistryPath { get; }
    public long Steam32 { get; }
    public long Steam64 { get; }

    public void Attach()
    {
        if (SteamDataConverter.IsSteam64(Steam64) is false) return;
        
        var localAccount = LocalAccount.GetAccount(this);
        localAccount.AddRegistryUsers(this);
    }

    public static IDetectedAccount CreateRegistryUsers(string registryPath, string id)
    {
        var registryUsers = new RegistryUsers(registryPath, id);
        return registryUsers;
    }

    public static Task<List<IDetectedAccount>> GetIDetectedAccounts()
    {
        var accounts = new List<IDetectedAccount>();
        using var registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Valve")?.OpenSubKey("Steam")?.OpenSubKey("Users");
        if (registryKey is null) return Task.FromResult(accounts);
        var users = registryKey.GetSubKeyNames();
        accounts.AddRange(from user in users let path = Path.Combine(registryKey.Name, user) select CreateRegistryUsers(path, user));
        return Task.FromResult(accounts);
    }
}