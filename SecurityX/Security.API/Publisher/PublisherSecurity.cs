using SharedX.Core.Entities;
using SharedX.Core.Interfaces;
using SharedX.Core.ValueObjects;

namespace Security.API.Publisher;

public class PublisherSecurity : IPublisherEngine<SecurityEngine>
{
    private readonly ILogger<PublisherSecurity> _logger;
    private readonly IOutboxCache<SecurityEngine> _cacheOutbox;

    public PublisherSecurity(ILogger<PublisherSecurity> logger, IOutboxCache<SecurityEngine> cacheOutbox)
    {
        _logger = logger;
        _cacheOutbox = cacheOutbox;
    }

    public void PublishEngine(SecurityEngine message, CancellationToken cancellationToken = default)
    {
        var envelope = new EnvelopeOutbox<SecurityEngine>();
        envelope.Body = message;
        envelope.Id = new Random().Next(10, 100000);
        envelope.LastTransaction = DateTime.UtcNow;

        _cacheOutbox.TryEnqueueZeroMQEnvelope(envelope);
        _cacheOutbox.TryEnqueueRabitMQEnvelope(envelope);
    }
}
