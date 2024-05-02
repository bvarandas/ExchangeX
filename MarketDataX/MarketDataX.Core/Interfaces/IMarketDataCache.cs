using SharedX.Core.Matching.MarketData;
namespace MarketDataX.Core.Interfaces;
public interface IMarketDataChache
{
    void AddMarketData(MarketData marketData);
    MarketData TryDequeueMarketData();
}