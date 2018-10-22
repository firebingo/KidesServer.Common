using KidesServer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KidesServer.Models
{
	public class FileUploadResult : BaseResult
	{
		public List<FileResult> Files;
	}

	public class FileResult
	{
		public string FileName;
		public string Url;
	}
}
