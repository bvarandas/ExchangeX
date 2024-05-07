using MarketDataX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickFix;
using QuickFix.Fields;
using SharedX.Core.Matching.OrderEngine;

namespace MarketDataX.ServerApp.Services;
internal class PublisherFixApp : BackgroundService
{
    private readonly ILogger<PublisherFixApp> _logger;
    private readonly ThreadedSocketAcceptor _acceptor;
    private readonly IFixServerApp app;

    public PublisherFixApp(ILogger<PublisherFixApp> logger, IFixServerApp app)
    {
        _logger = logger;
        SessionSettings settings = new SessionSettings(@"acceptor.cfg");
        IApplication application = app;
        ScreenLogFactory logFactory = new ScreenLogFactory(settings);

        IMessageStoreFactory messageFactory = new FileStoreFactory(settings);

        _acceptor = new
            ThreadedSocketAcceptor(application, messageFactory, settings, logFactory);
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Fix acceptor...");

        _acceptor.Start();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5000, stoppingToken);
        }
    }

    public async override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finalizando o publisher FIX...");
        _acceptor.Stop();

        _logger.LogInformation("Publisher FIX...Finalizado!");
    }
}

internal class FixServerApp : MessageCracker, IFixServerApp
{

    private readonly ILogger<FixServerApp> _logger;

    public FixServerApp(ILogger<FixServerApp> logger)
    {
        _logger = logger;
    }

    public void FromAdmin(Message message, SessionID sessionID)
    {
        throw new NotImplementedException();
    }

    public void FromApp(Message message, SessionID sessionID)
    {
        throw new NotImplementedException();
    }

    public void OnCreate(SessionID sessionID)
    {
        throw new NotImplementedException();
    }

    public void OnLogon(SessionID sessionID)
    {
        throw new NotImplementedException();
    }

    public void OnLogout(SessionID sessionID)
    {
        throw new NotImplementedException();
    }

    public void ToAdmin(Message message, SessionID sessionID)
    {
        throw new NotImplementedException();
    }

    public void ToApp(Message message, SessionID sessionId)
    {
        throw new NotImplementedException();
    }

    //public void SendTradeCaptureReport(ExecutedTrade trade, ExecType execType)
    //{
    //    Symbol symbol = new Symbol(trade.Symbol);

    //    //PartyID participator = new PartyID(trade.ParticipatorId.ToString());
    //    //PartyRole role = new PartyRole(1);
    //    //PartyIDSource partyIDSource = new PartyIDSource('C');
    //    //NoPartyIDs partyIDs = new NoPartyIDs(1);

    //    //var group = new QuickFix.FIX44.ExecutionReport.NoPartyIDsGroup();
    //    //group.SetField(participator);
    //    //group.SetField(role);
    //    //group.SetField(partyIDSource);
    //    //group.SetField(partyIDs);
    //    //ClOrdID clOrdID = new ClOrdID(trade..AccountId.ToString());
    //    LastQty lastQty = new LastQty(trade.LastQuantity);
    //    LastPx lastPx = new LastPx(trade.LastPrice);

    //    var exReport = new QuickFix.FIX44.TradeCaptureReport(
    //        new TradeReportID(trade.TradeId.ToString()),
    //        new PreviouslyReported(false),
    //        symbol,
    //        lastQty,
    //        lastPx,
    //        new TradeDate(trade.TradeDate.ToString("YYYmmDD")),
    //        new TransactTime(trade.TradeDate.ToUniversalTime())
    //        );

    //    ////exReport.Set(clOrdID);
    //    //exReport.Set(symbol);
    //    //exReport.Set(orderQty);
    //    //exReport.Set(lastQty);
    //    //exReport.Set(lastPx);
    //    //exReport.SetField(account);

    //    //exReport.AddGroup(group);

    //    try
    //    {
    //        Session.SendToTarget(exReport, trade.SessionID);
    //    }
    //    catch (SessionNotFound ex)
    //    {
    //        Console.WriteLine("==session not found exception!==");
    //        Console.WriteLine(ex.ToString());
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex.ToString());
    //    }
    //}
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
