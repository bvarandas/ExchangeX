using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Matching.MarketData;
namespace MatchingX.Core.Interfaces;
public interface IMatchingCache
{
    #region MarketData
    void AddIncremental(MarketData marketData);
    bool TryDequeueMarketData(out MarketData order);
    Task<decimal> GetPrice(string symbol);
    Task<MarketData> GetMarketDataBySymbol(string symbol);
    #endregion

    #region DropCopy
    void AddExecutionReport(ExecutionReport execution);
    void AddTradeCaptureReport(TradeCaptureReport trade);
    bool TryDequeueTradeCaptureReport(out TradeCaptureReport trade);
    bool TryDequeueExecuteReport(out ExecutionReport execution);
    bool TryDequeueExecuteToOrderReport(out ExecutionReport execution);
    #endregion
}