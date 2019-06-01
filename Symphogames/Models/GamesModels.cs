using KidesServer.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Symphogames.Models
{
	public enum CurrentTime
	{
		Day,
		Night
	}

	public enum SDirection
	{
		North,
		NorthEast,
		East,
		SouthEast,
		South,
		SouthWest,
		West,
		NorthWest
	}

	public enum SActionType
	{
		Move,
		Attack,
		Defend,
		Wait
	}

	public enum SImageType
	{
		Map,
		Avatar
	}

	public class SGame
	{
		public readonly uint Id;
		public readonly string Name;
		public readonly string Description;
		public readonly bool Started;
		public readonly uint MapId;
		public readonly SMap Map;
		public readonly Dictionary<uint, SDistrict> Districts;
		public readonly int PlayerCount;
		public readonly int RandomSeed;
		public readonly bool Completed;
		public readonly bool IsPastViewable; //Whether or not the game can be replayed in the interface later
		public readonly List<uint> ModeratorIds;
		//public readonly List<STurn> Turns;
		//TODO: Make a config for things like turns in day etc.
		//public int CurrentTurn { get; private set; }
		//public CurrentTime TimeOfDay { get; private set; }

		public SGame(uint id, string name, string description, uint mapId, SMap map, Dictionary<uint, SDistrict> districts, int? seed, bool isPastViewable, List<uint> moderatorIds)
		{
			if (!seed.HasValue)
				seed = (int)(id + name.ToCharArray().Sum(x => x) + DateTime.UtcNow.Millisecond);

			Id = id;
			Name = name;
			Description = description;
			Started = false;
			MapId = mapId;
			Map = map;
			Districts = districts;
			PlayerCount = districts?.Sum(x => x.Value?.Players?.Count ?? 0) ?? 0;
			Completed = false;
			IsPastViewable = isPastViewable;
			//Turns = new List<STurn>() { new STurn() };
			RandomSeed = seed.Value;
			ModeratorIds = moderatorIds;
		}

		public bool AllTurnsSubmitted
		{
			get
			{
				if (GetAlivePlayers().Count == Turns[CurrentTurn].Actions.Count)
					return true;
				return false;
			}
		}

		public void StartGame()
		{
			Started = true;
		}

		public void AddDistrict(SDistrict dis)
		{
			Districts.Add(dis.Id, dis);
			PlayerCount += dis.Players.Count;
		}

		public Dictionary<uint, SGamePlayer> GetPlayers()
		{
			var ret = new Dictionary<uint, SGamePlayer>();
			foreach (var d in Districts)
			{
				foreach (var p in d.Value.Players)
					ret.Add(p.Key, p.Value);
			}
			return ret;
		}

		public SGamePlayer GetPlayerById(uint id)
		{
			foreach (var d in Districts)
			{
				if (d.Value.Players.ContainsKey(id))
					return d.Value.Players[id];
			}
			return null;
		}

		public Dictionary<uint, SGamePlayer> GetAlivePlayers()
		{
			var ret = new Dictionary<uint, SGamePlayer>();
			foreach (var d in Districts)
			{
				foreach (var p in d.Value.Players)
					if (p.Value.State != SPlayerState.Dead)
						ret.Add(p.Key, p.Value);
			}
			return ret;
		}

		public Dictionary<uint, SGamePlayer> GetDeadPlayers()
		{
			var ret = new Dictionary<uint, SGamePlayer>();
			foreach (var d in Districts)
			{
				foreach (var p in d.Value.Players)
					if(p.Value.State == SPlayerState.Dead)
						ret.Add(p.Key, p.Value);
			}
			return ret;
		}

		public Dictionary<uint, SGamePlayer> GetPlayersAtGrid(Vector2<int> position)
		{
			var ret = new Dictionary<uint, SGamePlayer>();
			foreach (var d in Districts)
			{
				foreach (var p in d.Value.Players)
					if (p.Value.Position == position)
						ret.Add(p.Key, p.Value);
			}
			return ret;
		}

		public Dictionary<uint, SGamePlayer> GetNearPlayers(Vector2<int> position, int range)
		{
			var ret = new Dictionary<uint, SGamePlayer>();
			foreach (var d in Districts)
			{
				foreach (var p in d.Value.Players)
					if (MathHelpers.VectorDistance(position, p.Value.Position) < range)
						ret.Add(p.Key, p.Value);
			}
			return ret;
		}

		public void IncrementTurn()
		{
			Turns.Add(new STurn());
			CurrentTurn++;
		}

		public void FinishGame(bool canView)
		{
			Completed = true;
			IsPastViewable = canView;
		}
	}

	public class SMap
	{
		public Vector2<int> Size;
		public string MapImage;
		public SMapSpace[,] Map;

		public SMap(Vector2<int> size)
		{
			Size = size;
			Map = new SMapSpace[size.X, size.Y];
		}
	}

	public class SMapSpace
	{
		public bool Movable = true;
	}

	public class SAction
	{
		public SActionType Type;
		public SDirection? Direction;
		public uint? Target;
		public bool Result;
	}

	public class STurn
	{
		public ConcurrentDictionary<uint, SAction> Actions;

		public STurn()
		{
			Actions = new ConcurrentDictionary<uint, SAction>();
		}
	}
}
