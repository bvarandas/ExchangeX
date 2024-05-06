using ProtoBuf;
using SharedX.Core.Enums;
namespace SharedX.Core.Models;
[ProtoContract]
public class OrderModel 
{
    [ProtoMember(1)]
    public string Symbol { get; set; } = string.Empty;
    [ProtoMember(2)]
    public decimal Quantity { get; set; }
    [ProtoMember(3)]
    public decimal Price { get; set; }
    [ProtoMember(4)]
    public SideTrade Side { get; set; }
    [ProtoMember(5)]
    public OrderType OrderType { get; set; }
    [ProtoMember(6)]
    public virtual OrderStatus OrderStatus { get; set; }
    [ProtoMember(7)]
    public long MpId { get; set; }
    [ProtoMember(8)]
    public long OrderID { get; set; }
    [ProtoMember(9)]
    public int Account { get; set; }
    [ProtoMember(10)]
    public long ClOrdId { get; set; }
    [ProtoMember(11)]
    public decimal StopPrice { get; set; }
    [ProtoMember(12)]
    public TimeInForce TimeInForce { get; set; }
    [ProtoMember(13)]
    public DateTime TransactTime { get; set; }
    [ProtoMember(14)]
    public string ExpireDate { get; set; }
    [ProtoMember(15)]
    public string ExpireTime { get; set; }
    [ProtoMember(17)]
    public long ClOrdID { get; set; }
    [ProtoMember(18)]
    public long AccountId { get; set; }
    [ProtoMember(19)]
    public long ParticipatorId { get; set; }
    
    public OrderModel()
    {
        
    }
}