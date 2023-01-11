using SteamAccountsFinder;

var steam = new SteamClient();
var accountsFinder = new AccountsFinder(steam);
await accountsFinder.InitializeAccounts();
foreach (var account in LocalAccount.Accounts)
{
    Console.WriteLine(account.NumberOfDublicates + " " + account.Login);
    Console.WriteLine("From appmanifest: " + account.DataFromAppmanifest.Count);
    Console.WriteLine("From appworkshop: " + account.DataFromAppWorkshop.Count);
    Console.WriteLine("From userdata: " + account.DataFromUserdata.Count);
    Console.WriteLine("From config: " + account.DataFromConfig?.Login);
    Console.WriteLine("From loginusers: " + account.DataFromLoginusers?.Timestamp);
    Console.WriteLine("From registry: " + account.DataFromRegistry?.RegistryPath);
}