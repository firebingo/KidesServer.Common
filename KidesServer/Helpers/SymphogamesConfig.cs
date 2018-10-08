using KidesServer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KidesServer.Helpers
{
	public class SymphogamesConfig
	{
		public static string folderLocation = string.Empty;
		private static ConfigModel _config;

		static SymphogamesConfig()
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
					if (_config == null)
						_config = JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText($"{folderLocation}\\SGConfig.json"));
					return _config;
				}
				catch (Exception e)
				{
					ErrorLog.WriteLog(e.Message);
					return null;
				}
			}
		}
	}
}
