using KidesServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KidesServer.Symphogames
{
	public class SPlayer
	{
		public readonly uint Id;
		public string Name { get; private set; }
		public string Password { get; private set; }
		public readonly SPlayerHistory History;

		public SPlayer(uint id, string iN)
		{
			Id = id;
			Name = iN;
		}

		public Task ChangeName(string iN)
		{
			Name = iN;
			return Task.CompletedTask;
		}
	}

	public class SGamePlayer
	{
		public readonly SPlayer Player;
		public List<SKillRecord> Kills { get; set; }
		public Vector2<uint> Position { get; set; }
		public float Health = 1.0f;

		public SGamePlayer(SPlayer player, Vector2<uint> pos)
		{
			Player = player;
			Kills = new List<SKillRecord>();
			Position = pos;
		}
	}

	public class SKillRecord
	{
		public Guid GameId { get; }
		public uint PlayerId { get; }
		public int TurnNumber { get; }

		public SKillRecord(Guid game, uint player, int @event)
		{
			GameId = game;
			PlayerId = player;
			TurnNumber = @event;
		}
	}

	public class SPlayerHistory
	{
		public List<SKillRecord> Kills { get; set; }
		public List<SKillRecord> Deaths { get; set; }
		public List<Guid> Victories { get; set; }
	}

	public class SDistrict
	{
		public string Name;
		public readonly uint Id;
		public Dictionary<uint, SPlayer> Players;

		SDistrict(string iName, Dictionary<uint, SPlayer> iP)
		{
			Name = iName;
			Players = iP;
		}
	}
}
