using SharedX.Core.Entities;
using SharedX.Core.Interfaces;
using SharedX.Core.ValueObjects;
namespace OrderEngineX.API.Publisher;
public class PublisherOrderReport : IPublisherEngine<ReportFix>
{
    private readonly ILogger<PublisherOrderReport> _logger;
    private readonly IOutboxCache<ReportFix> _cacheOutbox;
    public PublisherOrderReport(ILogger<PublisherOrderReport> logger, IOutboxCache<ReportFix> cacheOutbox)
    {
        _logger = logger;
        _cacheOutbox = cacheOutbox;
    }
    public void PublishEngine(ReportFix message, CancellationToken cancellationToken = default)
    {
        var envelope = new EnvelopeOutbox<ReportFix>();
        envelope.Body = message;
        envelope.Id = message.ExecId;
        envelope.LastTransaction = DateTime.UtcNow;

        _cacheOutbox.TryEnqueueZeroMQEnvelope(envelope);
        _cacheOutbox.TryEnqueueRabitMQEnvelope(envelope);
    }
}