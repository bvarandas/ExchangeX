using MatchingX.Core.Filters;
using SharedX.Core.Matching.DropCopy;
namespace MatchingX.Core.Repositories;
public interface IExecutedTradeRepository
{
    Task<IEnumerable<DropCopyReport>> GetExecutedTradeAsync(ExecutedTradeParams specParams, CancellationToken cancellation);
    Task<bool> CreateExecutedTradeAsync(Dictionary<long, DropCopyReport> dicExecutedTrade, CancellationToken cancellationToken);
}