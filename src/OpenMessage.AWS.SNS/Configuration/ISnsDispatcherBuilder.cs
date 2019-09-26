using System;
using Microsoft.Extensions.Hosting;
using OpenMessage.Configuration;

namespace OpenMessage.AWS.SNS.Configuration
{
    /// <summary>
    /// SNS Dispatcher Builder
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISnsDispatcherBuilder<T> : IBuilder
    {
        /// <summary>
        /// Configure the dispatcher with the specified options
        /// </summary>
        /// <param name="configuration">The configuration action</param>
        /// <returns>The SQS dispatcher builder</returns>
        ISnsDispatcherBuilder<T> FromConfiguration(Action<SNSOptions<T>> configuration);

        /// <summary>
        /// Configure the dispatcher with the specified options
        /// </summary>
        /// <param name="configuration">The configuration action</param>
        /// <returns>The SQS dispatcher builder</returns>
        ISnsDispatcherBuilder<T> FromConfiguration(Action<HostBuilderContext, SNSOptions<T>> configuration);


        /// <summary>
        /// Configure the dispatcher with the specified options
        /// </summary>
        /// <param name="configurationSection">The configuration section to use</param>
        /// <returns>The SNS dispatcher builder</returns>
        ISnsDispatcherBuilder<T> FromConfiguration(string configurationSection);
    }
}