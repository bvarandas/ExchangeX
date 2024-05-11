using MacthingX.Application.Commands;
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
    protected static long TradeId;

    protected readonly ConcurrentQueue<OrderEngine> QueueOrderStatusChanged;
    protected readonly ConcurrentQueue<(TradeCaptureReport, TradeCaptureReport)> QueueExecutedTraded;

    private readonly Thread ThreadExecutedTrade;
    private readonly Thread ThreadOrdersStatus;
    public event PriceChangedEventHandler PriceChanged;

    public TradeOrderService(ILogger<TradeOrderService> logger, IMediatorHandler bus, IMatchingCache matchingCache, IMarketDataCache marketDataCache)
    {
        _logger = logger;
        Bus = bus;
        _matchingCache = matchingCache;
        _marketDataCache = marketDataCache;

        QueueExecutedTraded = new ConcurrentQueue<(TradeCaptureReport, TradeCaptureReport)>();
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
                    case OrderStatus.Trade:
                        Bus.SendCommand(new OrderTradeCommand(order, _matchingCache));
                        break;
                    case OrderStatus.Cancelled:
                        Bus.SendCommand(new OrderCancelCommand(order, _matchingCache));
                        break;
                }

                _marketDataCache.AddIncremental(order.ToMarketData());

                TriggerPriceChanged(order);

                if (_running)
                    break;

                Thread.Sleep(10);
            }
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
            if (QueueExecutedTraded.TryDequeue(out (TradeCaptureReport, TradeCaptureReport) trade))
                Bus.SendCommand(new ExecutedTradeCommand(trade));

            if (_running)
                break;

            Thread.Sleep(10);
        }
    }

    public bool CancelOrder(OrderEngine orderToCancel)
    {
        bool cancelled = false;
        if (orderToCancel.Side == SideTrade.Buy)
        {
            cancelled = RemoveCancelledOrdersAsync(orderToCancel).Result;
        }
        else if (orderToCancel.Side == SideTrade.Sell)
        {
            cancelled = RemoveCancelledOrdersAsync(orderToCancel).Result;
        }
        if (cancelled)
        {
            orderToCancel.OrderStatus = OrderStatus.Cancelled;
            AddOrderDetail(ref orderToCancel);
        }
        if (cancelled)
            QueueOrderStatusChanged.Enqueue(orderToCancel);

        return cancelled;
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

    public async Task<bool> RemoveTradedOrdersAsync(OrderEngine buyOrder, OrderEngine sellOrder)
    {
        var buyRemoved = await _matchingCache.DeleteBuyOrderAsync(buyOrder.Symbol, buyOrder.OrderID);
        var sellRemoved = await _matchingCache.DeleteBuyOrderAsync(sellOrder.Symbol, sellOrder.OrderID);

        return buyRemoved && sellRemoved;
    }
    public TradeCaptureReport CreateTradeCaptureCancelled(OrderEngine order)
    {
        var trade = new TradeCaptureReport()
        {
            TradeReportTransType = 1,
            TrdType = 0,
            CopyMsgIndicator = 'Y',
            PreviouslyReported = 'N',
            TradeId = TradeId,
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
        return trade;
    }
    public (TradeCaptureReport, TradeCaptureReport) CreateTradeCapture(OrderEngine orderBuyer, OrderEngine orderSeller)
    {
        var trade = _tradeRepository.GetTradeIdAsync(default(CancellationToken));
        TradeId = trade.Result.TradeId;

        var tradeBuyer = new TradeCaptureReport()
        {
            TradeReportTransType = 0,
            TrdType = 0,
            CopyMsgIndicator = 'Y',
            PreviouslyReported = 'N',
            TradeId = TradeId,
            NoSides = 1,
            OrderId = orderBuyer.OrderID.ToString(),
            ClOrderId = orderBuyer.ClOrdID.ToString(),
            LastQty = orderBuyer.LastQuantity,
            LastPx = orderBuyer.LastPrice,
            Symbol = orderBuyer.Symbol,
            Side = (char)orderBuyer.Side,
            Price = orderBuyer.Price,
            TransactTime = orderBuyer.TransactTime,
            TradeDate = DateTime.Now.ToString("yyyyMMdd"),
            AccountType = orderBuyer.Account.AccountType,
        };

        var tradeSeller = new TradeCaptureReport()
        {
            TradeReportTransType = 0,
            TrdType = 0,
            CopyMsgIndicator = 'Y',
            PreviouslyReported = 'N',
            TradeId = TradeId,
            NoSides = 1,
            OrderId = orderSeller.OrderID.ToString(),
            ClOrderId = orderSeller.ClOrdID.ToString(),
            LastQty = orderSeller.LastQuantity,
            LastPx = orderSeller.LastPrice,
            Symbol = orderSeller.Symbol,
            Side = (char)orderSeller.Side,
            Price = orderSeller.Price,
            TransactTime = orderSeller.TransactTime,
            TradeDate = DateTime.Now.ToString("yyyyMMdd"),
            AccountType = orderSeller.Account.AccountType,
        };

        QueueExecutedTraded.Enqueue((tradeBuyer, tradeSeller));

        return (tradeBuyer, tradeSeller);
    }

    public bool ReplaceOrder(OrderEngine order)
    {
        bool replaced = false;
        AddOrderDetail(ref order);

        if (order.Side == SideTrade.Buy)
            _matchingCache.UpsertBuyOrder(order);
        else if (order.Side == SideTrade.Sell)
            _matchingCache.UpsertSellOrder(order);

        order.OrderStatus = OrderStatus.New;
        QueueOrderStatusChanged.Enqueue(order);
        replaced = true;
        return replaced;
    }

    protected virtual void AddOrderDetail(ref OrderEngine order)
    {
        if (order.OrderDetails is null)
            order.OrderDetails = new List<OrderEngineDetail>();

        order.OrderDetails.Add((OrderEngineDetail)order);
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