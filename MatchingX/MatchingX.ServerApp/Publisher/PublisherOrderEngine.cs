using Microsoft.Extensions.Logging;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.ValueObjects;
namespace MatchingX.ServerApp.Publisher;
public class PublisherOrderEngine : IPublisherEngine<ExecutionReport>
{
    private readonly ILogger<PublisherOrderEngine> _logger;
    private readonly IOutboxCache<ExecutionReport> _cacheOutbox;

    public PublisherOrderEngine(ILogger<PublisherOrderEngine> logger, IOutboxCache<ExecutionReport> cacheOutbox)
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

        _cacheOutbox.TryEnqueueZeroMQEnvelope(envelope);
        _cacheOutbox.TryEnqueueRabitMQEnvelope(envelope);
    }
}