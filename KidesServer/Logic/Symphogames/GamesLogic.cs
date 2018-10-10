using KidesServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KidesServer.Symphogames
{
	public static class GamesLogic
	{
		public static async Task<UIntResult> CreatePlayer(string playerName)
		{
			try
			{
				var res = new UIntResult();
				var id = await GamesDb.GetNextPlayerId();
				if (SymphogamesStorage.Players[id] != null)
					return new UIntResult { message = "USER_EXISTS" };

				var p = new SPlayer(id, playerName);
				await GamesDb.CreatePlayer(p);

				res.value = p.Id;
				res.success = true;
				res.message = string.Empty;
				return res;
			}
			catch(Exception ex)
			{
				ErrorLog.WriteError(ex);
				return new UIntResult { message = "EXCEPTION" };
			}
		}

		public static async Task<UIntResult> StartGame(StartGameInput input)
		{
			try
			{
				var res = new UIntResult();
				var id = await GamesDb.GetNextGameId();
				res = await SymphogamesStorage.StartGame(id, input.GameName, input.Size.X, input.Size.Y, input.Districts);
				return res;
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				return new UIntResult { message = "EXCEPTION" };
			}
		}

		public static async Task<CurrentGamePlayerInfo> GetCurrentPlayerInfo(uint gameId, uint userId, string accessguid)
		{
			try
			{
				var res = await SymphogamesStorage.GetCurrentPlayerInfo(gameId, userId, accessguid);

				return res;
			}
			catch(Exception ex)
			{
				ErrorLog.WriteError(ex);
				return new CurrentGamePlayerInfo { message = "EXCEPTION" };
			}
		}
	}
}