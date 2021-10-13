using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Martelskiy.Net.Kafka.Topic;
using Microsoft.Extensions.Logging;

namespace Martelskiy.Net.Kafka.Producer
{
    public abstract class KafkaProducerBase<TKey, TValue> : IKafkaProducer<TKey, TValue>
    {
        private readonly IProducer<TKey, TValue> _kafkaProducer;
        private readonly TopicConfiguration _topicConfiguration;
        private readonly ILogger<KafkaProducerBase<TKey, TValue>> _logger;

        protected KafkaProducerBase(
            IProducer<TKey, TValue> kafkaProducer,
            TopicConfiguration topicConfiguration,
            ILogger<KafkaProducerBase<TKey, TValue>> logger)
        {
            _kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _topicConfiguration = topicConfiguration ?? throw new ArgumentNullException(nameof(topicConfiguration));
        }
        
        public async Task Produce(TKey key, TValue value, CancellationToken cancellationToken = default)
        {
            var result = await _kafkaProducer.ProduceAsync(_topicConfiguration.Topic, new Message<TKey, TValue>
            {
                Key = key, 
                Value = value
            }, cancellationToken);
            
            _logger.LogInformation("Kafka message delivered with status {status}", result.Status);
        }

        public async Task Produce(TValue value, CancellationToken cancellationToken = default)
        {
            var result = await _kafkaProducer.ProduceAsync(_topicConfiguration.Topic, new Message<TKey, TValue>
            {
                Value = value
            }, cancellationToken);

            _logger.LogInformation("Kafka message delivered to offset {offset} with status {status}", result.Offset, result.Status);
        }

        public void Dispose()
        {
            _kafkaProducer?.Flush();
            _kafkaProducer?.Dispose();
        }
    }
}
