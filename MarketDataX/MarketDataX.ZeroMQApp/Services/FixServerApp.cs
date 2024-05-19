using Microsoft.Extensions.Logging;
using QuickFix.Fields;
using QuickFix;
using MarketDataX.Core.Interfaces;
using SharedX.Core.Repositories;
using MongoDB.Bson;
using SharedX.Core.Entities;
using SharedX.Core.Matching.MarketData;
using StackExchange.Redis;
using NRedisStack.Graph;
using SharedX.Core.Matching;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using QuickFix.FIX44;

namespace MarketDataX.ServerApp.Services;
internal class FixServerApp : MessageCracker, IFixServerApp
{
    private readonly IFixSessionMarketDataCache _fixSessionCache;
    private Session _session = null!;
    private readonly ILogger<FixServerApp> _logger;
    private readonly ILoginRepository _loginRepository;
    private readonly CancellationTokenSource _tokenSource;
    private readonly IMarketDataChache _marketDataChache;

    private static Thread ThreadSenderIncremental = null!;
    
    private static QuickFix.FIX44.MarketDataRequest _marketDataRequest = null!;

    private ManualResetEvent _mseSenderIncremental = new ManualResetEvent(false);

    public FixServerApp(ILogger<FixServerApp> logger, 
        IMarketDataChache marketDataChache,
        ILoginRepository loginRepository,
        IFixSessionMarketDataCache fixSessionCache)
    {
        _logger = logger;
        _marketDataChache = marketDataChache;
        _loginRepository = loginRepository;

        ThreadSenderIncremental = new Thread(() => SenderMarketDataIncremental(_tokenSource.Token));
        ThreadSenderIncremental.Name = nameof(ThreadSenderIncremental);
        ThreadSenderIncremental.Start();

        _fixSessionCache = fixSessionCache;
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

    public void FromAdmin(QuickFix.Message message, SessionID sessionID)
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

    public void OnMessage(QuickFix.FIX44.MarketDataRequest message, SessionID sessionID)
    {
        //0 = Snapshot  1 = Snapshot + Updates(Subscribe)  2= Disable
        var subscriptionRequestType = message.SubscriptionRequestType;

        //'0'	Full Book        '1' Top of Book   '2'... '100000000000'  Report best N price tiers of data
        var marketDepth = message.MarketDepth;

        //0 = Full refresh
        var tradeType = message.MDUpdateType;

        // Number of 269 (MDEntryType) fields in the request.
        var noMDEntryTypes = message.NoMDEntryTypes;
        var entryTypesGroup = new MarketDataRequest.NoMDEntryTypesGroup();

        var entryTypeBid                = new MDEntryType('0');
        var entryTypeOffer              = new MDEntryType('1');
        var entryTypeTrade              = new MDEntryType('2');
        var entryTypeOppeningPrice      = new MDEntryType('4');
        var entryTypeClosingPrice       = new MDEntryType('5');
        var entryTypeSettlementPrice    = new MDEntryType('6');
        var entryTypeSessionHighPrice   = new MDEntryType('7');
        var entryTypeSessionLowPrice    = new MDEntryType('8');
        var entryTypeTRadeVolume        = new MDEntryType('B');

        message.GetGroup(1, entryTypesGroup);
        entryTypesGroup.Get(entryTypeBid);
        entryTypesGroup.Get(entryTypeOffer);
        entryTypesGroup.Get(entryTypeTrade);
        entryTypesGroup.Get(entryTypeOppeningPrice);
        entryTypesGroup.Get(entryTypeClosingPrice);
        entryTypesGroup.Get(entryTypeSettlementPrice);
        entryTypesGroup.Get(entryTypeSessionHighPrice);
        entryTypesGroup.Get(entryTypeSessionLowPrice);
        entryTypesGroup.Get(entryTypeTRadeVolume);

        //0 = Bid  1 = Offer 2 = Trade
        var mdEntryType = message.MDUpdateType;

        _marketDataRequest = message;
        _fixSessionCache.AddSessionAsync(message, sessionID);
        _mseSenderIncremental.Set();
    }

    public void OnMessage(QuickFix.FIX44.SecurityListRequest message, SessionID sessionID)
    {
        // Unique ID for this request
        var requestId = message.SecurityReqID;
        //4 = All Securities (will return all instrument where status = Active
        var tradeType = message.SecurityListRequestType;
        // 0 = Snapshot   1 = Snapshot + Updates(Subscribe)  2 =  Disable previous Snapshot + Update Request
        var subscriptionRequestType = message.SubscriptionRequestType;

        _fixSessionCache.AddSessionAsync(message, sessionID);

        //_marketDataChache.Get
    }

    public void OnMessage(QuickFix.FIX44.SecurityStatusRequest message, SessionID sessionID)
    {
        // Unique ID for this security request
        var requestId = message.SecurityStatusReqID;
        //0 = All trades 
        var tradeType = message.Symbol;
        // 0 = Snapshot   1 = Snapshot + Updates(Subscribe)  2 =  Disable previous Snapshot + Update Request
        var eecurityTradingStatus = message.SubscriptionRequestType;

        _fixSessionCache.AddSessionAsync(message, sessionID);
    }

    public void FromApp(QuickFix.Message message, SessionID sessionID)
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

    public void ToAdmin(QuickFix.Message message, SessionID sessionID)
    {
        
    }

    public void ToApp(QuickFix.Message message, SessionID sessionId)
    {
        
    }
}