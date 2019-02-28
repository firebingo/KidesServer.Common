﻿using KidesServer.Common;

namespace Symphogames.Models
{
    public class SymphogamesConfigModel
    {
        public string HashPepper = "478ab";
		public string JwtKey = "8hgbi";
		public int GameTickMs = 2500;
		public int ConfigExpireMs = 1000 * 60 * 15;
    }
}