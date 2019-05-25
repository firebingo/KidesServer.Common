using KidesServer.Common;
using KidesServer.Common.DataBase;
using Microsoft.Extensions.Options;
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
	public class SymphogamesConfigService
	{
		//public static string folderLocation = string.Empty;
		private SymphogamesConfigModel _config;
		private DateTime _configExpire;
		private AppSettings _appSettings;

		public SymphogamesConfigService(IOptions<AppSettings> appSettings)
		{
			try
			{
				_appSettings = appSettings.Value;
				//folderLocation = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
			}
			catch (Exception e)
			{
				ErrorLog.WriteLog(e.Message);
			}
		}

		public SymphogamesConfigModel GetConfig()
		{
			try
			{
				if (_config == null || _configExpire > DateTime.UtcNow)
				{
					var query = "SELECT * FROM gamesconfig LIMIT 1";
					SymphogamesConfigModel cfg = new SymphogamesConfigModel();
					DataLayerShortcut.ExecuteReader(ReadConfig, cfg, _appSettings.Database.ConnectionString, query);
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

		public async Task<SymphogamesConfigModel> GetConfigAsync()
		{
			try
			{
				if (_config == null || _configExpire > DateTime.UtcNow)
				{
					var query = "SELECT * FROM gamesconfig LIMIT 1";
					SymphogamesConfigModel cfg = new SymphogamesConfigModel();
					await DataLayerShortcut.ExecuteReaderAsync(ReadConfig, cfg, _appSettings.Database.ConnectionString, query);
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

		private void ReadConfig(IDataReader reader, SymphogamesConfigModel data)
		{
			data.HashPepper = _appSettings.Security.HashPepper;
			data.JwtKey = reader.GetString(2);
			data.GameTickMs = (reader[3] as uint?).Value;
			data.ConfigExpireMs = (reader[4] as uint?).Value;
		}
	}
}
