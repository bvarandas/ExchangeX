using ProtoBuf;
using SharedX.Core.Entities;
namespace SharedX.Core.Proto;

[ProtoContract]
public class ExecutedTrade : BaseEntityFix
{
    [ProtoMember(1)]
    public long TradeId { get; set; }
    [ProtoMember(2)]
    public long BuyerOrderId { get; set; }
    [ProtoMember(3)]
    public long SellerOrderId { get; set; }
    [ProtoMember(4)]
    public string Symbol { get; set; } = string.Empty;
    [ProtoMember(5)]
    public decimal CumQuantity { get; set; }
    [ProtoMember(6)]
    public decimal LeavesQuantity { get; set; }
    [ProtoMember(7)]
    public decimal OrderPrice { get; set; }
    [ProtoMember(8)]
    public DateTime TradeDate { get; set; }
    [ProtoMember(9)]
    public long BuyerAccountId { get; set; }
    [ProtoMember(10)]
    public long SellerAccountId { get; set; }
    [ProtoMember(11)]
    public bool Settlemented { get; set; }
    [ProtoMember(12)]
    public DateTime? SettlementDate { get; set; }

    public ExecutedTrade(string symbol,
        long buyerOrderId,
        long sellerOrderId,
        decimal cumQuantity,
        decimal leavesQuantity,
        decimal orderPrice,
        long buyerAccountId,
        long sellerAccountId,
        DateTime tradeDate)
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

    public ExecutedTrade() { }
}