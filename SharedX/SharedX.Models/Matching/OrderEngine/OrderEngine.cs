using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ProtoBuf;
using SharedX.Core.Account;
using SharedX.Core.Entities;
using SharedX.Core.Enums;
using SharedX.Core.Matching.OrderEngine;

namespace SharedX.Core.Matching.OrderEngine;

[ProtoContract]
public class OrderEngine : BaseEntity
{
    [ProtoMember(1)]
    public string Symbol { get; set; } = string.Empty;
    [ProtoMember(2)]
    public decimal Quantity { get; set; } // Original Order Qty- Removed (modification) Qty
    [ProtoMember(3)]
    public decimal LeavesQuantity { get; set; } //Remaining open quantity F OrdStatus(39) =    [4(Canceled) or C(Expired) or 8(Rejected)] Then LeavesQty(151) = 0 Else  LeavesQty(151) = OrderQty(38) - CumQty(14)
    [ProtoMember(4)]
    public SideTrade Side { get; set; }
    [ProtoMember(5)]
    public OrderType OrderType { get; set; }
    [ProtoMember(6)]
    public TimeInForce TimeInForce { get; set; }
    [ProtoMember(7)]
    public virtual OrderStatus OrderStatus { get; set; }
    [ProtoMember(8)]
    public long ParticipatorId { get; set; }
    [ProtoMember(10)]
    public long OrderID { get; set; }
    [ProtoMember(11)]
    public long ClOrdID { get; set; }
    [ProtoMember(12)]
    public long AccountId { get; set; }
    [ProtoMember(13)]
    public Limit Account { get; set; } = null!;
    [ProtoMember(14)]
    public string ExpireDate { get; set; } = string.Empty;
    [ProtoMember(15)]
    public string ExpireTime { get; set; } = string.Empty;
    [ProtoMember(16)]
    public decimal Price { get; set; }
    [ProtoMember(18)]
    public decimal StopPrice { get; set; }
    [ProtoMember(19)]
    public decimal AveragePrice { get; set; }
    [ProtoMember(20)]
    public DateTime TransactTime { get; set; }
    [ProtoMember(21)]
    public long OrigClOrdID { get; set; }
    [ProtoMember(22)]
    public Execution Execution { get; set; }
    [ProtoMember(23)]
    public decimal MinQty { get; set; }
    [ProtoMember(24)]
    public decimal LastPrice { get; set; }
    [ProtoMember(25)]
    public decimal LastQuantity { get; set; }
    [ProtoMember(26)]
    public IList<OrderEngineDetail> OrderDetails { get; set; } = null!;
}
