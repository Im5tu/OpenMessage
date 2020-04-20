using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenMessage.Builders;
using System;

namespace OpenMessage.AWS.SQS.Configuration
{
    internal sealed class SqsDispatcherBuilder<T> : Builder, ISqsDispatcherBuilder<T>
    {
        private Action<HostBuilderContext, SQSDispatcherOptions<T>>? _configuration;

        public SqsDispatcherBuilder(IMessagingBuilder hostBuilder)
            : base(hostBuilder) { }

        public override void Build()
        {
            if (_configuration is {})
                ConfigureOptions(_configuration, true);
            HostBuilder.Services.AddSingleton<IDispatcher<T>, SqsDispatcher<T>>();
            HostBuilder.Services.AddHostedService<SqsDispatcherService>();
            HostBuilder.Services.TryAddChannel<SendMessage>();
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

        public ISqsDispatcherBuilder<T> FromConfiguration(string configurationSection)
        {
            _configuration = (context, options) => context.Configuration.Bind(configurationSection, options);

            return this;
        }
    }
}