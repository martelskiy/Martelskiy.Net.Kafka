using System;
using Confluent.SchemaRegistry;

namespace Martelskiy.Net.Kafka.SchemaRegistry
{
    public class SchemaRegistryClientFactory : ISchemaRegistryClientFactory
    {
        private readonly SchemaRegistryConfiguration _config;

        public SchemaRegistryClientFactory(
            SchemaRegistryConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public ISchemaRegistryClient Create()
        {
            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = _config.Url
            };

            var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);

            return schemaRegistryClient;
        }
    }
}