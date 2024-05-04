using SharedX.Core.Matching.DropCopy;

namespace DropCopyX.Core.Interfaces;
public interface IExecutionReportChache
{
    void AddExecutionReport(ExecutionReport report);
    bool TryDequeueExecutionReport(out ExecutionReport report);
}