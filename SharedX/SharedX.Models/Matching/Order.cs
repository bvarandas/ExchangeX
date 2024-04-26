using SharedX.Core.Account;
using SharedX.Core.Entities;
using SharedX.Core.Enums;
namespace SharedX.Core.Matching;
public abstract class Order : BaseEntity 
{
    public string Symbol { get; set; } = string.Empty;
    
    #region quantity
    public decimal Quantity { get; set; }
    public decimal LeavesQuantity { get; set; }
    public decimal LastQuantity { get; set; }
    #endregion

    public SideTrade Side { get; set; }
    public  OrderType OrderType { get; set; }
    public virtual OrderStatus OrderStatus { get; set; }
    public long ParticipatorId { get; set; }
    public long OrderID { get; set; }
    public long AccountId { get; set; }
    public Limit Account { get; set; } = null!;

    #region Prices
    public decimal Price { get; set; }
    public decimal LastPrice { get; set; }
    public decimal StopPrice { get; set; }
    public decimal AveragePrice { get; set; }
    #endregion
    public IList<OrderDetail> OrderDetails { get; set; } = null!;
}