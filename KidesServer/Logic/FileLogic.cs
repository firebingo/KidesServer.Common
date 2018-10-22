using KidesServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KidesServer.Logic
{
	public static class FileLogic
	{
		public static FileUploadResult UploadFile(FileControllerPerson user, string tempFilePath, string newFileName, string baseurl)
		{
			try
			{
				var res = new FileUploadResult();

				var fullPath = $"{AppConfig.Config.FileAccess.RootDirectory}\\{newFileName}";
				File.Move(tempFilePath, newFileName);
				res.Files = new List<FileResult>
				{
					new FileResult()
					{
						FileName = Path.GetFileName(fullPath),
						Url = "https://server.icebingo.io/api/v1/get-file"
					}
				};
				res.success = true;
				res.message = string.Empty;
				return res;
			}
			catch(Exception ex)
			{
				return new FileUploadResult()
				{
					success = false,
					message = ex.Message
				};
			}
		}
	}
}
