using ProtoBuf;
namespace SharedX.Core.Entities;
[ProtoContract]
public class SecurityEngine : BaseEntity
{
    [ProtoMember(1)]
    public string Symbol { get; set; } = string.Empty;
    [ProtoMember(2)]
    public string SecurityID { get; set; }=string.Empty;
    [ProtoMember(3)]
    public string SecurityIDSource { get; set; }= string.Empty;
    [ProtoMember(4)]
    public string CFICode {  get; set; }=string.Empty;
    [ProtoMember(5)]
    public string SecurityType {  get; set; }=string.Empty; // FUT(future), OPT(option), SPOT, MLEG(Multileg instrument), CS(Common Stock)
    [ProtoMember(6)]
    public string SecuritySubType { get; set; } = string.Empty; // Foreign Exchange(F), Commodities(T), Fiancial futures, Commodities Futures(C), Call options (C), Put Options(P), Other (Miscellaneous M) 
    [ProtoMember(7)]
    public string MaturityDate { get; set; } = string.Empty; // yyyyMMdd
    [ProtoMember(8)]
    public string MaturityTime { get; set; } = string.Empty; // HH:mm:ssz
    [ProtoMember(9)]
    public string SecurityStatus { get; set; } = string.Empty; // 1=Active, 2 Inactive 
    [ProtoMember(10)]
    public string IssueDate { get; set; } = string.Empty; // yyyyMMdd
    [ProtoMember(11)]
    public decimal StrikePrice { get; set; } = decimal.Zero; // Strike
    [ProtoMember(12)]
    public decimal ContractMultiplier { get; set; } = decimal.Zero; // Fator multiplicador
    [ProtoMember(13)]
    public char SettlementMethod { get; set; }  // C=Cash Settlement required, PPhysical settlement required
    [ProtoMember(14)]
    public ushort ExerciseStyle { get; set; } // 0=European,1=American, 2=Bermuda
    [ProtoMember(15)]
    public ushort PutOrCall { get; set; } // SecurityType(167)=OPT, Call Options (C)=1, Put options(P)=0
    [ProtoMember(16)]
    public string SecurityExchange {  get; set; } = string.Empty; //Market Identifier Code (MIC) XBSP
    [ProtoMember(17)]
    public string SecurityDesc { get; set; } = string.Empty;
    [ProtoMember(18)]
    public decimal LowLimitPrice { get; set; } = decimal.Zero;
    [ProtoMember(19)]
    public decimal HighLimitPrice { get; set; }= decimal.Zero;
    [ProtoMember(20)]
    public ushort TradeVolType { get; set; }// 0=Number of units, 1=number of round lots
    [ProtoMember(21)]
    public decimal MinTradeVol { get; set; }// The minimum trading volume for a security
    [ProtoMember(22)]
    public decimal MaxTradeVol { get; set; }// The maximum trading volume for a security
    [ProtoMember(23)]
    public string Currency { get; set; } = string.Empty;  // Quote currency
}
