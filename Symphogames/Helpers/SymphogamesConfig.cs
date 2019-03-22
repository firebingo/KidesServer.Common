using KidesServer.Common;
using KidesServer.Common.DataBase;
using Newtonsoft.Json;
using Symphogames.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Symphogames.Helpers
{
	public class SymphogamesConfig
	{
		public static string folderLocation = string.Empty;
		private static SymphogamesConfigModel _config;
		private static DateTime _configExpire;
		private static DBConfigModel _dbConfig;

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

		public static async Task<SymphogamesConfigModel> GetConfig()
		{
			try
			{
				if (_config == null || _configExpire > DateTime.UtcNow)
				{
					var query = "SELECT * FROM gamesconfig LIMIT 1";
					SymphogamesConfigModel cfg = new SymphogamesConfigModel();
					await DataLayerShortcut.ExecuteReader(ReadConfig, cfg, DbConfig.ConnectionString, query);
					_config = cfg;
					_configExpire = DateTime.UtcNow.AddMilliseconds(cfg.ConfigExpireMs);
				}
				return _config;
			}
			catch (Exception e)
			{
				ErrorLog.WriteLog(e.Message);
				return null;
			}
		}

		public static DBConfigModel DbConfig
		{
			get
			{
				try
				{
					if (_dbConfig == null)
						_dbConfig = JsonConvert.DeserializeObject<DBConfigModel>(File.ReadAllText($"{folderLocation}\\SGDbConfig.json"));
					return _dbConfig;
				}
				catch (Exception e)
				{
					ErrorLog.WriteLog(e.Message);
					return null;
				}
			}
		}

		private static void ReadConfig(IDataReader reader, SymphogamesConfigModel data)
		{
			data.HashPepper = _dbConfig.hashPepper;
			data.JwtKey = reader.GetString(2);
			data.GameTickMs = (reader[3] as uint?).Value;
			data.ConfigExpireMs = (reader[4] as uint?).Value;
		}
	}
}
