using System.Collections.Generic;

namespace Martelskiy.Net.Kafka.Topic
{
    public class TopicConfiguration
    {
        public string Topic { get; set; }
        public IEnumerable<PartitionOffset> PartitionOffset { get; set; }
    }
}