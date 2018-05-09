using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Sample.API.Controllers
{
    [Authorize]
    [Route("api/claims")]
    public class ClaimsController : Controller
    {
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
            var discoveryClient = new DiscoveryClient("https://localhost:44379/");
            var metaDataResponse = await discoveryClient.GetAsync();

            // create a TokenClient
            var tokenClient = new TokenClient(
                metaDataResponse.TokenEndpoint,
                "sampleonbehalfofclient",
                "secret");

            // get current access token
            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var additionalParameters = new Dictionary<string, string>();
            additionalParameters.Add("assertion", accessToken);
            additionalParameters.Add("requested_token_use", "on_behalf_of");

            var tokenResult = await tokenClient.RequestCustomGrantAsync("urn:ietf:params:oauth:grant-type:jwt-bearer",
                "samplesecondapi", additionalParameters);

            if (tokenResult.IsError)
            {
                // can't call second API
                return claimsList;
            }

            // call the second API
            var httpClient = new HttpClient();

            // set as Bearer token
            httpClient.SetBearerToken(tokenResult.AccessToken);
            httpClient.BaseAddress = new Uri("https://localhost:44312/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var responseFromSecondApi = await httpClient.GetAsync("api/claims").ConfigureAwait(false);

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
