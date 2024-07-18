using MassTransit.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Bus;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
using TradeEngineX.Core.Interfaces;
using SharedX.Core.Extensions;
using MassTransit;
using SharedX.Core.ValueObjects;
using SharedX.Core.Interfaces;
using QuickFix.Fields;
using SharedX.Core.Matching;

namespace TradeEngineX.ServerApp.Consumer;
public class ConsumerTradeEngineApp : BackgroundService, IConsumer<EnvelopeOutbox<TradeReport>>
{
    private readonly ILogger<ConsumerTradeEngineApp> _logger;
    private PullSocket _receiver;
    private readonly ConnectionZmq _config;
    private static Thread ThreadReceiverTrade = null!;
    private readonly IMediatorHandler _mediator;
    private readonly ITradeEngineCache _tradeCache;
    private readonly IOutboxBackgroundService<TradeReport> _outboxBackgroundService = null!;
    public ConsumerTradeEngineApp(ILogger<ConsumerTradeEngineApp> logger,
        IOptions<ConnectionZmq> options,
        IMediatorHandler mediator,
        ITradeEngineCache tradeCache,
        IOutboxBackgroundService<TradeReport> outboxBackgroundService)
    {
        _logger = logger;
        _tradeCache = tradeCache;
        _mediator = mediator;
        _config = options.Value;
        _outboxBackgroundService = outboxBackgroundService; 
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing o Trade receiver ZeroMQ...");
        ThreadReceiverTrade = new Thread(() => ReceiverTrades(cancellationToken));
        ThreadReceiverTrade.Name = nameof(ThreadReceiverTrade);
        ThreadReceiverTrade.Start();

        return Task.CompletedTask;
    }

    private void ReceiverTrades(CancellationToken stoppingToken)
    {
        bool isConnected = false;

        do
        {
            try
            {
                _logger.LogInformation("Iniciando o Trade receiver do ZeroMQ...");

                using (_receiver = new PullSocket(">" + _config.MatchingToTradeEngine.Uri))
                {
                    _logger.LogInformation("Receiver de Trade Conectado!!!");
                    isConnected = true;

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var msg = _receiver.ReceiveMultipartBytes();
                        var trade = msg[1].DeserializeFromByteArrayProtobuf<TradeReport>();
                        var deleted = _outboxBackgroundService.DeleteOutboxCacheAsync(trade, trade.TradeId);
                        
                        if (!deleted.IsFaulted && deleted.IsCompleted)
                            _tradeCache.UpsertTradeEngineAsync(trade);
                    }
                }
            }catch(Exception ex)
            {
                isConnected = false;
                _logger.LogError(ex.Message, ex);
            }
        }while (!isConnected);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finishing the Consumer ZeroMQ...");
        _receiver.Disconnect(_config.OrderEntryToOrderEngine.Uri);
        _logger.LogInformation("Consumer ZeroMq...Finishing!");
        return base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<EnvelopeOutbox<TradeReport>> context)
    {
        var trade = context.Message.Body;

        var deleted = _outboxBackgroundService.DeleteOutboxCacheAsync(trade, trade.TradeId);

        if (!deleted.IsFaulted && deleted.IsCompleted)
            _tradeCache.UpsertTradeEngineAsync(trade);

        return Task.CompletedTask;
    }
}
