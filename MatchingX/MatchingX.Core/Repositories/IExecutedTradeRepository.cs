using FluentResults;
using MatchingX.Core.Filters;
using SharedX.Core.Matching.DropCopy;
namespace MatchingX.Core.Repositories;
public interface IExecutedTradeRepository
{
    Task<Result<IEnumerable<TradeReport>>> GetExecutedTradeAsync(ExecutedTradeParams specParams, CancellationToken cancellation);
    Task<Result> CreateExecutedTradeAsync(Dictionary<long, TradeReport> dicExecutedTrade, CancellationToken cancellationToken);
}