using MacthingX.Application.Commands;
using MacthingX.Application.Commands.Match.OrderStatus;
using MacthingX.Application.Events;
using MacthingX.Application.Extensions;
using MacthingX.Application.Interfaces;
using MatchingX.Core.Interfaces;
using MatchingX.Core.Repositories;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using SharedX.Core.Enums;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.DropCopy;
using SharedX.Core.Matching.OrderEngine;
using System.Collections.Concurrent;

namespace MacthingX.Application.Services;
public delegate void PriceChangedEventHandler(object sender, OrderPriceEventArgs args);
public class TradeOrderService : ITradeOrderService, IDisposable
{
    protected bool _running;
    protected readonly ILogger<TradeOrderService> _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly ITradeRepository _tradeRepository;
    protected readonly IMatchingCache _matchingCache;
    protected readonly IMarketDataCache _marketDataCache;
    protected readonly IMediatorHandler Bus;
    protected readonly ConcurrentQueue<OrderEngine> QueueOrderStatusChanged;
    protected readonly ConcurrentQueue<Dictionary<long, DropCopyReport>> QueueExecutedTraded;
    

    private readonly Thread ThreadExecutedTrade;
    private readonly Thread ThreadOrdersStatus;
    public event PriceChangedEventHandler PriceChanged;

    public TradeOrderService(ILogger<TradeOrderService> logger, IMediatorHandler bus, IMatchingCache matchingCache, IMarketDataCache marketDataCache)
    {
        _logger = logger;
        Bus = bus;
        _matchingCache = matchingCache;
        _marketDataCache = marketDataCache;

        QueueExecutedTraded = new ConcurrentQueue<Dictionary<long, DropCopyReport>>();
        QueueOrderStatusChanged = new ConcurrentQueue<OrderEngine>();

        ThreadExecutedTrade = new Thread(new ThreadStart(ExecutedTradeOutcome));
        ThreadExecutedTrade.Name = nameof(ExecutedTradeOutcome);
        ThreadExecutedTrade.Start();

        ThreadOrdersStatus = new Thread(new ThreadStart(OrderStatusChangedOutcome));
        ThreadOrdersStatus.Name = nameof(OrderStatusChangedOutcome);
        ThreadOrdersStatus.Start();
    }

    private void OrderStatusChangedOutcome()
    {
        while (true)
        {
            if (QueueOrderStatusChanged.TryDequeue(out OrderEngine order))
            {
                switch (order.OrderStatus)
                {
                    case OrderStatus.New:
                        Bus.SendCommand(new MatchingOpenedCommand(order));
                        break;
                    case OrderStatus.Filled:
                        Bus.SendCommand(new MatchingFilledCommand(order));
                        break;
                    case OrderStatus.PartiallyFilled:
                        Bus.SendCommand(new MatchingPartiallyFilledCommand(order));
                        break;
                    case OrderStatus.Cancelled:
                        Bus.SendCommand(new MatchingCancelCommand(order));
                        break;
                }

                _marketDataCache.AddIncremental(order.ToMarketData());

                TriggerPriceChanged(order);

                if (_running)
                    break;
            }else
                Thread.Sleep(10);
        }
    }
    private void TriggerPriceChanged(OrderEngine order)
    {
        if (PriceChanged is not null)
            PriceChanged(this, new OrderPriceEventArgs(order));
    }
    private void ExecutedTradeOutcome()
    {
        while (true)
        {
            if (QueueExecutedTraded.TryDequeue(out  Dictionary<long, DropCopyReport> reports))
                Bus.SendCommand(new ExecutedTradeCommand(reports));

            if (_running)
                break;

            Thread.Sleep(10);
        }
    }

    public bool CancelOrder(OrderEngine orderToCancel)
    {
        bool cancelled = false;
        cancelled = RemoveCancelledOrdersAsync(orderToCancel).Result;
        if (cancelled)
        {
            orderToCancel.OrderStatus = OrderStatus.Cancelled;
        }
        if (cancelled)
            QueueOrderStatusChanged.Enqueue(orderToCancel);

        return cancelled;
    }

    public async Task<bool> ModifyOrder(OrderEngine orderEngine)
    {
        bool modified = false;
        if (orderEngine.Side == SideTrade.Buy)
        {
            modified = await _matchingCache.UpsertBuyOrder(orderEngine);
        }
        else if (orderEngine.Side == SideTrade.Buy)
        {
            modified = await _matchingCache.UpsertSellOrder(orderEngine);
        }
        return modified;
    }

    public async Task<bool> RemoveCancelledOrdersAsync(OrderEngine orderToCancel)
    {
        bool cancelled = false;
        if (orderToCancel.Side == SideTrade.Buy)
        {
            cancelled = await _matchingCache.DeleteBuyOrderAsync(orderToCancel.Symbol, orderToCancel.OrderID);
        }else if (orderToCancel.Side == SideTrade.Buy)
        {
            cancelled = await _matchingCache.DeleteSellOrderAsync(orderToCancel.Symbol, orderToCancel.OrderID);
        }
        return cancelled;
    }

    public async Task<bool> RemoveTradedOrdersAsync(Dictionary<long, OrderEngine> dicOrders)
    {
        var removed = await _matchingCache.DeleteAllOrderAsync(dicOrders);
        return removed;
    }

    public void CreateReports(OrderEngine order, Dictionary<long, OrderEngine> dicOrders)
    {
        CreateExecutionReport(order, dicOrders);
        CreateTradeCapture(order, dicOrders);
    }

