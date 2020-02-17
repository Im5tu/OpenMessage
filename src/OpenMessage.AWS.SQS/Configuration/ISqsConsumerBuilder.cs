using Microsoft.Extensions.Hosting;
using OpenMessage.Builders;
using System;

namespace OpenMessage.AWS.SQS.Configuration
{
    /// <summary>
    ///     The builder for an SQS consumer
    /// </summary>
    /// <typeparam name="T">The type to be consumed</typeparam>
    public interface ISqsConsumerBuilder<T> : IBuilder
    {
        /// <summary>
        ///     Configure the consumer with the specified options
        /// </summary>
        /// <param name="configuration">The configuration action</param>
        /// <returns>The SQS consumer builder</returns>
        ISqsConsumerBuilder<T> FromConfiguration(Action<SQSConsumerOptions> configuration);

        /// <summary>
        ///     Configure the consumer with the specified options
        /// </summary>
        /// <param name="configuration">The configuration action</param>
        /// <returns>The SQS consumer builder</returns>
        ISqsConsumerBuilder<T> FromConfiguration(Action<HostBuilderContext, SQSConsumerOptions> configuration);

        /// <summary>
        ///     Configure the dispatcher with the specified options
        /// </summary>
        /// <param name="configurationSection">The configuration section to use</param>
        /// <returns>The SQS dispatcher builder</returns>
        ISqsConsumerBuilder<T> FromConfiguration(string configurationSection);
    }
}