using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace OpenMessage.Apache.Kafka.Configuration
{
    internal class KafkaOptionsPostConfigurationProvider : IPostConfigureOptions<KafkaOptions>
    {
        private static readonly IReadOnlyDictionary<string, string> Defaults = new Dictionary<string, string>
        {
            {"auto.commit.interval.ms", "1000"},
            {"auto.offset.reset", "earliest"},
            {"bootstrap.servers", "localhost:9092"},
            {"compression.codec", "LZ4"},
            {"enable.auto.commit", "true"},
            {"queue.buffering.max.ms", "5"}
        };

        public void PostConfigure(string name, KafkaOptions options)
        {
            // Apply the defaults where there are none
            foreach (var setting in Defaults)
                if (!options.KafkaConfiguration.ContainsKey(setting.Key))
                    options.KafkaConfiguration[setting.Key] = setting.Value;

            if (options.KafkaConfiguration.TryGetValue("enable.auto.commit", out var str)
                && bool.TryParse(str, out var autoCommitEnabled)
                && autoCommitEnabled)
                // Disables automatically storing of the offset of last message provided to application
                options.KafkaConfiguration["enable.auto.offset.store"] = "false";
        }
    }
}