using QuickFix;
using QuickFix.Fields;
using SharedX.Core.Entities;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Repositories;
using MongoDB.Bson;
using DropCopyX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using DropCopyX.Infra.Cache;

namespace DropCopyX.ServerApp.Services;
internal class FixServerApp : MessageCracker, IFixServerApp
{
    private readonly IExecutedTradeCache _executedTradeCache;
    private readonly IExecutionReportChache _executionReportCache;
    private readonly ILogger<FixServerApp> _logger;
    private readonly ILoginRepository _loginRepository;
    private Session _session = null!;
    private static Thread ThreadSenderReport = null!;
    private readonly CancellationTokenSource _tokenSource;
    public FixServerApp(ILogger<FixServerApp> logger,
        ILoginRepository loginRepository,
        IExecutedTradeCache executedTradeCache,
        IExecutionReportChache executionReportCache)
    {
        _logger = logger;
        _executedTradeCache = executedTradeCache;
        _executionReportCache = executionReportCache;
        _loginRepository = loginRepository;

        _tokenSource = new CancellationTokenSource();

        ThreadSenderReport = new Thread(() => SenderReport(_tokenSource.Token));
        ThreadSenderReport.Name = nameof(ThreadSenderReport);
        ThreadSenderReport.Start();
    }

    private void SenderReport(CancellationToken cancellationToken)
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            
            Thread.Sleep(10);
        }
    }



    public void FromAdmin(Message message, SessionID sessionID)
    {
        //Efetuar Login
        if (message.Header.GetString(Tags.MsgType).Equals("A"))
        {
            string username = message.GetString(Tags.Username);
            string password = message.GetString(Tags.Password);

            string ip = string.Empty;
            var login = new Login()
            {
                Password = password,
                UserName = username,
                ActualIP = ip,
                Active = true,
                Id = ObjectId.GenerateNewId().ToString(),
                GrantedIPs = new List<string>() {
                    "127.0.0.1",
                    "localhost"
                }
            };

            var loginExec = _loginRepository.ExecuteLogin(login, default(CancellationToken));

            if (!loginExec.Result.IsSuccess)
            {
                string messageError = string.Empty;
                loginExec.Result.Errors.ForEach(error => {
                    messageError = error.Message + "|";
                });

                //throw new RejectLogon(messageError); Throwing a RejectLogon QuickFIXException breaks the
                //whole code and interrupts the rest of the sessions (if you do have more than one).

                var logoutMess = new Message();
                logoutMess.Header.SetField(new MsgType() { Tag = 35, Obj = "5" });
                logoutMess.SetField(new Text("Invalid credentials"));

                Session.SendToTarget(logoutMess, sessionID);
            }
        }
    }

    public void FromApp(Message message, SessionID sessionID)
    {
        Crack(message, sessionID);
    }

    public void OnCreate(SessionID sessionID)
    {
        _session = Session.LookupSession(sessionID);
    }

    public void OnLogon(SessionID sessionID)
    {

    }

    public void OnLogout(SessionID sessionID)
    {

    }

    public void ToAdmin(Message message, SessionID sessionID)
    {

    }

    public void ToApp(Message message, SessionID sessionId)
    {

    }

    public void OnMessage(QuickFix.FIX44.OrderMassStatusRequest message, SessionID sessionID)
    {
        var requestId = message.MassStatusReqID;
        //0 = All trades 
        var tradeType = message.MassStatusReqType;
    }

    public void OnMessage(QuickFix.FIX44.TradeCaptureReportRequest message, SessionID sessionID)
    {
        var requestId = message.TradeRequestID;

        //0 = All trades 
        var tradeType = message.TradeRequestType;

        // 0=Snapshot, 1=Snapshot+Updates, 2=Disable previous Snapshot + Update Request (Unsubscribe) 
        var typeRequest = message.IsSetSubscriptionRequestType() ?
            message.SubscriptionRequestType : new SubscriptionRequestType('1');

        //1 = Single Security(default if not specified) 2 = Individual leg of a multi-leg security 3 = Multi - leg security
        var multiLegReportType = message.IsSetMultiLegReportingType() ?
            message.MultiLegReportingType : new MultiLegReportingType('1');
    }

    public void OnMessage(QuickFix.FIX44.TradeCaptureReportRequestAck message, SessionID sessionID)
    {
        var requestId = message.TradeRequestID;

        //0 = All trades 
        var tradeType = message.TradeRequestType;

        //0 = Successful (Default) 8 = TradeRequestType < 569 > not supported 9 = Unauthorized for Trade Capture Report Request < AD >
        var requestResult = message.IsSetTradeRequestResult() ?
            message.TradeRequestResult : new TradeRequestResult(0);

        //0 = Accepted (in case validations passed) 2 = Rejected (in case of issue on the request or no results were found or problem was found during results publication )
        var requestStatus = message.IsSetTradeRequestStatus() ?
            message.TradeRequestStatus : new TradeRequestStatus('0');

        // 0=Snapshot, 1=Snapshot+Updates, 2=Disable previous Snapshot + Update Request (Unsubscribe) 
        SubscriptionRequestType typeRequest = message.IsSetSubscriptionRequestType() ?
            message.SubscriptionRequestType : new SubscriptionRequestType('1');
    }

    public void SendTradeCaptureReport(TradeCaptureReport trade, long TradeIdRequest, ExecType execType)
    {
        var exReport = new QuickFix.FIX44.TradeCaptureReport();

        exReport.SetField(new TradeRequestID(TradeIdRequest.ToString()));
        exReport.SetField(new LastRptRequested(false));
        exReport.SetField(new QuickFix.Fields.TradeReportTransType(trade.TradeReportTransType));
        exReport.SetField(new TrdType(trade.TrdType));
        exReport.SetField(new CopyMsgIndicator(trade.CopyMsgIndicator == 'Y'));
        exReport.SetField(new PreviouslyReported(trade.PreviouslyReported == 'Y'));
        exReport.SetField(new TradeID(trade.TradeId.ToString()));
        exReport.SetField(new NoSides(1));
        exReport.SetField(new OrderID(trade.OrderId.ToString()));
        exReport.SetField(new ClOrdID(trade.ClOrderId.ToString()));
        exReport.SetField(new LastQty(trade.LastQty));
        exReport.SetField(new LastPx(trade.LastPx));
        exReport.SetField(new Symbol(trade.Symbol));
        exReport.SetField(new Side(trade.Side));
        exReport.SetField(new Price(trade.Price));
        exReport.SetField(new TransactTime(trade.TransactTime));
        exReport.SetField(new TradeDate(trade.TradeDate));
        exReport.SetField(new NoOrders(1));

        try
        {
            Session.SendToTarget(exReport, trade.SessionID);
        }
        catch (SessionNotFound ex)
        {
            Console.WriteLine("==session not found exception!==");
            Console.WriteLine(ex.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    public void SendExecutionReport(OrderEngine order, ExecType execType)
    {
        Symbol symbol = new Symbol(order.Symbol);
        Side side = new Side((char)order.Side);
        OrderQty orderQty = new OrderQty(order.Quantity);
        ClOrdID clOrdID = new ClOrdID(order.AccountId.ToString());
        LastQty lastQty = new LastQty(order.LastQuantity);
        LastPx lastPx = new LastPx(order.LastPrice);
        Account account = new Account(order.AccountId.ToString());

        PartyID participator = new PartyID(order.ParticipatorId.ToString());
        PartyRole role = new PartyRole(1);
        PartyIDSource partyIDSource = new PartyIDSource('C');
        NoPartyIDs partyIDs = new NoPartyIDs(1);

        var group = new QuickFix.FIX44.ExecutionReport.NoPartyIDsGroup();
        group.SetField(participator);
        group.SetField(role);
        group.SetField(partyIDSource);
        group.SetField(partyIDs);

        var exReport = new QuickFix.FIX44.ExecutionReport(
            new OrderID(order.OrderID.ToString()),
            new ExecID(order.Id.ToString()),
            new ExecType(ExecType.FILL),
            new OrdStatus(OrdStatus.FILLED),
            symbol,
            side,
            new LeavesQty(order.LeavesQuantity),
            new CumQty(order.Quantity),
            new AvgPx(order.AveragePrice));

        exReport.Set(clOrdID);
        exReport.Set(symbol);
        exReport.Set(orderQty);
        exReport.Set(lastQty);
        exReport.Set(lastPx);
        exReport.SetField(account);

        exReport.AddGroup(group);

        try
        {
            Session.SendToTarget(exReport);
        }
        catch (SessionNotFound ex)
        {
            Console.WriteLine("==session not found exception!==");
            Console.WriteLine(ex.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}

