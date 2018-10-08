using KidesServer.Models;
using KidesServer.Symphogames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KidesServer.Symphogames
{
	public static class GamesDb
	{
		private static uint _nextPlayerId = 1;

		public static Task CreatePlayer(SPlayer p)
		{
			SymphogamesStorage.Players.Add(p.Id, p);
			return Task.CompletedTask;
		}

		public static Task<uint> GetNextPlayerId()
		{
			return Task.FromResult(_nextPlayerId++);
		}
	}
}
