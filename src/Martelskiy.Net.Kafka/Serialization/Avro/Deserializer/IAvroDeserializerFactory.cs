using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace Martelskiy.Net.Kafka.Serialization.Avro.Deserializer
{
    public interface IAvroDeserializerFactory
    {
        AvroDeserializer<T> CreateValueDeserializer<T>(ISchemaRegistryClient schemaRegistryClient);
    }
}