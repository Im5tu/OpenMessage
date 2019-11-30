using System;

namespace OpenMessage.Pipelines
{
    /// <summary>
    /// The context that the message is being executed with
    /// </summary>
    public class MessageContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        public MessageContext(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// The service provider for this context
        /// </summary>
        public IServiceProvider ServiceProvider { get; }
    }
}