using System;
using Martelskiy.Net.Kafka.Serialization.Avro.Serializer.SubjectNameMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Martelskiy.Net.Kafka.SchemaRegistry
{
    internal static class SchemaRegistryConfigurator
    {
        private const string DefaultConfigurationSection = "Kafka:SchemaRegistryConfiguration";
        internal static void Configure(
            IServiceCollection serviceCollection, 
            IConfiguration configuration)
        {
            serviceCollection.Configure<SchemaRegistryConfiguration>(configuration.GetSection(DefaultConfigurationSection));

            serviceCollection.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<SchemaRegistryConfiguration>>();

                if (string.IsNullOrEmpty(options.Value?.Url))
                {
                    throw new ArgumentNullException($"Missing configuration {nameof(SchemaRegistryConfiguration)}");
                }

                return new SchemaRegistryConfiguration
                {
                    Url = options.Value.Url
                };
            });

            serviceCollection.AddSingleton<ISchemaRegistryClientFactory, SchemaRegistryClientFactory>();
            serviceCollection
                .AddSingleton<ISerializerSubjectNameStrategyMapper,
                    SerializerSubjectNameStrategyMapper>();
        }
    }
}
