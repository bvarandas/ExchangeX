using Amazon.Runtime.Internal.Util;
using DropCopyX.Application.Commands;
using DropCopyX.Core.Entities;
using DropCopyX.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Bus;
using SharedX.Core.Extensions;
using SharedX.Core.Proto;
using System.Diagnostics;

namespace DropCopyX.Infra.Client;
public class ConsumerDropCopyApp : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<ConsumerDropCopyApp> _logger;
    private SubscriberSocket _subscriber;
    private readonly string _addressConnect;
    private readonly string _topic;
    private readonly IExecutionReportChache _cache;
    private readonly IMediatorHandler _mediator;
    public ConsumerDropCopyApp(ILogger<ConsumerDropCopyApp> logger, IConfiguration config, IExecutionReportChache cache, IMediatorHandler mediator)
    {
        _logger = logger;
        _config = config;
        _cache = cache;
        _mediator = mediator;   
        _addressConnect = _config["ConnectionStrings:DropCopyZmqConsumer:uri"]!;
        _topic = _config["ConnectionStrings:DropCopyZmqConsumer:topic"]!;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o DropCopy Consumer do ZeroMQ...");
        var listExecutions = new List<ExecutionReport>();
            
        using (_subscriber = new SubscriberSocket())
        {
            _subscriber.Connect(_addressConnect);
            _subscriber.Subscribe(_topic);

            var timer = new Stopwatch();
            timer.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                var msg = _subscriber.ReceiveMultipartBytes(1);//.ReceiveFrameBytes();//.ReceiveFrameString();
                var execution = msg[1].DeserializeFromByteArrayProtobuf<ExecutionReport>();
                _cache.AddExecutionReport(execution);

                if (timer.Elapsed.TotalSeconds > 5)
                {
                    timer.Stop();
                    listExecutions.Add(execution);
                    _mediator.SendCommand(new ExecutionReportCommand(listExecutions));
                    listExecutions.Clear();
                }

                Thread.Sleep(10);
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
