using ProtoBuf;
using SharedX.Core.Enums;
using System.Text.Json.Serialization;

namespace SharedX.Core.Entities;
[ProtoContract]
public class NewOrderSingleFix
{
    [ProtoMember(1)]
    public long ClOrdID { get; set; }
    [ProtoMember(2)]
    public string Symbol { get; set; }=string.Empty;
    [ProtoMember(3)]
    public SideTrade Side {  get; set; }
    [ProtoMember(4)]
    public decimal OrderQty { get; set; }
    [ProtoMember(5)]
    public OrderType OrderType { get; set; }
    [ProtoMember(6)]
    public decimal Price { get; set; }=decimal.Zero;
    [ProtoMember(7)]
    public decimal StopPx {  get; set; }=decimal.Zero;
    [ProtoMember(8)]
    public TimeInForce TimeInForce { get; set; }
    [ProtoMember(9)]
    public string ExpireDate { get; set; } = string.Empty;
    [ProtoMember(10)]
    public string ExpireTime { get; set; } = string.Empty;
    [ProtoMember(11)]
    public ushort AccountType { get; set; }
    [ProtoMember(12)]
    public decimal MinQty { get; set; } = decimal.Zero;
    [ProtoMember(13)]
    public IList<PartyIDs> PartyIDs { get; set; } = null!;

}
[ProtoContract]
public class PartyIDs
{
    [ProtoMember(1)]
    public string PartyID { get; set; } = string.Empty; //participanteId //AccountId
    [ProtoMember(2)]
    public ushort PartyRole { get; set; } //5-Investor Id para participante e 3-CLient ID para Account
    [ProtoMember(3)]
    public char PartyIDSource { get; set; } // C
}

[ProtoContract]
public class OrderCancelRequestFix 
{
    [ProtoMember(1)]
    public long OrderID { get; set; }
    [ProtoMember(2)]
    public long OrigClOrdID { get; set; }
    [ProtoMember(3)]
    public long ClOrdID {  get; set; }
    [ProtoMember(4)]
    public string Symbol { get; set; }=string.Empty;
}
[ProtoContract]
public class OrderCancelReplaceRequestFix 
{
    [ProtoMember(1)]
    public long OrderID { get; set; }
    [ProtoMember(2)]
    public long ClOrdID { get; set; }
    [ProtoMember(3)]
    public decimal MinQty { get; set; } = decimal.Zero;
    [ProtoMember(4)]
    public string Symbol { get; set; } = string.Empty;
    [ProtoMember(5)]
    public SideTrade Side { get; set; }
    [ProtoMember(6)]
    public decimal OrderQty { get; set; }
    [ProtoMember(7)]
    public OrderType OrderType { get; set; }
    [ProtoMember(8)]
    public decimal Price { get; set; } = decimal.Zero;
    [ProtoMember(9)]
    public decimal StopPx { get; set; } = decimal.Zero;
    [ProtoMember(10)]
    public TimeInForce TimeInForce { get; set; }
    [ProtoMember(11)]
    public string ExpireDate { get; set; } = string.Empty;
    [ProtoMember(12)]
    public string ExpireTime { get; set; } = string.Empty;
}
[ProtoContract]
public class OrderMassCancelRequestFix 
{
    [ProtoMember(1)]
    public long ClOrdID { get; set; }
    [ProtoMember(2)]
    public string Symbol { get; set; } = string.Empty;
    [ProtoMember(3)]
    public char MassCancelRequestType { get; set; } //https://www.onixs.biz/fix-dictionary/5.0/tagnum_530.html
    [ProtoMember(4)]
    public IList<TargetParty> TagetPartyIds { get; set; } = null!;

}
[ProtoContract]
public class BusinessMessageRejectFix : ReportFix
{
    [ProtoMember(1)]
    public int BusinessRejectReason { get; set; }
    [ProtoMember(2)]
    public string Text { get; set; }=string.Empty;
    [ProtoMember(3)]
    public long RefSeqNum { get; set; }
    [ProtoMember(4)]
    public string RefMsgType { get; set; } = string.Empty; //The MsgType <35> of the FIX message being referenced.
}

[ProtoContract]
public class OrderCancelRejectFix : ReportFix
{
    [ProtoMember(1)]
    public long OrderID { get; set; }
    [ProtoMember(2)]
    public long ClOrdID { get; set; }
    [ProtoMember(3)]
    public OrderStatus OrderStatus { get; set; }
    [ProtoMember(4)]
    public int CxlRejReason { get; set; } //https://www.onixs.biz/fix-dictionary/4.4/tagnum_102.html
    [ProtoMember(5)]
    public char CxlRejResponseTo { get; set; } //https://www.onixs.biz/fix-dictionary/4.4/tagnum_434.html
    [ProtoMember(6)]
    public string Text { get; set; }=string.Empty;
}

[ProtoContract]
public class OrderMassCancelReportFix: ReportFix
{
    [ProtoMember(1)]
    public long ClOrdID { get; set; }
    [ProtoMember(2)]
    public char MassCancelRequestType { get; set; } //https://www.onixs.biz/fix-dictionary/5.0/tagnum_530.html
    [ProtoMember(3)]
    public char MassCancelResponse { get; set; } //0=Cancel request Rejected, 1=Cancel request succeeded
    [ProtoMember(4)]
    public char MassCancelRejectReason { get; set; } //https://www.onixs.biz/fix-dictionary/4.3/tagnum_532.html
    [ProtoMember(5)]
    public int TotalAffectedOrders { get; set; }
    [ProtoMember(6)]
    public string MassActionReportID { get; set; } = string.Empty;
    [ProtoMember(7)]
    public IList<TargetParty> TagetPartyIds { get; set; } = null!;
    [ProtoMember(8)]
    public string Text { get; set; }
}

[ProtoContract]
public class TargetParty
{
    [ProtoMember(1)]
    public string TargetPartyID { get; set; } = string.Empty;
    [ProtoMember(2)]
    public char TargetPartyIDSource { get; set; } //https://www.onixs.biz/fix-dictionary/5.0.sp2/tagNum_447.html
    [ProtoMember(3)]
    public int TargetPartyRole { get; set; } //https://www.onixs.biz/fix-dictionary/5.0.sp2/tagNum_452.html
}

[ProtoContract]
[JsonDerivedType(typeof(ReportFix), typeDiscriminator: "ReportFix")]
[JsonDerivedType(typeof(OrderMassCancelReportFix), typeDiscriminator: "OrderMassCancelReportFix")]
[JsonDerivedType(typeof(OrderCancelRejectFix), typeDiscriminator: "OrderMassCancelReportFix")]
[JsonDerivedType(typeof(BusinessMessageRejectFix), typeDiscriminator: "OrderMassCancelReportFix")]
public class ReportFix
{
    [ProtoMember(1)]
    public long ExecId {  get; set; }

}