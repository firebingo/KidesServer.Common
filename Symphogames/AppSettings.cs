using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Symphogames
{
	public class AppSettings
	{
		public DbConfig Database { get; set; }
		public SecurityConfig Security { get; set; }
		public CacheConfig Cache { get; set; }
	}

	public class SecurityConfig
	{
		public string HashPepper { get; set; }
		public string JwtKey { get; set; }
	}

	public class DbConfig
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Address { get; set; }
		public string Port { get; set; }
		public string SchemaName { get; set; }
		private string _connectionString { get; set; }
		[JsonIgnore]
		public string ConnectionString
		{
			get
			{
				if (_connectionString == null)
					_connectionString = $"server={Address};port={Port};uid={UserName};pwd={Password};database={SchemaName};charset=utf8mb4;Allow User Variables=True;SslMode=none";
				return _connectionString;
			}
		}
	}

	public class CacheConfig
	{
		public Dictionary<string, string> Keys { get; set; }
		public Dictionary<string, uint> Time { get; set; }
	}
}
