namespace OpenMessage.AWS.SQS.Configuration
{
    public class SQSConsumerOptions
    {
        public string QueueUrl { get; set; }
        public int MaxNumberOfMessages { get; set; }
        public int WaitTimeSeconds { get; set; }
        public string ServiceURL { get; set; }
        public string RegionEndpoint { get; set; }
        public int? VisibilityTimeout { get; set; }
    }
}