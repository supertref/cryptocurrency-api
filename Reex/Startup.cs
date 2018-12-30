using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reex.Helpers;
using Reex.Services.CosmosDbService;
using Reex.Services.RehiveService;
using Reex.Services.WalletManagementService;

namespace Reex
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMemoryCache();

            // configure token authentication 
            services.AddAuthentication("TokenAuthentication")
                .AddScheme<AuthenticationSchemeOptions, TokenAuthenticationHandler>("TokenAuthentication", null);

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<NBitcoin.Altcoins.Reex>();
            services.AddScoped<IWalletManagementService, WalletManagementService>();
            services.AddScoped<IRehiveService, RehiveService>();
            services.AddScoped<ICosmosDbService, CosmosDbService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
