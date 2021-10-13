using Martelskiy.Net.Kafka.Producer;
using Martelskiy.Net.Kafka.Producer.Factory;
using Martelskiy.Net.Kafka.SchemaRegistry;
using Martelskiy.Net.Kafka.Serialization;
using Martelskiy.Net.Kafka.Topic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Martelskiy.Net.Kafka
{
    public static class KafkaProducerConfigurator
    {
        private const string DefaultKafkaConfigurationSection = "Kafka";

        public static void ConfigureKafkaProducer<TKafkaProducer, TProducerKey, TProducerValue>(this IServiceCollection serviceCollection, IConfiguration configuration)
            where TKafkaProducer : class, IKafkaProducer<TProducerKey, TProducerValue>
        {
            ConfigureKafkaProducerInternal<TKafkaProducer, TProducerKey, TProducerValue>(serviceCollection, configuration, DefaultKafkaConfigurationSection);
        }

        public static void ConfigureKafkaProducer<TKafkaProducer, TProducerKey, TProducerValue>(this IServiceCollection serviceCollection, IConfiguration configuration, string configurationSection)
            where TKafkaProducer : class, IKafkaProducer<TProducerKey, TProducerValue>
        {
            ConfigureKafkaProducerInternal<TKafkaProducer, TProducerKey, TProducerValue>(serviceCollection, configuration, configurationSection);
        }

        private static void ConfigureKafkaProducerInternal<TKafkaProducer, TProducerKey, TProducerValue>(
            IServiceCollection serviceCollection, 
            IConfiguration configuration,
            string topicConfigurationSection)
            where TKafkaProducer : class, IKafkaProducer<TProducerKey, TProducerValue>
        {
            ProducerConfigurator.Configure(serviceCollection, configuration);
            SerializationConfigurator.ConfigureAvroSerialization(serviceCollection, configuration);
            SchemaRegistryConfigurator.Configure(serviceCollection, configuration);

            serviceCollection.AddSingleton<IKafkaProducer<TProducerKey, TProducerValue>, TKafkaProducer>(provider =>
            {
                var topicConfiguration = TopicConfigurator.Configure(configuration, topicConfigurationSection);
                var schemaRegistryClientFactory = provider.GetRequiredService<ISchemaRegistryClientFactory>();
                var producerFactory = provider.GetRequiredService<IKafkaProducerFactory>();
                    
                var confluentKafkaProducer = producerFactory.Create<TProducerKey, TProducerValue>(schemaRegistryClientFactory.Create());
                
                var kafkaProducer = ActivatorUtilities.CreateInstance<TKafkaProducer>(provider, confluentKafkaProducer, topicConfiguration);
                
                return kafkaProducer;
            });
        }
    }
}