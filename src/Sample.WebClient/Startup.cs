using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sample.WebClient.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Sample.WebClient
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register an ISampleHttpClient
            services.AddScoped<ISampleHttpClient, SampleHttpClient>();

            // register the TenantSelector scoped (so it's created once per request)
            services.AddScoped<ITenantSelector<Tenant>, TenantSelector>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            }).AddCookie("Cookies",
                (options) =>
                {
                    options.AccessDeniedPath = "/Authorization/AccessDenied";
                }
                    )
              .AddOpenIdConnect("oidc", options =>
              {
                  options.SignInScheme = "Cookies";

                  options.Authority = "https://localhost:44379";
                  options.ClientId = "samplewebclient";
                  options.ResponseType = "code id_token";

                  options.Scope.Add("openid");
                  options.Scope.Add("profile");
                  options.Scope.Add("sampleapi");

                  options.GetClaimsFromUserInfoEndpoint = true;
                  options.SaveTokens = true;
                  options.ClientSecret = "secret";

                  options.ClaimActions.Remove("amr");
                  options.ClaimActions.DeleteClaim("sid");
                  options.ClaimActions.DeleteClaim("idp");

                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      NameClaimType = JwtClaimTypes.GivenName,
                      RoleClaimType = JwtClaimTypes.Role,
                  };

                  options.Events = new OpenIdConnectEvents
                  {
                      OnRedirectToIdentityProvider = context =>
                      {
                          // pass tenant on all authentication request to 
                          // diversify at UI level (eg: other IDPs, user stores, ...)   

                          if (context.ProtocolMessage.RequestType ==
                            Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectRequestType.Authentication)
                          {
                              //tenant:name_of_tenant 
                              var tenantSelector = context.HttpContext
                                .RequestServices.GetService<ITenantSelector<Tenant>>();

                              var currentTenant = tenantSelector.Select();

                              if (currentTenant != null)
                              {
                                  context.ProtocolMessage.AcrValues = $"tenant:{currentTenant.Identifier}";
                              }
                          }
                          return Task.CompletedTask;
                      }
                  };
              });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
