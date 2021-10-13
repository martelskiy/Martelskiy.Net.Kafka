using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Avro;
using Avro.Generic;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Martelskiy.Net.Kafka.Consumer;
using Martelskiy.Net.Kafka.Consumer.Factory;
using Martelskiy.Net.Kafka.Topic;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;
using Schema = Avro.Schema;

namespace Martelskiy.Net.Kafka.Tests.Consumer
{
    public class KafkaConsumerBaseTests
    {
        private MockedKafkaConsumer _sut;
        private readonly IFixture _fixture = new Fixture();
        private readonly IKafkaConsumerFactory _fakeKafkaConsumerFactory;
        private readonly IConsumer<Ignore, GenericRecord> _fakeConsumer;
        private readonly CancellationToken _cancellationToken = new(true);

        public KafkaConsumerBaseTests()
        {
            _fakeKafkaConsumerFactory = Substitute.For<IKafkaConsumerFactory>();
            _fakeConsumer = Substitute.For<IConsumer<Ignore, GenericRecord>>();
        }

        [Fact]
        public async Task Should_SetTheOffsetPartitionToAssign_When_PartitionOffsetProvided_WhenSubscribe()
        {
            var partitionOffsets = _fixture.CreateMany<PartitionOffset>(1).ToList();

            var config = _fixture.Build<TopicConfiguration>()
                .With(configuration => configuration.PartitionOffset, partitionOffsets)
                .Create();

            SetupBehaviour(config);

            await _sut.Subscribe();

            _fakeConsumer
                .Received(1)
                .Assign(Arg.Is<IEnumerable<TopicPartitionOffset>>(
                    topicPartitionOffset => 
                        topicPartitionOffset.First().Offset == partitionOffsets.First().Offset && 
                        topicPartitionOffset.First().Partition == partitionOffsets.First().Partition &&
                        topicPartitionOffset.First().Topic == config.Topic));

            _fakeConsumer
                .DidNotReceiveWithAnyArgs()
                .Subscribe(Arg.Any<string>());
        }

        [Theory, AutoData]
        public async Task Should_Not_SetTheOffsetPartition_When_PartitionOffset_Not_Provided_WhenSubscribe(TopicConfiguration configuration)
        {
            configuration.PartitionOffset = null;
            SetupBehaviour(configuration);

            await _sut.Subscribe();

            _fakeConsumer
                .DidNotReceive()
                .Assign(Arg.Any<IEnumerable<TopicPartitionOffset>>());

            _fakeConsumer
                .Received(1)
                .Subscribe(configuration.Topic);
        }

        [Theory, AutoData]
        public async Task Should_ThrowException_When_ExceptionThrownWhenSubscribe(TopicConfiguration configuration, Exception subscribeException)
        {
            configuration.PartitionOffset = null;

            _fakeConsumer
                .When(consumer => consumer.Subscribe(Arg.Any<string>()))
                .Do(info => throw subscribeException);

            SetupBehaviour(configuration);
            
            var exception = await Should.ThrowAsync<Exception>(async () => await _sut.Subscribe());

            exception.ShouldNotBeNull();
            exception.ShouldBe(subscribeException);
        }

        [Theory, AutoData]
        public async Task Should_ThrowException_When_ExceptionThrownWhenAssign(TopicConfiguration configuration, Exception assignException)
        {
            _fakeConsumer
                .When(consumer => consumer.Assign(Arg.Any<IEnumerable<TopicPartitionOffset>>()))
                .Do(info => throw assignException);

            SetupBehaviour(configuration);

            var exception = await Should.ThrowAsync<Exception>(async () => await _sut.Subscribe());

            exception.ShouldNotBeNull();
            exception.ShouldBe(assignException);
        }

        [Fact]
        public async Task Should_ConsumerStartConsuming_When_Consume()
        {
            SetupBehaviour();

            await _sut.Consume(_cancellationToken);

            _fakeConsumer
                .Received(1)
                .Consume(_cancellationToken);
        }


        [Theory, AutoData]
        public async Task Should_ThrowException_When_ConsumerThrowsException_WhenConsume(Exception consumeException)
        {
            _fakeConsumer
                .When(consumer => consumer.Consume(Arg.Any<CancellationToken>()))
                .Do(info => throw consumeException);

            SetupBehaviour();

            await Should.ThrowAsync<Exception>(async () =>
            {
                await _sut.Consume(_cancellationToken);
            });
        }

        [Theory, AutoData]
        public void Should_Not_ThrowException_When_ThrowingOperationCanceledException_WhenConsume(OperationCanceledException consumeException)
        {
            _fakeConsumer
                .When(consumer => consumer.Consume(Arg.Any<CancellationToken>()))
                .Do(info => throw consumeException);

            SetupBehaviour();

            Should.NotThrow(async () =>
            {
                await _sut.Consume(_cancellationToken);
            });
        }

        [Fact]
        public async Task Should_ConsumerCommitTheResult_When_WhenCallCommit()
        {
            var result = Substitute.For<ConsumeResult<Ignore, GenericRecord>>();
            
            SetupBehaviour();

            await _sut.Commit(result);

            _fakeConsumer
                .Received(1)
                .Commit(result);
        }

        [Fact]
        public async Task Should_ConsumerUnsubscribe_WhenCallStop()
        {
            SetupBehaviour();

            await _sut.Stop();

            _fakeConsumer
                .Received(1)
                .Unsubscribe();
        }

        private void SetupBehaviour(
            TopicConfiguration topicConfiguration = null)
        {
            _sut = new MockedKafkaConsumer(
                Substitute.For<ILogger<KafkaConsumerBase<Ignore, GenericRecord>>>(),
                _fakeConsumer,
                topicConfiguration ?? new TopicConfiguration(),
                CultureInfo.InvariantCulture);

            SetupConsumer();
        }


        private void SetupConsumer()
        {
            var consumeResult = Substitute.For<ConsumeResult<Ignore, GenericRecord>>();

            var stringSchema = @"{
                    ""namespace"": ""Namespace"",
                    ""type"": ""record"",
                    ""name"": ""schemaName"",
                    ""fields"": [
                        {""name"": ""test"", ""type"": ""int""},
                    ]
                  }";

            var record = new GenericRecord((RecordSchema)Schema.Parse(stringSchema));

            consumeResult.Message = new Message<Ignore, GenericRecord> { Value = record };

            _fakeConsumer
                .Consume(Arg.Any<CancellationToken>())
                .Returns(consumeResult);

            _fakeKafkaConsumerFactory
                .Create<Ignore, GenericRecord>(Arg.Any<ISchemaRegistryClient>())
                .Returns(_fakeConsumer);
        }
    }
}