using FluentResults;
using SharedX.Core.Matching.DropCopy;
namespace OrderEngineX.Core.Interfaces;
public interface IExecutionReportCache
{
    Task<Result> DeleteExecutionReportAsync(ExecutionReport execution);
    Task<Result> UpsertExecutionReportAsync(ExecutionReport execution);
    Task<Result<Dictionary<long, ExecutionReport>>> GetExecutionReportAsync(string symbol);
}
