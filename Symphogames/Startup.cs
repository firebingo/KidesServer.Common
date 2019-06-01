using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Symphogames.Helpers;
using Symphogames.Models;
using Symphogames.Services;

namespace Symphogames
{
    public class Startup
    {
		private readonly IHostingEnvironment _env;
		public IConfiguration Configuration { get; private set; }

		public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
			_env = env;
			Configuration = configuration;
		}

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.Combine(_env.ContentRootPath, "App_Data"));

			services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

			var config = Configuration.GetSection("AppSettings").Get<AppSettings>();
			var key = Encoding.UTF8.GetBytes(config.Security.JwtKey);

			services.AddMemoryCache();

			services.AddSingleton<SymphogamesConfigService>();
			services.AddScoped<UserRoleCheckAttribute>();
			services.AddScoped<LoginService>();
			services.AddScoped<PlayerService>();
			services.AddSingleton<GameService>();

			services.AddAuthentication(x =>
			{
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(x =>
			{
				x.RequireHttpsMetadata = false;
				x.SaveToken = true;
				x.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false
				};
			});

			//services.AddAuthorization(options =>
			//{
			//	options.AddPolicy()
			//});
		}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
				app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
				{
					HotModuleReplacement = true,
					ReactHotModuleReplacement = true,
				});
			}
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
			app.UseAuthentication();

			app.UseMvc(routes =>
            {
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Root}");
				routes.MapRoute(
					name: "api",
					template: "api/{controller}/{id?}");
				routes.MapSpaFallbackRoute(
					name: "spa-fallback",
					defaults: new { controller = "Home", action = "Root" });
            });
        }
    }
}
