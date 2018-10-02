using MySql.Data.MySqlClient;
using System;
using System.Data;
using KidesServer.Models;
using System.Threading.Tasks;
using System.Data.Common;

namespace KidesServer.DataBase
{
	public static class DataLayerShortcut
	{
		public static bool SchemaExists { get; private set; } = true;

		public static BaseResult TestConnection()
		{
			var result = new BaseResult();
			try
			{
				var connection = new MySql.Data.MySqlClient.MySqlConnection(AppConfig.Config.DBConfig.ConnectionString);
				connection.Open();
				connection.Close();
				connection.Dispose();
			}
			catch (MySqlException e)
			{
				if (e.InnerException.Message.ToUpperInvariant().Contains("UNKNOWN DATABASE"))
					SchemaExists = false;

				result.success = false;
				result.message = e.Message;
				return result;
			}
			result.success = true;
			return result;
		}

		public static async Task ExecuteReader<T>(Action<IDataReader, T> workFunction, T otherdata, string query, params MySqlParameter[] parameters)
		{
			var connection = new MySqlConnection(AppConfig.Config.DBConfig.ConnectionString);
			connection.Open();
			if (connection.State == ConnectionState.Open)
			{
				MySqlCommand cmd = new MySqlCommand(query, connection);
				DbDataReader reader = null;
				if (parameters != null)
					DataHelper.AddParams(ref cmd, parameters);

				reader = await cmd.ExecuteReaderAsync();

				while (reader.Read())
					workFunction(reader, otherdata);

				reader.Close();
				cmd.Dispose();
			}
			connection.Close();
			connection.Dispose();
		}

		public static async Task<string> ExecuteNonQuery(string query, params MySqlParameter[] parameters)
		{
			var connection = new MySqlConnection(AppConfig.Config.DBConfig.ConnectionString);
			try
			{
				connection.Open();
				if (connection.State == ConnectionState.Open)
				{
					MySqlCommand cmd = new MySqlCommand(query, connection)
					{
						CommandType = CommandType.Text
					};
					if (parameters != null)
						DataHelper.AddParams(ref cmd, parameters);

					await cmd.ExecuteNonQueryAsync();
					cmd.Dispose();
				}
				connection.Close();
				connection.Dispose();
				return "";
			}
			catch (Exception e)
			{
				connection.Close();
				connection.Dispose();
				return $"Exception: {e.Message}, Query: {query}";
			}
		}

		public static async Task ExecuteSpecialNonQuery(string query, string connection, params MySqlParameter[] parameters)
		{
			var conn = new MySqlConnection(connection);
			conn.Open();
			MySqlCommand cmd = new MySqlCommand(query, conn)
			{
				CommandType = CommandType.Text
			};
			if (parameters != null)
				DataHelper.AddParams(ref cmd, parameters);

			await cmd.ExecuteNonQueryAsync();
			cmd.Dispose();
			conn.Close();
			conn.Dispose();
		}

		public static async Task<int?> ExecuteScalar(string query, params MySqlParameter[] parameters)
		{
			int? result = null;
			var connection = new MySqlConnection(AppConfig.Config.DBConfig.ConnectionString);
			connection.Open();
			if (connection.State == ConnectionState.Open)
			{
				MySqlCommand cmd = new MySqlCommand(query, connection)
				{
					CommandType = CommandType.Text
				};
				if (parameters != null)
					DataHelper.AddParams(ref cmd, parameters);

				object scalar = await cmd.ExecuteScalarAsync();
				try
				{
					result = Convert.ToInt32(scalar);
				}
				catch
				{
					cmd.Dispose();
					connection.Close();
					connection.Dispose();
					return result;
				}
				cmd.Dispose();
			}
			connection.Close();
			connection.Dispose();
			return result;
		}
	}
}
