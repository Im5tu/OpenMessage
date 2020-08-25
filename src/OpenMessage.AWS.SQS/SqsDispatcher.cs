using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using OpenMessage.AWS.SQS.Configuration;
using OpenMessage.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenMessage.AWS.SQS
{
    internal sealed class SqsDispatcher<T> : DispatcherBase<T>
    {
        //15min = 900sec is the maximum delay supported by sqs
        private const int MaximumSqsDelaySeconds = 900;
        private static readonly string AttributeType = "String";
        private readonly AmazonSQSClient _client;
        private readonly MessageAttributeValue _contentType;
        private readonly string _queueUrl;
        private readonly ISerializer _serializer;

        public SqsDispatcher(IOptions<SQSDispatcherOptions<T>> options, ISerializer serializer, ILogger<SqsDispatcher<T>> logger)
            : base(logger)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            var config = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _queueUrl = config.QueueUrl ?? throw new Exception("No queue url set for type: " + (TypeCache<T>.FriendlyName ?? string.Empty));

            var sqsConfig = new AmazonSQSConfig
            {
                ServiceURL = config.ServiceURL
            };

            if (!string.IsNullOrEmpty(config.RegionEndpoint))
                sqsConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(config.RegionEndpoint);

            config.AwsDispatcherConfiguration?.Invoke(sqsConfig);

            _client = new AmazonSQSClient(sqsConfig);

            _contentType = new MessageAttributeValue
            {
                DataType = AttributeType,
                StringValue = _serializer.ContentType
            };
        }

        public override async Task DispatchAsync(Message<T> message, CancellationToken cancellationToken)
        {
            LogDispatch(message);

            if (message.Value is null)
                Throw.Exception("Message value cannot be null");

            var msg = _serializer.AsString(message.Value);
            if (string.IsNullOrWhiteSpace(msg))
                Throw.Exception("Message could not be serialized");

            var request = new SendMessageRequest
            {
                MessageAttributes = GetMessageProperties(message),
                DelaySeconds = DelaySeconds(message),
                MessageBody = msg,
                QueueUrl = _queueUrl
            };

#if NETCOREAPP3_1
            var stopwatch = OpenMessageEventSource.Instance.ProcessMessageDispatchStart();
#endif

            try
            {
                var response = await _client.SendMessageAsync(request, cancellationToken);
                if (response.HttpStatusCode != HttpStatusCode.OK)
                    ThrowExceptionFromHttpResponse(response);
            }
            finally
            {
#if NETCOREAPP3_1
                if (stopwatch.HasValue)
                    OpenMessageEventSource.Instance.ProcessMessageDispatchStop(stopwatch.Value);
#endif
            }
        }

        private static int DelaySeconds(Message<T> message)
        {
            if (message is ISupportSendDelay delay && delay.SendDelay > TimeSpan.Zero)
            {
                return Math.Min(MaximumSqsDelaySeconds, (int) delay.SendDelay.TotalSeconds);
            }

            return 0;
        }

        private Dictionary<string, MessageAttributeValue> GetMessageProperties(Message<T> message)
        {
            var result = new Dictionary<string, MessageAttributeValue>
            {
                [KnownProperties.ContentType] = _contentType
            };

            if (!(message.Value is null))
                result[KnownProperties.ValueTypeName] = new MessageAttributeValue
                {
                    DataType = AttributeType,
                    StringValue = message.Value.GetType().AssemblyQualifiedName
                };

            if (Activity.Current is {})
                result[KnownProperties.ActivityId] = new MessageAttributeValue
                {
                    DataType = AttributeType,
                    StringValue = Activity.Current.Id
                };

            switch (message)
            {
                case ISupportProperties p:
                {
                    foreach (var prop in p.Properties)
                        result[prop.Key] = new MessageAttributeValue
                        {
                            DataType = AttributeType,
                            StringValue = prop.Value
                        };

                    break;
                }
                case ISupportProperties<byte[]> p2:
                {
                    foreach (var prop in p2.Properties)
                        result[prop.Key] = new MessageAttributeValue
                        {
                            DataType = AttributeType,
                            StringValue = Encoding.UTF8.GetString(prop.Value)
                        };

                    break;
                }
                case ISupportProperties<byte[], byte[]> p3:
                {
                    foreach (var prop in p3.Properties)
                        result[Encoding.UTF8.GetString(prop.Key)] = new MessageAttributeValue
                        {
                            DataType = AttributeType,
                            StringValue = Encoding.UTF8.GetString(prop.Value)
                        };

                    break;
                }
            }

            return result;
        }

        private void ThrowExceptionFromHttpResponse(SendMessageResponse response)
        {
            throw new Exception($"Failed to send the message to SQS. Type: '{TypeCache<T>.FriendlyName}' Queue Url: '{_queueUrl ?? "<NULL>"}' Status Code: '{response.HttpStatusCode}'.");
        }
    }
}