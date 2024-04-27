using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ.Sockets;
using NetMQ;
//using SharedX.Core;

namespace DropCopyX.ZeroMQApp.Services;
public class PublicherZeroMqApp : BackgroundService
{
    private readonly ILogger<PublicherZeroMqApp> _logger;
    public PublicherZeroMqApp(ILogger<PublicherZeroMqApp> logger)
    {
        _logger = logger;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o publisher do ZeroMQ...");

        using (var publisher = new PublisherSocket())
        {
            publisher.Bind("tcp://*:5555");
            int i = 0;
            SpinWait spin = new SpinWait();
            while (!stoppingToken.IsCancellationRequested)
            {
                //publisher
                //   .SendMoreFrame("A") // Topic
                //   .SendFrame(i.ToString()); // Message
                //var message = "Char Sui".SerializeToByteArrayProtobuf<string>();
                    //trade.SerializeToByteArrayProtobuf<Trade>();

                //publisher
                //    .SendMoreFrame("A")
                //    .SendMultipartBytes(message);


                //publisher
                //    .SendMoreFrame("B")
                //    .SendMultipartBytes(message);

                i++;

                if ((i % 10000) == 0)
                    //    spin.SpinOnce(1);
                    Thread.Sleep(1);
            }
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5000, stoppingToken);
        }
    }

    public async override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o publisher ZeroMQ...");

        
        _logger.LogInformation("Publisher ZeroMq...Finalizado!");
    }
}
