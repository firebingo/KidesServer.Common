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
		public readonly string Name;
		public readonly string Password;
		public readonly string Salt;
		public readonly bool IsVerified;
		public readonly PlayerRole Role;
		public readonly string Avatar;
		public readonly SPlayerHistory History;

		public string AvatarUrl
		{
			get
			{
				if(Uri.TryCreate(Avatar, UriKind.Absolute, out var u))
					return u.ToString();

				return $"/api/v1/symphogames/image?type=1&name={Avatar}";
			}
		}

		public SPlayer(uint id, string iName)
		{
			Id = id;
			Name = iName;
			Salt = Guid.NewGuid().ToString("n");
			Avatar = "default";
			IsVerified = false;
			Role = PlayerRole.Player;
		}

		public SPlayer(uint id, string iName, string iSalt, string IAvatar, string iPassword, bool iVerified, PlayerRole iRole)
		{
			Id = id;
			Name = iName;
			Salt = iSalt;
			Avatar = IAvatar;
			Password = iPassword;
			IsVerified = iVerified;
			Role = iRole;
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
