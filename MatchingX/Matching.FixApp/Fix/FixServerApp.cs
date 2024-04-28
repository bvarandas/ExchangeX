using Amazon.Runtime.Internal.Util;
using MacthingX.FixApp.Services;
using Microsoft.Extensions.Logging;
using QuickFix;
using QuickFix.Fields;
using SharedX.Core.Enums;
using SharedX.Core.Matching;

namespace MatchinX.API.Fix;

public class FixServerApp : MessageCracker, IApplication
{
    private readonly ILogger<FixServerApp> _logger;
    private MatchBase _matchEmgine;
    static readonly decimal DEFAULT_MARKET_PRICE = 10;
    public FixServerApp (ILogger<FixServerApp> logger)
    {
        _logger = logger;
    }

    int orderID = 0;
    int execID = 0;

    private string GenOrderID() { return (++orderID).ToString(); }
    private string GenExecID() { return (++execID).ToString(); }

    public void FromAdmin(Message message, SessionID sessionID)
    {
        Console.WriteLine("IN From Admin:  " + message);
        //Crack(message, sessionID);
    }
    public void FromApp(Message message, SessionID sessionID)
    {
        Console.WriteLine("IN From App:  " + message);
    }
    public void OnCreate(SessionID sessionID)
    {
    }
    public void OnLogon(SessionID sessionID)
    {
    }
    public void OnLogout(SessionID sessionID)
    {
    }
    public void ToAdmin(Message message, SessionID sessionID)
    {
        Console.WriteLine("TO Admin OUT:  " + message);
    }
    public void ToApp(Message message, SessionID sessionId)
    {
        Console.WriteLine("OUT: " + message);
    }
    public void OnMessage(QuickFix.FIX44.NewOrderSingle n, SessionID s )
    {
        Symbol symbol = n.Symbol;
        Side side = n.Side;
        OrdType ordType = n.OrdType;
        OrderQty orderQty = n.OrderQty;
        Price price = n.Price;
        StopPx stopPx = n.StopPx;

        ClOrdID clOrdID = n.ClOrdID;

        switch (ordType.getValue())
        {
            case OrdType.LIMIT:
                if (price.Obj == 0)
                    throw new IncorrectTagValue(price.Tag);
                break;
            case OrdType.MARKET: break;
            case OrdType.STOP:
                if (stopPx.Obj == 0)
                    throw new IncorrectTagValue(price.Tag);
                break;
            case OrdType.STOP_LIMIT:
                if (price.Obj == 0)
                    throw new IncorrectTagValue(price.Tag);
                if (stopPx.Obj == 0)
                    throw new IncorrectTagValue(price.Tag);
                break;
            default: throw new IncorrectTagValue(ordType.Tag);
        }

        QuickFix.FIX44.ExecutionReport exReport = new QuickFix.FIX44.ExecutionReport(
            new OrderID(GenOrderID()),
            new ExecID(GenExecID()),
            new ExecType(ExecType.FILL),
            new OrdStatus(OrdStatus.FILLED),
            symbol, //shouldn't be here?
            side,
            new LeavesQty(0),
            new CumQty(orderQty.getValue()),
            new AvgPx(price.getValue()));

        exReport.Set(clOrdID);
        exReport.Set(orderQty);
        exReport.Set(new LastQty(orderQty.getValue()));
        exReport.Set(new LastPx(price.getValue()));

        if (n.IsSetAccount())
            exReport.SetField(n.Account);

        try
        {
            Session.SendToTarget(exReport, s);
        }
        catch (SessionNotFound ex)
        {
            _logger.LogError("==session not found exception!==");
            _logger.LogError(ex.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    

    public void OnMessage(QuickFix.FIX44.OrderCancelRequest msg, SessionID s)
    {
        string orderid = (msg.IsSetOrderID()) ? msg.OrderID.Obj : "unknown orderID";
        QuickFix.FIX44.OrderCancelReject ocj = new QuickFix.FIX44.OrderCancelReject(
            new OrderID(orderid), msg.ClOrdID, msg.OrigClOrdID, new OrdStatus(OrdStatus.REJECTED), new CxlRejResponseTo(CxlRejResponseTo.ORDER_CANCEL_REQUEST));
        ocj.CxlRejReason = new CxlRejReason(CxlRejReason.OTHER);
        ocj.Text = new Text("Executor does not support order cancels");

        try
        {
            Session.SendToTarget(ocj, s);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public void OnMessage(QuickFix.FIX44.OrderCancelReplaceRequest msg, SessionID s)
    {
        string orderid = (msg.IsSetOrderID()) ? msg.OrderID.Obj : "unknown orderID";
        QuickFix.FIX44.OrderCancelReject ocj = new QuickFix.FIX44.OrderCancelReject(
            new OrderID(orderid), msg.ClOrdID, msg.OrigClOrdID, new OrdStatus(OrdStatus.REJECTED), new CxlRejResponseTo(CxlRejResponseTo.ORDER_CANCEL_REPLACE_REQUEST));
        ocj.CxlRejReason = new CxlRejReason(CxlRejReason.OTHER);
        ocj.Text = new Text("Executor does not support order cancel/replaces");

        try
        {
            Session.SendToTarget(ocj, s);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}