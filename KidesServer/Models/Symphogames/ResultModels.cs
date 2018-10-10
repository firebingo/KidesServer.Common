using KidesServer.Models;
using KidesServer.Symphogames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KidesServer.Symphogames
{
	[Serializable]
	public class CurrentGamePlayerInfo : BaseResult
	{
		public SGameInfo GameInfo;
		public SMapInfo MapInfo;
		public SPlayersInfo PlayerInfo;
	}

	[Serializable]
	public class SGameInfo
	{
		public uint Id;
		public int CurrentTurn;
		public CurrentTime TimeOfDay;
		public bool Completed;
	}

	[Serializable]
	public class SMapInfo
	{
		public SMap Map;
	}

	[Serializable]
	public class SPlayersInfo
	{
		public List<SPlayerInfo> DeadPlayers;
		public List<SPlayerInfo> Players;
	}

	[Serializable]
	public class SPlayerInfo
	{
		public uint Id;
		public string Name;
		public uint DistrictId;
		public Vector2<int> Position;
		public SPlayerState State;
		public int Kills;
	}
}
