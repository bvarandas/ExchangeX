using MatchingX.Core.Filters;
using SharedX.Core.Proto;

namespace MatchingX.Core.Repositories;
public interface IExecutedTradeRepository
{
    Task<IEnumerable<ExecutedTrade>> GetExecutedTradeAsync(ExecutedTradeParams specParams, CancellationToken cancellation);
    Task<bool> CreateExecutedTradeAsync(ExecutedTrade executedTrade, CancellationToken cancellation);
    Task<bool> CreateExecutedTradesAsync(List<ExecutedTrade> executedTrades, CancellationToken cancellationToken);
}