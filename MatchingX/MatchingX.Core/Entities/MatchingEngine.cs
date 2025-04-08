using SharedX.Core.Entities;
using SharedX.Core.Enums;

namespace MatchingX.Core.Entities;
public sealed class MatchingEngine : BaseEntity
{
    public MatchOrder PrincipalOrder { get; set; } = null!;
    public Dictionary<long, MatchOrder> PartOrders { get; set; } = null!;
}

public sealed class MatchOrder
{
    public string Symbol { get; set; } = string.Empty;
    public char AccountType { get; set; }
    public decimal Quantity { get; set; } // Original Order Qty- Removed (modification) Qty
    public decimal LeavesQuantity { get; set; }
    public SideTrade Side { get; set; }
    public OrderType OrderType { get; set; }
    public TimeInForce TimeInForce { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public long OrderID { get; set; }
    public long ClOrdID { get; set; }
    public long AccountId { get; set; }
    public decimal Price { get; set; }
    public decimal StopPrice { get; set; }
    public DateTime TransactTime { get; set; }
    public long OrigClOrdID { get; set; }
    public Execution Execution { get; set; }
    public decimal MinQty { get; set; }
    public decimal LastPrice { get; set; }
    public decimal LastQuantity { get; set; }
}

