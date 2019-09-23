using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KidesServer.Common.MessageQueue
{
	public class AwsMessageClient : IMessageClient
	{
		public readonly AmazonSQSClient _sqsClient;
		public AwsMessageClient(string serviceUrl, string accessKey, string secretKey)
		{
			var credentials = new BasicAWSCredentials(accessKey, secretKey);
			var config = new AmazonSQSConfig()
			{
				ServiceURL = serviceUrl
			};
			_sqsClient = new AmazonSQSClient(credentials, config);
		}

		public async Task<ReceiveMessageResponse> ReceiveMessageAsync(string queueUrl)
		{
			try
			{
				var receiveMessageRequest = new ReceiveMessageRequest()
				{
					QueueUrl = queueUrl
				};

				var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest);

				if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
				{
					return new ReceiveMessageResponse()
					{
						StatusCode = response.HttpStatusCode
					};
				}

				var messages = new ReceiveMessageResponse()
				{
					StatusCode = response.HttpStatusCode,
					Messages = new List<ReceiveMessage>()
				};
				
				foreach (var mes in response.Messages)
				{
					try
					{
						messages.Messages.Add(new ReceiveMessage()
						{
							Body = mes.Body,
							MessageId = mes.MessageId,
							ReceiptHandle = mes.ReceiptHandle,
							DedupeId = long.Parse(mes.Attributes["MessageDeduplicationId"]),
							TimeStamp = mes.Attributes.TryGetValue("SentTimestamp", out var value) ? DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(value)).UtcDateTime : DateTime.MinValue,
							Target = mes.MessageAttributes["Target"].StringValue,
							Type = mes.MessageAttributes["Type"].StringValue,
							Success = true,
							ErrorMsg = string.Empty
						});
					}
					catch (Exception ex)
					{
						messages.Messages.Add(new ReceiveMessage()
						{
							Body = mes.Body,
							MessageId = mes.MessageId,
							ReceiptHandle = mes.ReceiptHandle,
							Success = false,
							ErrorMsg = $"Error parsing message: {ex.Message}"
						});
					}
				}
				messages.Messages = messages.Messages.GroupBy(x => x.DedupeId).Select(x => x.First()).ToList();

				return messages;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public async Task<SendMessageResponse> SendMessageAsync(string queueUrl, string message, string target, string type)
		{
			try
			{
				var request = new SendMessageRequest(queueUrl, message)
				{
					MessageAttributes = new Dictionary<string, MessageAttributeValue>()
					{
						{
							"Target", new MessageAttributeValue()
							{
								DataType = "String",
								StringValue = target
							}
						},
						{
							"Type", new MessageAttributeValue()
							{
								DataType = "String",
								StringValue = type
							}
						}
					}
				};

				var response = await _sqsClient.SendMessageAsync(request);
				return new SendMessageResponse()
				{
					StatusCode = response.HttpStatusCode
				};
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public async Task<DeleteMessageResponse> DeleteMessageAsync(string queueUrl, string receiptHandle)
		{
			try
			{
				var request = new DeleteMessageRequest(queueUrl, receiptHandle);
				var response = await _sqsClient.DeleteMessageAsync(request);
				return new DeleteMessageResponse()
				{
					StatusCode = response.HttpStatusCode
				};
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}
}
