using System;
using Microsoft.Extensions.Configuration;

namespace Martelskiy.Net.Kafka.Topic
{
    internal static class TopicConfigurator
    {
        internal static TopicConfiguration Configure(IConfiguration configuration, string topicConfigurationSection)
        {
            var topicConfiguration = new TopicConfiguration();

            configuration.Bind(topicConfigurationSection, topicConfiguration);

            if (string.IsNullOrWhiteSpace(topicConfiguration.Topic))
            {
                throw new ArgumentNullException(
                    $"Missing configuration {nameof(TopicConfiguration)}, property topic at configuration section: {topicConfigurationSection}");
            }

            return topicConfiguration;
        }
    }
}