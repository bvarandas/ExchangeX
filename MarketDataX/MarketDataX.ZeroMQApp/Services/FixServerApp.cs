using Microsoft.Extensions.Logging;
using QuickFix.Fields;
using QuickFix;
using MarketDataX.Core.Interfaces;
using SharedX.Core.Repositories;
using MongoDB.Bson;
using SharedX.Core.Entities;
using SharedX.Core.Matching.MarketData;
using QuickFix.FIX44;
using FluentValidation;

namespace MarketDataX.ServerApp.Services;
internal class FixServerApp : MessageCracker, IFixServerApp
{
    private readonly IFixSessionMarketDataCache _fixSessionCache;
    private Session _session = null!;
    private readonly ILogger<FixServerApp> _logger;
    private readonly ILoginRepository _loginRepository;
    private readonly CancellationTokenSource _tokenSource;
    private readonly IMarketDataChache _marketDataChache;
    private readonly ISecurityCache _securityCache;

    private static Thread ThreadSenderIncremental = null!;
    private static Thread ThreadSenderSecurityStatus = null!;

    private static QuickFix.FIX44.MarketDataRequest _marketDataRequest = null!;

    private ManualResetEvent _mseSenderIncremental = new ManualResetEvent(false);
    private ManualResetEvent _mseSenderSecurityStatus = new ManualResetEvent(false);

    private static SecurityStatusReqID SecurityStatusRequestId = new SecurityStatusReqID();

    public FixServerApp(ILogger<FixServerApp> logger, 
        IMarketDataChache marketDataChache,
        ISecurityCache securityCache,
        ILoginRepository loginRepository,
        IFixSessionMarketDataCache fixSessionCache)
    {
        _logger = logger;
        _marketDataChache = marketDataChache;
        _securityCache = securityCache;
        _loginRepository = loginRepository;

        ThreadSenderIncremental = new Thread(() => SenderIncremental(_tokenSource!.Token));
        ThreadSenderIncremental.Name = nameof(ThreadSenderIncremental);
        ThreadSenderIncremental.Start();

        ThreadSenderSecurityStatus = new Thread(() => SenderSecurityStatus(_tokenSource!.Token));
        ThreadSenderSecurityStatus.Name = nameof(ThreadSenderIncremental);
        ThreadSenderSecurityStatus.Start();

        _fixSessionCache = fixSessionCache;
    }

