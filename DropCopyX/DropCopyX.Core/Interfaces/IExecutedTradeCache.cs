using SharedX.Core.Matching.DropCopy;

namespace DropCopyX.Core.Interfaces;
public interface IExecutedTradeCache
{
    long GetLastTradeId();
    void SetLastTradeId(long tradeId);
    void AddExecutionReport(TradeCaptureReport trade);
    bool TryDequeueuExecutionReport(out TradeCaptureReport executionReport);
    Task<Dictionary<long, TradeCaptureReport>> GetSnapShotTradeCaptureReport();
}