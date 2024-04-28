using DropCopyX.Core.Entities;
using SharedX.Core.Commands;
namespace DropCopyX.Application.Commands;
public class ExecutionReportCommand : Command
{
    public readonly IList<ExecutionReport> ExecutionReports;
    public DateTime Timestamp { get; private set; }
    public ExecutionReportCommand(List<ExecutionReport> executionReport)
    {
        Timestamp = DateTime.Now;
        ExecutionReports = executionReport;
    }
}