using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Symphogames.Helpers;

namespace Symphogames.Models
{
	public class User
	{
		public uint Id;
		public string Username;
		public string Token;
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class UserRoleCheckAttribute : ActionFilterAttribute
	{
		private PlayerRole _role = PlayerRole.Unknown;

		public UserRoleCheckAttribute(PlayerRole role)
		{
			_role = role;
		}

		public override async Task OnActionExecutionAsync(ActionExecutingContext filterContext, ActionExecutionDelegate next)
		{
			try
			{
				if (!(filterContext.HttpContext.User?.Identity is ClaimsIdentity claim))
				{
					filterContext.Result = new StatusCodeResult(401);
					return;
				}

				if (!Enum.TryParse<PlayerRole>(claim.FindFirst(ClaimTypes.Role).Value, true, out var role))
				{
					filterContext.Result = new StatusCodeResult(401);
					return;
				}

				if (role < _role)
				{
					filterContext.Result = new StatusCodeResult(403);
					return;
				}
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				filterContext.Result = new StatusCodeResult(403);
				return;
			}

			await next();
		}
	}
}
