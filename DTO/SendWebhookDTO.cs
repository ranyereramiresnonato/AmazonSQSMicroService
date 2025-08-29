namespace AmazonSQS.DTO
{
    public class SendWebhookDTO
    {
        public string Url { get; set; } = string.Empty;
        public object Payload { get; set; } = string.Empty;
    }
}
