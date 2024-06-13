using FluentResults;
using SharedX.Core.Matching.DropCopy;
namespace DropCopyX.Core.Repositories;
public interface IDropCopyRepository
{
    Task<Result> AddExecutionReports(IList<ExecutionReport> executions, CancellationToken cancellation);
}
