using Confluent.SchemaRegistry;

namespace Martelskiy.Net.Kafka.SchemaRegistry
{
    public interface ISchemaRegistryClientFactory
    {
        ISchemaRegistryClient Create();
    }
}
