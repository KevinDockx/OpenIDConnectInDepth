using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Marvin.IDP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddCors();

            var identityServerDBConnectionString =
                Configuration["ConnectionStrings:IdentityServerDBConnectionString"];

            var migrationsAssembly = typeof(Startup)
                .GetTypeInfo().Assembly.GetName().Name;

            // use in-mem resources, users, clients for the demo, but 
            // an operational SQL store for showing grants/revocation
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddTestUsers(Config.GetUsers())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(identityServerDBConnectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                })
                .AddExtensionGrantValidator<OnBehalfOfGrantValidator>();
            
            services.AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = "925417044554-bsjt1rtt0npu421equo2elurn6h9639i.apps.googleusercontent.com";
                    options.ClientSecret = "AEEo2KVJwwP-gECRgcvTgocj";
                    options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents()
                    {
                        OnCreatingTicket = context =>
                        {
                            var claims = new List<Claim>();
                            claims.AddRange(context.Principal.Claims);
                            claims.Add(new Claim("tenant", "belgianstatedepartment"));

                            // overwrite the old principal 
                            context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Google"));

                            return Task.CompletedTask;
                        },
                        OnTicketReceived = context =>
                        {
                            var test = context.Principal;
                            return Task.CompletedTask;
                        }
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // allow CORS requests (JavaScript) - any origin for demo purposes
            app.UseCors(c => c.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            app.UseIdentityServer();

            app.UseStaticFiles();

            app.UseMvcWithDefaultRoute();
        }
    }
}
