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
		Unknown = 0,
		Player = 1,
		Moderator = 2,
		Admin = 3
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
		public readonly string Description;
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
			Description = "";
			Salt = Guid.NewGuid().ToString("n");
			Avatar = "default";
			IsVerified = false;
			Role = PlayerRole.Player;
		}

		public SPlayer(uint id, string iName, string iDes, string iSalt, string IAvatar, string iPassword, bool iVerified, PlayerRole iRole)
		{
			Id = id;
			Name = iName;
			Description = iDes;
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
		public readonly uint Id;
		public readonly uint GameId;
		public readonly string Name;
		public readonly string Description;
		public readonly List<uint> PlayerIds;
		Dictionary<uint, SGamePlayer> Players; //TODO: Once i have the rest of the structure for a game player sorted out figure out loading this with the district.

		public SDistrict(uint id, uint gameId, string districtName, string description, List<uint> playerIds)
		{
			Id = id;
			GameId = gameId;
			Name = districtName;
			Description = description;
			PlayerIds = playerIds;
		}
	}
}
