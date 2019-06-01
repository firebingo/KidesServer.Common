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
		public string GameDescription;
		public string MapImage;
		public uint MapId;
		public bool IsPastViewable;
		public List<DistrictInput> Districts;
		public int? Seed;
		public List<uint> ModeratorIds;
	}

	public class DistrictInput
	{
		public string Name;
		public List<uint> PlayerIds;
	}

	public class AuthenticateInput
	{
		public string Username;
		public string Password;
	}
}
