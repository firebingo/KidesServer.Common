using KidesServer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KidesServer.Controllers
{
	[Route("api/v1/files")]
	[ApiController]
	public class FileController : ControllerBase
	{
		private static readonly FormOptions _defaultFormOptions = new FormOptions();

		[HttpPost, Route("upload-file")]
		[DisableFormValueModelBinding]
		[Authorize]
		public async Task<IActionResult> UploadFile([FromBody] string fileName)
		{
			var fileUser = AppConfig.Config.FileAccess.People[User.Identity.Name];
			if (fileUser == null || !fileUser.Upload)
				return BadRequest(new BaseResult() { message = "User not allowed to upload", success = false });

			var splitPath = fileName.Split('\\').ToList();
			if (splitPath.Count > 1)
			{
				splitPath.RemoveAt(splitPath.Count - 1);
				var dir = string.Join('\\', splitPath);
				if(!fileUser.Directories.Contains(dir))
					return BadRequest(new BaseResult() { message = "Invalid path", success = false });
			}
			else
			{
				if (!fileUser.Directories.Contains("\\"))
					return BadRequest(new BaseResult() { message = "Invalid path", success = false });
			}

			if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
				return BadRequest(new BaseResult() { message = $"Expected a multipart request, but got {Request.ContentType}", success = false });

			var formAccumulator = new KeyValueAccumulator();
			string targetFilePath = null;

			var boundary = MultipartRequestHelper.GetBoundary(
				MediaTypeHeaderValue.Parse(Request.ContentType),
				_defaultFormOptions.MultipartBoundaryLengthLimit);
			var reader = new MultipartReader(boundary, HttpContext.Request.Body);

			var section = await reader.ReadNextSectionAsync();
			while (section != null)
			{
				ContentDispositionHeaderValue contentDisposition;
				var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

				if (hasContentDispositionHeader)
				{
					if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
					{
						targetFilePath = $"{AppDomain.CurrentDomain.GetData("DataDirectory").ToString()}\\Temp\\{Guid.NewGuid().ToString("n")}";
						using (var targetStream = System.IO.File.Create(targetFilePath))
						{
							await section.Body.CopyToAsync(targetStream);
						}
					}
					else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
					{
						// Content-Disposition: form-data; name="key"
						//
						// value

						// Do not limit the key name length here because the 
						// multipart headers length limit is already in effect.
						var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).ToString();
						var encoding = GetEncoding(section);
						using (var streamReader = new StreamReader(
							section.Body,
							encoding,
							detectEncodingFromByteOrderMarks: true,
							bufferSize: 1024,
							leaveOpen: true))
						{
							// The value length limit is enforced by MultipartBodyLengthLimit
							var value = await streamReader.ReadToEndAsync();
							if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
							{
								value = String.Empty;
							}
							formAccumulator.Append(key, value);

							if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
							{
								throw new InvalidDataException($"Form key count limit {_defaultFormOptions.ValueCountLimit} exceeded.");
							}
						}
					}
				}

				// Drains any remaining section body that has not been consumed and
				// reads the headers for the next section.
				section = await reader.ReadNextSectionAsync();
			}

			var res = Logic.FileLogic.UploadFile(fileUser, targetFilePath, fileName, $"{Request.Scheme}://{Request.Host}{Request.PathBase}");
			if (res.success)
				return Ok(res);
			return BadRequest(res);
		}

		[HttpGet, Route("get-file")]
		[Authorize]
		public async Task<IActionResult> GetFile([FromQuery]string fileName)
		{
			var fileUser = AppConfig.Config.FileAccess.People[User.Identity.Name];
			if (fileUser == null || !fileUser.Download)
				return BadRequest(new BaseResult() { message = "User not allowed to download", success = false });

			var fullPath = $"{AppConfig.Config.FileAccess.RootDirectory}\\{fileName}";
			if(!System.IO.File.Exists(fullPath))
				return BadRequest(new BaseResult { success = false, message = "FILE_NOT_EXIST" });
			new FileExtensionContentTypeProvider().TryGetContentType(fullPath, out var contentType);

			return PhysicalFile(fullPath, contentType);
		}

		private static Encoding GetEncoding(MultipartSection section)
		{
			MediaTypeHeaderValue mediaType;
			var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
			// UTF-7 is insecure and should not be honored. UTF-8 will succeed in 
			// most cases.
			if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
			{
				return Encoding.UTF8;
			}
			return mediaType.Encoding;
		}
	}
}
