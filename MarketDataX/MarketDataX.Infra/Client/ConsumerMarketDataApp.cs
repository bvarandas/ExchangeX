using MarketDataX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Bus;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.Specs;
namespace MarketDataX.Infra.Client;
public class ConsumerMarketDataApp : BackgroundService
{
    private readonly ConnectionZmq _config;
    private readonly ILogger<ConsumerMarketDataApp> _logger;
    private PullSocket _receiver;
    private readonly IMarketDataChache _cache;
    private readonly IMediatorHandler _mediator;
    public ConsumerMarketDataApp(ILogger<ConsumerMarketDataApp> logger, 
        IOptions<ConnectionZmq> options, 
        IMarketDataChache cache, 
        IMediatorHandler mediator)
    {
        _logger = logger;
        _config = options.Value;
        _cache = cache;
        _mediator = mediator;   
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o MarketData Consumer do ZeroMQ...");
        var listExecutions = new List<MarketData>();
            
        using (_receiver = new PullSocket(_config.MatchingToMarketData.Uri))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var msg = _receiver.ReceiveMultipartBytes(1);//.ReceiveFrameBytes();//.ReceiveFrameString();
                var marketData = msg[1].DeserializeFromByteArrayProtobuf<MarketData>();
                _cache.AddMarketData(marketData);
                Thread.Sleep(10);
            }
        }
        return Task.CompletedTask;
    }
    public async override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o consumer ZeroMQ...");
        _receiver.Disconnect(_config.MatchingToMarketData.Uri);
        _logger.LogInformation("Consumer ZeroMq...Finalizado!");
    }
}
