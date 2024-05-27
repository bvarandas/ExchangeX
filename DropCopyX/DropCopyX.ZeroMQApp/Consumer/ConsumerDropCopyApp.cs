using DropCopyX.Application.Commands;
using DropCopyX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Bus;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
namespace DropCopyX.Infra.Client;
public class ConsumerDropCopyApp : BackgroundService
{
    private readonly ConnectionZmq _config;
    private readonly ILogger<ConsumerDropCopyApp> _logger;
    private PullSocket _receiver;
    private readonly IExecutionReportChache _cache;
    private readonly IMediatorHandler _mediator;
    private static Thread ThreadReceiverDropCopy = null!;
    public ConsumerDropCopyApp(ILogger<ConsumerDropCopyApp> logger, 
        IOptions<ConnectionZmq>  options, 
        IExecutionReportChache cache, 
        IMediatorHandler mediator)
    {
        _logger = logger;
        _config = options.Value;
        _cache = cache;
        _mediator = mediator;
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Receiver Marketdata ZeroMQ...");

        ThreadReceiverDropCopy = new Thread(() => ReceiverDropCopy(cancellationToken));
        ThreadReceiverDropCopy.Name = nameof(ThreadReceiverDropCopy);
        ThreadReceiverDropCopy.Start();
        return base.StartAsync(cancellationToken);
    }
    private void ReceiverDropCopy(CancellationToken stoppingToken)
    {
        bool isConnected = false;
        var listExecutions = new List<ExecutionReport>();

        do
        {
            try
            {
                _logger.LogInformation("Iniciando o DropCopy receiver do ZeroMQ...");

                using (_receiver = new PullSocket(_config.MatchingToDropCopy.Uri))
                {
                    _logger.LogInformation("Receiver de Dropcopy Conectado!!!");
                    isConnected = true;

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var msg = _receiver.ReceiveMultipartBytes();
                        var execution = msg[1].DeserializeFromByteArrayProtobuf<ExecutionReport>();

                        _cache.AddExecutionReport(execution);
                        _mediator.SendCommand(new ExecutionReportCommand(listExecutions));

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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public async override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o receiver de dropcopy ZeroMQ...");
        _receiver.Disconnect(_config.MatchingToDropCopy.Uri);
        _logger.LogInformation("Receiver de dropcopy ZeroMq...Finalizado!");
    }
}
