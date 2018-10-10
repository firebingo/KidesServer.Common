using Newtonsoft.Json;

namespace KidesServer.Models
{
	public class BaseResult
	{
		public bool success;
		public string message;
	}

	public class UIntResult : BaseResult
	{
		public ulong value;
	}

	public class ULongResult : BaseResult
	{
		public ulong value;
	}

	public class ConfigModel
	{
		public string wotAppId;
		public string iisLogLocation;
		public string baseMusicUrl;
		public DBConfigModel DBConfig;
		public string botId;
	}

	public class DBConfigModel
	{
		public string userName;
		public string password;
		public string address;
		public string port;
		public string schemaName;
		private string _connectionString;
		[JsonIgnore]
		public string ConnectionString
		{
			get
			{
				if(_connectionString == null)
					_connectionString = $"server={address};port={port};uid={userName};pwd={password};database={schemaName};charset=utf8mb4;Allow User Variables=True;SslMode=none";
				return _connectionString;
			}
		}
	}

	public class SymphogamesConfigModel
	{
		public string HashPepper;
		public DBConfigModel DBConfig;
	}
}