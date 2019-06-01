using KidesServer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Symphogames;
using Symphogames.Logic;
using Symphogames.Models;
using Symphogames.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KidesServer.Controllers
{
	[Route("api/v1/symphogames")]
	[ApiController]
	public class SymphogamesController : ControllerBase
	{
		private readonly AppSettings _appSettings;
		private readonly PlayerService _playerService;
		private readonly GameService _gameService;

		public SymphogamesController(IOptions<AppSettings> appSettings,
			PlayerService playerService,
			GameService gameService)
		{
			_playerService = playerService;
			_gameService = gameService;
			_appSettings = appSettings.Value;
		}

		[Returns(typeof(UIntResult))]
		[Authorize]
		[UserRoleCheck(PlayerRole.Admin)]
		[HttpPost, Route("create-player")]
		public async Task<IActionResult> CreatePlayer([FromQuery]string playerName)
		{
			var result = await _playerService.CreatePlayer(playerName);
			
			if (result.success)
				return Ok(result);
			else
				return BadRequest(result);
		}

		//[Returns(typeof(UIntResult))]
		//[HttpGet, Route("list-players")]
		//public async Task<IActionResult> ListPlayers([FromQuery]string playerName)
		//{
		//	var result = await GamesLogic.CreatePlayer(playerName);
		//
		//	if (result.success)
		//		return Ok(result);
		//	else
		//		return BadRequest(result.message);
		//}

		[Returns(typeof(UIntResult))]
		[Authorize]
		[UserRoleCheck(PlayerRole.Moderator)]
		[HttpPost, Route("create-game")]
		public async Task<IActionResult> CreateGame([FromBody]CreateGameInput input)
		{
			var result = await _gameService.CreateGame(input);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result);
		}

		[Returns(typeof(UIntResult))]
		[Authorize]
		[UserRoleCheck(PlayerRole.Moderator)]
		[HttpPost, Route("start-game")]
		public async Task<IActionResult> StartGame([FromBody]CreateGameInput input)
		{
			var result = await GamesLogic.StartGame(input);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result);
		}

		[Returns(typeof(JoinGameResult))]
		[Authorize]
		[UserRoleCheck(PlayerRole.Player)]
		[HttpGet, Route("join-game")]
		public async Task<IActionResult> Join([FromQuery]uint gameId, [FromQuery]uint playerId)
		{
			var result = await GamesLogic.UserJoinGame(gameId, playerId);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result);
		}

		[Returns(typeof(CurrentGamePlayerInfo))]
		[Authorize]
		[UserRoleCheck(PlayerRole.Player)]
		[HttpGet, Route("current-player-game-info")]
		public async Task<IActionResult> GetCurrentPlayerInfo([FromQuery]uint gameId)
		{
			var claim = User.Identity as ClaimsIdentity;
			var pId = uint.Parse(claim.FindFirst(ClaimTypes.Name).Value);
			var result = await GamesLogic.GetCurrentPlayerInfo(gameId, pId);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result);
		}

		[Returns(typeof(BaseResult))]
		[Authorize]
		[UserRoleCheck(PlayerRole.Player)]
		[HttpPost, Route("submit-turn")]
		public async Task<IActionResult> SubmitTurn([FromQuery]uint gameId, [FromBody]SActionInfo action)
		{
			var claim = User.Identity as ClaimsIdentity;
			var pId = uint.Parse(claim.FindFirst(ClaimTypes.Name).Value);
			var result = await GamesLogic.SubmitTurn(gameId, pId, action);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result);
		}

		[Returns(typeof(PhysicalFileResult))]
		[Authorize]
		[UserRoleCheck(PlayerRole.Unknown)]
		[HttpGet, Route("image")]
		public IActionResult GetImage([FromQuery]SImageType type, [FromQuery]string name)
		{
			var path = "Avatars";
			var ext = ".png";
			var mime = "image/png";
			if (type == SImageType.Map)
			{
				ext = ".jpg";
				mime = "image/jpeg";
				path = "Maps";
			}
			var filePath = $"{AppDomain.CurrentDomain.GetData("DataDirectory").ToString()}\\Images\\Symphogames\\{path}\\{name}{ext}";
			if (!System.IO.File.Exists(filePath))
				return BadRequest(new BaseResult { success = false, message = "FILE_NOT_EXIST" });
			return PhysicalFile(filePath, mime);
		}
	}
}
