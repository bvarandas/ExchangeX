using Microsoft.Extensions.Logging;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.ValueObjects;

namespace MatchingX.ServerApp.Publisher;

public class PublisherTradeEngine : IPublisherEngine<TradeReport>
{
    private readonly ILogger<PublisherTradeEngine> _logger;
    private readonly IOutboxCache<TradeReport> _cacheOutbox;

    public PublisherTradeEngine(ILogger<PublisherTradeEngine> logger, IOutboxCache<TradeReport> cacheOutbox)
    {
        _logger = logger;
        _cacheOutbox = cacheOutbox;
    }
    public void PublishEngine(TradeReport message, CancellationToken cancellationToken = default)
    {
        var envelope = new EnvelopeOutbox<TradeReport>();
        envelope.Body = message;
        envelope.Id = message.TradeId;
        envelope.LastTransaction = DateTime.UtcNow;

        _cacheOutbox.TryEnqueueZeroMQEnvelope(envelope);
        _cacheOutbox.TryEnqueueRabitMQEnvelope(envelope);
    }
}
