using SharedX.Core.Matching.DropCopy;
namespace MatchingX.Core.Interfaces;
public interface IDropCopyCache
{
    void AddExecutionReport(ExecutionReport execution);
    void AddTradeCaptureReport(TradeCaptureReport trade);
    bool TryDequeueTradeCaptureReport(out TradeCaptureReport trade);
    bool TryDequeueExecuteReport(out ExecutionReport execution);
}