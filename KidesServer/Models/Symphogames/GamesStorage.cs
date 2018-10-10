using KidesServer.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KidesServer.Symphogames
{
	public static class SymphogamesStorage
	{
		public static readonly ConcurrentDictionary<uint, SPlayer> Players;
		public static readonly ConcurrentDictionary<uint, SGame> CurrentGames;

		static SymphogamesStorage()
		{
			Players = new ConcurrentDictionary<uint, SPlayer>();
			CurrentGames = new ConcurrentDictionary<uint, SGame>();
		}

		public static Task<UIntResult> StartGame(uint id, string name, int width, int height, List<DistrictInput> districts)
		{
			var res = new UIntResult();
			var game = new SGame(id, name, width, height);
			for(int i = 0; i < districts.Count; ++i)
			{
				Dictionary<uint, SGamePlayer> dPlayers = new Dictionary<uint, SGamePlayer>();
				foreach(var p in districts[i].PlayerIds)
				{
					if(!Players.ContainsKey(p))
						return Task.FromResult(new UIntResult { message = $"USER_NOT_EXIST|{p}" });
					var newPlayer = new SGamePlayer(Players[p], (uint)i, new Vector2<int>(0, 0));
					dPlayers.Add(p, newPlayer);
				}
				var newDist = new SDistrict(i.ToString(), (uint)i, dPlayers);
				game.AddDistrict(newDist);
			}
			CurrentGames.TryAdd(id, game);

			res.success = true;
			res.message = string.Empty;
			res.value = game.Id;
			return Task.FromResult(res);
		}

		public static Task<CurrentGamePlayerInfo> GetCurrentPlayerInfo(uint gameId, uint userId, string accessguid)
		{
			var res = new CurrentGamePlayerInfo();
			if (!CurrentGames.ContainsKey(gameId))
				return Task.FromResult(new CurrentGamePlayerInfo { message = $"GAME_NOT_EXIST|{gameId}" });
			var currentGame = CurrentGames[gameId];
			var gamePlayer = currentGame.GetPlayerById(userId);
			if (gamePlayer == null)
				return Task.FromResult(new CurrentGamePlayerInfo { message = $"USER_NOT_EXIST|{userId}" });
			if(gamePlayer.AccessGuid != accessguid)
				return Task.FromResult(new CurrentGamePlayerInfo { message = $"INVALID_ACCESS" });

			res.GameInfo = new SGameInfo()
			{
				Id = currentGame.Id,
				CurrentTurn = currentGame.CurrentTurn,
				TimeOfDay = currentGame.TimeOfDay,
				Completed = currentGame.Completed
			};

			res.MapInfo = new SMapInfo()
			{
				Map = currentGame.Map
			};

			var deadPlayers = currentGame.GetDeadPlayers();
			var nearPlayers = currentGame.GetNearPlayers(gamePlayer.Position, 2);

			res.PlayerInfo = new SPlayersInfo()
			{
				DeadPlayers = deadPlayers.Select(x => new SPlayerInfo() {
					Id = x.Value.Player.Id,
					Name = x.Value.Player.Name,
					DistrictId = x.Value.DistrictId,
					Position = x.Value.Position,
					State = x.Value.State,
					Kills = x.Value.Kills.Count
				}).ToList(),
				Players = nearPlayers.Select(x => new SPlayerInfo()
				{
					Id = x.Value.Player.Id,
					Name = x.Value.Player.Name,
					DistrictId = x.Value.DistrictId,
					Position = x.Value.Position,
					State = x.Value.State,
					Kills = x.Value.Kills.Count
				}).ToList(),
			};

			return Task.FromResult(res);
		}
	}
}
