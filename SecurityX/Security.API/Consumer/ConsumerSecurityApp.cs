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
public class ConsumerSecurityApp : BackgroundService
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

    //public override Task StartAsync(CancellationToken cancellationToken)
    //{
    //    _logger.LogInformation("Initializing o receiver Securities ZeroMQ...");
    //    ThreadReceiverSecurity = new Thread(() => ReceiverSecurities(cancellationToken));
    //    ThreadReceiverSecurity.Name = nameof(ThreadReceiverSecurity);
    //    ThreadReceiverSecurity.Start();

    //    return Task.CompletedTask;
    //}

    private void ReceiverSecurities(CancellationToken stoppingToken)
    {
        bool isConnected = false;
        do
        {
            try
            {
                _logger.LogInformation("Iniciando o Securities receiver do ZeroMQ...");
                using (_receiver = new RequestSocket(_config.ReceiverEngine.Uri))
                {
                    _logger.LogInformation("Receiver de Securities Conectado!!!");
                    isConnected = true;

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var message = _receiver.ReceiveFrameString();
                        switch (message)
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
            catch (Exception ex)
            {
                isConnected = false;
                _logger.LogError(ex.Message, ex);
            }
        } while (!isConnected);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finishing the consumer ZeroMQ...");
        _receiver.Disconnect(_config.ReceiverEngine.Uri);
        _logger.LogInformation("Consumer ZeroMq...Finishing!");
        return base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}