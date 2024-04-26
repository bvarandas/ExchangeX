using SharedX.Core.Entities;
namespace SharedX.Core.Matching;
public abstract class ExecutedTradeBase : BaseEntity
{
    public long BuyerOrderId { get; set; }
    public long SellerOrderId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal CumQuantity { get; set; }
    public decimal LeavesQuantity { get; set; }
    public decimal OrderPrice { get; set; }
    public long BuyerAccountId { get; set; }
    public long SellerAccountId { get; set; }
    public bool Settlemented { get; set; }
    public DateTime? SettlementDate { get; set; }
}

public class ExecutedTrade : ExecutedTradeBase
{
    public ExecutedTrade(string symbol,
        long buyerOrderId,
        long sellerOrderId,
        decimal cumQuantity, 
        decimal leavesQuantity,
        decimal orderPrice, 
        long buyerAccountId, 
        long sellerAccountId)
    {
        BuyerOrderId = buyerOrderId;
        SellerOrderId = sellerOrderId;
        Symbol = symbol;
        CumQuantity = cumQuantity;
        LeavesQuantity = leavesQuantity;
        OrderPrice = orderPrice;
        BuyerAccountId = buyerAccountId;
        SellerAccountId = sellerAccountId;
        Settlemented = false;
    }
}