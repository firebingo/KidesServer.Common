using KidesServer.Common;
using Symphogames.Helpers;
using Symphogames.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Symphogames.Logic
{
	public static class GamesLogic
	{
		public static async Task<UIntResult> CreatePlayer(string playerName)
		{
			try
			{
				var res = new UIntResult();
				var id = await GamesDb.GetNextPlayerId();
				if (GamesDb.Players.ContainsKey(id))
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

		public static async Task<UIntResult> CreateGame(CreateGameInput input)
		{
			try
			{
				var res = new UIntResult();
				var id = await GamesDb.GetNextGameId();
				res = await SymphogamesStorage.CreateGame(id, input.GameName, input.MapImage, input.Size.X, input.Size.Y, input.Districts, input.Seed);
				return res;
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				return new UIntResult { message = "EXCEPTION" };
			}
		}

		public static async Task<JoinGameResult> UserJoinGame(uint gameId, uint playerId)
		{
			try
			{
				var res = await SymphogamesStorage.GetPlayerAccessData(gameId, playerId);
				return res;
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				return new JoinGameResult { message = "EXCEPTION" };
			}
		}

		public static async Task<CurrentGamePlayerInfo> GetCurrentPlayerInfo(uint gameId, uint playerId, string accessguid)
		{
			try
			{
				var res = await SymphogamesStorage.GetCurrentPlayerInfo(gameId, playerId, accessguid);
				return res;
			}
			catch(Exception ex)
			{
				ErrorLog.WriteError(ex);
				return new CurrentGamePlayerInfo { message = "EXCEPTION" };
			}
		}

		public static async Task<BaseResult> SubmitTurn(uint gameId, uint playerId, string accessguid, SActionInfo action)
		{
			try
			{
				var res = await SymphogamesStorage.SubmitTurn(gameId, playerId, accessguid, action);
				return res;
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				return new CurrentGamePlayerInfo { message = "EXCEPTION" };
			}
		}
	}
}