    private void SenderIncremental(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _mseSenderIncremental.WaitOne();
            while (_marketDataChache.TryDequeueMarketData(out MarketData marketData))
            {
                SendMarketDataIncremental(marketData);
            }
            Thread.Sleep(10);
        }
    }

    private void SenderSecurityStatus(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _mseSenderSecurityStatus.WaitOne();
            while (_securityCache.TryDequeueSecurityStatus(out Security security))
            {
                SendSecurityStatus(SecurityStatusRequestId, security.Symbol, security.SecurityStatus, _session.SessionID);
            }
            Thread.Sleep(10);
        }
    }

    public void SendMarketDataSnapshot(string symbol)
    {
        var snapShotCache = _marketDataChache.GetSnapShotMarketData(symbol);
        var snapShot = new QuickFix.FIX44.MarketDataSnapshotFullRefresh();

        if (snapShotCache.Result.IsFailed)
        {
            //Reject 
            /*
             *  1 = Invalid instrument requested
                2 = Instrument already exists
                3 = Request type not supported
                4 = System unavailable for instrument creation
                5 = Ineligible instrument group
                6 = Instrument ID unavailable
                7 = Invalid or missing data on option leg
                8 = Invalid or missing data on future leg
                10 = Invalid or missing data on FX leg
                11 = Invalid leg price specified
                12 = Invalid instrument structure specified
             */
        }

        var group = new QuickFix.FIX44.MarketDataSnapshotFullRefresh.NoMDEntriesGroup();
        group.SetField(new Symbol(symbol));
        group.SetField(new LastUpdateTime());

        snapShot.AddGroup(group);
        try
        {
            Session.SendToTarget(snapShot, _session.SessionID);
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

    public void SendMarketDataIncremental(MarketData marketData)
    {
        var incremental = new QuickFix.FIX44.MarketDataIncrementalRefresh();
        incremental.MDReqID.setValue(_marketDataRequest.MDReqID.getValue());
        incremental.NoMDEntries.setValue(1);
        incremental.SetField(new MDUpdateAction(marketData.UpdateAction));

        var group = new QuickFix.FIX44.MarketDataIncrementalRefresh.NoMDEntriesGroup();

        group.SetField(new Symbol(marketData.Symbol));
        group.SetField(new SecurityID(marketData.SecurityID));
        group.SetField(new SecurityIDSource(marketData.SecuritSourceId.ToString()));
        group.SetField(new MDEntryID(marketData.EntryID.ToString()));
        group.SetField(new MDEntryType(marketData.EntryType));
        group.SetField(new MDEntryPx(marketData.EntryPx));
        group.SetField(new MDEntrySize(marketData.EntrySize));
        group.SetField(new MDEntryDate(DateTime.Parse(marketData.EntryDate)));
        group.SetField(new MDEntryTime (DateTime.Parse(marketData.EntryTime)));
        group.SetField(new QuoteCondition(marketData.QuoteCondition));
        group.SetField(new TradeCondition(marketData.TradeCondition));
        group.SetField(new MDPriceLevel( int.Parse(marketData.PriceLevel)));
        group.SetField(new MDQuoteType(int.Parse(marketData.QuoteType)));
        group.SetField(new MultiLegReportingType(marketData.MultiLegReportingType));

        incremental.AddGroup(group);

        try
        {
            Session.SendToTarget(incremental, _session.SessionID);
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

    public void SendSecurityList(SecurityReqID id, SessionID sessionId)
    {
        var list = new QuickFix.FIX44.SecurityList();

        var securities = _securityCache.GetSnapShotSecuritiesAsync();

        if(securities.Result.IsFailed)
        {
            //Reject 
            /*
             *  1 = Invalid instrument requested
                2 = Instrument already exists
                3 = Request type not supported
                4 = System unavailable for instrument creation
                5 = Ineligible instrument group
                6 = Instrument ID unavailable
                7 = Invalid or missing data on option leg
                8 = Invalid or missing data on future leg
                10 = Invalid or missing data on FX leg
                11 = Invalid leg price specified
                12 = Invalid instrument structure specified
             */
            list.SetField(new SecurityRequestResult(1));
            //list.SetField(new SecurityReject (1));
        }

        list.SetField(id);
        list.SetField(new SecurityRequestResult(0));
        list.SetField(new LastFragment(false));

        var dicSecurity = securities.Result.Value;
        var countSecurity = dicSecurity.Count;
        var noRelatedSym = new NoRelatedSym(countSecurity);

        list.SetField(new SecurityRequestResult(0));
        list.SetField(noRelatedSym);

        foreach (var security in dicSecurity.Values)
        {
            var group = new QuickFix.FIX44.SecurityList.NoRelatedSymGroup();

            group.SetField(new Symbol(security.Symbol));
            group.SetField(new SecurityID(security.SecurityID));
            group.SetField(new SecurityIDSource(security.SecuritSourceId));
            group.SetField(new CFICode(security.CFICode));
            group.SetField(new SecurityType(security.SecurityType));
            group.SetField(new SecuritySubType(security.SecuritySubType.ToString()));
            group.SetField(new MaturityDate(security.MaturityDate));
            group.SetField(new MaturityTime(security.MaturityTime));
            group.SetField(new QuickFix.Fields.SecurityStatus(security.SecurityStatus.ToString()));
            group.SetField(new IssueDate(security.IssueDate));
            group.SetField(new StrikePrice(security.StrikePrice));
            group.SetField(new ContractMultiplier(security.ContractMultiplier));
            group.SetField(new SettlMethod(security.SettlMethod));
            group.SetField(new ExerciseStyle(security.ExerciseStyle));
            group.SetField(new PutOrCall(security.PutOrCall));
            group.SetField(new SecurityExchange(security.SecurityExchange));
            group.SetField(new SecurityDesc(security.SecurityDesc));
            //group.SetField(new InstrumentPricePrecision(security.InstrumentPricePrecision));
            //group.SetField(new TradeVolType(security.TradeVolType));
            group.SetField(new MinTradeVol(security.MinTradeVol));
            group.SetField(new MaxTradeVol(security.MaxTradeVol));
            group.SetField(new Currency(security.Currency));

            list.AddGroup(group);
        }
        
        try
        {
            Session.SendToTarget(list, sessionId);
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

    private void SendSecurityStatus(SecurityStatusReqID requestId,  string symbol, int statusId, SessionID sessionId)
    {
        var status = new QuickFix.FIX44.SecurityStatus();

        status.SetField(requestId);
        status.SetField(new Symbol(symbol));
        status.SetField(new SecurityTradingStatus(statusId));

        try
        {
            Session.SendToTarget(status, sessionId);
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
        var entryTypeTradeVolume        = new MDEntryType('B');

        message.GetGroup(1, entryTypesGroup);
        entryTypesGroup.Get(entryTypeBid);
        entryTypesGroup.Get(entryTypeOffer);
        entryTypesGroup.Get(entryTypeTrade);
        entryTypesGroup.Get(entryTypeOppeningPrice);
        entryTypesGroup.Get(entryTypeClosingPrice);
        entryTypesGroup.Get(entryTypeSettlementPrice);
        entryTypesGroup.Get(entryTypeSessionHighPrice);
        entryTypesGroup.Get(entryTypeSessionLowPrice);
        entryTypesGroup.Get(entryTypeTradeVolume);

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

        if (subscriptionRequestType.getValue() == 0 ||
            subscriptionRequestType.getValue() == 2)
            _mseSenderSecurityStatus.Reset();

        _fixSessionCache.AddSessionAsync(message, sessionID);

        SendSecurityList(requestId, sessionID);
    }

    public void OnMessage(QuickFix.FIX44.SecurityStatusRequest message, SessionID sessionID)
    {
        // Unique ID for this security request
        var requestId = message.SecurityStatusReqID;
        SecurityStatusRequestId = requestId;
        //0 = All trades 
        var tradeType = message.Symbol;
        // 0 = Snapshot   1 = Snapshot + Updates(Subscribe)  2 =  Disable previous Snapshot + Update Request
        var securityTradingStatus = message.SubscriptionRequestType;

        if (securityTradingStatus.getValue() == 0 || 
            securityTradingStatus.getValue() == 2)
            _mseSenderSecurityStatus.Reset();

        string symbol = message.Symbol.getValue();

        _fixSessionCache.AddSessionAsync(message, sessionID);

        var security = _securityCache.GetSecurity(symbol, "").Result;

        SendSecurityStatus(requestId, symbol, security.Value.SecurityStatus, sessionID);
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
        _mseSenderIncremental.Set();
        _mseSenderSecurityStatus.Set();
    }

    public void OnLogout(SessionID sessionID)
    {
        _mseSenderIncremental.Reset();
        _mseSenderSecurityStatus.Reset();
    }

    public void ToAdmin(QuickFix.Message message, SessionID sessionID)
    {
        
    }

    public void ToApp(QuickFix.Message message, SessionID sessionId)
    {
        
    }
}