using System.Collections.Generic;

namespace Martelskiy.Net.Kafka.Producer
{
    public class KafkaProducerConfiguration
    {
        public IDictionary<string,string> ProducerConfiguration { get; set; }
    }
}