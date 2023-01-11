using SteamAccountsFinder.Helpers;

namespace SteamAccountsFinder.Models.Userdata;

public class Userdata : ISteamID, IDetectedAccount
{
    private Userdata(DirectoryInfo directory)
    {
        Steam32 = long.Parse(directory.Name);
        Steam64 = SteamDataConverter.GetSteam64(Steam32);
        ContainingDirectory = directory;
    }

    public DirectoryInfo ContainingDirectory { get; }
    public long Steam32 { get; }
    public long Steam64 { get; }

    public void Attach()
    {
        if (SteamDataConverter.IsSteam64(Steam64) is false) return;
        
        var localAccount = LocalAccount.GetAccount(this);
        localAccount.AddUserdata(this);
    }

    private static IDetectedAccount CreateUserdata(DirectoryInfo directory)
    {
        var userdata = new Userdata(directory);
        return userdata;
    }
    
    public static Task<List<IDetectedAccount>> GetIDetectedAccounts(SteamClient steamClient)
    {
        var matches = steamClient.UserdataDirectory.GetDirectories();
        var userdata = matches.Select(CreateUserdata).ToList();
        return Task.FromResult(userdata);
    }
}