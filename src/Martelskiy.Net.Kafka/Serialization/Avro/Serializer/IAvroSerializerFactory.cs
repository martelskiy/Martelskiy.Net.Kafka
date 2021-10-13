using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace Martelskiy.Net.Kafka.Serialization.Avro.Serializer
{
    public interface IAvroSerializerFactory
    {
        AvroSerializer<T> CreateValueSerializer<T>(ISchemaRegistryClient schemaRegistryClient);
        AvroSerializer<T> CreateKeySerializer<T>(ISchemaRegistryClient schemaRegistryClient);
    }
}
