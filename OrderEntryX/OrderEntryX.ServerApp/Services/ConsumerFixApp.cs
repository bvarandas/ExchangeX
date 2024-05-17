using OrderEntryX.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickFix;
using QuickFix.Fields;
using SharedX.Core.Repositories;
using SharedX.Core.Entities;
using MongoDB.Bson;
using SharedX.Core.Extensions;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Enums;

namespace OrderEntryX.ServerApp.Services;
public class ConsumerFixApp : BackgroundService
{
    private readonly ILogger<ConsumerFixApp> _logger;
    private readonly ThreadedSocketAcceptor _acceptor;
    private readonly IFixServerApp app;
    
    public ConsumerFixApp(ILogger<ConsumerFixApp> logger, IFixServerApp app)
    {
        _logger = logger;
        SessionSettings settings = new SessionSettings(@"./services/acceptor.cfg");
        IApplication application = app;
        ScreenLogFactory logFactory = new ScreenLogFactory(settings);
        IMessageStoreFactory messageFactory = new FileStoreFactory(settings);
        _acceptor = new
            ThreadedSocketAcceptor(application, messageFactory, settings, logFactory);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o Fix acceptor...");
        
        _acceptor.Start();

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
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
    private readonly ILoginRepository _loginRepository;
    Session _session = null!;
    private readonly IOrderEntryChache _cache;
    public FixServerApp(ILogger<FixServerApp> logger, ILoginRepository loginRepository, IOrderEntryChache cache)
    {
        _logger = logger;
        _loginRepository = loginRepository;
        _cache = cache;
    }

    public void FromAdmin(Message message, SessionID sessionID)
    {
        //Efetuar Login
        if (message.Header.GetString(Tags.MsgType).Equals("A"))
        {
            string username = message.GetString(Tags.Username);
            string password = message.GetString(Tags.Password);

            string ip = string.Empty;
            var login = new Login() { 
                Password = password, 
                UserName = username, 
                ActualIP = ip, 
                Active=true, 
                Id=ObjectId.GenerateNewId().ToString(), 
                GrantedIPs=new List<string>() { 
                    "127.0.0.1",
                    "localhost"
                }};

            var loginExec = _loginRepository.ExecuteLogin(login, default(CancellationToken));

            if (!loginExec.Result.IsSuccess )
            {
                string messageError = string.Empty;
                loginExec.Result.Errors.ForEach(error => {
                    messageError =error.Message+ "|";
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
        // throw new NotImplementedException();
    }

    public void OnLogon(SessionID sessionID)
    {
        //throw new NotImplementedException();
    }

    public void OnLogout(SessionID sessionID)
    {
        //throw new NotImplementedException();
    }

    public void ToAdmin(Message message, SessionID sessionID)
    {
        //throw new NotImplementedException();
    }

    public void ToApp(Message message, SessionID sessionId)
    {
        //throw new NotImplementedException();
    }

    public void OnMessage(QuickFix.FIX44.NewOrderSingle newOrder, SessionID sessionID)
    {
        OrderEngine order = newOrder.ToOrderEngine(sessionID);
        order.Execution = Execution.ToOpen;
        _cache.AddOrderEntryAsync(order);
    }

    public void OnMessage(QuickFix.FIX44.OrderCancelRequest cancelOrder, SessionID sessionID)
    {
        OrderEngine order = new OrderEngine();
        order.OrderID = long.Parse( cancelOrder.OrderID.getValue());
        order.OrigClOrdID = long.Parse(cancelOrder.OrigClOrdID.getValue());
        order.ClOrdID = long.Parse(cancelOrder.ClOrdID.getValue());
        order.Symbol = cancelOrder.Symbol.getValue();
        order.Execution = Execution.ToCancel;

        _cache.AddOrderEntryAsync(order);
    }

    public void OnMessage(QuickFix.FIX44.OrderCancelReplaceRequest cancelRequestOrder, SessionID sessionID)
    {
        OrderEngine order = new OrderEngine();
        order.OrderID   = long.Parse(cancelRequestOrder.OrderID.getValue());
        order.ClOrdID   = long.Parse(cancelRequestOrder.ClOrdID.getValue());
        order.Symbol    = cancelRequestOrder.Symbol.getValue();
        order.Side      = (SideTrade)Enum.Parse(typeof(SideTrade), cancelRequestOrder.Side.getValue().ToString());
        order.TransactTime = cancelRequestOrder.TransactTime.getValue();
        order.Quantity  = cancelRequestOrder.OrderQty.getValue();
        order.OrderType = (OrderType)Enum.Parse(typeof(OrderType), cancelRequestOrder.OrdType.getValue().ToString());
        order.Price     = cancelRequestOrder.Price.getValue();
        
        if (cancelRequestOrder.IsSetStopPx())
            order.StopPrice = cancelRequestOrder.StopPx.getValue();

        order.TimeInForce = (SharedX.Core.Enums.TimeInForce)Enum.Parse(
            typeof(SharedX.Core.Enums.TimeInForce), cancelRequestOrder.TimeInForce.getValue().ToString());

        if (cancelRequestOrder.IsSetExpireDate())
            order.ExpireDate= cancelRequestOrder.ExpireDate.getValue();

        if (cancelRequestOrder.IsSetExpireTime())
            order.ExpireTime = cancelRequestOrder.ExpireTime.getValue().ToString();

        order.Execution = Execution.ToCancelReplace;

        _cache.AddOrderEntryAsync(order);
    }

    public void SendExecutionReport(SharedX.Core.Matching.OrderEngine.OrderEngine order, ExecType execType)
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
