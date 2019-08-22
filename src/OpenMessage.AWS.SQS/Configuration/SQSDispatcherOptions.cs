namespace OpenMessage.AWS.SQS.Configuration
{
    public class SQSDispatcherOptions<T>
    {
        public string QueueUrl { get; set; }
        public string ServiceURL { get; set; }
        public string RegionEndpoint { get; set; }
    }
}