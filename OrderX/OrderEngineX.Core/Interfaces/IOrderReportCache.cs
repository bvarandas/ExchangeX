using SharedX.Core.Entities;
namespace OrderEngineX.Core.Interfaces;
public interface IOrderReportCache
{
    void AddReport(ReportFix order);
    bool TryDequeueReport(out ReportFix trade);
}
