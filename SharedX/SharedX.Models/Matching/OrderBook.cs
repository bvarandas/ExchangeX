using Microsoft.Extensions.Logging;
using ShareX.Core.Interfaces;
using SharedX.Core.Enums;
using ProtoBuf;
using QuickFix.Fields;

namespace SharedX.Core.Matching;

[ProtoContract]
public class Book
{
    [ProtoMember(1)]
    public SideTrade Side { get; set; }
    [ProtoMember(2)]
    public decimal Price { get; set; }
    [ProtoMember(3)]
    public decimal Amount { get; set; }
    [ProtoMember(4)]
    public long OrderId { get; set; }
    [ProtoMember(5)]
    public DateTime Timestamp { get; set; }
    [ProtoMember(6)]
    public string Symbol { get; set; }
    public Book()
    {

    }
}
[ProtoContract]
public class OrderBook : IOrderBook
{
    protected readonly ILogger<OrderBook> _logger;
    
    [ProtoMember(1)]
    public string Symbol { get; set; } = string.Empty;
    [ProtoMember(2)]
    public DateTime Timestamp { get; set; }
    [ProtoMember(3)]
    protected Book[] _bookBid;
    [ProtoMember(4)]
    protected Book[] _bookAsk;
    public OrderBook(ILogger<OrderBook> logger)
    {
        _logger = logger;
    }
    public OrderBook()
    {

    }

    public void AddOrder(Book book)
    {
        if (book.Side == SideTrade.Buy)
        {
            _bookBid = _bookBid.Concat(new Book[] { book }).ToArray();
            Array.Sort(_bookBid, new OrderBuyComparer());
        }
        else if (book.Side == SideTrade.Sell)
        {
            _bookAsk = _bookAsk.Concat(new Book[] { book }).ToArray();
            Array.Sort(_bookAsk, new OrderSellComparer());
        }
    }

    public void ReplaceOrder(Book order)
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

    public void CancelOrder(Book book)
    {
        if (book.Side == SideTrade.Buy)
        {
            _bookBid = Array.FindAll(_bookBid, (bookBid) => { return bookBid.OrderId != book.OrderId; }).ToArray();
        }
        else if (book.Side == SideTrade.Sell)
        {
            _bookAsk = Array.FindAll(_bookAsk, (bookAsk) => { return bookAsk.OrderId != book.OrderId; }).ToArray();
        }
    }

}