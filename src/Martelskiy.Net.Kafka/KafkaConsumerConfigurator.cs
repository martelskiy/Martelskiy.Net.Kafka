using System;
using System.Collections.Generic;
using System.Globalization;
using Martelskiy.Net.Kafka.Consumer;
using Martelskiy.Net.Kafka.Consumer.Factory;
using Martelskiy.Net.Kafka.SchemaRegistry;
using Martelskiy.Net.Kafka.Serialization;
using Martelskiy.Net.Kafka.Topic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Martelskiy.Net.Kafka
{
    public static class KafkaConsumerConfigurator
    {
        private const string DefaultKafkaConfigurationSection = "Kafka";
        private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

        public static void ConfigureBackgroundConsumer<TKafkaConsumer, TDaemon>(
            this IServiceCollection serviceCollection, 
            IConfiguration configuration)
            where TKafkaConsumer : IKafkaConsumer
            where TDaemon : BackgroundService
        {
            ConfigureBackgroundConsumerInternal<TKafkaConsumer, TDaemon>(serviceCollection, configuration, DefaultKafkaConfigurationSection, DefaultCulture);
        }

        public static void ConfigureBackgroundConsumer<TKafkaConsumer, TDaemon>(
            this IServiceCollection serviceCollection,
            IConfiguration configuration, 
            string providedTopicConfigurationSection)
            where TKafkaConsumer : IKafkaConsumer
            where TDaemon : BackgroundService
        {
            ConfigureBackgroundConsumerInternal<TKafkaConsumer, TDaemon>(serviceCollection, configuration, providedTopicConfigurationSection, DefaultCulture);
        }

        public static void ConfigureBackgroundConsumer<TKafkaConsumer, TDaemon>(
            this IServiceCollection serviceCollection,
            IConfiguration configuration, 
            string providedTopicConfigurationSection, 
            CultureInfo culture)
            where TKafkaConsumer : IKafkaConsumer
            where TDaemon : BackgroundService
        {
            ConfigureBackgroundConsumerInternal<TKafkaConsumer, TDaemon>(serviceCollection, configuration, providedTopicConfigurationSection, culture);
        }

        public static void  ConfigureBackgroundConsumer<TKafkaConsumer, TDaemon>(
            this IServiceCollection serviceCollection,
            IConfiguration configuration, 
            string providedTopicConfigurationSection, 
            Func<IServiceProvider, TKafkaConsumer> createConsumer)
            where TKafkaConsumer : IKafkaConsumer
            where TDaemon : BackgroundService
        {
            ConfigureBackgroundConsumerInternal<TKafkaConsumer, TDaemon>(serviceCollection, configuration, providedTopicConfigurationSection, DefaultCulture, createConsumer);
        }

        private static void ConfigureBackgroundConsumerInternal<TKafkaConsumer, TDaemon>(
            IServiceCollection serviceCollection,
            IConfiguration configuration,
            string providedTopicConfigurationSection,
            CultureInfo culture,
            Func<IServiceProvider, TKafkaConsumer> createConsumer = null)
            where TKafkaConsumer : IKafkaConsumer
            where TDaemon : BackgroundService
        {
            ConsumerConfigurator.Configure(serviceCollection, configuration);
            SerializationConfigurator.ConfigureAvroSerialization(serviceCollection, configuration);
            SchemaRegistryConfigurator.Configure(serviceCollection, configuration);

            var enabled = configuration.GetValue(providedTopicConfigurationSection + ":enabled", true);

            if (enabled)
            {
                RegisterBackgroundService<TKafkaConsumer, TDaemon>(serviceCollection, configuration, providedTopicConfigurationSection, culture, createConsumer);
            }
        }

        private static void RegisterBackgroundService<TKafkaConsumer, TDaemon>(
            IServiceCollection serviceCollection,
            IConfiguration configuration, 
            string providedTopicConfigurationSection, 
            CultureInfo culture, 
            Func<IServiceProvider, TKafkaConsumer> createConsumer = null)
            where TKafkaConsumer : IKafkaConsumer where TDaemon : BackgroundService
        {
            serviceCollection.AddHostedService(provider =>
            {
                var topicConfiguration = TopicConfigurator.Configure(configuration, providedTopicConfigurationSection);

                var consumerGenericTypes = GetKafkaKeyValueTypes<TKafkaConsumer>();

                var confluentConsumer = CreateConfluentConsumer(provider, consumerGenericTypes);

                var consumer = CreateConsumer(culture, createConsumer, provider, topicConfiguration, confluentConsumer);

                var daemon = ActivatorUtilities.CreateInstance<TDaemon>(provider, consumer);

                return daemon;
            });
        }

        private static Type[] GetKafkaKeyValueTypes<TKafkaConsumer>()
        {
            var consumerType = typeof(TKafkaConsumer);
            var interfaceTypes = consumerType.GetInterfaces();

            foreach (var interfaceType in interfaceTypes)
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IKafkaConsumer<,>))
                {
                    var key = interfaceType.GetGenericArguments()[0];

                    var value = interfaceType.GetGenericArguments()[1];

                    return new[] { key, value };
                }
            }

            throw new InvalidOperationException("Could not instantiate KafkaConsumer. It should implement IKafkaConsumer generic interface");
        }

        private static object CreateConfluentConsumer(IServiceProvider provider, IReadOnlyList<Type> genericTypes)
        {
            var kafkaConsumerFactory = provider.GetRequiredService<IKafkaConsumerFactory>();
            var schemaRegistryClientFactory = provider.GetRequiredService<ISchemaRegistryClientFactory>();

            var kafkaConsumerCreateMethod = kafkaConsumerFactory.GetType().GetMethod(nameof(IKafkaConsumerFactory.Create))?.MakeGenericMethod(genericTypes[0], genericTypes[1]);

            if (kafkaConsumerCreateMethod != null)
            {
                var consumer = kafkaConsumerCreateMethod.Invoke(kafkaConsumerFactory, new object[] { schemaRegistryClientFactory.Create() });

                return consumer;
            }

            throw new InvalidOperationException($"Could not instantiate KafkaConsumer from factory {nameof(IKafkaConsumerFactory)}");
        }

        private static IKafkaConsumer CreateConsumer<TKafkaConsumer>(
            CultureInfo culture, 
            Func<IServiceProvider, TKafkaConsumer> createConsumer, 
            IServiceProvider provider, 
            TopicConfiguration topicConfiguration, 
            object confluentConsumer) 
            where TKafkaConsumer : IKafkaConsumer 
        {
            return createConsumer == null
                ? (IKafkaConsumer)ActivatorUtilities.CreateInstance<TKafkaConsumer>(provider,
                    topicConfiguration, confluentConsumer, culture)
                : createConsumer(provider);
        }
    }
}
