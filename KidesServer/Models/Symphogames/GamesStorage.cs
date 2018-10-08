using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KidesServer.Symphogames
{
	public static class SymphogamesStorage
	{
		public static readonly Dictionary<uint, SPlayer> Players;
		public static SGame CurrentGame { get; private set; }

		static SymphogamesStorage()
		{
			Players = new Dictionary<uint, SPlayer>();
		}

		public static void StartGame(int width, int height)
		{
			CurrentGame = new SGame(width, height);
		}
	}
}
