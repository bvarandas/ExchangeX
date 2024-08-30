using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SecurityX.Core.Interfaces;
using SharedX.Core;
using SharedX.Core.Extensions;
using SharedX.Core.Specs;
namespace Security.API.Receiver;
public class ReceiverSecurity : BackgroundService
{
    private readonly ILogger<ReceiverSecurity> _logger;
    private RequestSocket _receiver;
    private readonly ConnectionZmq _config;
    private static Thread ThreadReceiverSecurity = null!;
    private readonly ISecurityCache _securityCache;
    public ReceiverSecurity(ILogger<ReceiverSecurity> logger,
        IOptions<ConnectionZmq> options,
        ISecurityCache securityCache)
    {
        _logger = logger;
        _config = options.Value;
        _securityCache = securityCache;
    }

    public async override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing o receiver Securities ZeroMQ...");
        ThreadReceiverSecurity = new Thread(() => ReceiverSecuritiesAsync(cancellationToken));
        ThreadReceiverSecurity.Name = nameof(ThreadReceiverSecurity);
        ThreadReceiverSecurity.Start();
    }

    private async Task ReceiverSecuritiesAsync(CancellationToken stoppingToken)
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
                                    var dicSecurity = await _securityCache.GetAllSecurityAsync();
                                    _receiver.SendFrame(dicSecurity.SerializeToByteArrayProtobuf());
                                }
                                break;
                            case RequestTypeSecurity.Status:
                                {
                                    var dicSecurity = await _securityCache.GetAllSecurityAsync();
                                    _receiver.SendFrame(dicSecurity.SerializeToByteArrayProtobuf());
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