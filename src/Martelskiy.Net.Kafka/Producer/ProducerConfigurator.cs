using System;
using Martelskiy.Net.Kafka.Producer.Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Martelskiy.Net.Kafka.Producer
{
    internal static class ProducerConfigurator
    {
        private const string DefaultConsumerConfigurationSection = "Kafka";

        internal static void Configure(
            IServiceCollection serviceCollection, 
            IConfiguration configuration)
        {
            serviceCollection.AddSingleton<IKafkaProducerFactory, KafkaProducerFactory>();

            serviceCollection.Configure<KafkaProducerConfiguration>(configuration.GetSection(DefaultConsumerConfigurationSection));

            serviceCollection.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<KafkaProducerConfiguration>>();

                if (options.Value?.ProducerConfiguration == null)
                {
                    throw new ArgumentNullException($"Missing configuration {nameof(KafkaProducerConfiguration)}");
                }

                return new KafkaProducerConfiguration
                {
                    ProducerConfiguration = options.Value.ProducerConfiguration
                };
            });
        }
    }
}
