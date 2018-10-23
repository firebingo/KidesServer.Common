using KidesServer.Helpers;
using KidesServer.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace KidesServer
{
	public static class AppConfig
	{
		public static string folderLocation = string.Empty;
		private static object cfgLock = new object();
		private static ConfigModel _config;

		static AppConfig()
		{
			try
			{
				folderLocation = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
			}
			catch (Exception e)
			{
				ErrorLog.WriteLog(e.Message);
			}
		}

		public static ConfigModel Config
		{
			get
			{
				try
				{
					lock (cfgLock)
					{
						if (_config == null)
							_config = JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText($"{folderLocation}\\Config.json"));
					}
					_config.FileAccess.CheckPasswordHashes();
					SaveConfig();
					return _config;
				}
				catch (Exception e)
				{
					ErrorLog.WriteLog(e.Message);
					return null;
				}
			}
		}

		public static void SaveConfig()
		{
			try
			{
				lock (cfgLock)
				{
					if (_config == null)
						_config = JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText($"{folderLocation}\\Config.json"));
					var cfg = JsonConvert.SerializeObject(_config, Formatting.Indented);
					if(!string.IsNullOrWhiteSpace(cfg))
						File.WriteAllText($"{folderLocation}\\Config.json", cfg);
				}
			}
			catch (Exception e)
			{
				ErrorLog.WriteLog(e.Message);
			}
		}
	}
}