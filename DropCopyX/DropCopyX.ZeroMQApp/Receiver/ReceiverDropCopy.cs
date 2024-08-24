using DropCopyX.Application.Commands;
using DropCopyX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.DropCopy;
namespace DropCopyX.ServerApp.Receiver;
public class ReceiverDropCopy : IReceiverEngine<ExecutionReport>
{
    private readonly ILogger<ReceiverDropCopy> _logger;
    private readonly IExecutionReportChache _cache;
    private readonly IMediatorHandler _mediator;
    public ReceiverDropCopy(ILogger<ReceiverDropCopy> logger,
        IExecutionReportChache cache,
        IMediatorHandler mediator,
        IOutboxCache<ExecutionReport> outboxCache) 
    {
        _logger = logger;
        _cache = cache;
        _mediator = mediator;
    }
    
    public void ReceiveEngine(ExecutionReport message, CancellationToken cancellationToken)
    {
        var listExecutions = new List<ExecutionReport>();

        _cache.AddExecutionReport(message);
        listExecutions.Add(message);
        _mediator.Send(new ExecutionReportCommand(listExecutions));
    }
}