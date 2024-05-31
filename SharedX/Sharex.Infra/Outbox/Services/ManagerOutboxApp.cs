using Microsoft.Extensions.Logging;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using MassTransit;
using SharedX.Core.Extensions;
using SharedX.Core.ValueObjects;
namespace Sharedx.Infra.Outbox.Services;
public class ManagerOutboxApp<T> where T :class ,  IManagerOutboxApp<T>
{
    private readonly ILogger<ManagerOutboxApp<T>> _logger;
    private static Thread ThreadReceiverActivity = null!;
    private readonly IOrderOutboxCache<T> _cacheOutbox;
    private readonly ManualResetEvent _manualResetOutbox = null!;
    private static ActivityOutbox _activityOutbox = null!;
    private readonly CancellationTokenSource _source = new CancellationTokenSource();
    private readonly IBus _bus;
    
    public ManagerOutboxApp(ILogger<ManagerOutboxApp<T>> logger,
        IOrderOutboxCache<T> cache, 
        IBus bus)
    {
        _logger = logger;
        _cacheOutbox = cache;
        _manualResetOutbox = new ManualResetEvent(false);
        _source = new CancellationTokenSource();
        _bus = bus;

        StartAsync(_source.Token);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing o receiver Activities ZeroMQ...");
        ThreadReceiverActivity = new Thread(() => ReceiverActivity(cancellationToken));
        ThreadReceiverActivity.Name = nameof(ThreadReceiverActivity);
        ThreadReceiverActivity.Start();

        _manualResetOutbox.Set();

        return Task.CompletedTask; 
    }
    public void SetActivity(ActivityOutbox activity)
    {
        _logger.LogInformation($"Setando a activity para {activity.Activity} e a próxima para {activity.NextActivity}");
        _activityOutbox = activity;
    }

    public async Task<bool> AddActivityAsync(EnvelopeOutbox<T> envelope)
    {
        this.SetActivity( envelope.ActivityOutbox);

        _logger.LogInformation($"Adicionando a activity  {envelope.ActivityOutbox.Activity} e a próxima para {envelope.ActivityOutbox.NextActivity}");
        var result = await _cacheOutbox.UpsertOutboxAsync(envelope);
        return result;
    }

    private async void ReceiverActivity(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Receiver de Activities Conectado!!!");

        while (!stoppingToken.IsCancellationRequested)
        {
            var message =await _cacheOutbox.GetOutboxByActivityAsync(_activityOutbox.Activity);

            if(message.IsSuccess)
                foreach (var item in message.Value)
                {
                    var envelope = item.Value;
                    
                    if(IsActivityNotSent(envelope))
                        SendActivityQueueAsync(envelope, stoppingToken);
                }

            Thread.Sleep(100);
        }
    }

    private bool IsActivityNotSent(EnvelopeOutbox<T> envelope)
    {
        var now = DateTime.Now;
        return now.Subtract(envelope.LastTransaction).TotalSeconds > 1;
    }

    private async void SendActivityQueueAsync(EnvelopeOutbox<T> envelope,  CancellationToken cancellationToken)
    {
        var endpoint = await _bus.GetSendEndpoint(new Uri(_activityOutbox.NextActivity));
        
        var message = envelope.SerializeToByteArrayProtobuf<EnvelopeOutbox<T>>();

        await endpoint.Send(message, cancellationToken);

        //await _cacheOutbox.DeleteOutboxAsync(_activityOutbox.activity, order.OrderID);
    }

    ~ManagerOutboxApp()
    {
        _logger.LogInformation("Finalizando o objeto de ConsumerOutboxCacheApp");
        _source.Cancel();
    }
}
