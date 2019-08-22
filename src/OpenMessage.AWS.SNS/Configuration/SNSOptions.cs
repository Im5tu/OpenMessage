namespace OpenMessage.AWS.SNS.Configuration
{
    public class SNSOptions<T>
    {
        public string ServiceURL { get; set; }
        public string RegionEndpoint { get; set; }
        public string TopicArn { get; set; }
    }
}