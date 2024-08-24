using Microsoft.Extensions.Logging;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.ValueObjects;

namespace MatchingX.ServerApp.Publisher;

public class PublisherDropCopy : IPublisherEngine<ExecutionReport>
{
    private readonly ILogger<PublisherMarketData> _logger;
    private readonly IOutboxCache<ExecutionReport> _cacheOutbox;

    public PublisherDropCopy(ILogger<PublisherMarketData> logger, IOutboxCache<ExecutionReport> cacheOutbox)
    {
        _logger = logger;
        _cacheOutbox = cacheOutbox;
    }

    public void PublishEngine(ExecutionReport message, CancellationToken cancellationToken = default)
    {
        var envelope = new EnvelopeOutbox<ExecutionReport>();
        envelope.Body = message;
        envelope.Id = message.ExecID;
        envelope.LastTransaction = DateTime.UtcNow;

        _logger.LogInformation("");

        _cacheOutbox.TryEnqueueZeroMQEnvelope(envelope);
        _cacheOutbox.TryEnqueueRabitMQEnvelope(envelope);
    }
}
