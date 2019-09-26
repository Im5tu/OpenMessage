using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenMessage.Configuration;

namespace OpenMessage.AWS.SNS.Configuration
{
    internal sealed class SnsDispatcherBuilder<T> : Builder, ISnsDispatcherBuilder<T>
    {
        private Action<HostBuilderContext, SNSOptions<T>> _configuration;

        public SnsDispatcherBuilder(IMessagingBuilder hostBuilder) : base(hostBuilder)
        {
        }

        public override void Build()
        {
            ConfigureOptions(_configuration, true);
            HostBuilder.Services.AddSingleton<IDispatcher<T>, SnsDispatcher<T>>();
        }

        public ISnsDispatcherBuilder<T> FromConfiguration(Action<SNSOptions<T>> configuration)
        {
            return FromConfiguration((context, options) => configuration(options));
        }

        public ISnsDispatcherBuilder<T> FromConfiguration(Action<HostBuilderContext, SNSOptions<T>> configuration)
        {
            _configuration = configuration;
            return this;
        }

        public ISnsDispatcherBuilder<T> FromConfiguration(string configurationSection)
        {
            _configuration = (context, options) => context.Configuration.Bind(configurationSection, options);
            return this;
        }
    }
}