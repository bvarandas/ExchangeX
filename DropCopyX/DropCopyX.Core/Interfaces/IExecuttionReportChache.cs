using SharedX.Core.Matching.DropCopy;

namespace DropCopyX.Core.Interfaces;
public interface IExecutionReportChache
{
    void AddExecutionReport(ExecutionReport report);
    ExecutionReport TryDequeueExecutionReport();
}