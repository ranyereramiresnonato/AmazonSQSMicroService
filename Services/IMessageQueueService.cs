namespace AmazonSQS.Services
{
    public interface IMessageQueueService
    {
        Task EnqueueAsync(string message);
    }
}
