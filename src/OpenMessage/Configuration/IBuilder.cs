using Microsoft.Extensions.DependencyInjection;

namespace OpenMessage.Configuration
{
    public interface IBuilder
    {
        IMessagingBuilder HostBuilder { get; }

        void Build();
    }
}