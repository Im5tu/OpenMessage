using Amazon.SQS;
using System;
using System.Collections.Generic;

namespace OpenMessage.AWS.SQS.Configuration
{
    /// <summary>
    ///     Options for an SQS consumer
    /// </summary>
    public class SQSConsumerOptions
    {
        /// <summary>
        ///     Allow the configuration of the raw AWS SQS Client Config during initialization of the consumer.
        /// </summary>
        public Action<AmazonSQSConfig>? AwsConsumerConfiguration { get; set; }

        /// <summary>
        ///     The maximum number of messages to consume
        /// </summary>
        public int MaxNumberOfMessages { get; set; } = 10;

        /// <summary>
        ///     The url of the queue to consume from
        /// </summary>
        public string? QueueUrl { get; set; }

        /// <summary>
        ///     The region endpoint to use
        /// </summary>
        public string? RegionEndpoint { get; set; }

        /// <summary>
        ///     The service url to use
        /// </summary>
        public string? ServiceURL { get; set; }

        /// <summary>
        ///     How long to leave the message on the queue before it becomes consumable again
        /// </summary>
        public int? VisibilityTimeout { get; set; }

        /// <summary>
        ///     The waiting period before return the messages, in seconds
        /// </summary>
        public int WaitTimeSeconds { get; set; }

        /// <summary>
        ///     The minimum number of consumers to manage
        /// </summary>
        public byte MinimumConsumerCount { get; set; } = 1;

        /// <summary>
        ///     The maximum number of consumers to manage
        /// </summary>
        public byte MaximumConsumerCount { get; set; } = 10;

        /// <summary>
        ///     The SQS specific messages attributes to retrieve, eg: ApproximateFirstReceiveTimestamp, ApproximateReceiveCount, AWSTraceHeader, SenderId, SentTimestamp, MessageDeduplicationId, MessageGroupId, SequenceNumber
        /// </summary>
        public List<string> SQSMessageAttributes { get; set; } = new List<string>(0);

        /// <summary>
        ///     The custom message properties, eg: ContentType, to load from the message
        /// </summary>
        public List<string> CustomMessageAttributes { get; set; } = new List<string> { "All" };
    }
}