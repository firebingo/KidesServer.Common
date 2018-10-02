using System.Net;
using System.Net.Http;
using System.Web.Http;
using KidesServer.Models;
using KidesServer.Logic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;

namespace KidesServer.Controllers
{
	[Route("api/v1/discord")]
	[ApiController]
	public class DiscordBotController : ControllerBase
	{
		[HttpGet, Route("message-count/list")]
		public async Task<IActionResult> getMessageList([FromQuery]int count, [FromQuery]ulong serverId, [FromQuery]int start, [FromQuery]DateTime? startDate = null, [FromQuery]MessageSort sort = MessageSort.messageCount,
			[FromQuery]bool isDesc = true, [FromQuery]string userFilter = "", [FromQuery]ulong? roleId = null, [FromQuery]bool includeTotal = false)
		{
			var input = new DiscordMessageListInput(count, serverId, start, (startDate.HasValue ? startDate : null), sort, isDesc, userFilter, roleId, includeTotal);
			var result = DiscordBotLogic.getMessageList(input);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result.message);
		}

		[HttpGet, Route("user-info")]
		public async Task<IActionResult> getUserInfo([FromQuery]ulong userId, [FromQuery]ulong serverId)
		{
			var result = DiscordBotLogic.getUserInfo(userId, serverId);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result.message);
		}

		[HttpGet, Route("roles")]
		public async Task<IActionResult> getRoles([FromQuery]ulong serverId)
		{
			var result = DiscordBotLogic.getRoleList(serverId);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result.message);
		}

		[HttpGet, Route("emoji-count/list")]
		public async Task<IActionResult> getEmojiList([FromQuery]int count, [FromQuery]ulong serverId, [FromQuery]int start, [FromQuery]DateTime? startDate = null, [FromQuery]EmojiSort sort = EmojiSort.emojiCount,
			[FromQuery]bool isDesc = true, [FromQuery]string nameFilter = "", [FromQuery]bool includeTotal = false, [FromQuery]ulong? userFilterId = null)
		{
			var input = new DiscordEmojiListInput(count, serverId, start, (startDate.HasValue ? startDate.Value : DateTime.MinValue), sort, isDesc, nameFilter, includeTotal, userFilterId);
			var result = DiscordBotLogic.getEmojiList(input);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result.message);
		}

		[HttpGet, Route("word-count/list")]
		public async Task<IActionResult> getWordCountList([FromQuery]int count, [FromQuery]ulong serverId, [FromQuery]int start, [FromQuery]DateTime? startDate = null, [FromQuery]WordCountSort sort = WordCountSort.count,
			[FromQuery]bool isDesc = true, [FromQuery]string wordFilter = "", [FromQuery]bool includeTotal = false, [FromQuery]ulong? userFilterId = null, int lengthFloor = 0, bool englishOnly = false)
		{
			var input = new DiscordWordListInput(count, serverId, start, startDate, sort, isDesc, wordFilter, includeTotal, userFilterId, lengthFloor, englishOnly);
			var result = await DiscordBotLogic.getWordCountList(input);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result.message);
		}

		[HttpGet, Route("stats")]
		public async Task<IActionResult> getServerStats([FromQuery]ulong serverId, [FromQuery]DateTime startDate, [FromQuery]StatType type, [FromQuery]DateGroup dateGroup = DateGroup.day, [FromQuery]DateTime? endDate = null)
		{
			var input = new DiscordStatListInput(startDate, endDate ?? DateTime.UtcNow, type, dateGroup, serverId);
			var result = await DiscordBotLogic.getServerStats(input);

			if (result.success)
				return Ok(result);
			else
				return BadRequest(result.message);
		}
	}
}