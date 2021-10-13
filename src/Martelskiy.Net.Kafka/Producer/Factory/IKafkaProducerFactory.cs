using Confluent.Kafka;
using Confluent.SchemaRegistry;

namespace Martelskiy.Net.Kafka.Producer.Factory
{
    public interface IKafkaProducerFactory
    {
        IProducer<TKey, TValue> Create<TKey, TValue>(ISchemaRegistryClient schemaRegistry);
    }
}