﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KidesServer.Models;

namespace KidesServer.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Root()
		{
			return View();
		}

		public IActionResult Symphogames()
		{
			return View("symphogames-react");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}