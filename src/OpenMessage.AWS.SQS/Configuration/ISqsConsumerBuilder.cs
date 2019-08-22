using System;
using Microsoft.Extensions.Hosting;
using OpenMessage.Configuration;

namespace OpenMessage.AWS.SQS.Configuration
{
    public interface ISqsConsumerBuilder<T> : IBuilder
    {
        ISqsConsumerBuilder<T> FromConfiguration(Action<SQSConsumerOptions> configuration);
        ISqsConsumerBuilder<T> FromConfiguration(Action<HostBuilderContext, SQSConsumerOptions> configuration);
        ISqsConsumerBuilder<T> FromConsumerCount(int count);
    }
}