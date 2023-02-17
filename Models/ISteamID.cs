namespace SteamAccountsFinder.Models;

public interface ISteamID
{
    long Steam32 { get; }
    long Steam64 { get; }
}