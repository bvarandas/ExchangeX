using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.ValueObjects;
namespace OrderEngineX.API.Publisher;
public class PublisherOrder : IPublisherEngine<OrderEngine>
{
    private readonly ILogger<PublisherOrder> _logger;
    private readonly IOutboxCache<OrderEngine> _cacheOutbox;

    public PublisherOrder(ILogger<PublisherOrder> logger, IOutboxCache<OrderEngine> cache)
    {
        _logger = logger;
        _cacheOutbox = cache;
    }

    public void PublishEngine(OrderEngine message, CancellationToken cancellationToken = default)
    {
        var envelope = new EnvelopeOutbox<OrderEngine>();
        envelope.Body = message;
        envelope.Id = message.OrderID;
        envelope.LastTransaction = DateTime.UtcNow;

        _cacheOutbox.TryEnqueueZeroMQEnvelope(envelope);
        _cacheOutbox.TryEnqueueRabitMQEnvelope(envelope);
    }
}
