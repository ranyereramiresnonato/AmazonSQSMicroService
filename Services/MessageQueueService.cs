using Amazon.SQS;
using Amazon.SQS.Model;

namespace AmazonSQS.Services
{
    public class MessageQueueService : IMessageQueueService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueName;
        private string? _queueUrl;

        public MessageQueueService(IAmazonSQS sqsClient, IConfiguration config)
        {
            _sqsClient = sqsClient;
            _queueName = config["SQS:QueueName"] ?? "webhooks";
        }

        private async Task EnsureQueueExistsAsync()
        {
            if (_queueUrl != null) return;

            try
            {
                var response = await _sqsClient.GetQueueUrlAsync(_queueName);
                _queueUrl = response.QueueUrl;
            }
            catch (QueueDoesNotExistException)
            {
                var createResponse = await _sqsClient.CreateQueueAsync(new CreateQueueRequest
                {
                    QueueName = _queueName
                });
                _queueUrl = createResponse.QueueUrl;
            }
        }

        public async Task EnqueueAsync(string message)
        {
            await EnsureQueueExistsAsync();

            await _sqsClient.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = _queueUrl!,
                MessageBody = message
            });
        }
    }
}
