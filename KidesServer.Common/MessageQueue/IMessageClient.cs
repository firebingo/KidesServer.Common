using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KidesServer.Common.MessageQueue
{
	public interface IMessageClient
	{
		Task<ReceiveMessageResponse> ReceiveMessageAsync(string queueUrl);
		Task<SendMessageResponse> SendMessageAsync(string queueUrl, string message, string target, string type);
		Task<DeleteMessageResponse> DeleteMessageAsync(string queueUrl, string receiptHandle);
	}
}
