using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using KidesServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using React.AspNet;
using ZNetCS.AspNetCore.Authentication.Basic;
using ZNetCS.AspNetCore.Authentication.Basic.Events;

namespace KidesServer
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
			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddReact();

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
			services.AddMvc().AddJsonOptions(opt =>
			{
				opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
				opt.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
			});

			services
				.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
				.AddBasicAuthentication(options =>
				{
					options.Realm = "KidesServer";
					options.Events = new BasicAuthenticationEvents
					{
						OnValidatePrincipal = context =>
						{
							FileControllerPerson user = null;
							if(!string.IsNullOrWhiteSpace(context.UserName))
								user = AppConfig.Config.FileAccess.People[context.UserName.ToLowerInvariant()];
							if (user == null)
								user = AppConfig.Config.FileAccess.People["anon"];

							if (user != null && user.CheckPassword(context.Password))
							{
								var claims = new List<Claim>
								{
									new Claim(ClaimTypes.Name,
											  context.UserName,
											  context.Options.ClaimsIssuer)
								};

								var ticket = new AuthenticationTicket(
								  new ClaimsPrincipal(new ClaimsIdentity(
									claims,
									BasicAuthenticationDefaults.AuthenticationScheme)),
								  new AuthenticationProperties(),
								  BasicAuthenticationDefaults.AuthenticationScheme);

								return Task.FromResult(AuthenticateResult.Success(ticket));
							}

							return Task.FromResult(AuthenticateResult.Fail("Authentication failed."));
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
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(env.ContentRootPath, "App_Data"));
			Directory.CreateDirectory($"{AppDomain.CurrentDomain.GetData("DataDirectory").ToString()}\\Temp");

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();
			app.UseAuthentication();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Root}");
				routes.MapRoute(
					name: "KidesApi",
					template: "api/{controller}/{id}");
			});
		}
	}
}
