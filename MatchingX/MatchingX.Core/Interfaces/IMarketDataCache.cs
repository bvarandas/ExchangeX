using SharedX.Core.Matching;
using SharedX.Core.Matching.MarketData;
namespace MatchingX.Core.Interfaces;
public interface IMarketDataCache
{
    void AddIncremental(MarketData marketData);
    bool TryDequeueMarketData(out MarketData order);
}