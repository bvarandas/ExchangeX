using MatchingX.Core.Filters;
using SharedX.Core.Matching.DropCopy;

namespace MatchingX.Core.Repositories;
public interface IExecutedTradeRepository
{
    Task<IEnumerable<TradeCaptureReport>> GetExecutedTradeAsync(ExecutedTradeParams specParams, CancellationToken cancellation);
    Task<bool> CreateExecutedTradeAsync((TradeCaptureReport, TradeCaptureReport) executedTrade, CancellationToken cancellation);
    Task<bool> CreateExecutedTradesAsync(List<TradeCaptureReport> executedTrades, CancellationToken cancellationToken);
}