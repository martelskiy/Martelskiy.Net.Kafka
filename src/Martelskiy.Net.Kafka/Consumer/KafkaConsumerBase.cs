using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Martelskiy.Net.Kafka.Topic;
using Microsoft.Extensions.Logging;

namespace Martelskiy.Net.Kafka.Consumer
{
    public abstract class KafkaConsumerBase<TKey, TValue> : IKafkaConsumer<TKey, TValue>
    {
        private readonly ILogger<KafkaConsumerBase<TKey, TValue>> _logger;
        private readonly TopicConfiguration _topicConfiguration;
        private readonly IConsumer<TKey, TValue> _consumer;

        protected KafkaConsumerBase(
            ILogger<KafkaConsumerBase<TKey, TValue>> logger,
            IConsumer<TKey, TValue> consumer,
            TopicConfiguration topicConfiguration,
            CultureInfo culture)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _topicConfiguration = topicConfiguration ?? throw new ArgumentNullException(nameof(topicConfiguration));
        }

        public Task Subscribe()
        {
            try
            {
                if (_topicConfiguration.PartitionOffset != null)
                {
                    _logger.LogWarning("Recovery values were provided. Will start consuming from specific offsets");

                    var topicPartitionOffsets = _topicConfiguration.PartitionOffset
                        .Select(recoveryPartitionOffset => new TopicPartitionOffset(
                            _topicConfiguration.Topic, 
                            new Partition(recoveryPartitionOffset.Partition), 
                            new Offset(recoveryPartitionOffset.Offset)))
                        .ToList();

                    topicPartitionOffsets.ForEach(offset => 
                        _logger.LogWarning($"Start reading topic: {_topicConfiguration.Topic} from partition: {offset.Partition}, offset: {offset.Offset}"));

                    _consumer.Assign(topicPartitionOffsets);
                }
                else
                {
                    _logger.LogInformation("Subscribing to topic '{Topic}'", _topicConfiguration.Topic);
                   
                    _consumer.Subscribe(_topicConfiguration.Topic);
                }

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error when subscribing to '{_topicConfiguration.Topic}'");
                throw;
            }
        }

        public async Task Consume(CancellationToken cancellationToken)
        {
            try
            {
                var consumeResult = _consumer.Consume(cancellationToken);

                _logger.LogInformation("Message consumed. Offset: {offset}. Running message handler...",
                    consumeResult?.Offset.Value);

                await Handle(consumeResult, cancellationToken)
                    .ConfigureAwait(false);

            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Consumer has been cancelled");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Exception during Consume");
                throw;
            }
        }

        public Task Commit(ConsumeResult<TKey, TValue> consumeResult)
        {
            _consumer.Commit(consumeResult);

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _consumer.Unsubscribe();
            return Task.CompletedTask;
        }

        protected abstract Task Handle(ConsumeResult<TKey,TValue> record, CancellationToken cancellationToken);
    }
}
