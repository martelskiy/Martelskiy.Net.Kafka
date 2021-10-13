using System.Threading.Tasks;
using AutoFixture;
using Confluent.Kafka;
using Martelskiy.Net.Kafka.Producer;
using Martelskiy.Net.Kafka.Topic;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Martelskiy.Net.Kafka.Tests.Producer
{
    public class KafkaProducerBaseTests
    {
        private readonly MockedKafkaProducer _sut;
        private readonly IProducer<string, string> _fakeConfluentProducer;
        private readonly TopicConfiguration _topicConfiguration;

        private readonly IFixture _fixture = new Fixture();

        public KafkaProducerBaseTests()
        {
            _fakeConfluentProducer = Substitute.For<IProducer<string, string>>();
            _topicConfiguration = _fixture.Create<TopicConfiguration>();

            _sut = new MockedKafkaProducer(
                _fakeConfluentProducer, 
                _topicConfiguration, 
                Substitute.For<ILogger<KafkaProducerBase<string, string>>>());
        }

        [Fact]
        public async Task Should_CallConfluentProducerProduce_When_ProduceWithValue()
        {
            SetupBehaviour();

            var eventPayload = _fixture.Create<string>();

            await _sut.Produce(eventPayload);

            await _fakeConfluentProducer
                .Received(1)
                .ProduceAsync(_topicConfiguration.Topic,
                    Arg.Is<Message<string, string>>(message => message.Value == eventPayload));
        }

        [Fact]
        public async Task Should_CallConfluentProducerProduce_When_ProduceWithValueAndKey()
        {
            SetupBehaviour();

            var key = _fixture.Create<string>();
            var eventPayload = _fixture.Create<string>();

            await _sut.Produce(key, eventPayload);

            await _fakeConfluentProducer
                .Received(1)
                .ProduceAsync(_topicConfiguration.Topic,
                    Arg.Is<Message<string, string>>(message => 
                        message.Key == key &&
                        message.Value == eventPayload));
        }

        private void SetupBehaviour()
        {
            _fakeConfluentProducer
                .ProduceAsync(Arg.Any<string>(), Arg.Any<Message<string, string>>())
                .Returns(new DeliveryResult<string, string>());
        }
    }
}
