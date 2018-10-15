using KidesServer.Helpers;
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

		public static Task<UIntResult> StartGame(uint id, string name, string image, int width, int height, List<DistrictInput> districts)
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
			if(Uri.TryCreate(image, UriKind.Absolute, out var u))
				game.Map.MapImage = image;
			else
				game.Map.MapImage = $"/api/v1/symphogames/image?type=0&name={image}";

			res.success = true;
			res.message = string.Empty;
			res.value = game.Id;
			return Task.FromResult(res);
		}

		public static Task<JoinGameResult> GetPlayerAccessData(uint gameId, uint playerId)
		{
			var res = new JoinGameResult();

			if (!CurrentGames.ContainsKey(gameId))
				return Task.FromResult(new JoinGameResult { message = $"GAME_NOT_EXIST|{gameId}" });
			var currentGame = CurrentGames[gameId];
			var gamePlayer = currentGame.GetPlayerById(playerId);
			if (gamePlayer == null)
				return Task.FromResult(new JoinGameResult { message = $"USER_NOT_EXIST|{playerId}" });

			res.GameId = currentGame.Id;
			res.PlayerId = gamePlayer.Player.Id;
			res.AccessGuid = gamePlayer.AccessGuid;
			res.success = true;
			res.message = string.Empty;

			return Task.FromResult(res);
		}

		public static Task<CurrentGamePlayerInfo> GetCurrentPlayerInfo(uint gameId, uint playerId, string accessguid)
		{
			var res = new CurrentGamePlayerInfo();
			if (!CurrentGames.ContainsKey(gameId))
				return Task.FromResult(new CurrentGamePlayerInfo { message = $"GAME_NOT_EXIST|{gameId}" });
			var currentGame = CurrentGames[gameId];
			var gamePlayer = currentGame.GetPlayerById(playerId);
			if (gamePlayer == null)
				return Task.FromResult(new CurrentGamePlayerInfo { message = $"USER_NOT_EXIST|{playerId}" });
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
				ThisPlayer = new SPlayerInfo()
				{
					Id = gamePlayer.Player.Id,
					Name = gamePlayer.Player.Name,
					DistrictId = gamePlayer.DistrictId,
					Position = gamePlayer.Position,
					State = gamePlayer.State,
					Kills = gamePlayer.Kills.Count,
					Range = 0.0
				},
				DeadPlayers = deadPlayers.Select(x => new SPlayerInfo() {
					Id = x.Value.Player.Id,
					Name = x.Value.Player.Name,
					DistrictId = x.Value.DistrictId,
					Position = x.Value.Position,
					State = x.Value.State,
					Kills = x.Value.Kills.Count,
					Range = MathHelpers.VectorDistance(gamePlayer.Position, x.Value.Position)
				}).ToList(),
				Players = nearPlayers.Select(x => new SPlayerInfo()
				{
					Id = x.Value.Player.Id,
					Name = x.Value.Player.Name,
					DistrictId = x.Value.DistrictId,
					Position = x.Value.Position,
					State = x.Value.State,
					Kills = x.Value.Kills.Count,
					Range = MathHelpers.VectorDistance(gamePlayer.Position, x.Value.Position)
				}).ToList()
			};

			GetAviliableActions(ref res);

			res.success = true;
			res.message = string.Empty;

			return Task.FromResult(res);
		}

		private static void GetAviliableActions(ref CurrentGamePlayerInfo info)
		{
			info.ActionInfo = new List<SActionInfo>();

			info.ActionInfo.Add(new SActionInfo { Type = SActionType.Wait, ActionName = "WAIT" });

			var gamePlayer = info.PlayerInfo.ThisPlayer;
			if (gamePlayer.Position.X != 0)
				info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.West, ActionName = "MOVE|WEST" });
			if(gamePlayer.Position.X != 0 && gamePlayer.Position.Y < info.MapInfo.Map.Size.Y)
				info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.SouthWest, ActionName = "MOVE|SOUTHWEST" });
			if (gamePlayer.Position.Y < info.MapInfo.Map.Size.Y)
				info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.South, ActionName = "MOVE|SOUTH" });
			if (gamePlayer.Position.Y < info.MapInfo.Map.Size.Y && gamePlayer.Position.X < info.MapInfo.Map.Size.X)
				info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.SouthEast, ActionName = "MOVE|SOUTHEAST" });
			if (gamePlayer.Position.X < info.MapInfo.Map.Size.X)
				info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.East, ActionName = "MOVE|EAST" });
			if (gamePlayer.Position.X < info.MapInfo.Map.Size.X && gamePlayer.Position.Y != 0)
				info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.NorthEast, ActionName = "MOVE|NORTHEAST" });
			if (gamePlayer.Position.Y != 0)
				info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.North, ActionName = "MOVE|NORTH" });
			if (gamePlayer.Position.Y != 0 && gamePlayer.Position.X != 0)
				info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.NorthWest, ActionName = "MOVE|NORTHWEST" });
			var inRangePlayers = info.PlayerInfo.Players.Where(x => x.Range < 1.0);
			if (inRangePlayers.Count() > 1)
			{
				info.ActionInfo.Add(new SActionInfo { Type = SActionType.Defend, ActionName = "DEFEND" });
				foreach (var player in inRangePlayers)
				{
					info.ActionInfo.Add(new SActionInfo { Type = SActionType.Attack, Target = player.Id, ActionName = $"ATTRACK|{player.Name}" });
				}
			}
		}
	}
}
