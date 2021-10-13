using Confluent.Kafka;
using Martelskiy.Net.Kafka.Producer;
using Martelskiy.Net.Kafka.Topic;
using Microsoft.Extensions.Logging;

namespace Martelskiy.Net.Kafka.Tests.Producer
{
    public class MockedKafkaProducer : KafkaProducerBase<string, string>
    {
        public MockedKafkaProducer(IProducer<string, string> kafkaProducer, TopicConfiguration topicConfiguration, ILogger<KafkaProducerBase<string, string>> logger) 
            : base(kafkaProducer, topicConfiguration, logger)
        {
        }
    }
}
