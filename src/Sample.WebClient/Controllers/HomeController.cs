using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using Sample.WebClient.Services;
using Sample.WebClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sample.WebClient.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ISampleHttpClient _sampleHttpClient;

        public HomeController(ISampleHttpClient sampleHttpClient)
        {
            _sampleHttpClient = sampleHttpClient;
        }

        // GET: Home
        public async Task<IActionResult> Index()
        {
            await WriteOutIdentityInformation();
            return View(new CallApiViewModel());
        }
        
        public async Task<IActionResult> CallApi()
        {
            // call the API
            var httpClient = await _sampleHttpClient.GetClient();

            var response = await httpClient.GetAsync("api/claims").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var claimsFromApiAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var callApiViewModel = new CallApiViewModel(
                    JsonConvert.DeserializeObject<IList<string>>(claimsFromApiAsString));

                return View("Index", callApiViewModel);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToAction("AccessDenied", "Authorization");
            }

            return View("Index", new CallApiViewModel());
        }

        public async Task Logout()
        {
            await RevokeTokens();
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");
        }

        [AllowAnonymous]
        public async Task<IActionResult> IDPTriggeredLogout()
        {
            if (User.Identity.IsAuthenticated)
            {
                await RevokeTokens();
                await HttpContext.SignOutAsync("Cookies");
                await HttpContext.SignOutAsync("oidc");             
            }

            return NoContent();
        }
        
        public async Task RevokeTokens()
        {  
            // get the metadata
            var discoveryClient = new DiscoveryClient("https://localhost:44379/");
            var metaDataResponse = await discoveryClient.GetAsync();

            // create a TokenRevocationClient
            var revocationClient = new TokenRevocationClient(
                metaDataResponse.RevocationEndpoint,
                "samplewebclient",
                "secret");

            // get the access token to revoke 
            var accessToken = await HttpContext
                .GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var revokeAccessTokenResponse =
                    await revocationClient.RevokeAccessTokenAsync(accessToken);

                if (revokeAccessTokenResponse.IsError)
                {
                    throw new Exception("Problem encountered while revoking the access token."
                        , revokeAccessTokenResponse.Exception);
                }
            }

            // revoke the refresh token as well
            var refreshToken = await HttpContext
                .GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var revokeRefreshTokenResponse =
                    await revocationClient.RevokeRefreshTokenAsync(refreshToken);

                if (revokeRefreshTokenResponse.IsError)
                {
                    throw new Exception("Problem encountered while revoking the refresh token."
                        , revokeRefreshTokenResponse.Exception);
                }
            }
        }       

        public async Task WriteOutIdentityInformation()
        {
            // get the saved identity token
            var identityToken = await HttpContext
                .GetTokenAsync(OpenIdConnectParameterNames.IdToken);

            // write it out
            Debug.WriteLine($"Identity token: {identityToken}");

            // write out the user claims
            foreach (var claim in User.Claims)
            {
                Debug.WriteLine($"Claim type: {claim.Type} - Claim value: {claim.Value}");
            }
        }

    }
}