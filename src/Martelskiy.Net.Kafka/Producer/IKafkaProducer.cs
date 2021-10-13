using System;
using System.Threading;
using System.Threading.Tasks;

namespace Martelskiy.Net.Kafka.Producer
{
    public interface IKafkaProducer<in TKey, in TValue> : IKafkaProducer
    {
        Task Produce(TKey key, TValue value, CancellationToken cancellationToken = default);
        Task Produce(TValue value, CancellationToken cancellationToken = default);
    }

    public interface IKafkaProducer : IDisposable
    {
    }
}