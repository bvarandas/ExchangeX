using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SecurityX.Core.Interfaces;
using SharedX.Core;
using SharedX.Core.Bus;
using SharedX.Core.Entities;
using SharedX.Core.Extensions;
using SharedX.Core.Specs;
namespace Security.API.Consumer;
public class ConsumerSecurityApp: BackgroundService
{
    private readonly ILogger<ConsumerSecurityApp> _logger;
    private RequestSocket _receiver;
    private readonly ConnectionZmq _config;
    private static Thread ThreadReceiverSecurity = null!;
    private readonly IMediatorHandler _mediator;
    private readonly ISecurityCache _securityCache;
    public ConsumerSecurityApp(ILogger<ConsumerSecurityApp> logger,
        IOptions<ConnectionZmq> options,
        IMediatorHandler mediator,
        ISecurityCache securityCache)
    {
        _logger = logger;
        _config = options.Value;
        _mediator = mediator;
        _securityCache = securityCache;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing o receiver Orders ZeroMQ...");
        ThreadReceiverSecurity = new Thread(() => ReceiverOrders(cancellationToken));
        ThreadReceiverSecurity.Name = nameof(ThreadReceiverSecurity);
        ThreadReceiverSecurity.Start();

        return Task.CompletedTask;
    }

    private void ReceiverOrders(CancellationToken stoppingToken)
    {
        using (_receiver = new RequestSocket(_config.OrderEntryToOrderEngine.Uri))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = _receiver.ReceiveFrameString();
                switch(message)
                {
                    case RequestTypeSecurity.List:
                        {
                            var dicSecurity = _securityCache.GetAllSecurityAsync().Result.Value;
                            _receiver.SendFrame(dicSecurity.SerializeToByteArrayProtobuf<Dictionary<string, SecurityEngine>>());
                        }
                        break;
                    case RequestTypeSecurity.Status:
                        {
                            var dicSecurity = _securityCache.GetAllSecurityAsync().Result.Value;
                            _receiver.SendFrame(dicSecurity.SerializeToByteArrayProtobuf<Dictionary<string, SecurityEngine>>());
                        }
                        break;
                }
                
                Thread.Sleep(10);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finishing the publisher ZeroMQ...");
        _receiver.Disconnect(_config.OrderEntryToOrderEngine.Uri);
        _logger.LogInformation("Publisher ZeroMq...Finishing!");
        return base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}