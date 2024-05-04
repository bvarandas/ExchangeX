using Microsoft.Extensions.Logging;
using OrderEngineX.Core.Interfaces;
using OrderEngineX.Infra.Publisher;
using SharedX.Core.Matching.DropCopy;
using System.Collections.Concurrent;
namespace OrderEngineX.Infra.Cache;
public class ExecutionReportCache : IExecutionReportCache
{
    private readonly ConcurrentQueue<ExecutionReport> _executionReportQueue;
    private readonly ILogger<ConsumerExecutionReportApp> _logger;

    public ExecutionReportCache()
    {
        _executionReportQueue = new ConcurrentQueue<ExecutionReport>();
    }
    public void AddQueueExecutionReport(ExecutionReport execution)
    {
        _executionReportQueue.Enqueue(execution);
    }

    public bool TryDequeueExecutionReport(out ExecutionReport execution)
    {
        execution = default;
        if (_executionReportQueue.TryDequeue(out ExecutionReport executionFound))
        {
            execution = executionFound;
            return true;
        }
        return false;
    }
}