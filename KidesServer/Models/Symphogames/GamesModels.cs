using KidesServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace KidesServer.Symphogames
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

	public class SGame
	{
		public Guid Id;
		public readonly SMap Map;
		public Dictionary<uint, SDistrict> Districts;
		public List<STurn> Turns;
		public int CurrentTurn { get; set; }
		public CurrentTime TimeOfDay { get; set; }

		public SGame(int width, int height)
		{
			Id = Guid.NewGuid();
			Map = new SMap(new Vector2<int>(width, height));
			Districts = new Dictionary<uint, SDistrict>();
			CurrentTurn = 0;
			TimeOfDay = CurrentTime.Day;
		}

		public void AddDistrict(SDistrict dis)
		{
			Districts.Add(dis.Id, dis);
		}
	}

	public class SMap
	{
		public Vector2<int> Size;
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
		public Dictionary<uint, SAction> Actions;
	}
}
