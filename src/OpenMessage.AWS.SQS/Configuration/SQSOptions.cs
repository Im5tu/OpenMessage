using System;
using Amazon.SQS;

namespace OpenMessage.AWS.SQS.Configuration
{
    /// <summary>
    /// Options for an SQS consumer
    /// </summary>
    public class SQSConsumerOptions
    {
        /// <summary>
        /// The url of the queue to consume from
        /// </summary>
        public string QueueUrl { get; set; }

        /// <summary>
        /// The maximum number of messages to consume
        /// </summary>
        public int MaxNumberOfMessages { get; set; } = 10;

        /// <summary>
        /// The waiting period before return the messages, in seconds
        /// </summary>
        public int WaitTimeSeconds { get; set; }

        /// <summary>
        /// The service url to use
        /// </summary>
        public string ServiceURL { get; set; }

        /// <summary>
        /// The region endpoint to use
        /// </summary>
        public string RegionEndpoint { get; set; }

        /// <summary>
        /// How long to leave the message on the queue before it becomes consumable again
        /// </summary>
        public int? VisibilityTimeout { get; set; }

        /// <summary>
        /// Allow the configuration of the raw AWS SQS Client Config during initialization of the consumer.
        /// </summary>
        public Action<AmazonSQSConfig>? AwsConsumerConfiguration { get; set; }
    }
}