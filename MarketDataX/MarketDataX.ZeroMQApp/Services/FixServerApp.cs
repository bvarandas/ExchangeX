using Microsoft.Extensions.Logging;
using QuickFix.Fields;
using QuickFix;
using MarketDataX.Core.Interfaces;
using SharedX.Core.Repositories;
using MongoDB.Bson;
using SharedX.Core.Entities;
using SharedX.Core.Matching.MarketData;

namespace MarketDataX.ServerApp.Services;
internal class FixServerApp : MessageCracker, IFixServerApp
{
    private Session _session = null!;
    private readonly ILogger<FixServerApp> _logger;
    private readonly ILoginRepository _loginRepository;
    private readonly CancellationTokenSource _tokenSource;
    private readonly IMarketDataChache _marketDataChache;

    private static Thread ThreadSenderIncremental = null!;
    private ManualResetEvent _mseSenderIncremental = new ManualResetEvent(false);

    public FixServerApp(ILogger<FixServerApp> logger, 
        IMarketDataChache marketDataChache,
        ILoginRepository loginRepository)
    {
        _logger = logger;
        _marketDataChache = marketDataChache;
        _loginRepository = loginRepository;

        ThreadSenderIncremental = new Thread(() => SenderMarketDataIncremental(_tokenSource.Token));
        ThreadSenderIncremental.Name = nameof(ThreadSenderIncremental);
        ThreadSenderIncremental.Start();

    }

    private void SenderMarketDataIncremental(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _mseSenderIncremental.WaitOne();
            while (_marketDataChache.TryDequeueMarketData(out MarketData marketData))
            {

            }
                //SendTradeCaptureReport(report, long.Parse(_tradeCaptureReportRequest.TradeRequestID.getValue()));

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

                var logoutMess = new QuickFix.Message();
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
        throw new NotImplementedException();
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

    
}