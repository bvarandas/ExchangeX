using MacthingX.Application.Commands;
using MacthingX.Application.Events;
using MacthingX.Application.Extensions;
using MacthingX.Application.Interfaces;
using MassTransit.Futures.Contracts;
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
                    case OrderStatus.New:
                        Bus.SendCommand(new OrderOpenedCommand(order, _matchingCache));
                        break;
                    case OrderStatus.Filled:
                        Bus.SendCommand(new OrderFilledCommand(order, _matchingCache));
                        break;
                    case OrderStatus.PartiallyFilled:
                        Bus.SendCommand(new OrderPartiallyFilledCommand(order, _matchingCache));
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

    public (ExecutionReport, ExecutionReport) CreateExecutionReport(OrderEngine orderBuyer, OrderEngine orderSeller) 
    {
        var trade = _tradeRepository.GetTradeIdAsync(default(CancellationToken));
        var tradeId = trade.Result.TradeId;
        var now = DateTime.Now; 

        var reportBuyer = new ExecutionReport();
        var reportSeller = new ExecutionReport();

        var buyQtyExecuted = reportBuyer.LeavesQuantity>= reportSeller.LeavesQuantity? reportSeller.LeavesQuantity: reportBuyer.LeavesQuantity;
        var sellQtyExecuted = reportSeller.LeavesQuantity >= reportBuyer.LeavesQuantity? reportBuyer.LeavesQuantity: reportSeller.LeavesQuantity;

        var netBuy = reportBuyer.LeavesQuantity - reportSeller.LeavesQuantity;
        var netSell = reportSeller.LeavesQuantity - reportBuyer.LeavesQuantity;

        reportBuyer.LeavesQuantity = netBuy <= 0 ? 0 : netBuy;
        reportSeller.LeavesQuantity = netSell <= 0 ?0 : netSell;

        reportBuyer.LastPrice = orderBuyer.Price != 0 ? orderBuyer.Price : orderSeller.Price;
        reportSeller.LastPrice = orderBuyer.Price != 0 ? orderBuyer.Price : orderSeller.Price;

        // Status
        reportBuyer.OrderStatus = reportBuyer.LeavesQuantity == 0 ? OrderStatus.Filled : OrderStatus.PartiallyFilled;
        reportSeller.OrderStatus = reportSeller.LeavesQuantity == 0 ? OrderStatus.Filled : OrderStatus.PartiallyFilled;

        reportBuyer.LastQuantity = buyQtyExecuted;
        reportSeller.LastQuantity = sellQtyExecuted;

        reportBuyer.MinQty = orderBuyer.MinQty;
        reportBuyer.ParticipatorId = orderBuyer.ParticipatorId;
        reportBuyer.AccoutType = '1'; //1= Client and 3 = HOuse
        reportBuyer.ExpireTime = orderBuyer.ExpireTime;
        reportBuyer.ExpireDate = orderBuyer.ExpireDate;
        reportBuyer.TimeInForce = orderBuyer.TimeInForce;
        reportBuyer.StopPrice = orderBuyer.StopPrice;
        reportBuyer.Symbol = orderBuyer.Symbol;
        reportBuyer.Quantity = orderBuyer.Quantity;
        reportBuyer.Side = SideTrade.Buy;
        reportBuyer.OrigCLOrdID = orderBuyer.ClOrdID;
        reportBuyer.OrderID = orderBuyer.OrderID;
        reportBuyer.TrdMatchID = tradeId;
        reportBuyer.ExecID = tradeId;
        reportBuyer.Price = orderBuyer.Price;
        reportBuyer.ExecType = 'F';      // Fully or partially 
        
        reportSeller.MinQty = orderSeller.MinQty;
        reportSeller.ParticipatorId = orderSeller.ParticipatorId;
        reportSeller.AccoutType = '1'; //1= Client and 3 = HOuse
        reportSeller.ExpireTime = orderSeller.ExpireTime;
        reportSeller.ExpireDate = orderSeller.ExpireDate;
        reportSeller.TimeInForce = orderSeller.TimeInForce;
        reportSeller.StopPrice = orderSeller.StopPrice;
        reportSeller.Symbol = orderSeller.Symbol;
        reportSeller.Quantity = orderSeller.Quantity;
        reportSeller.Side = SideTrade.Sell;
        reportSeller.OrigCLOrdID = orderSeller.ClOrdID;
        reportSeller.OrderID = orderSeller.OrderID;
        reportSeller.TrdMatchID = tradeId;
        reportSeller.ExecID = tradeId;
        reportSeller.Price = orderSeller.Price;
        reportSeller.ExecType = 'F';      // Fully or partially 

        //Orders
        orderBuyer.OrderStatus = reportBuyer.OrderStatus;
        orderSeller.OrderStatus = reportSeller.OrderStatus;

        orderBuyer.LeavesQuantity = reportBuyer.LeavesQuantity;
        orderSeller.LeavesQuantity = reportSeller.LeavesQuantity;

        orderBuyer.LastPrice = reportBuyer.LastPrice;
        orderSeller.LastPrice = reportSeller.LastPrice;

        orderBuyer.LastQuantity = buyQtyExecuted;
        orderSeller.LastQuantity = sellQtyExecuted;

        orderBuyer.TransactTime = now;
        orderSeller.TransactTime = now;

        QueueOrderStatusChanged.Enqueue(orderBuyer);
        QueueOrderStatusChanged.Enqueue(orderSeller);

        return (reportBuyer, reportSeller);
    }
    public (TradeCaptureReport, TradeCaptureReport) CreateTradeCapture(OrderEngine orderBuyer, OrderEngine orderSeller)
    {
        var trade = _tradeRepository.GetTradeIdAsync(default(CancellationToken));
        var tradeId = trade.Result.TradeId;

        var orderStatus = OrderStatus.Filled;
        if (orderBuyer.LastQuantity != orderSeller.LastQuantity)
        {
            orderStatus = OrderStatus.PartiallyFilled;
        }else if (orderBuyer.LastQuantity == orderSeller.LastQuantity)
        {
            orderStatus = OrderStatus.PartiallyFilled;
        }
        /// novo Status das ordens
        orderBuyer.OrderStatus = orderStatus;
        orderSeller.OrderStatus = orderStatus;

        //// novas quantidades das ordens
        //orderBuyer.LastQuantity = 
        //orderBuyer.LastQuantity = orderSeller.



        var tradeBuyer = new TradeCaptureReport()
        {
            TradeReportTransType = 0,
            TrdType = 0,
            CopyMsgIndicator = 'Y',
            PreviouslyReported = 'N',
            TradeId = tradeId,
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
            TradeId = tradeId,
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

        if (order.Side == SideTrade.Buy)
            _matchingCache.UpsertBuyOrder(order);
        else if (order.Side == SideTrade.Sell)
            _matchingCache.UpsertSellOrder(order);

        order.OrderStatus = OrderStatus.New;
        QueueOrderStatusChanged.Enqueue(order);
        replaced = true;
        return replaced;
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