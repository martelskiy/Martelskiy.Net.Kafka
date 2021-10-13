using System;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Logging;

namespace Martelskiy.Net.Kafka.Consumer.Factory
{
    public class KafkaConsumerFactory : IKafkaConsumerFactory
    {
        private readonly KafkaConsumerConfiguration _kafkaConsumerOptions;
        private readonly ILogger<KafkaConsumerFactory> _logger;
        
        public KafkaConsumerFactory(
            KafkaConsumerConfiguration kafkaConsumerOptions, 
            ILogger<KafkaConsumerFactory> logger)
        {
            _kafkaConsumerOptions = kafkaConsumerOptions ?? throw new ArgumentNullException(nameof(kafkaConsumerOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IConsumer<TKey, TValue> Create<TKey, TValue>(ISchemaRegistryClient schemaRegistry)
        {
            var consumerBuilder =
                new ConsumerBuilder<TKey, TValue>(_kafkaConsumerOptions.ConsumerConfiguration)
                    .SetValueDeserializer(new AvroDeserializer<TValue>(schemaRegistry).AsSyncOverAsync())
                    .SetLogHandler((cons, message) =>
                    {
                        _logger.LogDebug(message.Message);
                    })
                    .SetErrorHandler((_, error) =>
                    {
                        _logger.LogError(error.Reason);

                        throw new Exception(error.Reason);
                    });

            var consumer = consumerBuilder.Build();

            return consumer; 
        }
    }
}
