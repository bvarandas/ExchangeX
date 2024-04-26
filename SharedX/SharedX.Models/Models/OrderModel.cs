using ProtoBuf;
using SharedX.Core.Enums;
using SharedX.Core.Account;
namespace SharedX.Core.Models;
[ProtoContract]
public class OrderModel 
{
    [ProtoMember(1)]
    public string Symbol { get; set; } = string.Empty;
    [ProtoMember(2)]
    public double Quantity { get; set; }
    [ProtoMember(3)]
    public double Price { get; set; }
    [ProtoMember(4)]
    public SideTrade Side { get; set; }
    [ProtoMember(5)]
    public OrderType OrderType { get; set; }
    [ProtoMember(6)]
    public virtual OrderStatus OrderStatus { get; set; }
    [ProtoMember(7)]
    public long MpOrderId { get; set; }
    [ProtoMember(8)]
    public long OrderID { get; set; }
    [ProtoMember(9)]
    public Account.Limit Account { get; set; } = null!;

    public OrderModel()
    {
        
    }
}