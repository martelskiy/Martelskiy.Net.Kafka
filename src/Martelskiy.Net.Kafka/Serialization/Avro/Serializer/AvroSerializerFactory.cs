using System;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Martelskiy.Net.Kafka.Serialization.Avro.Serializer.SubjectNameMapper;

namespace Martelskiy.Net.Kafka.Serialization.Avro.Serializer
{
    public class AvroSerializerFactory : IAvroSerializerFactory
    {
        private readonly AvroSerializerConfiguration _config;
        private readonly ISerializerSubjectNameStrategyMapper _registrySubjectNameStrategyMapper;

        public AvroSerializerFactory(
            AvroSerializerConfiguration config, 
            ISerializerSubjectNameStrategyMapper registrySubjectNameStrategyMapper)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _registrySubjectNameStrategyMapper = registrySubjectNameStrategyMapper ?? throw new ArgumentNullException(nameof(registrySubjectNameStrategyMapper));
        }

        public AvroSerializer<T> CreateValueSerializer<T>(ISchemaRegistryClient schemaRegistryClient)
        {
            var valueAvroSerializerConfiguration = new AvroSerializerConfig
            {
                SubjectNameStrategy = _registrySubjectNameStrategyMapper.Map(_config.ValueSubjectNameStrategy),
            };

            return new AvroSerializer<T>(schemaRegistryClient, valueAvroSerializerConfiguration);
        }

        public AvroSerializer<T> CreateKeySerializer<T>(ISchemaRegistryClient schemaRegistryClient)
        {
            var keyAvroSerializerConfiguration = new AvroSerializerConfig
            {
                SubjectNameStrategy = _registrySubjectNameStrategyMapper.Map(_config.KeySubjectNameStrategy),
            };

            return new AvroSerializer<T>(schemaRegistryClient, keyAvroSerializerConfiguration);
        }
    }
}