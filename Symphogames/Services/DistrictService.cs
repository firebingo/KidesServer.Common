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
	public class DistrictService
	{
		private readonly AppSettings _appSettings;
		private readonly PlayerService _playerService;
		private readonly IMemoryCache _cache;

		public DistrictService(IOptions<AppSettings> appSettings, 
			IMemoryCache cache,
			PlayerService playerService)
		{
			_cache = cache;
			_appSettings = appSettings.Value;
			_playerService = playerService;
		}

		public async Task<SDistrict> GetDistrictById(uint id)
		{
			return await _cache.GetOrCreateAsync<SDistrict>(string.Format(_appSettings.Cache.Keys["District"], id), async entry =>
			{
				var sql = "SELECT * FROM districts WHERE id = @id";
				List<SDistrict> rows = new List<SDistrict>();
				await DataLayerShortcut.ExecuteReaderAsync(ReadDistrictRow, rows, _appSettings.Database.ConnectionString, sql, new MySqlParameter[] { new MySqlParameter("@id", id) });
				entry.Value = rows.FirstOrDefault();
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(_appSettings.Cache.Time["District"]);
				return rows.FirstOrDefault();
			});
		}

		//I am generally assuming for now that the input for the district has already been verified in the game creation process.
		public async Task<UIntResult> CreateDistrict(uint gameId, string districtName, string description, List<uint> playerIds)
		{
			try
			{
				var res = new UIntResult();

				var id = await GetNextDistrictId();
				var p = new SDistrict(id, gameId, districtName, description, playerIds);

				var sql = "INSERT INTO districts (id, gameId, name, description, players) VALUES (@id, @gameId, @name, @description, @players)";
				var param = new MySqlParameter[]
				{
					new MySqlParameter("@id", id),
					new MySqlParameter("@gameId", p.GameId),
					new MySqlParameter("@name", p.Name),
					new MySqlParameter("@description", p.Description),
					new MySqlParameter("@players", string.Join(',', playerIds.Select(x => x.ToString()))),
				};

				await DataLayerShortcut.ExecuteNonQueryAsync(_appSettings.Database.ConnectionString, sql, param);

				var created = await GetDistrictById(id);
				if (created == null)
					return new UIntResult() { message = "DISTRICT_CREATE_FAILED" };

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

		public async Task<uint> GetNextDistrictId()
		{
			var sql = "SELECT AUTO_INCREMENT FROM districts";
			return await DataLayerShortcut.ExecuteScalarAsync<uint>(_appSettings.Database.ConnectionString, sql);
		}

		#region DB Reader Functions
		private Task ReadDistrictRow(IDataReader reader, List<SDistrict> data)
		{
			try
			{
				uint? id = reader.GetValue(0) as uint?;
				uint? gameId = reader.GetValue(1) as uint?;
				var playerIds = reader.GetString(4).Split(',')
				.Select((x) => 
				{
					uint? resid = null;
					if (uint.TryParse(x, out var pid)) { resid = pid; }
					return resid;
				})
				.Where((x) => x.HasValue)
				.Select((x) => x.Value)
				.ToList();

				var row = new SDistrict(id.Value,
					gameId.Value,
					reader.GetString(2),
					reader.GetString(3),
					playerIds
					);
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
