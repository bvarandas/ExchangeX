using DropCopyX.Core.Entities;
using FluentResults;

namespace DropCopyX.Core.Repositories;
public interface IDropCopyRepository
{
    Task<Result<bool>> AddExecutionReports(IList<ExecutionReport> executions, CancellationToken cancellation);
}
