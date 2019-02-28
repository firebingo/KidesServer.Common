using KidesServer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Symphogames.Models
{
	[Serializable]
	public class JoinGameResult : BaseResult
	{
		public uint GameId;
		public uint PlayerId;
	}

	[Serializable]
	public class CurrentGamePlayers : BaseResult
	{
		public List<SPlayerInfo> Players;
	}

	[Serializable]
	public class CurrentGamePlayerInfo : BaseResult
	{
		public SGameInfo GameInfo;
		public SMapInfo MapInfo;
		public SPlayersInfo PlayerInfo;
		public List<SActionInfo> ActionInfo;
	}

	[Serializable]
	public class AuthenticateResult : BaseResult
	{
		public User User;
	}

	[Serializable]
	public class SGameInfo
	{
		public uint Id;
		public bool Started;
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
		public SPlayerInfo ThisPlayer;
		public List<SPlayerInfo> DeadPlayers;
		public List<SPlayerInfo> Players;
	}

	[Serializable]
	public class SPlayerInfo
	{
		public uint Id;
		public bool? HasSubmittedTurn;
		public string Name;
		public string Avatar;
		public uint DistrictId;
		public Vector2<int> Position;
		public SPlayerState State;
		public double? Health;
		public double? MaxHealth;
		public double? Energy;
		public double? MaxEnergy;
		public int Kills;
		public double Range; //How far this player is from current player
	}

	[Serializable]
	public class SActionInfo
	{
		public SActionType Type;
		public SDirection? Direction;
		public uint? Target;
		public string ActionName;
	}
}
