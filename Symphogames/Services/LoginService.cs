using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Symphogames.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Symphogames.Services
{
	public class LoginService
	{
		private readonly AppSettings _appSettings;
		private readonly PlayerService _playerService;

		public LoginService(IOptions<AppSettings> appSettings,
			PlayerService playerService)
		{
			_playerService = playerService;
			_appSettings = appSettings.Value;
		}

		public async Task<AuthenticateResult> Authenticate(AuthenticateInput authInput)
		{
			var result = new AuthenticateResult
			{
				User = new User()
			};
			var player = await _playerService.GetPlayerByName(authInput.Username);

			if (player == null)
				return new AuthenticateResult() { success = false, message = "USER_NOT_EXIST" };

			var loginResult = await _playerService.CheckPlayerLogin(player.Id, authInput.Password);
			if (loginResult.success)
				return new AuthenticateResult() { success = false, message = loginResult.message };

			result.User.Username = player.Name;
			result.User.Id = player.Id;

			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(_appSettings.Security.JwtKey);
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
	}
}
