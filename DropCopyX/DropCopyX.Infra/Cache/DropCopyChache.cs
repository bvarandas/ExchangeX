using DropCopyX.Core.Entities;
using DropCopyX.Core.Interfaces;
using System.Collections.Concurrent;
namespace DropCopyX.Infra.Cache;
public class DropCopyChache : IDropCopyChache
{
    private static ConcurrentDictionary<long, ExecutionReport> ExecutionReportDic;
    private static ConcurrentQueue<ExecutionReport> ExecutionReportQueue;
    public DropCopyChache()
    {
        ExecutionReportDic = new ConcurrentDictionary<long, ExecutionReport>();
        ExecutionReportQueue = new ConcurrentQueue<ExecutionReport>();
    }

    public void AddExecutionReport(ExecutionReport report)
    {
        if(ExecutionReportDic.TryAdd(report.ExecID, report))
            ExecutionReportQueue.Enqueue(report);
    }

    public ExecutionReport TryDequeueExecutionReport()
    {
        if (ExecutionReportQueue.TryDequeue(out ExecutionReport report))
        {
            return report;
        }
        return default(ExecutionReport);
    }

    public void Clear()
    {
        ExecutionReportDic.Clear();
    }

}