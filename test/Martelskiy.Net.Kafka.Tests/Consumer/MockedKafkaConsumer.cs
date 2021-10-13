using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avro.Generic;
using Confluent.Kafka;
using Martelskiy.Net.Kafka.Consumer;
using Martelskiy.Net.Kafka.Topic;
using Microsoft.Extensions.Logging;

namespace Martelskiy.Net.Kafka.Tests.Consumer
{
    internal class MockedKafkaConsumer : KafkaConsumerBase<Ignore, GenericRecord>
    {
        public MockedKafkaConsumer(ILogger<KafkaConsumerBase<Ignore, GenericRecord>> logger, IConsumer<Ignore, GenericRecord> consumer, TopicConfiguration topicConfiguration, CultureInfo culture) 
            : base(logger, consumer, topicConfiguration, culture)
        {
        }

        protected override Task Handle(ConsumeResult<Ignore, GenericRecord> record, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
