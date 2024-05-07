using SharedX.Core.Matching;
using SharedX.Core.Enums;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SharedX.Core.Bus;
using MacthingX.Application.Commands;
using MacthingX.Application.Events;
using QuickFix;
using SharedX.Core.Matching.DropCopy;
using TradeReportTransType = SharedX.Core.Enums.TradeReportTransType;
using MatchingX.Core.Repositories;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;

namespace MacthingX.Application.Services;
public abstract class MatchBase : MatchLastPrice, IDisposable
{
    protected bool _running;
    protected readonly ILogger<MatchBase> _logger;
    protected readonly ConcurrentDictionary<string, Dictionary<long, OrderEngine>> _buyOrders;
    protected readonly ConcurrentDictionary<string, Dictionary<long, OrderEngine>> _sellOrders;

    protected readonly ConcurrentQueue<OrderEngine> QueueOrderStatusChanged;
    protected readonly ConcurrentQueue<(TradeCaptureReport, TradeCaptureReport)> QueueExecutedTraded;
    
    private readonly Thread ThreadExecutedTrade;
    private readonly Thread ThreadOrdersStatus;

    protected readonly IMediatorHandler Bus;
    protected static long TradeId;

    private readonly IOrderRepository _orderRepository;
    private readonly ITradeRepository _tradeRepository;
    //protected MatchBase(ILogger<MatchBase> logger, 
    //    IMediatorHandler bus, 
    //    IOrderRepository orderRepository,
    //    ITradeRepository tradeRepository)
    //{
    //    _logger = logger;
    //    Bus = bus;
    //    _orderRepository = orderRepository;
    //    _tradeRepository = tradeRepository;

    //    _buyOrders = new ConcurrentDictionary<string, Dictionary<long, Order>>();
    //    _sellOrders = new ConcurrentDictionary<string, Dictionary<long, Order>>();

    //    QueueExecutedTraded = new ConcurrentQueue<(TradeCaptureReport, TradeCaptureReport)>();
    //    QueueOrderStatusChanged = new ConcurrentQueue<Order>();

    //    //ThreadExecutedTrade = new Thread(new ThreadStart(ExecutedTradeOutcome));
    //    //ThreadExecutedTrade.Name = nameof(ExecutedTradeOutcome);
    //    //ThreadExecutedTrade.Start();

    //    //ThreadOrdersStatus = new Thread(new ThreadStart(OrderStatusChangedOutcome));
    //    //ThreadOrdersStatus.Name = nameof(OrderStatusChangedOutcome);
    //    //ThreadOrdersStatus.Start();

    //    //LoadOrdersOnRestart();

    //}
    private void LoadOrdersOnRestart()
    {
        var ordersDb = _orderRepository.GetOrdersOnRestartAsync(default(CancellationToken));
        var buyOrders = ordersDb.Result.Where(o => o.Side == SideTrade.Buy);
        var sellOrders = ordersDb.Result.Where(o => o.Side == SideTrade.Sell);

        foreach (var order in buyOrders) {
            if (_buyOrders.TryGetValue(order.Symbol, out var orders)){
                orders.TryAdd(order.OrderID, order);
            }
            else
            {
                var dicOrders = new Dictionary<long, OrderEngine>();
                dicOrders.Add(order.OrderID, order);
                _buyOrders.TryAdd(order.Symbol, dicOrders);
            }
        }

        foreach (var order in sellOrders)
        {
            if (_sellOrders.TryGetValue(order.Symbol, out var orders)){
                orders.TryAdd(order.OrderID, order);
            }
            else
            {
                var dicOrders = new Dictionary<long, OrderEngine>();
                dicOrders.Add(order.OrderID, order);
                _sellOrders.TryAdd(order.Symbol, dicOrders);
            }
        }
    }

    private void OrderStatusChangedOutcome()
    {
        while (true)
        {
            if (!QueueOrderStatusChanged.TryDequeue(out OrderEngine order))
                continue;

            switch(order.OrderStatus)
            {
                case OrderStatus.New:
                    Bus.SendCommand(new OrderOpenedCommand(order));
                    break;
                case OrderStatus.Filled:
                    Bus.SendCommand(new OrderFilledCommand(order));
                    break;
                case OrderStatus.Rejected:
                    Bus.SendCommand(new OrderRejectedCommand(order));
                    break;
                case OrderStatus.Cancelled:
                    Bus.SendCommand(new OrderCanceledCommand(order));
                    break;
            }

            AddUpdatePrice(order);

            if (_running)
                break;

            Thread.Sleep(10);
        }
    }

    private void ExecutedTradeOutcome()
    {
        while(true)
        {
            if (QueueExecutedTraded.TryDequeue(out (TradeCaptureReport, TradeCaptureReport) trade))
                Bus.SendCommand(new ExecutedTradeCommand(trade));
            
            if (_running)
                break;

            Thread.Sleep(10);
        }
    }

    protected void SortBuyOrders(ref Dictionary<long, OrderEngine> buyOrders)
    {
        var ordersSorted = buyOrders.OrderBy(kvp => kvp.Value.Price);
        buyOrders = ordersSorted.ToDictionary<KeyValuePair<long, OrderEngine>, long, OrderEngine>(
        pair => pair.Key, pair => pair.Value);
    }

