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
		[HttpPost, Route("create-player")]
		public async Task<IActionResult> CreatePlayer([FromQuery]string playerName)
		{
			var result = await GamesLogic.CreatePlayer(playerName);
			
			if (result.success)
				return Ok(result);
			else
				return BadRequest(result.message);
		}

		[HttpPost, Route("start-game")]
		public async Task<IActionResult> StartGame([FromQuery]string playerName)
		{
			var result = await GamesLogic.CreatePlayer(playerName);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result.message);
		}
	}
}
