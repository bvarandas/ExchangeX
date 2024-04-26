using Microsoft.Extensions.Logging;
using ShareX.Core.Interfaces;
using System.Collections.Concurrent;
using SharedX.Core.Enums;

namespace SharedX.Core.Matching;

public class OrderBook : Order, IOrderBook
{
    protected readonly ILogger<OrderBook> _logger;
    
    protected readonly Order[] _bookBid;
    protected readonly Order[] _bookAsk;
    public OrderBook(ILogger<OrderBook> logger)
    {
        _logger = logger;
        
    }

    public void AddOrder(Order order)
    {
        if (order.Side == SideTrade.Buy)
        {
            _bookBid.Concat(new Order[] { order });
            Array.Sort(_bookBid, new OrderBuyComparer());
        }
        else if (order.Side == SideTrade.Sell)
        {
            _bookAsk.Concat(new Order[] { order });
            Array.Sort(_bookAsk, new OrderSellComparer());
        }
    }

    public void ReplaceOrder(Order order)
    {
        if (order.Side == SideTrade.Buy)
        {
            //_bookBid[Array.IndexOf(_bookBid, )]
            Array.Sort(_bookBid, new OrderBuyComparer());
        }
        else if (order.Side == SideTrade.Sell)
        {
            //_bookAsk.Concat(new Order[] { order });
            Array.Sort(_bookAsk, new OrderSellComparer());
            
        }
    }

    public void CancelOrder(Order order)
    {

    }


}
