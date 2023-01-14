using SteamAccountsFinder;

await SteamClient.InitializeAsync();
await AccountsFinder.InitializeAccounts();
foreach (var account in LocalAccount.Accounts)
{
    Console.WriteLine(account.DetectionsCount + " " + account.Login);
    Console.WriteLine("From appmanifest: " + account.DataFromAppmanifest.Count);
    Console.WriteLine("From appworkshop: " + account.DataFromAppWorkshop.Count);
    Console.WriteLine("From userdata: " + account.DataFromUserdata.Steam64);
    Console.WriteLine("From config: " + account.DataFromConfig?.Login);
    Console.WriteLine("From loginusers: " + account.DataFromLoginusersContent?.Timestamp);
    Console.WriteLine("From registry: " + account.DataFromRegistry?.RegistryPath);
}