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
		public static async Task<CurrentGamePlayerInfo> GetCurrentPlayerInfo(uint gameId, uint playerId)
		{
			try
			{
				var res = await SymphogamesStorage.GetCurrentPlayerInfo(gameId, playerId);
				return res;
			}
			catch(Exception ex)
			{
				ErrorLog.WriteError(ex);
				return new CurrentGamePlayerInfo { message = "EXCEPTION" };
			}
		}

		public static async Task<BaseResult> SubmitTurn(uint gameId, uint playerId, SActionInfo action)
		{
			try
			{
				var res = await SymphogamesStorage.SubmitTurn(gameId, playerId, action);
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