using System;

namespace OpenMessage.Pipelines
{
    /// <summary>
    ///     The context that the message is being executed with
    /// </summary>
    public class MessageContext
    {
        /// <summary>
        ///     The service provider for this context
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public MessageContext(IServiceProvider serviceProvider) => ServiceProvider = serviceProvider;
    }
}