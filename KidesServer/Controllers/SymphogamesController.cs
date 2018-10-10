using KidesServer.Models;
using KidesServer.Symphogames;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KidesServer.Controllers
{
	[Route("api/v1/discord")]
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
				return BadRequest(result.message);
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
				return BadRequest(result.message);
		}

		[Returns(typeof(CurrentGamePlayerInfo))]
		[HttpGet, Route("current-player-game-info")]
		public async Task<IActionResult> GetCurrentPlayerInfo([FromQuery]uint gameId, [FromQuery]uint userId, [FromQuery]string accessguid)
		{
			var result = await GamesLogic.GetCurrentPlayerInfo(gameId, userId, accessguid);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result.message);
		}
	}
}
