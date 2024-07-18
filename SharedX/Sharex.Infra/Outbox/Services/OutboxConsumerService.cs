using MassTransit;
using Microsoft.Extensions.Logging;
using SharedX.Core.Interfaces;
namespace Sharedx.Infra.Outbox.Services;
public class OutboxConsumerService<T> : 
    IOutboxConsumerService<T> where T : class,
    IConsumer<T>
{
    private readonly IOutboxCache<T> _cacheOutbox;
    private readonly ILogger<OutboxConsumerService<T>> _logger;
    

    public OutboxConsumerService()
    {

    }
}
