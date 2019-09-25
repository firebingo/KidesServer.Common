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

		public static void ExecuteReader<T>(Action<IDataReader, IDbConnection, T> workFunction, T otherdata, string connectionString, string query, params MySqlParameter[] parameters)
		{
			using (var connection = new MySqlConnection(connectionString))
			{
				connection.Open();
				ExecuteReader(workFunction, otherdata, connection, query, parameters);
				connection.Close();
			}
		}

		public static void ExecuteReader<T>(Action<IDataReader, IDbConnection, T> workFunction, T otherdata, MySqlConnection connection, string query, params MySqlParameter[] parameters)
		{
			if (connection == null)
				return;

			if (connection.State == ConnectionState.Open)
			{
				MySqlCommand cmd = new MySqlCommand(query, connection);
				if (parameters != null)
					DataHelper.AddParams(ref cmd, parameters);

				var reader = cmd.ExecuteReader();

				while (reader.Read())
					workFunction(reader, connection, otherdata);

				reader.Close();
				cmd.Dispose();
			}
		}

		public static async Task ExecuteReaderAsync<T>(Func<IDataReader, IDbConnection, T, Task> workFunction, T otherdata, string connectionString, string query, params MySqlParameter[] parameters)
		{
			using (var connection = new MySqlConnection(connectionString))
			{
				await connection.OpenAsync();
				await ExecuteReaderAsync(workFunction, otherdata, connection, query, parameters);
				await connection.CloseAsync();
			}
		}

		public static async Task ExecuteReaderAsync<T>(Func<IDataReader, IDbConnection, T, Task> workFunction, T otherdata, MySqlConnection connection, string query, params MySqlParameter[] parameters)
		{
			if (connection == null)
				return;
			
			if (connection.State == ConnectionState.Open)
			{
				MySqlCommand cmd = new MySqlCommand(query, connection);
				if (parameters != null)
					DataHelper.AddParams(ref cmd, parameters);

				var reader = await cmd.ExecuteReaderAsync();

				while (reader.Read())
					await workFunction(reader, connection, otherdata);

				reader.Close();
				cmd.Dispose();
			}
		}

		public static async Task ExecuteNonQueryAsync(string connectionString, string query, params MySqlParameter[] parameters)
		{
			using (var connection = new MySqlConnection(connectionString))
			{
				await connection.OpenAsync();
				await ExecuteNonQueryAsync(connection, query, parameters);
				await connection.CloseAsync();
			}
		}

		public static async Task ExecuteNonQueryAsync(MySqlConnection connection, string query, params MySqlParameter[] parameters)
		{
			if (connection == null)
				return;

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
		}

		public static async Task<T> ExecuteScalarAsync<T>(string connectionString, string query, params MySqlParameter[] parameters)
		{
			T result = default;
			using (var connection = new MySqlConnection(connectionString))
			{
				await connection.OpenAsync();
				result = await ExecuteScalarAsync<T>(connection, query, parameters);
				await connection.CloseAsync();
			}
			return result;
		}

		public static async Task<T> ExecuteScalarAsync<T>(MySqlConnection connection, string query, params MySqlParameter[] parameters)
		{
			if (connection == null)
				return default;

			T result = default;
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
					switch (result)
					{
						case int i:
							result = (T)Convert.ChangeType(Convert.ToInt32(scalar), typeof(T));
							break;
						case uint i:
							result = (T)Convert.ChangeType(Convert.ToUInt32(scalar), typeof(T));
							break;
						default:
							result = (T)Convert.ChangeType(scalar, typeof(T));
							break;
					}
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