    private Dictionary<long, DropCopyReport> CreateExecutionReport(OrderEngine order, Dictionary<long, OrderEngine> dicOrders) 
    {
        var now = DateTime.Now; 

        var result = new Dictionary<long, DropCopyReport>();
        
        var report = new ExecutionReport();
        report.MinQty = order.MinQty;
        report.ParticipatorId = order.ParticipatorId;
        report.AccoutType = '1'; //1= Client and 3 = HOuse
        report.ExpireTime = order.ExpireTime;
        report.ExpireDate = order.ExpireDate;
        report.TimeInForce = order.TimeInForce;
        report.StopPrice = order.StopPrice;
        report.Symbol = order.Symbol;
        report.Quantity = order.Quantity;
        report.Side = order.Side; 
        report.OrigCLOrdID = order.ClOrdID;
        report.OrderID = order.OrderID;
        report.TradeId = order.OrderID;
        report.ExecID = order.OrderID;
        report.Price = order.Price;
        report.ExecType = 'F';      // Fully or partially 

        result.Add(report.TradeId, report);

        QueueOrderStatusChanged.Enqueue(order);

        foreach (var orderPart in dicOrders.Values)
        {
            var reportPart = new ExecutionReport();
            reportPart.MinQty = orderPart.MinQty;
            reportPart.ParticipatorId = orderPart.ParticipatorId;
            reportPart.AccoutType = '1'; //1= Client and 3 = HOuse
            reportPart.ExpireTime = orderPart.ExpireTime;
            reportPart.ExpireDate = orderPart.ExpireDate;
            reportPart.TimeInForce = orderPart.TimeInForce;
            reportPart.StopPrice = orderPart.StopPrice;
            reportPart.Symbol = orderPart.Symbol;
            reportPart.Quantity = orderPart.Quantity;
            reportPart.Side = orderPart.Side;
            reportPart.OrigCLOrdID = orderPart.ClOrdID;
            reportPart.OrderID = orderPart.OrderID;
            reportPart.TradeId = order.OrderID;
            reportPart.ExecID = order.OrderID;
            reportPart.Price = orderPart.Price;
            reportPart.ExecType = 'F';      // Fully or partially 

            result.Add(reportPart.TradeId, reportPart);
            
            QueueOrderStatusChanged.Enqueue(orderPart);
        }

        QueueExecutedTraded.Enqueue(result);

        return result;
    }
    private Dictionary<long, DropCopyReport> CreateTradeCapture(OrderEngine order, Dictionary<long, OrderEngine> dicOrders)
    {
        var result =new Dictionary<long, DropCopyReport>();
        var report = new TradeCaptureReport()
        {
            TradeReportTransType = 0,
            TrdType = 0,
            CopyMsgIndicator = 'Y',
            PreviouslyReported = 'N',
            TradeId = order.OrderID,
            NoSides = 1,
            OrderId = order.OrderID.ToString(),
            ClOrderId = order.ClOrdID.ToString(),
            LastQty = order.LastQuantity,
            LastPx = order.LastPrice,
            Symbol = order.Symbol,
            Side = (char)order.Side,
            Price = order.Price,
            TransactTime = order.TransactTime,
            TradeDate = DateTime.Now.ToString("yyyyMMdd"),
            AccountType = order.Account.AccountType,
        };

        result.Add(report.TradeId, report);

        foreach (var orderPart in dicOrders.Values)
        {
            var reportPart = new TradeCaptureReport()
            {
                TradeReportTransType = 0,
                TrdType = 0,
                CopyMsgIndicator = 'Y',
                PreviouslyReported = 'N',
                TradeId = order.OrderID,
                NoSides = 1,
                OrderId = orderPart.OrderID.ToString(),
                ClOrderId = orderPart.ClOrdID.ToString(),
                LastQty = orderPart.LastQuantity,
                LastPx = orderPart.LastPrice,
                Symbol = orderPart.Symbol,
                Side = (char)orderPart.Side,
                Price = orderPart.Price,
                TransactTime = orderPart.TransactTime,
                TradeDate = DateTime.Now.ToString("yyyyMMdd"),
                AccountType = orderPart.Account.AccountType,
            };

            result.Add(reportPart.TradeId, reportPart);
        }

        QueueExecutedTraded.Enqueue(result);
        return result;
    }

    public void SortBuyOrders(ref Dictionary<long, OrderEngine> buyOrders)
    {
        var ordersSorted = buyOrders.OrderBy(kvp => kvp.Value.Price);
        buyOrders = ordersSorted.ToDictionary<KeyValuePair<long, OrderEngine>, long, OrderEngine>(
        pair => pair.Key, pair => pair.Value);
    }

    public void SortSellOrders(ref Dictionary<long, OrderEngine> sellOrders)
    {
        var ordersSorted = sellOrders.OrderByDescending(kvp => kvp.Value.Price);
        sellOrders = ordersSorted.ToDictionary<KeyValuePair<long, OrderEngine>, long, OrderEngine>(
        pair => pair.Key, pair => pair.Value);
    }

    
    public bool AddOrder(OrderEngine order)
    {
        bool added = false;
        if (order.Side == SideTrade.Buy)
        {
            _matchingCache.UpsertBuyOrder(order);
        }
        else if (order.Side == SideTrade.Sell)
        {
            _matchingCache.UpsertSellOrder(order);
        }
        order.OrderStatus = OrderStatus.New;
        QueueOrderStatusChanged.Enqueue(order);
        added = true;
        return added;
    }

    public void Dispose()
    {
        _running = false;
    }
}