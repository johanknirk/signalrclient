
using IdentityModel.OidcClient;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using signalrclient;
using signalrclient.Backend;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var sp = Configuration.BuildServices();

var backendService = sp.GetRequiredService<IBackendService>();
await backendService.Connect();

var oidcClientOptions = new OidcClientOptions
{
    Authority = $"https://test.id.trumf.no",
    ClientId = "trumf.app",
    Scope = "openid profile http://id.trumf.no/scopes/medlem api.tm.bff offline_access",
    RedirectUri = "trumfapp://callback",
    PostLogoutRedirectUri = "trumfapp://callback"
};
//var refreshtoken = "7080F75DAEF558642091F88B4F06F5EB7141B22A0522CF51D599E14783272567-1";
//var oidcClient = new OidcClient(oidcClientOptions);
//var accessToken = await oidcClient.RefreshTokenAsync(refreshtoken);

var accessToken = File.ReadAllText(@"c:\temp\token.txt");

Console.WriteLine("Connected, enter command:");

var command = Console.ReadLine();

while (command != "q")
{
    if (command == "r")
    {
        var response = await backendService.Send(new RegisterPushTokenRequest("push", true, "installid"), accessToken);
        Log.Information("RegisterPushTokenRequest {Success}", response.Success);
    }
    command = Console.ReadLine();
}