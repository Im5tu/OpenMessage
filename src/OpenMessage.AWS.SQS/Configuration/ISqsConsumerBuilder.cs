using System;
using Microsoft.Extensions.Hosting;
using OpenMessage.Configuration;

namespace OpenMessage.AWS.SQS.Configuration
{
    /// <summary>
    /// The builder for an SQS consumer
    /// </summary>
    /// <typeparam name="T">The type to be consumed</typeparam>
    public interface ISqsConsumerBuilder<T> : IBuilder
    {
        /// <summary>
        /// Configure the consumer with the specified options
        /// </summary>
        /// <param name="configuration">The configuration action</param>
        /// <returns>The SQS consumer builder</returns>
        ISqsConsumerBuilder<T> FromConfiguration(Action<SQSConsumerOptions> configuration);

        /// <summary>
        /// Configure the consumer with the specified options
        /// </summary>
        /// <param name="configuration">The configuration action</param>
        /// <returns>The SQS consumer builder</returns>
        ISqsConsumerBuilder<T> FromConfiguration(Action<HostBuilderContext, SQSConsumerOptions> configuration);


        /// <summary>
        /// Configure the dispatcher with the specified options
        /// </summary>
        /// <param name="configurationSection">The configuration section to use</param>
        /// <returns>The SQS dispatcher builder</returns>
        ISqsConsumerBuilder<T> FromConfiguration(string configurationSection);

        /// <summary>
        /// Configure how many consumers to construct for a competing consumer scenario
        /// </summary>
        /// <param name="count">The count of how many consumers to construct</param>
        /// <returns>The SQS consumer builder</returns>
        ISqsConsumerBuilder<T> FromConsumerCount(int count);
    }
}