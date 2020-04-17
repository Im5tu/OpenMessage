using System.Collections.Generic;

namespace OpenMessage.Apache.Kafka.Configuration
{
    /// <summary>
    ///     The basic options for a Kafka consumer/dispatcher
    /// </summary>
    public class KafkaOptions
    {
        /// <summary>
        ///     The Kafka specific configuration to use
        /// </summary>
        public IDictionary<string, string> KafkaConfiguration { get; set; } = new Dictionary<string, string>();

        /// <summary>
        ///     The name of the topic to consume from/dispatch to
        /// </summary>
        public string? TopicName { get; set; }
    }
}