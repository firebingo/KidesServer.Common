using KidesServer.Logic;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace KidesServer.Controllers
{
	[Route("api/v1")]
	[ApiController]
	public class MusicController : ControllerBase
	{
		[HttpGet, Route("song-url")]
		public async Task<IActionResult> getSongUrl([FromQuery]string searchString)
		{
			try
			{
				var result = MusicLogic.searchForSong(searchString);

				if (result.success)
					return Ok(result);
				else
					return BadRequest(result.message);
			}
			catch(Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}

		//[HttpGet, Route("song-stats")]
		//public async Task<IActionResult> getSongStats()
		//{
		//	var result = MusicLogic.getSongStats();
		//
		//	if (result.success)
		//		return Ok(result);
		//	else
		//		return BadRequest(result.message);
		//}
	}
}