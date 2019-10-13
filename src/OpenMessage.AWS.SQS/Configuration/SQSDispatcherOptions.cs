using System;
using Amazon.SQS;

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

        /// <summary>
        /// Allow the configuration of the raw AWS SQS Dispatcher Config during initialization of the dispatcher.
        /// </summary>
        public Action<AmazonSQSConfig> AwsDispatcherConfiguration { get; set; }
    }
}