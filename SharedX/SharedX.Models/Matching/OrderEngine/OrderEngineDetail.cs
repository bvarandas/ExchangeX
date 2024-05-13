using ProtoBuf;

namespace SharedX.Core.Matching.OrderEngine;
[ProtoContract] 
public class OrderEngineDetail 
{
    [ProtoMember(1)]
    public long OrderID { get; set; }
    [ProtoMember(2)]
    public decimal LeavesQuantity { get; set; } //Remaining open quantity F OrdStatus(39) =    [4(Canceled) or C(Expired) or 8(Rejected)] Then LeavesQty(151) = 0 Else  LeavesQty(151) = OrderQty(38) - CumQty(14)
    [ProtoMember(3)]
    public decimal LastQuantity { get; set; } // Executed amount on current fill only. Quantity (e.g. shares) bought/sold on this (last) fill.
    [ProtoMember(4)]
    public decimal CumQty { get; set; } // Order total Filled Quantity (somatório do total executado)
    [ProtoMember(5)]
    public DateTime TransactTime { get; set; }
    [ProtoMember(6)]
    public string Symbol { get; set; } = string.Empty;
    [ProtoMember(7)]
    public decimal LastPrice { get; set; }
}
