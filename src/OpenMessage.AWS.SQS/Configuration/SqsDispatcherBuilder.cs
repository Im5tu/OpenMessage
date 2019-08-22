using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenMessage.Configuration;

namespace OpenMessage.AWS.SQS.Configuration
{
    internal sealed class SqsDispatcherBuilder<T> : Builder, ISqsDispatcherBuilder<T>
    {
        private Action<HostBuilderContext, SQSDispatcherOptions<T>> _configuration;

        public SqsDispatcherBuilder(IMessagingBuilder hostBuilder)
            : base(hostBuilder)
        {
        }

        public ISqsDispatcherBuilder<T> FromConfiguration(Action<SQSDispatcherOptions<T>> configuration)
        {
            return FromConfiguration((context, options) => configuration(options));
        }

        public ISqsDispatcherBuilder<T> FromConfiguration(Action<HostBuilderContext, SQSDispatcherOptions<T>> configuration)
        {
            _configuration = configuration;
            return this;
        }

        public override void Build()
        {
            ConfigureOptions(_configuration);
            HostBuilder.Services.AddSingleton<IDispatcher<T>, SqsDispatcher<T>>();
        }
    }

}