using MarketDataX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ.Sockets;
using SharedX.Core.Bus;
using SharedX.Core.Specs;
namespace MarketDataX.ServerApp.Consumer;
public class ConsumerSecurityApp : BackgroundService
{
    private readonly ConnectionZmq _config;
    private readonly ILogger<ConsumerSecurityApp> _logger;
    private PullSocket _receiver;
    private readonly IMediatorHandler _mediator;
    private static Thread ThreadReceiverSecurity = null!;

    public ConsumerSecurityApp(ILogger<ConsumerSecurityApp> logger,
        IOptions<ConnectionZmq> options,
        IMarketDataChache cache,
        IMediatorHandler mediator)
    {
        _logger = logger;
        _config = options.Value;
        //_cache = cache;
        _mediator = mediator;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}
