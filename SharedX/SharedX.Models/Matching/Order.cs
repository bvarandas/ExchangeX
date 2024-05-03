using SharedX.Core.Account;
using SharedX.Core.Entities;
using SharedX.Core.Enums;
namespace SharedX.Core.Matching;
public class Order : BaseEntityFix 
{
    public string Symbol { get; set; } = string.Empty;
    
    #region quantity
    public decimal Quantity { get; set; }
    public decimal LeavesQuantity { get; set; }
    public decimal LastQuantity { get; set; }
    #endregion

    public SideTrade Side { get; set; }
    public OrderType OrderType { get; set; }
    public TimeInForce TimeInForce { get; set; }
    public virtual OrderStatus OrderStatus { get; set; }
    public long ParticipatorId { get; set; }
    public long OrderID { get; set; }
    public long ClOrdID { get; set; }
    public long AccountId { get; set; }
    public Limit Account { get; set; } = null!;
    public string ExpireDate { get; set; } = string.Empty;
    public string ExpireTime { get; set; } = string.Empty;

    #region Prices
    public decimal Price { get; set; }
    public decimal LastPrice { get; set; }
    public decimal StopPrice { get; set; }
    public decimal AveragePrice { get; set; }
    #endregion
    public DateTime TransactTime { get; set; }
    public IList<OrderDetail> OrderDetails { get; set; } = null!;
}