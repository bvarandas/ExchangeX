using FluentResults;
using SharedX.Core.Matching.MarketData;
namespace MarketDataX.Core.Interfaces;
public interface IMarketDataRepository
{
    Task<bool> UpsertMarketDataSnapshotAsync(MarketDataSnapshot marketDataSnapshot, CancellationToken cancellation);
    Task<Result<MarketDataSnapshot>> GetSnapshotAsync(string symbol, DateTime date, CancellationToken cancellation);
    Task<Result<Dictionary<string, MarketDataSnapshot>>> GetAllSnapshotAsync(string symbol, CancellationToken cancellation);
}