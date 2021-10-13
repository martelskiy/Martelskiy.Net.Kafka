using System;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Martelskiy.Net.Kafka.Serialization.Avro.Serializer;
using Microsoft.Extensions.Logging;

namespace Martelskiy.Net.Kafka.Producer.Factory
{
    public class KafkaProducerFactory : IKafkaProducerFactory
    {
        private readonly KafkaProducerConfiguration _kafkaProducerConfiguration;
        private readonly IAvroSerializerFactory _avroSerializerFactory;
        private readonly ILogger<KafkaProducerFactory> _logger;

        public KafkaProducerFactory(
            KafkaProducerConfiguration kafkaProducerConfiguration,
            IAvroSerializerFactory avroSerializerFactory,
            ILogger<KafkaProducerFactory> logger)
        {
            _kafkaProducerConfiguration = kafkaProducerConfiguration ?? throw new ArgumentNullException(nameof(kafkaProducerConfiguration));
            _avroSerializerFactory = avroSerializerFactory ?? throw new ArgumentNullException(nameof(avroSerializerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public IProducer<TKey, TValue> Create<TKey, TValue>(ISchemaRegistryClient schemaRegistry)
        {
            return new ProducerBuilder<TKey, TValue>(_kafkaProducerConfiguration.ProducerConfiguration)
                .SetKeySerializer(_avroSerializerFactory.CreateKeySerializer<TKey>(schemaRegistry))
                .SetValueSerializer(_avroSerializerFactory.CreateValueSerializer<TValue>(schemaRegistry))
                .SetLogHandler((producer, message) =>
                {
                    if (
                        message.Level == SyslogLevel.Error || 
                        message.Level == SyslogLevel.Alert ||
                        message.Level == SyslogLevel.Critical ||
                        message.Level == SyslogLevel.Emergency)
                    {
                        _logger.LogError(message.Message);
                    }
                    else if (message.Level == SyslogLevel.Warning)
                    {
                        _logger.LogWarning(message.Message);
                    }
                    else if (message.Level == SyslogLevel.Info || message.Level == SyslogLevel.Notice)
                    {
                        _logger.LogInformation(message.Message);
                    }
                })
                .Build();
        }
    }
}