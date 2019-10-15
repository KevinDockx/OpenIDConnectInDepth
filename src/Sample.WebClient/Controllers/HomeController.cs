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
using System.Net.Http;
using System.Threading.Tasks;

namespace Sample.WebClient.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ??
                throw new ArgumentNullException(nameof(httpClientFactory));
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
            var httpClient = _httpClientFactory.CreateClient("APIClient");

            var response = await httpClient.GetAsync("api/claims");

            if (response.IsSuccessStatusCode)
            {
                var claimsFromApiAsString = await response.Content.ReadAsStringAsync();

                var callApiViewModel = new CallApiViewModel(
                    JsonConvert.DeserializeObject<IList<string>>(claimsFromApiAsString));

                return View("Index", callApiViewModel);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToAction("AccessDenied", "Authorization");
            }

            await SetViewBag();
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
            var idpClient = _httpClientFactory.CreateClient("IDPClient");
            var discoveryDocumentResponse = await idpClient.GetDiscoveryDocumentAsync();
            if (discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            // get the access token to revoke 
            var accessToken = await HttpContext
                .GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                // revoke the token
                var revokeAccessTokenResponse = await idpClient.RevokeTokenAsync(
                    new TokenRevocationRequest
                    {
                        ClientId = "samplewebclient",
                        ClientSecret = "secret",
                        Token = accessToken
                    });

                if (revokeAccessTokenResponse.IsError)
                {
                    throw new Exception("Problem encountered while revoking the access token.",
                        revokeAccessTokenResponse.Exception);
                }
            }

            // revoke the refresh token as well
            var refreshToken = await HttpContext
                .GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var revokeRefreshTokenResponse = await idpClient.RevokeTokenAsync(
                    new TokenRevocationRequest
                    {
                        ClientId = "samplewebclient",
                        ClientSecret = "secret",
                        Token = refreshToken
                    });

                if (revokeRefreshTokenResponse.IsError)
                {
                    throw new Exception("Problem encountered while revoking the refresh token.",
                        revokeRefreshTokenResponse.Exception);
                }
            }
        }
        private async Task SetViewBag()
        {
            ViewBag.IdentityToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);
            ViewBag.AccessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
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