using ProtoBuf;

namespace SharedX.Core.Matching.MarketData;
[ProtoContract]
public class MarketData
{

    [ProtoMember(1)]
    public char UpdateAction { get; set; } //'0'-New, '1'-Change, '2'-Delete
    [ProtoMember(2)]
    public string Symbol { get; set; } = string.Empty;
    [ProtoMember(3)]
    public string SecurityID { get; set; } = string.Empty;
    [ProtoMember(4)]
    public char SecuritSourceId { get; set; }  //8 //https://www.onixs.biz/fix-dictionary/5.0/tagnum_22.html
    [ProtoMember(5)]
    public ushort EntryID { get; set; }  //0 (bid) or 1 (ask)
    [ProtoMember(6)]
    public ushort EntryType { get; set; } ////0 (bid) or 1 (ask) 2 trade 
    [ProtoMember(7)]
    public decimal EntryPx { get; set; } = decimal.Zero;  /// Preço
    [ProtoMember(8)]
    public decimal EntrySize { get; set; } = decimal.Zero; // quanity
    [ProtoMember(9)]
    public string EntryDate { get; set; } = string.Empty; //YYYYMMDD UTC
    [ProtoMember(10)]
    public string EntryTime { get; set; } = string.Empty;  //HH:MM:SS.ssssss
    [ProtoMember(11)]
    public string QuoteCondition { get; set; } = string.Empty; // Available values:  Z  = Order Imbalance
    [ProtoMember(12)]
    public string TradeCondition { get; set; } = string.Empty; //não vai ser utilizado
    [ProtoMember(13)]
    public string PriceLevel { get; set; } = string.Empty; //não vai ser utilizado
    [ProtoMember(14)]
    public string QuoteType { get; set; } = string.Empty;//não vai ser utilizado
    [ProtoMember(15)]
    public char AggressorSide { get; set; }  //1 Comprador 2 vendedor
    [ProtoMember(16)]
    public char MultiLegReportingType { get; set; } //Ver mais para frente
    [ProtoMember(17)]
    public long Id { get; set; }
}
