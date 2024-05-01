using SharedX.Core.Matching;
namespace MatchingX.Core.Repositories;
public interface ITradeRepository
{
    Task<Trade> GetTradeIdAsync( CancellationToken cancellation);
}