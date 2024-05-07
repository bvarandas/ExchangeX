using Microsoft.Extensions.Logging;
using ShareX.Core.Interfaces;


using SharedX.Core.Enums;

namespace SharedX.Core.Matching;

public class OrderBook : OrderEngine.OrderEngine, IOrderBook
{
    protected readonly ILogger<OrderBook> _logger;
    
    protected readonly OrderEngine.OrderEngine[] _bookBid;
    protected readonly OrderEngine.OrderEngine[] _bookAsk;
    public OrderBook(ILogger<OrderBook> logger)
    {
        _logger = logger;
        
    }

    public void AddOrder(OrderEngine.OrderEngine order)
    {
        if (order.Side == SideTrade.Buy)
        {
            _bookBid.Concat(new OrderEngine.OrderEngine[] { order });
            Array.Sort(_bookBid, new OrderBuyComparer());
        }
        else if (order.Side == SideTrade.Sell)
        {
            _bookAsk.Concat(new OrderEngine.OrderEngine[] { order });
            Array.Sort(_bookAsk, new OrderSellComparer());
        }
    }

    public void ReplaceOrder(OrderEngine.OrderEngine order)
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

    public void CancelOrder(OrderEngine.OrderEngine order)
    {

    }


}
