namespace OpenMessage.AWS.SQS.Configuration
{
    /// <summary>
    /// Configuration options for dispatchers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SQSDispatcherOptions<T>
    {
        /// <summary>
        /// The queue url to dispatch to
        /// </summary>
        public string QueueUrl { get; set; }

        /// <summary>
        /// The url to use for authentication
        /// </summary>
        public string ServiceURL { get; set; }

        /// <summary>
        /// The region endpoint to use
        /// </summary>
        public string RegionEndpoint { get; set; }
    }
}