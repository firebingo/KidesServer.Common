using KidesServer.Common;
using Microsoft.AspNetCore.Mvc;
using Symphogames.Logic;
using Symphogames.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KidesServer.Controllers
{
	[Route("api/v1/symphogames")]
	[ApiController]
	public class SymphogamesController : ControllerBase
	{
		[Returns(typeof(UIntResult))]
		[HttpPost, Route("create-player")]
		public async Task<IActionResult> CreatePlayer([FromQuery]string playerName)
		{
			var result = await GamesLogic.CreatePlayer(playerName);
			
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
		[HttpPost, Route("start-game")]
		public async Task<IActionResult> StartGame([FromBody] StartGameInput input)
		{
			var result = await GamesLogic.StartGame(input);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result);
		}

		[Returns(typeof(JoinGameResult))]
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
		[HttpGet, Route("current-player-game-info")]
		public async Task<IActionResult> GetCurrentPlayerInfo([FromQuery]uint gameId, [FromQuery]uint playerId, [FromQuery]string accessguid)
		{
			var result = await GamesLogic.GetCurrentPlayerInfo(gameId, playerId, accessguid);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result);
		}

		[Returns(typeof(PhysicalFileResult))]
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
