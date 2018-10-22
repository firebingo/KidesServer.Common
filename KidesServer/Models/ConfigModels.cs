using KidesServer.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace KidesServer.Models
{
	public class ConfigModel
	{
		public string wotAppId;
		public string iisLogLocation;
		public string baseMusicUrl;
		public DBConfigModel DBConfig;
		public string botId;
		public FileControllerConfig FileAccess;
	}

	public class FileControllerConfig
	{
		public string RootDirectory;
		public List<FileControllerPerson> PeopleList;
		private Dictionary<string, FileControllerPerson> _people;
		[JsonIgnore]
		public Dictionary<string, FileControllerPerson> People
		{
			get
			{
				if (_people == null)
					_people = PeopleList.ToDictionary(k => k.Username.ToLowerInvariant(), v => v);
				return _people;
			}
		}
	}

	public class FileControllerPerson
	{
		public string Username;
		public string Password;
		public bool NeedsPasswordHashed;
		public string HashSalt;
		public bool Upload;
		public bool Download;
		public bool List;
		public List<string> Directories;

		public FileControllerPerson()
		{
			if (string.IsNullOrWhiteSpace(HashSalt))
				HashSalt = Guid.NewGuid().ToString("n");
			if (NeedsPasswordHashed)
			{
				NeedsPasswordHashed = false;
				StringBuilder builder = new StringBuilder();
				using (var hash = SHA256.Create())
				{
					var result = hash.ComputeHash(Encoding.UTF8.GetBytes($"{HashSalt}{Password}"));

					foreach (var b in result)
					{
						builder.Append(b.ToString("x2"));
					}
				}
				Password = builder.ToString();
			}
		}

		public bool CheckPassword(string password)
		{
			StringBuilder builder = new StringBuilder();
			using (var hash = SHA256.Create())
			{
				var result = hash.ComputeHash(Encoding.UTF8.GetBytes($"{HashSalt}{password}"));

				foreach (var b in result)
				{
					builder.Append(b.ToString("x2"));
				}
			}
			if (password == builder.ToString())
				return true;
			return false;
		}
	}
}
