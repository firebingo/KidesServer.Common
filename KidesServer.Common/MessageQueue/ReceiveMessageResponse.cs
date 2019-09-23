using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace KidesServer.Common.MessageQueue
{
	public class ReceiveMessageResponse
	{
		public HttpStatusCode StatusCode { get; set; }
		public List<ReceiveMessage> Messages { get; set; }
	}

	public class ReceiveMessage
	{
		public bool Success { get; set; }
		public string ErrorMsg { get; set; }
		public string Body { get; set; }
		public string MessageId { get; set; }
		public string ReceiptHandle { get; set; }
		public long DedupeId { get; set; }
		public DateTime TimeStamp { get; set; }
		public string Target { get; set; }
		public string Type { get; set; }
	}
}
