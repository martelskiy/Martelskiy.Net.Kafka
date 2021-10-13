using System;
using System.Collections.Generic;
using Martelskiy.Net.Kafka.Consumer.Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Martelskiy.Net.Kafka.Consumer
{
    internal static class ConsumerConfigurator
    {
        private const string DefaultConsumerConfigurationSection = "Kafka";
        private static readonly HashSet<string> RequiredConfigurationKeys = new(new []
        {
            "group.id"
        });

        internal static void Configure(
            IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            serviceCollection.AddSingleton<IKafkaConsumerFactory, KafkaConsumerFactory>();

            serviceCollection.Configure<KafkaConsumerConfiguration>(configuration.GetSection(DefaultConsumerConfigurationSection));

            serviceCollection.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<KafkaConsumerConfiguration>>();

                if (options.Value?.ConsumerConfiguration == null)
                {
                    throw new ArgumentNullException($"Missing configuration {nameof(KafkaConsumerConfiguration)}");
                }

                foreach (var requiredConfigurationKey in RequiredConfigurationKeys)
                {
                    if (!options.Value.ConsumerConfiguration.ContainsKey(requiredConfigurationKey))
                    {
                        throw new ArgumentException($"Missing configuration key {requiredConfigurationKey} in ConsumerConfiguration");
                    }
                }

                return new KafkaConsumerConfiguration
                {
                    ConsumerConfiguration = options.Value.ConsumerConfiguration
                };
            });
        }
    }
}
