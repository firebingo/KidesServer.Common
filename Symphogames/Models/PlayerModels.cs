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
	public enum PlayerRole
	{
		Admin,
		Moderator,
		Player
	}

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
		private string Salt;
		public bool IsVerified { get; private set; }
		public PlayerRole Role { get; private set; }
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
			IsVerified = false;
			Role = PlayerRole.Player;
		}

		public SPlayer(uint id, string iN, string iS, string IA, string iP, bool iV, PlayerRole iR)
		{
			Id = id;
			Name = iN;
			Salt = iS;
			Avatar = IA;
			Password = iP;
			IsVerified = iV;
			Role = iR;
		}

		public Task ChangeName(string iN)
		{
			Name = iN;
			return Task.CompletedTask;
		}

		public async Task SetPassword(string pass)
		{
			StringBuilder builder = new StringBuilder();
			using (var hash = SHA256.Create())
			{
				var hashResult = hash.ComputeHash(Encoding.UTF8.GetBytes($"{Salt}{pass}{(await SymphogamesConfig.GetConfig()).HashPepper ?? "478ab"}"));

				foreach (var b in hashResult) {
					builder.Append(b.ToString("x2"));
				}
			}

			Password = builder.ToString();
		}

		public async Task<bool> CheckLogin(string pass)
		{
			if (!IsVerified)
				return false;

			StringBuilder builder = new StringBuilder();
			using (var hash = SHA256.Create())
			{
				var hashResult = hash.ComputeHash(Encoding.UTF8.GetBytes($"{Salt}{pass}{(await SymphogamesConfig.GetConfig()).HashPepper ?? "478ab"}"));

				foreach (var b in hashResult)
				{
					builder.Append(b.ToString("x2"));
				}
			}

			if(builder.ToString() == Password)
				return true;

			return false;
		}

		public Task VerifyUser()
		{
			IsVerified = true;
			return Task.CompletedTask;
		}
	}

	public class SGamePlayer
	{
		public readonly SPlayer Player;
		public readonly uint DistrictId;
		public List<SKillRecord> Kills;
		public Vector2<int> Position;
		public double MaxHealth = 1.0;
		public double MaxEnergy = 1.0;
		public double _health = 1.0;
		public double Health
		{
			get => _health;
			set => _health = Math.Round(value, 2);
		}
		public double _energy = 1.0;
		public double Energy
		{
			get => _energy;
			set => _energy = Math.Round(value, 2);
		}
		public SPlayerState State;
		public int DeathTurn = 0; //The turn the player died on
		public bool HasSubmittedTurn = false;

		public SGamePlayer(SPlayer player, uint DistrictId, Vector2<int> pos)
		{
			Player = player;
			Kills = new List<SKillRecord>();
			Position = pos;
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
