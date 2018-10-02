using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KidesServer.Models;
using System.Net.Http.Headers;
using System.IO;

namespace KidesServer.Logic
{
	public static class WoTLogic
	{
		private static readonly string appId = "";
		private static readonly Dictionary<string, string> userInfoUrls = null;
		private static readonly Dictionary<string, string> userDataUrls = null;
		//public static string userInfoUrl = "https://api.worldoftanks.com/wot/account/list/";
		//public static string userDataUrl = "https://api.worldoftanks.com/wot/account/info/";

		static WoTLogic()
		{
			try
			{
				appId = AppConfig.Config.wotAppId;
				userInfoUrls = new Dictionary<string, string>()
				{
					{ "na", "https://api.worldoftanks.com/wot/account/list/" },
					{ "eu", "https://api.worldoftanks.eu/wot/account/list/" },
					{ "ru", "https://api.worldoftanks.ru/wot/account/list/" },
					{ "kr", "https://api.worldoftanks.kr/wot/account/list/" },
					{ "asia", "https://api.worldoftanks.asia/wot/account/list/" },
				};
				userDataUrls = new Dictionary<string, string>()
				{
					{ "na", "https://api.worldoftanks.com/wot/account/info/" },
					{ "eu", "https://api.worldoftanks.eu/wot/account/info/" },
					{ "ru", "https://api.worldoftanks.ru/wot/account/info/" },
					{ "kr", "https://api.worldoftanks.kr/wot/account/info/" },
					{ "asia", "https://api.worldoftanks.asia/wot/account/info/" },
				};
			}
			catch (Exception e)
			{
				ErrorLog.writeLog(e.Message);
			}
		}

		public static async Task<WotBasicUser> CallInfoAPI(string searchString, string region)
		{
			HttpClient client = new HttpClient
			{
				BaseAddress = new Uri(userInfoUrls[region])
			};

			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			HttpResponseMessage response = await client.GetAsync($"?application_id={appId}&search={searchString}");
			if (response.IsSuccessStatusCode)
			{
				try
				{
					var dataObjects = response.Content.ReadAsAsync<WotBasicUser>().Result;
					if (dataObjects != null)
					{
						return dataObjects;
					}
					else
					{
						return null;
					}
				}
				catch (Exception e)
				{
					ErrorLog.writeLog(e.Message);
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		public static Task<WotUserInfo> CallDataAPI(string accoundId, string accessToken, string region)
		{
			HttpClient client = new HttpClient
			{
				BaseAddress = new Uri(userDataUrls[region])
			};

			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			HttpResponseMessage response = client.GetAsync($"?application_id={appId}&account_id={accoundId}{(accessToken != null ? $"&access_token={accessToken}" : "")}").Result;
			if (response.IsSuccessStatusCode)
			{
				try
				{
					var dataObjects = response.Content.ReadAsAsync<WotUserInfo>().Result;
					if (dataObjects != null)
						return Task.FromResult(dataObjects);
					else
						return Task.FromResult<WotUserInfo>(null);
				}
				catch (Exception e)
				{
					ErrorLog.writeLog(e.Message);
					return Task.FromResult<WotUserInfo>(null);
				}
			}
			else
			{
				return Task.FromResult<WotUserInfo>(null);
			}
		}
	}
}