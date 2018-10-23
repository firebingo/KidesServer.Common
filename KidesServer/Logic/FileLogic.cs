using KidesServer.Helpers;
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
				if (File.Exists(fullPath))
				{
					res.success = false;
					res.message = "FILE_EXISTS";
					res.Files = null;
					return res;
				}
				File.Move(tempFilePath, fullPath);

				var fSplit = newFileName.Split("\\").ToList();
				var fName = fSplit.Last();
				fSplit.RemoveAt(fSplit.Count - 1);

				res.Files = new List<FileResult>
				{
					new FileResult()
					{
						FileName = Path.GetFileName(fullPath),
						Url = $"{baseurl}/api/v1/files/get-file/{fName}{(fSplit.Count != 0 ? $"?directory={string.Join('\\', fSplit)}" : "")}"
					}
				};
				res.success = true;
				res.message = string.Empty;
				return res;
			}
			catch(Exception ex)
			{
				ErrorLog.WriteError(ex);
				return new FileUploadResult()
				{
					success = false,
					message = ex.Message
				};
			}
		}

		public static ListDirectoryResult ListDirectory(FileControllerPerson user, string directory)
		{
			try
			{
				var res = new ListDirectoryResult();

				if (string.IsNullOrWhiteSpace(directory))
					directory = "\\";

				var hasPermission = false;
				var hasSubPermission = false;

				var dir = directory.ToLowerInvariant();
				foreach (var d in user.Directories)
				{
					if (d == "*" || d == "\\*")
					{
						hasPermission = true;
						hasSubPermission = true;
						break;
					}

					var dl = d.ToLowerInvariant();
					if (dl.EndsWith("*"))
					{
						if (dir.Contains(dl.TrimEnd('*').TrimEnd('\\')))
						{
							hasSubPermission = true;
							hasPermission = true;
						}
					}
					else
					{
						if (dir == dl)
							hasPermission = true;
					}
				}

				if (!hasPermission)
					return new ListDirectoryResult() { success = false, message = "USER_NO_PERMISSIONS" };

				var fullPath = $"{AppConfig.Config.FileAccess.RootDirectory}\\{directory}";
				if (hasSubPermission)
					res.Directories = Directory.EnumerateDirectories(fullPath).ToList();
				else
					res.Directories = new List<string>();

				res.Files = Directory.EnumerateFiles(fullPath).ToList();

				return res;
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				return new ListDirectoryResult()
				{
					success = false,
					message = ex.Message
				};
			}
		}

		public static bool CheckDirectoryPermission(FileControllerPerson user, string fileName)
		{
			try
			{
				if (user.Directories.Contains("*") || user.Directories.Contains("\\*"))
					return true;

				var splitPath = fileName.Split('\\').ToList();
				if (splitPath.Count > 1)
				{
					splitPath.RemoveAt(splitPath.Count - 1);
					var dir = string.Join('\\', splitPath).ToLowerInvariant();
					foreach (var d in user.Directories)
					{
						var dl = d.ToLowerInvariant();
						if (dl.EndsWith("*"))
						{
							if (dir.Contains(dl.TrimEnd('*').TrimEnd('\\')))
								return true;
						}
						else
						{
							if (dir == dl)
								return true;
						}
					}
					return false;
				}
				else
				{
					if (!user.Directories.Contains("\\"))
						return false;
				}

				return true;
			}
			catch(Exception ex)
			{
				ErrorLog.WriteError(ex);
				return false;
			}
		}
	}
}
