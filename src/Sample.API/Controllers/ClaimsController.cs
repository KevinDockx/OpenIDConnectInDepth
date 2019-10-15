using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Sample.API.Controllers
{
    [Authorize]
    [Route("api/claims")]
    public class ClaimsController : ControllerBase
    {

        private readonly IHttpClientFactory _httpClientFactory;

        public ClaimsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? 
                throw new ArgumentNullException(nameof(httpClientFactory));
        }

        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            var claimsList = new List<string>();

            // return the user claims
            foreach (var claim in User.Claims)
            {
                claimsList.Add($"{claim.Type} : {claim.Value}");
            }

            // call the second API - requires an access token with the
            // "samplesecondapi" scope

            // get the token via the token exchange flow
            var idpClient = _httpClientFactory.CreateClient("IDPClient");
            var discoveryDocumentResponse = await idpClient.GetDiscoveryDocumentAsync();
            if (discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            var customParams = new Dictionary<string, string>
            {
                { "subject_token_type",  "urn:ietf:params:oauth:token-type:access_token"},
                { "subject_token", await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken) },
                { "scope", "profile samplesecondapi" }
            };

            var tokenResponse = await idpClient.RequestTokenAsync(new TokenRequest()
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                GrantType = "urn:ietf:params:oauth:grant-type:token-exchange",
                Parameters = customParams,
                ClientId = "sampletokenexchangeclient",
                ClientSecret = "secret"
            });

            if (tokenResponse.IsError)
            {
                // can't call second API
                return claimsList;
            }

            // call second API on behalf of the currently identified user
            // & add the claims returned via that API
            var multiHopClient = _httpClientFactory.CreateClient("MultiHopClient");
            multiHopClient.SetBearerToken(tokenResponse.AccessToken);

            var responseFromSecondApi = await multiHopClient.GetAsync("api/claims");

            if (responseFromSecondApi.IsSuccessStatusCode)
            {
                var claimsFromSecondApiAsString =
                    await responseFromSecondApi.Content.ReadAsStringAsync().ConfigureAwait(false);

                claimsList.Add("------------------------------------------");
                claimsList.Add("Claims from user at second API (multi-hop)");
                claimsList.Add("------------------------------------------");

                claimsList.AddRange(
                    JsonConvert.DeserializeObject<IList<string>>(claimsFromSecondApiAsString));
            }

            return claimsList;        
        }
    }
}
