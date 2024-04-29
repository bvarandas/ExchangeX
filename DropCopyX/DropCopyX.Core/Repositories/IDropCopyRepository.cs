using FluentResults;
using SharedX.Core.Proto;
namespace DropCopyX.Core.Repositories;
public interface IDropCopyRepository
{
    Task<Result<bool>> AddExecutionReports(IList<ExecutionReport> executions, CancellationToken cancellation);
}
