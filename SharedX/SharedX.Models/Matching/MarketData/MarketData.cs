using ProtoBuf;

namespace SharedX.Core.Matching.MarketData;
[ProtoContract]
public class MarketData
{
    [ProtoMember(1)]
    public string Symbol { get; set; } = string.Empty;
    [ProtoMember(2)]
    public string SecurityID { get; set; } = string.Empty;
    [ProtoMember(3)]
    public string SecuritSourceId { get; set; } = string.Empty; //8 //https://www.onixs.biz/fix-dictionary/5.0/tagnum_22.html
    [ProtoMember(4)]
    public ushort EntryID { get; set; }  //0 (bid) or 1 (ask)
    [ProtoMember(5)]
    public ushort EntryType { get; set; } ////0 (bid) or 1 (ask) 2 trade 
    [ProtoMember(6)]
    public decimal EntryPx { get; set; } = decimal.Zero;  /// Preço
    [ProtoMember(7)]
    public decimal EntrySize { get; set; } = decimal.Zero; // quanity
    [ProtoMember(8)]
    public string EntryDate { get; set; } = string.Empty; //YYYYMMDD UTC
    [ProtoMember(9)]
    public string EntryTime { get; set; } = string.Empty;  //HH:MM:SS.ssssss
    [ProtoMember(10)]
    public string QuoteCondition { get; set; } = string.Empty; // Available values:  Z  = Order Imbalance
    [ProtoMember(11)]
    public string TradeCondition { get; set; } = string.Empty; //não vai ser utilizado
    [ProtoMember(12)]
    public string PriceLevel { get; set; } = string.Empty; //não vai ser utilizado
    [ProtoMember(13)]
    public string QuoteType { get; set; } = string.Empty;//não vai ser utilizado
    [ProtoMember(14)]
    public char AggressorSide { get; set; }  //1 Comprador 2 vendedor
    [ProtoMember(15)]
    public char MultiLegReportingType { get; set; } //Ver mais para frente
    [ProtoMember(16)]
    public long Id { get; set; }
}
