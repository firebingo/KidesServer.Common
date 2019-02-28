using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Data.Common;
using KidesServer.Common;

namespace KidesServer.Common.DataBase
{
	public static class DataLayerShortcut
	{
		public static bool SchemaExists { get; private set; } = true;

		public static BaseResult TestConnection(string connectionString)
		{
			var result = new BaseResult();
			try
			{
				var connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
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

		public static async Task ExecuteReader<T>(Action<IDataReader, T> workFunction, T otherdata, string connectionString, string query, params MySqlParameter[] parameters)
		{
			using (var connection = new MySqlConnection(connectionString))
			{
				await connection.OpenAsync();
				await ExecuteReader(workFunction, otherdata, connection, query, parameters);
				await connection.CloseAsync();
			}
		}

		public static async Task ExecuteReader<T>(Action<IDataReader, T> workFunction, T otherdata, MySqlConnection connection, string query, params MySqlParameter[] parameters)
		{
			if (connection == null)
				return;
			
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
		}

		public static async Task<string> ExecuteNonQuery(string connectionString, string query, params MySqlParameter[] parameters)
		{
			var result = string.Empty;
			using (var connection = new MySqlConnection(connectionString))
			{
				await connection.OpenAsync();
				result = await ExecuteNonQuery(connection, query, parameters);
				await connection.CloseAsync();
			}
			return result;
		}

		public static async Task<string> ExecuteNonQuery(MySqlConnection connection, string query, params MySqlParameter[] parameters)
		{
			try
			{
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
				return string.Empty;
			}
			catch (Exception e)
			{
				return $"Exception: {e.Message}, Query: {query}";
			}
		}

		public static async Task<int?> ExecuteScalar(string connectionString, string query, params MySqlParameter[] parameters)
		{
			int? result = null;
			using (var connection = new MySqlConnection(connectionString))
			{
				await connection.OpenAsync();
				result = await ExecuteScalar(connection, query, parameters);
				await connection.CloseAsync();
			}
			return result;
		}

		public static async Task<int?> ExecuteScalar(MySqlConnection connection, string query, params MySqlParameter[] parameters)
		{
			int? result = null;
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
					return result;
				}
				cmd.Dispose();
			}
			return result;
		}
	}
}
