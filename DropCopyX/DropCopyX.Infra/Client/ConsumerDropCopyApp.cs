using Amazon.Runtime.Internal.Util;
using DropCopyX.Application.Commands;
using DropCopyX.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SharedX.Core.Bus;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Specs;
using System.Diagnostics;

namespace DropCopyX.Infra.Client;
public class ConsumerDropCopyApp : BackgroundService
{
    private readonly ConnectionZmq _config;
    private readonly ILogger<ConsumerDropCopyApp> _logger;
    private PullSocket _receiver;
    private readonly IExecutionReportChache _cache;
    private readonly IMediatorHandler _mediator;
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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o DropCopy Consumer do ZeroMQ...");
        var listExecutions = new List<ExecutionReport>();
            
        using (_receiver = new PullSocket(_config.MatchingToDropCopy.Uri))
        {
            var timer = new Stopwatch();
            timer.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                var msg = _receiver.ReceiveMultipartBytes();
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
        _receiver.Disconnect(_config.MatchingToDropCopy.Uri);
        _logger.LogInformation("Publisher ZeroMq...Finalizado!");
    }
}
