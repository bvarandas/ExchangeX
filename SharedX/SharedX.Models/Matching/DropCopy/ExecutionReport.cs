using ProtoBuf;
using SharedX.Core.Account;
using SharedX.Core.Enums;

namespace SharedX.Core.Matching.DropCopy;
[ProtoContract]
public class ExecutionReport
{
    [ProtoMember(1)]
    public string Symbol { get; set; } = string.Empty;
    [ProtoMember(2)]
    public decimal Quantity { get; set; }
    [ProtoMember(3)]
    public decimal LeavesQuantity { get; set; }
    [ProtoMember(4)]
    public decimal LastQuantity { get; set; }
    [ProtoMember(5)]
    public SideTrade Side { get; set; }
    [ProtoMember(6)]
    public OrderType OrderType { get; set; }
    [ProtoMember(7)]
    public virtual OrderStatus OrderStatus { get; set; }
    [ProtoMember(8)]
    public long ParticipatorId { get; set; }
    [ProtoMember(9)]
    public long OrderID { get; set; }
    [ProtoMember(10)]
    public long OrigCLOrdID { get; set; } //AccountId
    [ProtoMember(11)]
    public Limit Account { get; set; } = null!;
    [ProtoMember(12)]
    public decimal Price { get; set; }
    [ProtoMember(13)]
    public decimal LastPrice { get; set; }
    [ProtoMember(14)]
    public decimal StopPrice { get; set; }
    [ProtoMember(15)]
    public decimal AveragePrice { get; set; }
    [ProtoMember(16)]
    public DateTime TransactTime { get; set; }
    [ProtoMember(17)]
    public TimeInForce TimeInForce { get; set; }
    [ProtoMember(18)]
    public long ExecID { get; set; }// somente no caso de execução
    [ProtoMember(19)]
    public long ClOrdID { get; set; }
    [ProtoMember(20)]
    public long TrdMatchID { get; set; }
    [ProtoMember(21)]
    public char ExecType { get; set; }
    [ProtoMember(22)]
    public string OrdRejReason { get; set; }
    [ProtoMember(23)]
    public decimal CumQty { get; set; }
    [ProtoMember(24)]
    public string ExpireDate { get; set; }
    [ProtoMember(25)]
    public string ExpireTime { get; set; }
    [ProtoMember(26)]
    public char AccoutType {  get; set; }
    

    public ExecutionReport() { }
}