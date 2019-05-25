using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Symphogames
{
	public class Symphogames
	{
		public static string envName { get; private set; } = "Debug";

		public static void Main(string[] args)
		{

#if Debug
			envName = "Debug"
#elif Release
			envName = "Release"
#endif

			CreateWebHostBuilder(args).Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseKestrel()
				.UseEnvironment(envName)
				.UseContentRoot(Directory.GetCurrentDirectory())
				.ConfigureAppConfiguration((builderContext, config) =>
				{
					 config.AddJsonFile($"appsettings.{envName}.json", optional: false, reloadOnChange: true);
				})
				.UseStartup<Startup>();
	}
}
