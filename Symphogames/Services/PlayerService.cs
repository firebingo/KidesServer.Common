using KidesServer.Common;
using KidesServer.Common.DataBase;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Symphogames.Helpers;
using Symphogames.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Symphogames.Services
{
	public class PlayerService
	{
		private readonly AppSettings _appSettings;
		private readonly IMemoryCache _cache;

		public PlayerService(IOptions<AppSettings> appSettings, IMemoryCache cache)
		{
			_cache = cache;
			_appSettings = appSettings.Value;
		}

		public async Task<SPlayer> GetPlayerById(uint id)
		{
			return await _cache.GetOrCreateAsync<SPlayer>(string.Format(_appSettings.Cache.Keys["Player"], id), async entry =>
			{
				var sql = "SELECT * FROM players WHERE id = @id";
				List<SPlayer> rows = new List<SPlayer>();
				await DataLayerShortcut.ExecuteReaderAsync(ReadPlayerRow, rows, _appSettings.Database.ConnectionString, sql, new MySqlParameter[] { new MySqlParameter("@id", id) });
				//TODO: read player history once that is a thing that exists
				entry.Value = rows.FirstOrDefault();
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(_appSettings.Cache.Time["Player"]);
				return rows.FirstOrDefault();
			});
		}

		public async Task<bool> GetPlayerExistsById(uint id)
		{
			if (_cache.Get<SPlayer>(string.Format(_appSettings.Cache.Keys["Player"], id)) == null)
			{
				var sql = "SELECT EXISTS(SELECT 1 FROM players WHERE id = @id)";
				if (await DataLayerShortcut.ExecuteScalarAsync<int>(_appSettings.Database.ConnectionString, sql, new MySqlParameter("@id", id)) == 1)
					return true;
			}

			return false;
		}

		public async Task<SPlayer> GetPlayerByName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return null;

			var sql = "SELECT * FROM players WHERE username = @username";
			List<SPlayer> rows = new List<SPlayer>();
			await DataLayerShortcut.ExecuteReaderAsync(ReadPlayerRow, rows, _appSettings.Database.ConnectionString, sql, new MySqlParameter("@username", name));
			//TODO: read player history once thats a thing that exists
			return rows.FirstOrDefault();
		}

		public async Task<bool> GetPlayerExistsByName(string name)
		{
			var sql = "SELECT EXISTS(SELECT 1 FROM players WHERE username = @username)";
			if (await DataLayerShortcut.ExecuteScalarAsync<int>(_appSettings.Database.ConnectionString, sql, new MySqlParameter("@username", name)) == 1)
				return true;

			return false;
		}

		public async Task<UIntResult> CreatePlayer(string playerName)
		{
			try
			{
				var res = new UIntResult();
				if (await GetPlayerExistsByName(playerName))
					return new UIntResult { message = "USER_NAME_EXISTS" };

				var id = await GetNextPlayerId();
				var p = new SPlayer(id, playerName);

				var sql = "INSERT INTO players (id, username, password, salt, role, avatar, isVerified, isDeleted) VALUES (@id, @username, @password, @salt, @role, @avatar, @isVerified, @isDeleted)";
				var param = new MySqlParameter[]
				{
					new MySqlParameter("@id", id),
					new MySqlParameter("@username", playerName),
					new MySqlParameter("@password", p.Password),
					new MySqlParameter("@salt", p.Salt),
					new MySqlParameter("@role", (uint)p.Role),
					new MySqlParameter("@avatar", p.Avatar),
					new MySqlParameter("@isVerified", p.IsVerified),
					new MySqlParameter("@isDeleted", false)
				};

				await DataLayerShortcut.ExecuteNonQueryAsync(_appSettings.Database.ConnectionString, sql, param);

				var created = await GetPlayerById(id);
				if (created == null)
					return new UIntResult() { message = "USER_CREATE_FAILED" };

				res.value = p.Id;
				res.success = true;
				res.message = string.Empty;
				return res;
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				return new UIntResult { message = "EXCEPTION" };
			}
		}

		public async Task<BaseResult> CheckPlayerLogin(uint playerId, string pass)
		{
			try
			{
				var player = await GetPlayerById(playerId);
				if (player == null)
					return new BaseResult() { message = "USER_NOT_EXIST" };

				if (!player.IsVerified)
					return new BaseResult() { message = "USER_NOT_VERIFIED" };

				StringBuilder builder = new StringBuilder();
				using (var hash = SHA512.Create())
				{
					var hashResult = hash.ComputeHash(Encoding.UTF8.GetBytes($"{player.Salt}{pass}{_appSettings.Security.HashPepper ?? "478ab"}"));

					foreach (var b in hashResult)
					{
						builder.Append(b.ToString("x2"));
					}
				}

				if (builder.ToString() == player.Password)
					return new BaseResult() { success = true, message = "" };
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				return new BaseResult() { message = "EXCEPTION" };
			}

			return new BaseResult() { message = "INCORRECT_LOGIN" };
		}

		public async Task<BaseResult> ChangePlayerName(uint playerId, string name)
		{
			try
			{
				if (await GetPlayerExistsByName(name))
					return new BaseResult() { message = "USER_NAME_EXISTS" };

				if (!await GetPlayerExistsById(playerId))
					return new BaseResult() { message = "USER_NOT_EXIST" };

				var sql = "UPDATE players SET username = @username WHERE id = @id";
				await DataLayerShortcut.ExecuteNonQueryAsync(_appSettings.Database.ConnectionString, sql, new MySqlParameter("@id", playerId), new MySqlParameter("@username", name));

				_cache.Remove(string.Format(_appSettings.Cache.Keys["Player"], playerId));
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				return new BaseResult() { message = "EXCEPTION" };
			}

			return new BaseResult() { success = true, message = "" };
		}

		public async Task<BaseResult> ChangePlayerPassword(uint playerId, string pass)
		{
			try
			{
				var player = await GetPlayerById(playerId);
				if (player == null)
					return new BaseResult() { success = false, message = "USER_NOT_EXIST" };

				StringBuilder builder = new StringBuilder();
				using (var hash = SHA256.Create())
				{
					var hashResult = hash.ComputeHash(Encoding.UTF8.GetBytes($"{player.Salt}{pass}{(_appSettings.Security.HashPepper ?? "478ab")}"));

					foreach (var b in hashResult)
					{
						builder.Append(b.ToString("x2"));
					}
				}

				var sql = "UPDATE players SET password = @password WHERE id = @id";
				await DataLayerShortcut.ExecuteNonQueryAsync(_appSettings.Database.ConnectionString, sql, new MySqlParameter("@id", playerId), new MySqlParameter("@password", builder.ToString()));

				_cache.Remove(string.Format(_appSettings.Cache.Keys["Player"], playerId));
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				return new BaseResult() { message = "EXCEPTION" };
			}
			
			return new BaseResult() { success = true, message = "" };
		}

		public async Task<BaseResult> VerifyPlayer(uint playerId)
		{
			try
			{
				if (!await GetPlayerExistsById(playerId))
					return new BaseResult() { message = "USER_NOT_EXIST" };

				var sql = "UPDATE players SET isVerified = @isVerified WHERE id = @id";
				await DataLayerShortcut.ExecuteNonQueryAsync(_appSettings.Database.ConnectionString, sql, new MySqlParameter("@id", playerId), new MySqlParameter("@isVerified", true));

				_cache.Remove(string.Format(_appSettings.Cache.Keys["Player"], playerId));
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				return new BaseResult() { message = "EXCEPTION" };
			}

			return new BaseResult() { success = true, message = "" };
		}

		public async Task<uint> GetNextPlayerId()
		{
			var sql = "SELECT AUTO_INCREMENT FROM players";
			return await DataLayerShortcut.ExecuteScalarAsync<uint>(_appSettings.Database.ConnectionString, sql);
		}

		#region DB Reader Functions
		private Task ReadPlayerRow(IDataReader reader, List<SPlayer> data)
		{
			try
			{
				uint? id = reader.GetValue(0) as uint?;
				if (!id.HasValue)
					return Task.CompletedTask;

				PlayerRole role = (PlayerRole)((reader.GetValue(5) as uint?) ?? 0);
				var row = new SPlayer(id ?? 0,
					reader.GetValue(1) as string,
					reader.GetValue(4) as string,
					reader.GetValue(3) as string,
					reader.GetValue(6) as string,
					reader.GetValue(2) as string,
					reader.GetBoolean(7),
					role);
				data.Add(row);
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
			}

			return Task.CompletedTask;
		}
		#endregion
	}
}
