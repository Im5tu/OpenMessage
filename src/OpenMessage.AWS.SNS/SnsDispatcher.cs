using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Options;
using OpenMessage.AWS.SNS.Configuration;
using OpenMessage.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenMessage.AWS.SNS
{
    internal sealed class SnsDispatcher<T> : DispatcherBase<T>
    {
        private static readonly string AttributeType = "String";
        private readonly AmazonSimpleNotificationServiceClient _client;
        private readonly MessageAttributeValue _contentType;
        private readonly ISerializer _serializer;
        private readonly string _topicArn;
        private readonly MessageAttributeValue _valueTypeName;

        public SnsDispatcher(IOptions<SNSOptions<T>> options, ISerializer serializer, ILogger<SnsDispatcher<T>> logger)
            : base(logger)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            var config = options?.Value ?? throw new ArgumentNullException(nameof(options));

            var snsConfig = new AmazonSimpleNotificationServiceConfig
            {
                ServiceURL = config.ServiceURL
            };

            if (!string.IsNullOrEmpty(config.RegionEndpoint))
                snsConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(config.RegionEndpoint);

            config.AwsDispatcherConfiguration?.Invoke(snsConfig);
            _client = new AmazonSimpleNotificationServiceClient(snsConfig);

            _contentType = new MessageAttributeValue
            {
                DataType = AttributeType,
                StringValue = _serializer.ContentType
            };

            _valueTypeName = new MessageAttributeValue
            {
                DataType = AttributeType,
                StringValue = typeof(T).AssemblyQualifiedName
            };
            _topicArn = config.TopicArn ?? throw new Exception("No topic arn set for type: " + (TypeCache<T>.FriendlyName ?? string.Empty));
        }

        public override async Task DispatchAsync(Message<T> message, CancellationToken cancellationToken)
        {
            LogDispatch(message);

            if (message.Value is null)
                Throw.Exception("Message value cannot be null");

            var msg = _serializer.AsString(message.Value);
            if (string.IsNullOrWhiteSpace(msg))
                Throw.Exception("Message could not be serialized");

            var request = new PublishRequest
            {
                MessageAttributes = GetMessageProperties(message),
                Message = msg,
                TopicArn = _topicArn
            };

#if NETCOREAPP3_1
            var stopwatch = OpenMessageEventSource.Instance.ProcessMessageDispatchStart();
#endif

            try
            {
                var response = await _client.PublishAsync(request, cancellationToken);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                    ThrowExceptionFromHttpResponse(response.HttpStatusCode);
            }
            catch (AmazonSimpleNotificationServiceException e) when (e.ErrorCode == "NotFound")
            {
                ThrowExceptionFromHttpResponse(e.StatusCode, e);
            }
            finally
            {
#if NETCOREAPP3_1
                if (stopwatch.HasValue)
                    OpenMessageEventSource.Instance.ProcessMessageDispatchStop(stopwatch.Value);
#endif
            }
        }

        private Dictionary<string, MessageAttributeValue> GetMessageProperties(Message<T> message)
        {
            var result = new Dictionary<string, MessageAttributeValue>
            {
                [KnownProperties.ContentType] = _contentType,
                [KnownProperties.ValueTypeName] = _valueTypeName
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

        private void ThrowExceptionFromHttpResponse(HttpStatusCode statusCode, Exception? innerException = null)
        {
            var msg = $"Failed to send the message to SNS. Type: '{TypeCache<T>.FriendlyName}' Topic ARN: '{_topicArn ?? "<NULL>"}' Status Code: '{statusCode}'.";

            if (innerException is null)
                throw new Exception(msg);

            throw new Exception(msg, innerException);
        }
    }
}