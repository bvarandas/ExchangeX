using SharedX.Core.Entities;
using SharedX.Core.Matching.OrderEngine;
namespace SharedX.Core.Extensions;
public static class ReportFixExtensions
{
    public static ReportFix ReportOrderCancelRejectFix(this OrderEngine orderEngine, int CxlRejReason, char CxlRejResponseTo,string text)
    {
        var report = new OrderCancelRejectFix();
        report.OrderID = orderEngine.OrderID;
        report.ClOrdID = orderEngine.ClOrdID;
        report.OrderStatus = orderEngine.OrderStatus;
        report.CxlRejReason = CxlRejReason;
        report.CxlRejResponseTo = CxlRejResponseTo;
        report.Text = text;
        return report;
    }

    public static ReportFix ReportBusinessMessageRejectFix(this OrderEngine orderEngine, int BusinessRejectReason, string RefMsgType, string text)
    {
        var report = new BusinessMessageRejectFix();
        report.RefSeqNum = orderEngine.OrderID;
        report.RefMsgType = RefMsgType;
        report.BusinessRejectReason = BusinessRejectReason;
        report.Text = text;
        return report;
    }

    public static ReportFix ReportOrderMassCancelReportFix(this OrderEngine orderEngine, 
        char massCancelRequestType, 
        char massCancelResponse, 
        char massCancelRejectReason,
        int totalAffectedOrders,
        string MassActionReportID,
        IList<TargetParty> tagetPartyIds,
        string text)
    {
        var report = new OrderMassCancelReportFix();
        report.ClOrdID = orderEngine.ClOrdID;
        report.MassCancelRequestType = massCancelRequestType;
        report.MassCancelResponse = massCancelResponse;
        report.MassCancelRejectReason = massCancelRejectReason;
        report.TotalAffectedOrders = totalAffectedOrders;
        report.MassActionReportID = MassActionReportID;
        report.Text = text;
        return report;
    }
}