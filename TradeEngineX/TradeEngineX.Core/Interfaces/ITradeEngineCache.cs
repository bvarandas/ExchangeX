using FluentResults;
using SharedX.Core.Matching.DropCopy;
namespace TradeEngineX.Core.Interfaces;
public interface ITradeEngineCache
{
    bool TryDequeueTradeEngine(out TradeReport trade);
    Task UpsertTradeEngineAsync(TradeReport trade);
    Task RemoveTradeEngineAsync(TradeReport trade);
    Task<Result<TradeReport>> GetTradeByIdAsync(long tradeId);
}