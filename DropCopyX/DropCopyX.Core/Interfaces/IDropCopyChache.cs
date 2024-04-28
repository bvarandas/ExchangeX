using DropCopyX.Core.Entities;
namespace DropCopyX.Core.Interfaces;
public interface IDropCopyChache
{
    void AddExecutionReport(ExecutionReport report);
    ExecutionReport TryDequeueExecutionReport();
    void Clear();
}