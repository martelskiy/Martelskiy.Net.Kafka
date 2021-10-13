using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Martelskiy.Net.Kafka.Consumer
{
    public interface IKafkaConsumer<TKey,TValue> : IKafkaConsumer
    {
        Task Subscribe();
        Task Consume(CancellationToken cancellationToken);
        Task Commit(ConsumeResult<TKey, TValue> consumeResult);
        Task Stop();
    }

    public interface IKafkaConsumer
    {

    }
}