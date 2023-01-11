namespace SteamAccountsFinder.Models;

public interface ISteamID
{
    public long Steam32 { get; }
    public long Steam64 { get; }

    private const long MinimumSteam64Value = 76561197960265728;

    public static long GetSteam64(long steam32)
    {
        if (IsSteam64(steam32)) return steam32;

        return MinimumSteam64Value + steam32;
    }

    public static long GetSteam32(long steam64)
    {
        if (IsSteam64(steam64) is false) return steam64;

        return steam64 - MinimumSteam64Value;
    }

    public static bool IsSteam64(long id)
    {
        return id > MinimumSteam64Value;
    }
}