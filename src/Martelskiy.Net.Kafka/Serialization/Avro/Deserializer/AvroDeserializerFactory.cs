using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace Martelskiy.Net.Kafka.Serialization.Avro.Deserializer
{
    public class AvroDeserializerFactory : IAvroDeserializerFactory
    {
        public AvroDeserializer<T> CreateValueDeserializer<T>(ISchemaRegistryClient schemaRegistryClient)
        {
            return new AvroDeserializer<T>(schemaRegistryClient);
        }
    }
}