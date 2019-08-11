using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Helps construct the OpenMessage framework and serves as the base for helpful extension methods
    /// </summary>
    public interface IMessagingBuilder
    {
        /// <summary>
        ///     The context of the application being constructed
        /// </summary>
        HostBuilderContext Context { get; }

        /// <summary>
        ///     The service collection of the application being constructed
        /// </summary>
        IServiceCollection Services { get; }
    }
}