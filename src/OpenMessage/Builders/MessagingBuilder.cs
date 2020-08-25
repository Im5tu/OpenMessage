using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace OpenMessage.Builders
{
    /// <summary>
    ///     Helps construct the OpenMessage framework and serves as the base for helpful extension methods
    /// </summary>
    internal sealed class MessagingBuilder : IMessagingBuilder
    {
        public HostBuilderContext Context { get; }
        public IServiceCollection Services { get; }

        internal MessagingBuilder(HostBuilderContext context, IServiceCollection services)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
    }
}