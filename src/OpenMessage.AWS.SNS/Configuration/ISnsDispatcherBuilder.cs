using System;
using Microsoft.Extensions.Hosting;
using OpenMessage.Configuration;

namespace OpenMessage.AWS.SNS.Configuration
{
    public interface ISnsDispatcherBuilder<T> : IBuilder
    {
        ISnsDispatcherBuilder<T> FromConfiguration(Action<SNSOptions<T>> configuration);
        ISnsDispatcherBuilder<T> FromConfiguration(Action<HostBuilderContext, SNSOptions<T>> configuration);
    }
}