
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using IdentityModel.OidcClient;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using signalrclient;
using signalrclient.Backend;
using signalrclient.Requests;

const string tokenfilepath = @"c:\temp\token.txt";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var oidcClientOptions = new OidcClientOptions
{
    Authority = $"https://test.id.trumf.no",
    ClientId = "trumf.app",
    Scope = "openid profile http://id.trumf.no/scopes/medlem api.tm.bff offline_access",
    RedirectUri = "trumfapp://callback",
    PostLogoutRedirectUri = "trumfapp://callback"
};

var oidcClient = new OidcClient(oidcClientOptions);

string refreshToken = "";
if (!File.Exists(tokenfilepath))
{
    Console.WriteLine("Enter refreshtoken:");
    refreshToken = Console.ReadLine();
    File.WriteAllText(tokenfilepath, refreshToken);
}
refreshToken = File.ReadAllText(tokenfilepath);
var result = await oidcClient.RefreshTokenAsync(refreshToken);
if (result.IsError)
{
    Console.WriteLine("auth failed, check refresh token");
    File.Delete(tokenfilepath);
    return;
}
var accessToken = result.AccessToken;
File.WriteAllText(tokenfilepath, result.RefreshToken);

Log.Information("Got access token");
Log.Information(accessToken);

var sp = Configuration.BuildServices();
var backendService = sp.GetRequiredService<IBackendService>();

bool connected = false;


do
{
    try
    {
        await backendService.Connect();
        connected = backendService.ConnectionStatus == BackendConnectionStatus.Connected;
    }
    catch (Exception e)
    {
        await Task.Delay(1000);
        Console.WriteLine("Retrying connect");
    }
} while (connected == false);

Console.WriteLine("Connected to signalr, enter command:");

var input = Console.ReadLine();

while (input != "q")
{
    var parts = input.Split(" ");
    var command = parts[0];  

    if (command == "acrm")
    {
        var response = await backendService.Send(new GetCrmBankAccountsRequest(), accessToken);
        var responseString = JsonSerializer.Serialize(response);
        Log.Information("GetBankAccountsResponse {Success} {content}", response.Success, responseString);
    }
    if (command == "a")
    {
        var response = await backendService.Send(new GetBankAccountRequest("34113473047"), accessToken);
        var responseString = JsonSerializer.Serialize(response);
        Log.Information("GetBankAccountsResponse {Success} {content}", response.Success, responseString);
    }
    if (command == "d")
    {
        var response = await backendService.Send(new DeclinePaymentRequest("123"), accessToken);
        Log.Information("DeclinePaymentRequest {Success}", response.Success);
    }    
    if (command == "qr")
    {
        var response = await backendService.Send(new GenerateQrCodeRequest("id"), accessToken);
        Log.Information("GenerateQrCodeRequest {Success}", response.Success);
    }
    if (command == "r")
    {
        var response = await backendService.Send(new RegisterPushTokenRequest("push", true, "installid"), accessToken);
        Log.Information("RegisterPushTokenRequest {Success}", response.Success);
    }
    if (command == "c")
    {
        var response = await backendService.Send(new GetNotificationConsentsRequest(), accessToken);
        Log.Information("Consentresponse {Success}", response.Success);
        if (response.Success)
        {
            Log.Information("trumfpay {trumfpay}, loyalty {loyalty}",response.Response.Consents.TrumfPay, response.Response.Consents.Loyalty);
        }
    }
    if (command == "u")
    {
        var req = new UpdateNotificationConsentsRequest(new NotificationConsents(true, false));
        var response = await backendService.Send(req, accessToken);
        var r = response.Response;
        Log.Information("UpdateNotificationConsentsRequest {Success}", response.Success);
    }
    if (command == "b")
    {
        var req = new GetActiveBonussjekkRequest();
        var response = await backendService.Send(req, accessToken);
        var r = response.Response;

        Log.Information("GetActiveBonussjekkResponse {Success}", response.Success);
        Log.Information("GetActiveBonussjekkResponse {Body}", JsonSerializer.Serialize(r));
    }
    if (command == "ab")
    {
        var req = new AddBonussjekkRequest
        {
            Amount = decimal.Parse(parts[1]),
            ForbrukerkjedeNr = "1100"
        };

        var response = await backendService.Send(req, accessToken);
        var r = response.Response;

        Log.Information("AddBonussjekkResponse {Success}", response.Success);
        Log.Information("AddBonussjekkResponse {Body}", JsonSerializer.Serialize(r));
    }
    if (command == "db")
    {
        var req = new DeleteBonussjekkRequest
        {
            BonusSjekkId = parts[1]
        };

        var response = await backendService.Send(req, accessToken);
        var r = response.Response;

        Log.Information("DeleteBonussjekkResponse {Success}", response.Success);
        Log.Information("DeleteBonussjekkResponse {Body}", JsonSerializer.Serialize(r));
    }

    if (command == "k")
    {

        var response = await backendService.Send(new GetBonusKjederRequest(), accessToken);
        var r = response.Response;

        Log.Information("GetBonusKjederResponse {Success}", response.Success);
        Log.Information("GetBonusKjederResponse {Body}", JsonSerializer.Serialize(r));
    }

    input = Console.ReadLine();
}