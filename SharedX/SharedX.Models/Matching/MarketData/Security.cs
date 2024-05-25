using ProtoBuf;
namespace SharedX.Core.Matching.MarketData;
[ProtoContract]
public class Security
{
    [ProtoMember(1)]
    public string Symbol { get; set; } = string.Empty;
    [ProtoMember(2)]
    public string SecurityID { get; set; } = string.Empty;
    [ProtoMember(3)]
    public string SecuritSourceId { get; set; } = string.Empty; //8 //https://www.onixs.biz/fix-dictionary/5.0/tagnum_22.html
    [ProtoMember(4)]
    public string CFICode { get; set; } = string.Empty; //ISO 10962 standard: Classification of Financial Instruments (CFI code). 
    [ProtoMember(5)]
    public string SecurityType { get; set; } = string.Empty; //FUT (Future)    OPT(Option) SPOT MLEG(Multileg Instrument) CS(Common Stock)
    [ProtoMember(6)]
    public char SecuritySubType { get; set; } //Foreign exchange (F) Commodities(T) Financial futures(F) Commodities futures(C) Call options(C) Put options(P) Other(miscellaneous) (M)
    [ProtoMember(7)]
    public string MaturityDate { get; set; } = string.Empty; // YYYYMMDD
    [ProtoMember(8)]
    public string MaturityTime { get; set; } = string.Empty; // HH:MM:SSZ 
    [ProtoMember(9)]
    public char SecurityStatus { get; set; } // 1 -Active 2-Inactive
    [ProtoMember(10)]
    public string IssueDate { get; set; } = string.Empty; // rading is allowed from this time    format: YYYYMMDD
    [ProtoMember(11)]
    public decimal StrikePrice { get; set; } = decimal.Zero; // Strike price.
    [ProtoMember(12)]
    public decimal ContractMultiplier { get; set; } = decimal.Zero;// Contract size
    [ProtoMember(13)]
    public char SettlMethod { get; set; } // C = Cash settlement required    P = Physical settlement required    E=Election at exercise
    [ProtoMember(14)]
    public char ExerciseStyle { get; set; }// 0 = European 1 = American
    [ProtoMember(15)]
    public char PutOrCall { get; set; } // 1=Call 2=Put
    [ProtoMember(16)]
    public string SecurityExchange { get; set; } = string.Empty; // Market identifier 
    [ProtoMember(17)]
    public string SecurityDesc { get; set; } = string.Empty;
    [ProtoMember(18)]
    public int InstrumentPricePrecision { get; set; }  // 8 Specifies the number of decimal places for instrument prices.
    [ProtoMember(19)]
    public ushort TradeVolType { get; set; } //0 - Number of Units, 1 = NUmber of round Lots
    [ProtoMember(20)]
    public decimal MinTradeVol { get; set; } = decimal.Zero; // The minimun order quantity
    [ProtoMember(21)]
    public decimal MaxTradeVol { get; set; } = decimal.Zero; // The Maximun order quantity
    [ProtoMember(22)]
    public string Currency { get; set; } = string.Empty;//BRL https://btobits.com/fixopaedia/fixdic44/index.html?tag_15_Currency.html

}