using System;
using Microsoft.Extensions.Hosting;

namespace OpenMessage.AWS.SNS.Configuration
{
    public interface ISnsDispatcherBuilder<T>
    {
        ISnsDispatcherBuilder<T> FromConfiguration(Action<SNSOptions<T>> configuration);
        ISnsDispatcherBuilder<T> FromConfiguration(Action<HostBuilderContext, SNSOptions<T>> configuration);
    }
}