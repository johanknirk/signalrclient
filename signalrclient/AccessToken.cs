using IdentityModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace signalrclient
{
    public class AccessTokenClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public static string TrumfIdUrl = "https://test.id.trumf.no/";

        public static string TrumfIdClientScope = "api.sylinder api.sylinder.trumfpayment.payments.merchant api.tm.bff.pos";
        private readonly HttpClient _trumfIdClient;

        public AccessTokenClient(IHttpClientFactory httpClientFactory)
        {
            _trumfIdClient = httpClientFactory.CreateClient(nameof(AccessTokenClient));
        }

        private async Task<TokenResponse> GetAccessToken(string clientid, string clientsecret)
        {
            var tokenRequest = new TokenRequest
            {
                Address = "connect/token",

                GrantType = OidcConstants.GrantTypes.ClientCredentials,
                ClientId = clientid,
                ClientSecret = clientsecret
            };

            tokenRequest.Parameters.Add("scope", TrumfIdClientScope);

            var tokenResponse = await _trumfIdClient.RequestTokenAsync(tokenRequest);

            return tokenResponse;
        }

        public async Task<TokenResponse> GetToken(string clientId, string clientSecret)
        {
            var token = await GetAccessToken(clientId, clientSecret);

            if (token.IsError)
            {
                Log.Error("Trumf ID error: " + token.Error + ", " + token.ErrorDescription);
            }

            token.HttpResponse.EnsureSuccessStatusCode();
            return token;
        }
    }
}
