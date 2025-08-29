using System.Text;
using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using AmazonSQS.DTO;

public class SqsConsumerService : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string _queueUrl;
    private readonly HttpClient _httpClient;

    public SqsConsumerService(IAmazonSQS sqsClient, IConfiguration config, HttpClient httpClient)
    {
        _sqsClient = sqsClient;
        _httpClient = httpClient;

        var queueName = config["SQS:QueueName"] ?? "webhooks";

        try
        {
            var queueUrlResponse = _sqsClient.GetQueueUrlAsync(queueName).GetAwaiter().GetResult();
            _queueUrl = queueUrlResponse.QueueUrl;
        }
        catch (QueueDoesNotExistException)
        {
            var createQueueRequest = new CreateQueueRequest
            {
                QueueName = queueName,
                Attributes = new Dictionary<string, string>
            {
                { "DelaySeconds", "0" },
                { "MessageRetentionPeriod", "345600" },
                { "VisibilityTimeout", "30" }
            }
            };

            var createQueueResponse = _sqsClient.CreateQueueAsync(createQueueRequest).GetAwaiter().GetResult();
            _queueUrl = createQueueResponse.QueueUrl;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var receiveResponse = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 5
            }, stoppingToken);

            if (receiveResponse.Messages.Count == 0) continue;

            foreach (var message in receiveResponse.Messages)
            {
                try
                {
                    var webhook = JsonSerializer.Deserialize<SendWebhookDTO>(message.Body);

                    if (webhook != null && !string.IsNullOrEmpty(webhook.Url))
                    {
                        var jsonContent = JsonSerializer.Serialize(webhook.Payload);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                        await _httpClient.PostAsync(webhook.Url, content, stoppingToken);
                    }

                    await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                }
                catch
                {
                    // Ignora erros para não travar o loop
                }
            }
        }
    }
}
