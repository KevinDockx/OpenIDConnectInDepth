using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Sample.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddCors();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
             .AddIdentityServerAuthentication(options =>
             {
                 options.Authority = "https://localhost:44379";
                 options.ApiName = "sampleapi";
                 options.ApiSecret = "apisecret";
                 options.SaveToken = true;
             });

            services.AddHttpClient("IDPClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44379");
            });

            services.AddHttpClient("MultiHopClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44312");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // allow CORS requests (JavaScript) - any origin for demo purposes
            app.UseCors(c => c.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            app.UseAuthentication();
            
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });             
        }
    }
}
