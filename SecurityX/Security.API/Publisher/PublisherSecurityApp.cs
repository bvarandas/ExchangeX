﻿using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using SecurityX.Core.Interfaces;
using SharedX.Core.Entities;
using SharedX.Core.Extensions;
using SharedX.Core.Specs;
namespace Security.API.Publisher;
public class PublisherSecurityApp : BackgroundService
{
    private readonly ILogger<PublisherSecurityApp> _logger;
    private PushSocket _sender;
    private readonly ConnectionZmq _config;
    private readonly ISecurityCache _cache;
    private static Thread TreadSenderSecurity = null!;
    
    public PublisherSecurityApp(ILogger<PublisherSecurityApp> logger,
        IOptions<ConnectionZmq> options, ISecurityCache cache)
    {
        _logger = logger;
        _config = options.Value;
        _cache = cache;
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Sender Securities ZeroMQ...");

        TreadSenderSecurity = new Thread(() => SenderSecurity(cancellationToken));
        TreadSenderSecurity.Name = nameof(TreadSenderSecurity);
        TreadSenderSecurity.Start();

        return base.StartAsync(cancellationToken);
    }

    private void SenderSecurity(CancellationToken stoppingToken)
    {
        using (_sender = new PushSocket("@"+_config.SecurityToMarketData.Uri))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (_cache.TryDequeueSecurity(out var security))
                {
                    var message = security.SerializeToByteArrayProtobuf<SecurityEngine>();
                    _sender.SendMultipartBytes(message);
                }
                Thread.Sleep(10);
            }
        }
    }
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalizando o Sender Securities ZeroMQ...");
        _sender.Disconnect(_config.SecurityToMarketData.Uri);
        return base.StopAsync(cancellationToken);
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
