using KidesServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KidesServer.Symphogames
{
	public class StartGameInput
	{
		public string GameName;
		public Vector2<int> Size;
		public List<DistrictInput> Districts;
	}

	public class DistrictInput
	{
		public string Name;
		public List<uint> PlayerIds;
	}
}
