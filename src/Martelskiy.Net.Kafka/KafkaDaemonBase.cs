using System;
using System.Threading;
using System.Threading.Tasks;
using Martelskiy.Net.Kafka.Consumer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Martelskiy.Net.Kafka
{
    public abstract class KafkaDaemonBase<TKey, TValue> : BackgroundService
    {
        protected readonly IKafkaConsumer<TKey, TValue> KafkaConsumer;
        protected readonly ILogger<KafkaDaemonBase<TKey, TValue>> Logger;
        protected readonly CancellationTokenSource CancellationTokenSource;
        protected readonly string Name;
        private Task _executingTask;
        private readonly IHostApplicationLifetime _host;
        private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);
        private bool _isStopping;

        protected KafkaDaemonBase(
            IKafkaConsumer<TKey, TValue> kafkaConsumer,
            ILogger<KafkaDaemonBase<TKey, TValue>> logger,
            IHostApplicationLifetime host)
        {
            KafkaConsumer = kafkaConsumer ?? throw new ArgumentNullException(nameof(kafkaConsumer));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _host = host ?? throw new ArgumentNullException(nameof(host));
            Name = GetType().UnderlyingSystemType.Name;
            CancellationTokenSource = new CancellationTokenSource();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("{DaemonName} is starting to consume...", Name);

            try
            {
                while (!CancellationTokenSource.Token.IsCancellationRequested)
                {
                    await KafkaConsumer.Consume(CancellationTokenSource.Token);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error when running {DaemonName}, stopping application", Name);
                await SemaphoreSlim.WaitAsync(CancellationTokenSource.Token);
                Environment.ExitCode = -100;
                _host.StopApplication();
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("{DaemonName} is starting...", Name);
            await KafkaConsumer.Subscribe();
            _executingTask = Task.Run(() => ExecuteAsync(CancellationTokenSource.Token), CancellationTokenSource.Token);
            Logger.LogInformation("{DaemonName} has started", Name);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
            {
                return;
            }

            if (!_isStopping)
            {
                _isStopping = true;
                Logger.LogInformation("{DaemonName} is stopping...", Name);
                CancellationTokenSource.Cancel();
                await KafkaConsumer.Stop();
                Logger.LogInformation("{DaemonName} has stopped", Name);
            }
        }
    }
}