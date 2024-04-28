using DropCopyX.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Extensions; 
namespace DropCopyX.Infra.Client;

public class ConsumerDropCopyApp : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<ConsumerDropCopyApp> _logger;
    private SubscriberSocket _subscriber;
    private readonly string _addressConnect;
    public ConsumerDropCopyApp(ILogger<ConsumerDropCopyApp> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        _addressConnect = _config["ConnectionStrings:DropCopyZmqConsumer"];
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o DropCopy Consumer do ZeroMQ...");

        using (_subscriber = new SubscriberSocket())
        {
            _subscriber.Connect(_addressConnect);
            _subscriber.Subscribe("A");
            //subscriber.Subscribe("B");

            while (!stoppingToken.IsCancellationRequested)
            {
                //var topic = subscriber.ReceiveFrameString();
                var msg = _subscriber.ReceiveMultipartBytes(1);//.ReceiveFrameBytes();//.ReceiveFrameString();
                var trade = msg[1].DeserializeFromByteArrayProtobuf<ExecutionReport>();

                //Console.WriteLine("From Publisher tipo mensagem {0}, MaxFloor {1}, account {2}, Balance {3}, Latency {4} Nanoseconds ",
                //    trade.messageType,
                //    trade.MaxFloor,
                //    trade.Account,
                //    trade.Balance, 
                //    latency);
            }
        }

        return Task.CompletedTask;
    }

    public async override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o publisher ZeroMQ...");
        _subscriber.Disconnect(_addressConnect);
        _logger.LogInformation("Publisher ZeroMq...Finalizado!");
    }
}
