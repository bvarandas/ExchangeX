using SharedX.Core.Matching.DropCopy;
using FluentResults;
namespace TradeEngineX.Core.Interfaces;
public interface ITradeEngineRepository
{
    Task<Result<Dictionary<long, TradeReport>>> GetAllTodayTradesAsync(CancellationToken cancellation);
    Task<Result<TradeReport>> GetTradeAsync(long tradeId, CancellationToken cancellation);
    Task<Result> UpsertTradeAsync(TradeReport trade, CancellationToken cancellationToken);
    Task<Result> RemoveTradeAsync(TradeReport trade, CancellationToken cancellationToken);
}