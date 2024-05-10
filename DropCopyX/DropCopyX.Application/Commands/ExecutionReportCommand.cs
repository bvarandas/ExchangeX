using SharedX.Core.Commands;
using SharedX.Core.Matching.DropCopy;

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

    public override bool IsValid()
    {
        throw new NotImplementedException();
    }
}