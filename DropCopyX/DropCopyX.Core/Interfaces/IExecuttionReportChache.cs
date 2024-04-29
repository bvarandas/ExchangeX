using SharedX.Core.Proto;
namespace DropCopyX.Core.Interfaces;
public interface IExecutionReportChache
{
    void AddExecutionReport(ExecutionReport report);
    ExecutionReport TryDequeueExecutionReport();
}