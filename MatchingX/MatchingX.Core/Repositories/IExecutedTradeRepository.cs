using FluentResults;
using MatchingX.Core.Filters;
using SharedX.Core.Matching.DropCopy;
namespace MatchingX.Core.Repositories;
public interface IExecutedTradeRepository
{
    Task<Result<IEnumerable<DropCopyReport>>> GetExecutedTradeAsync(ExecutedTradeParams specParams, CancellationToken cancellation);
    Task<Result> CreateExecutedTradeAsync(Dictionary<long, DropCopyReport> dicExecutedTrade, CancellationToken cancellationToken);
}