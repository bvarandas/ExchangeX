using FluentResults;
using SharedX.Core.Matching.DropCopy;
namespace MarketDataX.Core.Repositories;
public interface IMarketDataRepository
{
    Task<Result<bool>> AddExecutionReports(IList<ExecutionReport> executions, CancellationToken cancellation);
}
