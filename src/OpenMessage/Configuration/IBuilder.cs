using Microsoft.Extensions.DependencyInjection;

namespace OpenMessage.Configuration
{
    /// <summary>
    /// Defines a common base for a builder.
    /// </summary>
    public interface IBuilder
    {
        /// <summary>
        /// The underlying host builder.
        /// </summary>
        IMessagingBuilder HostBuilder { get; }

        /// <summary>
        /// Build.
        /// </summary>
        void Build();
    }
}