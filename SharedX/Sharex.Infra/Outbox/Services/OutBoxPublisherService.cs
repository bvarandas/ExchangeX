using MassTransit;
using Microsoft.Extensions.Logging;
using SharedX.Core.Interfaces;
namespace Sharedx.Infra.Outbox.Services;
public class OutBoxPublisherService<T> : 
    IOutboxPublisherService<T> where T : class
{
    private readonly IOutboxCache<T> _cacheOutbox;
    private readonly ILogger<OutBoxPublisherService<T>> _logger;
    private readonly IBus _bus;

    public OutBoxPublisherService(
        ILogger<OutBoxPublisherService<T>> logger,
        IOutboxCache<T> cacheOutbox,
        IBus bus)
    {
        _cacheOutbox = cacheOutbox;
        _logger = logger;
        _bus = bus;
    }
}
