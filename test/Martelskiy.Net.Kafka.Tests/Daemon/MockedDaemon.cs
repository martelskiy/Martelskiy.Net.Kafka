using Martelskiy.Net.Kafka.Consumer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Martelskiy.Net.Kafka.Tests.Daemon
{
    public class MockedDaemon : KafkaDaemonBase<object, object>
    {
        public MockedDaemon(IKafkaConsumer<object, object> kafkaConsumer, ILogger<KafkaDaemonBase<object, object>> logger, IHostApplicationLifetime host) 
            : base(kafkaConsumer, logger, host)
        {
        }
    }
}
