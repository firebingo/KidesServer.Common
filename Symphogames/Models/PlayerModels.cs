using KidesServer.Common;
using Symphogames.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Symphogames.Models
{
	public enum SPlayerState
	{
		Awake,
		Sleeping,
		Dead
	}

	public class SPlayer
	{
		public readonly uint Id;
		public string Name { get; private set; }
		private string Password;
		private readonly string Salt;
		public string Avatar;
		public readonly SPlayerHistory History;

		public string AvatarUrl
		{
			get
			{
				if(Uri.TryCreate(Avatar, UriKind.Absolute, out var u))
				{
					return Avatar;
				}
				return $"/api/v1/symphogames/image?type=1&name={Avatar}";
			}
		}

		public SPlayer(uint id, string iN)
		{
			Id = id;
			Name = iN;
			Salt = Guid.NewGuid().ToString("n");
			Avatar = "default";
		}

		public Task ChangeName(string iN)
		{
			Name = iN;
			return Task.CompletedTask;
		}

		public Task SetPassword(string pass)
		{
			StringBuilder builder = new StringBuilder();
			using (var hash = SHA256.Create())
			{
				var result = hash.ComputeHash(Encoding.UTF8.GetBytes($"{Salt}{pass}{SymphogamesConfig.Config.HashPepper ?? "478ab"}"));

				foreach (var b in result) {
					builder.Append(b.ToString("x2"));
				}
			}

			Password = builder.ToString();
			return Task.CompletedTask;
		}
	}

	public class SGamePlayer
	{
		public readonly SPlayer Player;
		public readonly string AccessGuid; //Used for api calls
		public readonly uint DistrictId;
		public List<SKillRecord> Kills;
		public Vector2<int> Position;
		public float MaxHealth = 1.0f;
		public float MaxEnergy = 1.0f;
		public float Health = 1.0f;
		public float Energy = 1.0f;
		public SPlayerState State;
		public int DeathTurn = 0; //The turn the player died on
		public bool HasSubmittedTurn = false;

		public SGamePlayer(SPlayer player, uint DistrictId, Vector2<int> pos)
		{
			Player = player;
			Kills = new List<SKillRecord>();
			Position = pos;
			AccessGuid = Guid.NewGuid().ToString();
		}
	}

	public class SKillRecord
	{
		public uint GameId { get; }
		public uint PlayerId { get; }
		public uint TargetId { get; }
		public int TurnNumber { get; }
		public string Text { get; }

		public SKillRecord(uint game, uint player, uint target, int turnNumber, string text)
		{
			GameId = game;
			PlayerId = player;
			TargetId = target;
			TurnNumber = turnNumber;
			Text = text;
		}
	}

	public class SPlayerHistory
	{
		public List<SKillRecord> Kills { get; set; }
		public List<SKillRecord> Deaths { get; set; }
		public List<uint> Victories { get; set; }
	}

	public class SDistrict
	{
		public string Name;
		public readonly uint Id;
		public Dictionary<uint, SGamePlayer> Players;

		public SDistrict(string iName, uint id, Dictionary<uint, SGamePlayer> iP)
		{
			Name = iName;
			Id = id;
			Players = iP;
		}
	}
}