    protected void SortSellOrders(ref Dictionary<long, OrderEngine> sellOrders)
    {
        var ordersSorted = sellOrders.OrderByDescending(kvp => kvp.Value.Price);
        sellOrders = ordersSorted.ToDictionary<KeyValuePair<long, OrderEngine>, long, OrderEngine>(
        pair => pair.Key, pair => pair.Value);
    }

    protected virtual void AddOrder(OrderEngine order)
    {
        if (order.Side == SideTrade.Buy)
        {
            if (_buyOrders.TryGetValue(order.Symbol, out Dictionary<long, OrderEngine> buyOrder))
            {
                buyOrder.TryAdd(order.OrderID, order);
                SortBuyOrders(ref buyOrder);
            }
            else
            {
                var buyOrderNew = new Dictionary<long, OrderEngine>();
                buyOrderNew.TryAdd(order.OrderID, order);
                _buyOrders.TryAdd(order.Symbol, buyOrderNew);
            }

        }
        else if (order.Side == SideTrade.Sell)
        {
            if (_sellOrders.TryGetValue(order.Symbol, out Dictionary<long, OrderEngine> sellOrder))
            {
                sellOrder.TryAdd(order.OrderID, order);
                SortSellOrders(ref sellOrder);
            }
            else
            {
                var sellOrderNew = new Dictionary<long, OrderEngine>();
                sellOrderNew.TryAdd(order.OrderID, order);
                _sellOrders.TryAdd(order.Symbol, sellOrderNew);
            }
        }

        order.OrderStatus = OrderStatus.New;
        QueueOrderStatusChanged.Enqueue(order);
    }

    protected virtual void AddOrderDetail(ref OrderEngine order)
    {
        if (order.OrderDetails is null)
            order.OrderDetails = new List<OrderEngineDetail>();

        order.OrderDetails.Add((OrderEngineDetail)order);
    }

    protected virtual void ReplaceOrder(OrderEngine order)
    {
        if (order.Side == SideTrade.Buy)
        {
            if (_buyOrders.TryGetValue(order.Symbol, out Dictionary<long, OrderEngine> buyOrders))
            {
                if (buyOrders.TryGetValue(order.OrderID, out OrderEngine orderBuy))
                {
                    orderBuy = order;
                }
            }
        }
        else if (order.Side == SideTrade.Sell)
        {
            if (_sellOrders.TryGetValue(order.Symbol, out Dictionary<long, OrderEngine> sellOrders))
            {
                if (sellOrders.TryGetValue(order.OrderID, out OrderEngine orderSell))
                {
                    orderSell = order;
                }
            }
        }

        AddOrderDetail(ref order);

        order.OrderStatus = OrderStatus.New;
        QueueOrderStatusChanged.Enqueue(order);
    }
    protected abstract void MatchOrderLimit(OrderEngine order);
    protected abstract void MatchOrderMarket(OrderEngine order);
    protected virtual void CancelOrder(OrderEngine orderToCancel)
    {
        bool canceled = false;
        if (orderToCancel.Side == SideTrade.Buy)
        {
            if (_buyOrders.TryGetValue(orderToCancel.Symbol, out Dictionary<long, OrderEngine> buyOrders))
            {
                RemoveCancelledOrders(ref buyOrders, orderToCancel, ref canceled);
            }
        }else if (orderToCancel.Side == SideTrade.Sell)
        {
            if (_sellOrders.TryGetValue(orderToCancel.Symbol, out Dictionary<long, OrderEngine> sellOrders))
            {
                RemoveCancelledOrders(ref sellOrders, orderToCancel, ref canceled);
            }
        }
        if (canceled)
        {
            orderToCancel.OrderStatus = OrderStatus.Cancelled;
            AddOrderDetail(ref orderToCancel);
            QueueOrderStatusChanged.Enqueue(orderToCancel);
        }
    }
    protected void RemoveCancelledOrders(ref Dictionary<long, OrderEngine> orders, OrderEngine orderToCancel, ref bool canceled)
    {
        if (orders.TryGetValue(orderToCancel.OrderID, out OrderEngine order))
        {
            //orders.TryRemove(new KeyValuePair<long, Order>(order.OrderID, order));
            canceled = orders.Remove(orderToCancel.OrderID);
        }
    }
    protected void RemoveTradedOrders(ref Dictionary<long,OrderEngine> buyOrders, 
        ref Dictionary<long, OrderEngine> sellOrders, 
        OrderEngine buyOrder, 
        OrderEngine sellOrder)
    {
        if (buyOrders.TryGetValue(buyOrder.OrderID, out OrderEngine orderBuyFound))
        {
            buyOrders.Remove(buyOrder.OrderID);
        }

        if (sellOrders.TryGetValue(sellOrder.OrderID, out OrderEngine orderSellFound))
        {
            sellOrders.Remove(sellOrder.OrderID);
        }
    }

    protected virtual TradeCaptureReport CreateTradeCaptureCancelled(OrderEngine order)
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

    protected virtual (TradeCaptureReport, TradeCaptureReport) 
        CreateTradeCapture(OrderEngine orderBuyer, OrderEngine orderSeller)
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

    public void Dispose()
    {
        _running = false;
        _buyOrders.Clear();
        _sellOrders.Clear();
    }
}