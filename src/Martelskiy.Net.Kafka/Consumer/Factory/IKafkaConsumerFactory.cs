using Confluent.Kafka;
using Confluent.SchemaRegistry;

namespace Martelskiy.Net.Kafka.Consumer.Factory
{
    public interface IKafkaConsumerFactory
    {
        IConsumer<TKey, TValue> Create<TKey, TValue>(ISchemaRegistryClient schemaRegistry);
    }
}