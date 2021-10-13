using Martelskiy.Net.Kafka.Serialization.Avro.Deserializer;
using Martelskiy.Net.Kafka.Serialization.Avro.Serializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Martelskiy.Net.Kafka.Serialization
{
    internal static class SerializationConfigurator
    {
        private const string DefaultConfigurationSection = "Kafka:AvroSerializerConfiguration";

        internal static void ConfigureAvroSerialization(
            IServiceCollection serviceCollection, 
            IConfiguration configuration)
        {
            serviceCollection.Configure<AvroSerializerConfiguration>(configuration.GetSection(DefaultConfigurationSection));

            serviceCollection.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<AvroSerializerConfiguration>>();

                return new AvroSerializerConfiguration
                {
                    ValueSubjectNameStrategy = !string.IsNullOrWhiteSpace(options.Value.ValueSubjectNameStrategy)
                        ? options.Value.ValueSubjectNameStrategy
                        : "Topic",
                    KeySubjectNameStrategy = !string.IsNullOrWhiteSpace(options.Value.KeySubjectNameStrategy)
                        ? options.Value.KeySubjectNameStrategy
                        : "Topic"
                };
            });

            serviceCollection.AddSingleton<IAvroDeserializerFactory, AvroDeserializerFactory>();
            serviceCollection.AddSingleton<IAvroSerializerFactory, AvroSerializerFactory>();
        }
    }
}
