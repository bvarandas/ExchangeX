using MarketDataX.Core.Interfaces;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using Sharedx.Infra.Outbox.Services;
using SharedX.Core.Bus;
using SharedX.Core.Entities;
using SharedX.Core.Extensions;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
namespace MarketDataX.ServerApp.Consumer;
public class ConsumerMarketDataApp : OutboxBackgroundService<MarketData>, IHostedService
{
    private readonly ConnectionZmq _config;
    private readonly ILogger<ConsumerMarketDataApp> _logger;
    private PullSocket _receiver;
    private readonly IMarketDataChache _cache;
    private readonly IMediatorHandler _mediator;
    private static Thread ThreadReceiverMarketData = null!;
    public ConsumerMarketDataApp(ILogger<ConsumerMarketDataApp> logger, 
        IOptions<ConnectionZmq> options, 
        IMarketDataChache cache, 
        IMediatorHandler mediator,
        IOutboxCache<MarketData> outboxCache,
        IBus bus) : base(logger, outboxCache, bus)
    {
        _logger = logger;
        _config = options.Value;
        _cache = cache;
        _mediator = mediator;   
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Receiver Marketdata ZeroMQ...");

        ThreadReceiverMarketData = new Thread(() => ReceiverMarketData(cancellationToken));
        ThreadReceiverMarketData.Name = nameof(ThreadReceiverMarketData);
        ThreadReceiverMarketData.Start();
        
        return Task.CompletedTask;
    }

    protected Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
    public async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o consumer de marketdata ZeroMQ...");
        _receiver.Disconnect(_config.MatchingToMarketData.Uri);
        _logger.LogInformation("Consumer de marketdata ZeroMq...Finalizado!");
    }

    private void ReceiverMarketData(CancellationToken stoppingToken)
    {
        bool isConnected = false;

        do
        {
            try
            {
                _logger.LogInformation($"Receiver de marketdata tentando conectar..{_config.MatchingToMarketData.Uri}");
                using (_receiver = new PullSocket(">"+ _config.MatchingToMarketData.Uri))
                {
                    _logger.LogInformation("Receiver de marketdata Conectado!!!");
                    isConnected = true;

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var message = _receiver.ReceiveMultipartBytes()[0];
                        var marketData = message.DeserializeFromByteArrayProtobuf<MarketData>();
                        _cache.AddMarketDataIncremental(marketData);
                        _cache.AddMarketDataBook(marketData);

                        DeleteOutboxCacheAsync(marketData, marketData.Id);
                            
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ex)
            {
                isConnected = false;
                _logger.LogError(ex.Message, ex);
            }

            Thread.Sleep(100);
        } while (!isConnected);
    }
}