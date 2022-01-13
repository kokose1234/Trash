using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using TrashClient.Data;

namespace TrashClient.Manager
{
    internal class SqsManager
    {
        private static readonly Lazy<SqsManager> Lazy = new(() => new SqsManager());
        private readonly AmazonSQSClient sqsClient;

        internal static SqsManager Instance => Lazy.Value;

        private SqsManager() =>
            sqsClient = new AmazonSQSClient(new BasicAWSCredentials(Setting.Value.AwsAccessKeyId, Setting.Value.AwsSecretAccessKey), RegionEndpoint.APNortheast2);

        internal async Task DeleteMessageAsync(Message message) => await sqsClient.DeleteMessageAsync(Setting.Value.QueueUrl, message.ReceiptHandle);

        internal async Task<ReceiveMessageResponse> GetMessageAsync()
        {
            return await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = Setting.Value.QueueUrl,
                WaitTimeSeconds = 0,
                MaxNumberOfMessages = 1
            });
        }
    }
}