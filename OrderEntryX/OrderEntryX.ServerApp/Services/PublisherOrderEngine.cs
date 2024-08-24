using Microsoft.Extensions.Logging;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.ValueObjects;

namespace OrderEntryX.ServerApp.Services;

public class PublisherOrderEngine : IPublisherEngine<OrderEngine>
{
    private readonly ILogger<PublisherOrderEngine> _logger;
    private readonly IOutboxCache<OrderEngine> _cacheOutbox;

    public PublisherOrderEngine(ILogger<PublisherOrderEngine> logger, IOutboxCache<OrderEngine> cacheOutbox)
    {
        _logger = logger;
        _cacheOutbox = cacheOutbox;
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
