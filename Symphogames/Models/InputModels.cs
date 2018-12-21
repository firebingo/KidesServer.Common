using KidesServer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Symphogames.Models
{
	public class CreateGameInput
	{
		public string GameName;
		public string MapImage;
		public Vector2<int> Size;
		public List<DistrictInput> Districts;
	}

	public class DistrictInput
	{
		public string Name;
		public List<uint> PlayerIds;
	}
}
