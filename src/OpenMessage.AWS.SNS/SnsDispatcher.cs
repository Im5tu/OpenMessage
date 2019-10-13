using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Options;
using OpenMessage.AWS.SNS.Configuration;
using OpenMessage.Serialisation;

namespace OpenMessage.AWS.SNS
{
    internal sealed class SnsDispatcher<T> : IDispatcher<T>
    {
        private static readonly string AttributeType = "String";
        private readonly ISerializer _serializer;
        private readonly AmazonSimpleNotificationServiceClient _client;
        private readonly MessageAttributeValue _contentType;
        private readonly MessageAttributeValue _valueTypeName;
        private readonly string _topicArn;

        public SnsDispatcher(IOptions<SNSOptions<T>> options, ISerializer serializer)
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
            _contentType = new MessageAttributeValue {DataType = AttributeType, StringValue = _serializer.ContentType};
            _valueTypeName = new MessageAttributeValue {DataType = AttributeType, StringValue = typeof(T).AssemblyQualifiedName};
            _topicArn = config.TopicArn;
        }

        public Task DispatchAsync(T entity, CancellationToken cancellationToken)
            => DispatchAsync(new Message<T> {Value = entity}, cancellationToken);

        public async Task DispatchAsync(Message<T> message, CancellationToken cancellationToken)
        {
            var request = new PublishRequest
            {
                MessageAttributes = GetMessageProperties(message),
                Message = _serializer.AsString(message.Value),
                TopicArn = _topicArn
            };

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
        }

        private void ThrowExceptionFromHttpResponse(HttpStatusCode statusCode, Exception innerException = null)
        {
            var msg = $"Failed to send the message to SNS. Type: '{TypeCache<T>.FriendlyName}' Topic ARN: '{_topicArn ?? "<NULL>"}' Status Code: '{statusCode}'.";
            if (innerException == null)
                throw new Exception(msg);

            throw new Exception(msg, innerException);
        }

        private Dictionary<string, MessageAttributeValue> GetMessageProperties(Message<T> message)
        {
            var result = new Dictionary<string, MessageAttributeValue>
            {
                [KnownProperties.ContentType] = _contentType,
                [KnownProperties.ValueTypeName] = _valueTypeName
            };

            if (Activity.Current != null)
                result[KnownProperties.ActivityId] = new MessageAttributeValue {DataType = AttributeType, StringValue = Activity.Current.Id};

            switch (message)
            {
                case ISupportProperties p:
                {
                    foreach (var prop in p.Properties)
                        result[prop.Key] = new MessageAttributeValue {DataType = AttributeType, StringValue = prop.Value};
                    break;
                }
                case ISupportProperties<byte[]> p2:
                {
                    foreach (var prop in p2.Properties)
                        result[prop.Key] = new MessageAttributeValue {DataType = AttributeType, StringValue = Encoding.UTF8.GetString(prop.Value)};
                    break;
                }
                case ISupportProperties<byte[], byte[]> p3:
                {
                    foreach (var prop in p3.Properties)
                        result[Encoding.UTF8.GetString(prop.Key)] = new MessageAttributeValue {DataType = AttributeType, StringValue = Encoding.UTF8.GetString(prop.Value)};
                    break;
                }
            }

            return result;
        }
    }
}