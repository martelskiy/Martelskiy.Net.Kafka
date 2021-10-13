using System;
using System.Threading;
using System.Threading.Tasks;
using Martelskiy.Net.Kafka.Consumer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Martelskiy.Net.Kafka.Tests.Daemon
{
    public class KafkaDaemonBaseTests
    {
        private readonly MockedDaemon _sut;
        private readonly IKafkaConsumer<object, object> _fakeConsumer;
        private readonly IHostApplicationLifetime _fakeHost;

        public KafkaDaemonBaseTests()
        {
            _fakeConsumer = Substitute.For<IKafkaConsumer<object, object>>();
            _fakeHost = Substitute.For<IHostApplicationLifetime>();

            _sut = new MockedDaemon(
                _fakeConsumer, 
                Substitute.For<ILogger<KafkaDaemonBase<object, object>>>(), 
                _fakeHost);
        }

        [Fact]
        public async Task Should_SubscribeAndStartConsuming_When_StartAsync()
        {
            await _sut.StartAsync(CancellationToken.None);

            await _fakeConsumer.Received(1).Subscribe();
            await _fakeConsumer.Received().Consume(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_HostStopApplication_When_ConsumeThrowsException()
        {
            _fakeConsumer.Consume(Arg.Any<CancellationToken>()).Throws(new Exception(string.Empty));

            await _sut.StartAsync(CancellationToken.None);
            await Task.Delay(100);

            _fakeHost.Received(1).StopApplication();
        }

        [Fact]
        public async Task Should_StopConsuming_When_StopDaemonAsync()
        {
            await _sut.StartAsync(CancellationToken.None);

            await _sut.StopAsync(CancellationToken.None);

            await _fakeConsumer.Received(1).Stop();
        }
    }
}