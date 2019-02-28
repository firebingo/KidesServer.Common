using KidesServer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Symphogames.Logic;
using Symphogames.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Symphogames.Controllers
{
	[Authorize]
	[Route("api/v1/login")]
	[ApiController]
	public class LoginController : ControllerBase
	{
		[Returns(typeof(AuthenticateResult))]
		[AllowAnonymous]
		[HttpPost, Route("authenticate")]
		public async Task<IActionResult> Authenticate([FromBody]AuthenticateInput authInput)
		{
			var result = await LoginLogic.Authenticate(authInput);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result);
		}
	}
}
