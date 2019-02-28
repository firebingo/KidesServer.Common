using Microsoft.IdentityModel.Tokens;
using Symphogames.Helpers;
using Symphogames.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Symphogames.Logic
{
	public static class LoginLogic
	{
		public static async Task<AuthenticateResult> Authenticate(AuthenticateInput authInput)
		{
			var result = new AuthenticateResult
			{
				User = new User()
			};
			var player = await GamesDb.GetPlayerByName(authInput.Username);

			if (player == null || !await player.CheckLogin(authInput.Password))
				return null;

			result.User.Username = player.Name;
			result.User.Id = player.Id;

			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes((await SymphogamesConfig.GetConfig()).JwtKey);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
				{
					new Claim(ClaimTypes.Name, player.Id.ToString()),
					new Claim(ClaimTypes.Role, player.Role.ToString())
				}),
				IssuedAt = DateTime.UtcNow,
				Expires = DateTime.UtcNow.AddDays(1),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			result.User.Token = tokenHandler.WriteToken(token);

			return result;
		}

		//public static async 
	}
}
