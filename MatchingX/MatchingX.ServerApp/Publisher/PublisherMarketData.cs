using Microsoft.Extensions.Logging;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.MarketData;
using SharedX.Core.ValueObjects;
namespace MatchingX.ServerApp.Publisher;
public class PublisherMarketData : IPublisherEngine<MarketData>
{
    private readonly ILogger<PublisherMarketData> _logger;
    private readonly IOutboxCache<MarketData> _cacheOutbox;

    public PublisherMarketData(ILogger<PublisherMarketData> logger, IOutboxCache<MarketData> cacheOutbox)
    {
        _logger = logger;
        _cacheOutbox = cacheOutbox;
    }

    public void PublishEngine(MarketData message, CancellationToken cancellationToken = default)
    {
        var envelope = new EnvelopeOutbox<MarketData>();
        envelope.Body = message;
        envelope.Id = message.Id;
        envelope.LastTransaction = DateTime.UtcNow;

        _cacheOutbox.TryEnqueueZeroMQEnvelope(envelope);
        _cacheOutbox.TryEnqueueRabitMQEnvelope(envelope);
    }
}