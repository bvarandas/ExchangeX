using SharedX.Core.Matching.DropCopy;
namespace MatchingX.Core.Interfaces;
public interface IDropCopyCache
{
    void AddExecutionReport(ExecutionReport execution);
    void AddTradeCaptureReport(TradeCaptureReport trade);
    bool TryDequeueTradeCaptureReport(ref TradeCaptureReport trade);
    bool TryDequeueExecuteReport(ref ExecutionReport execution);
}