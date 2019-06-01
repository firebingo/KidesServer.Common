﻿using KidesServer.Common;
using KidesServer.Common.DataBase;
using Microsoft.Extensions.Options;
using Symphogames.Helpers;
using Symphogames.Logic;
using Symphogames.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Symphogames.Services
{
	public class GameService
	{
		private readonly AppSettings _appSettings;
		private readonly PlayerService _playerService;
		//public readonly ConcurrentDictionary<uint, SGame> CurrentGames;
		//private readonly ConcurrentDictionary<uint, GamesThread> _gameThreads;

		public GameService(IOptions<AppSettings> appSettings,
			PlayerService playerService)
		{
			//CurrentGames = new ConcurrentDictionary<uint, SGame>();
			//_gameThreads = new ConcurrentDictionary<uint, GamesThread>();
			_playerService = playerService;
			_appSettings = appSettings.Value;
		}

		//public async Task<UIntResult> CreateGame(CreateGameInput input)
		//{
		//	try
		//	{
		//		var res = new UIntResult();
		//		var id = await GamesDb.GetNextGameId();
		//		res = await SymphogamesStorage.CreateGame(id, input.GameName, input.MapImage, input.Size.X, input.Size.Y, input.Districts, input.Seed);
		//		return res;
		//	}
		//	catch (Exception ex)
		//	{
		//		ErrorLog.WriteError(ex);
		//		return new UIntResult { message = "EXCEPTION" };
		//	}
		//}
		//
		//public async Task<JoinGameResult> UserJoinGame(uint gameId, uint playerId)
		//{
		//	try
		//	{
		//		var res = await SymphogamesStorage.GetPlayerAccessData(gameId, playerId);
		//		return res;
		//	}
		//	catch (Exception ex)
		//	{
		//		ErrorLog.WriteError(ex);
		//		return new JoinGameResult { message = "EXCEPTION" };
		//	}
		//}

		public async Task<UIntResult> CreateGame(CreateGameInput input)
		{
			var verifyResult = await VerifyCreateGameInput(input);
			if (!string.IsNullOrWhiteSpace(verifyResult))
			{
				return new UIntResult()
				{
					success = false,
					message = verifyResult
				};
			}

			var res = new UIntResult();
			var id = await GetNextGameId();
			var game = new SGame(id, input.GameName, input.GameDescription, input.MapId, null, null, input.Seed, input.IsPastViewable, input.ModeratorIds);

			foreach(var district in input.Districts)
			{

			}

			for (int i = 0; i < districts.Count; ++i)
			{
				Dictionary<uint, SGamePlayer> dPlayers = new Dictionary<uint, SGamePlayer>();
				foreach (var p in districts[i].PlayerIds)
				{
					if (!GamesDb.Players.ContainsKey(p))
						return new UIntResult { message = $"USER_NOT_EXIST|{p}" };
					var newPlayer = new SGamePlayer(GamesDb.Players[p], (uint)i, new Vector2<int>(0, 0));
					dPlayers.Add(p, newPlayer);
				}
				var newDist = new SDistrict(i.ToString(), (uint)i, dPlayers);
				game.AddDistrict(newDist);
			}
			CurrentGames.TryAdd(id, game);
			var newThread = new GamesThread(game);
			_gameThreads.TryAdd(id, newThread);
			await newThread.StartThread();
			if (Uri.TryCreate(image, UriKind.Absolute, out var u))
				game.Map.MapImage = image;
			else
				game.Map.MapImage = $"/api/v1/symphogames/image?type=0&name={image}";

			//TODO: make another api for this and make a front end way to call it.
			game.StartGame();
			res.success = true;
			res.message = string.Empty;
			res.value = game.Id;
			return res;
		}

		private async Task<string> VerifyCreateGameInput(CreateGameInput input)
		{
			if(input.Districts == null || string.IsNullOrWhiteSpace(input.GameName))
				return "INVALID_INPUT";

			foreach(var d in input.Districts)
			{
				if (string.IsNullOrWhiteSpace(d.Name) || d.PlayerIds == null)
					return $"INVALID_DISTRICT|{d.Name}";

				foreach(var player in d.PlayerIds)
				{
					if(!await _playerService.GetPlayerExistsById(player))
						return $"USER_NOT_EXIST|{player}";
				}
			}

			if (input.ModeratorIds != null)
			{
				foreach (var mod in input.ModeratorIds)
				{
					if (!await _playerService.GetPlayerExistsById(mod))
						return $"USER_NOT_EXIST|{mod}";
				}
			}

			return string.Empty;
		}

		public Task<JoinGameResult> GetPlayerAccessData(uint gameId, uint playerId)
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
			res.success = true;
			res.message = string.Empty;

			return Task.FromResult(res);
		}

		public Task<CurrentGamePlayerInfo> GetCurrentPlayerInfo(uint gameId, uint playerId)
		{
			var res = new CurrentGamePlayerInfo();
			if (!CurrentGames.ContainsKey(gameId))
				return Task.FromResult(new CurrentGamePlayerInfo { message = $"GAME_NOT_EXIST|{gameId}" });
			var currentGame = CurrentGames[gameId];
			var gamePlayer = currentGame.GetPlayerById(playerId);
			if (gamePlayer == null)
				return Task.FromResult(new CurrentGamePlayerInfo { message = $"USER_NOT_EXIST|{playerId}" });

			res.GameInfo = new SGameInfo()
			{
				Id = currentGame.Id,
				Started = currentGame.Started,
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
					Avatar = gamePlayer.Player.AvatarUrl,
					DistrictId = gamePlayer.DistrictId,
					Position = gamePlayer.Position,
					State = gamePlayer.State,
					Kills = gamePlayer.Kills.Count,
					Range = 0.0,
					HasSubmittedTurn = gamePlayer.HasSubmittedTurn,
					Health = gamePlayer.Health,
					MaxHealth = gamePlayer.MaxHealth,
					Energy = gamePlayer.Energy,
					MaxEnergy = gamePlayer.MaxEnergy
				},
				DeadPlayers = deadPlayers.Select(x => new SPlayerInfo()
				{
					Id = x.Value.Player.Id,
					Name = x.Value.Player.Name,
					Avatar = x.Value.Player.AvatarUrl,
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
					Avatar = x.Value.Player.AvatarUrl,
					DistrictId = x.Value.DistrictId,
					Position = x.Value.Position,
					State = x.Value.State,
					Kills = x.Value.Kills.Count,
					Range = MathHelpers.VectorDistance(gamePlayer.Position, x.Value.Position)
				}).ToList()
			};

			GetAviliableActions(gamePlayer, ref res);

			res.success = true;
			res.message = string.Empty;

			return Task.FromResult(res);
		}

		private void GetAviliableActions(SGamePlayer gamePlayer, ref CurrentGamePlayerInfo info)
		{
			if (gamePlayer.State != SPlayerState.Awake)
				return;

			info.ActionInfo = new List<SActionInfo>
			{
				new SActionInfo { Type = SActionType.Wait, ActionName = "WAIT" }
			};

			var gamePlayerInfo = info.PlayerInfo.ThisPlayer;
			if (gamePlayer.Energy >= 0.1f)
			{
				if (gamePlayerInfo.Position.X != 0)
					info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.West, ActionName = "MOVE|WEST" });
				if (gamePlayerInfo.Position.X != 0 && gamePlayerInfo.Position.Y < info.MapInfo.Map.Size.Y)
					info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.SouthWest, ActionName = "MOVE|SOUTHWEST" });
				if (gamePlayerInfo.Position.Y < info.MapInfo.Map.Size.Y)
					info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.South, ActionName = "MOVE|SOUTH" });
				if (gamePlayerInfo.Position.Y < info.MapInfo.Map.Size.Y && gamePlayerInfo.Position.X < info.MapInfo.Map.Size.X)
					info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.SouthEast, ActionName = "MOVE|SOUTHEAST" });
				if (gamePlayerInfo.Position.X < info.MapInfo.Map.Size.X)
					info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.East, ActionName = "MOVE|EAST" });
				if (gamePlayerInfo.Position.X < info.MapInfo.Map.Size.X && gamePlayerInfo.Position.Y != 0)
					info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.NorthEast, ActionName = "MOVE|NORTHEAST" });
				if (gamePlayerInfo.Position.Y != 0)
					info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.North, ActionName = "MOVE|NORTH" });
				if (gamePlayerInfo.Position.Y != 0 && gamePlayerInfo.Position.X != 0)
					info.ActionInfo.Add(new SActionInfo { Type = SActionType.Move, Direction = SDirection.NorthWest, ActionName = "MOVE|NORTHWEST" });
			}
			var inRangePlayers = info.PlayerInfo.Players.Where(x => x.Range <= 1.0 && x.Id != gamePlayer.Player.Id);
			if (inRangePlayers.Count() > 0)
			{
				info.ActionInfo.Add(new SActionInfo { Type = SActionType.Defend, ActionName = "DEFEND" });
				foreach (var player in inRangePlayers)
				{
					info.ActionInfo.Add(new SActionInfo { Type = SActionType.Attack, Target = player.Id, ActionName = $"ATTACK|{player.Name}" });
				}
			}
		}

		public async Task<BaseResult> SubmitTurn(uint gameId, uint playerId, SActionInfo action)
		{
			var res = new BaseResult();

			var gameInfo = await GetCurrentPlayerInfo(gameId, playerId);
			if (!gameInfo.success)
				return new BaseResult() { message = gameInfo.message };

			var currentGame = CurrentGames[gameId];
			var gamePlayer = currentGame.GetPlayerById(playerId);
			GetAviliableActions(gamePlayer, ref gameInfo);

			SActionInfo foundAction = null;
			foreach (var a in gameInfo.ActionInfo)
			{
				if (a.Type == action.Type && a.Direction == action.Direction && a.Target == action.Target)
					foundAction = a;
			}

			if (foundAction == null)
				return new BaseResult() { message = "INVALID_ACTION" };

			if (!currentGame.Turns[currentGame.CurrentTurn].Actions.TryAdd(playerId, new SAction() { Type = action.Type, Direction = action.Direction, Target = action.Target }))
				return new BaseResult() { message = "TURN_ALREADY_SUBMIT" };

			gamePlayer.HasSubmittedTurn = true;
			res.success = true;
			res.message = string.Empty;

			return res;
		}

		public async Task<uint> GetNextGameId()
		{
			var sql = "SELECT AUTO_INCREMENT FROM games";
			return await DataLayerShortcut.ExecuteScalarAsync<uint>(_appSettings.Database.ConnectionString, sql);
		}
	}
}