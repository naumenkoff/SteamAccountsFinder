using SteamAccountsFinder.Models;

namespace SteamAccountsFinder.Extensions;

public static class SteamIDExtension
{
    public static bool IsMatch(this ISteamID first, ISteamID second)
    {
        return first.Steam32 == second.Steam32 && first.Steam64 == second.Steam64;
    }
}