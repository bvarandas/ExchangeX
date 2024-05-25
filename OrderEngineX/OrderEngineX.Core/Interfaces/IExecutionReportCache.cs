using SharedX.Core.Matching.DropCopy;
namespace OrderEngineX.Core.Interfaces;
public interface IExecutionReportCache
{
    bool TryDequeueExecutionReport(out ExecutionReport execution);
    void AddQueueExecutionReport(ExecutionReport execution);
}
