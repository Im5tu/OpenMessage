using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using OpenMessage.AWS.SQS.Configuration;

namespace OpenMessage.AWS.SQS
{
    internal sealed class QueueMonitor<T> : IQueueMonitor<T>
    {
        private readonly IOptionsMonitor<SQSConsumerOptions> _sqsOptions;
        private readonly List<string> QueueAttributes = new List<string>
        {
            "ApproximateNumberOfMessages",
            "ApproximateNumberOfMessagesDelayed",
            "ApproximateNumberOfMessagesNotVisible"
        };

        public QueueMonitor(IOptionsMonitor<SQSConsumerOptions> sqsOptions)
        {
            _sqsOptions = sqsOptions ?? throw new ArgumentNullException(nameof(sqsOptions));
        }

        public async Task<int> GetQueueCountAsync(string consumerId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(consumerId))
                throw new ArgumentNullException(nameof(consumerId));

            var options = _sqsOptions.Get(consumerId);

            var config = new AmazonSQSConfig
            {
                ServiceURL = options.ServiceURL
            };

            if (!string.IsNullOrEmpty(options.RegionEndpoint))
                config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.RegionEndpoint);

            options.AwsConsumerConfiguration?.Invoke(config);
            using var client = new AmazonSQSClient(config);

            var attributes = await client.GetQueueAttributesAsync(new GetQueueAttributesRequest
            {
                QueueUrl = options.QueueUrl,
                AttributeNames = QueueAttributes
            }, cancellationToken);

            return attributes.ApproximateNumberOfMessages;
        }
    }
}