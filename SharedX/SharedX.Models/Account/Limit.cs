using ProtoBuf;
namespace SharedX.Core.Account;
[ProtoContract]
[Serializable]
public class Limit
{
    [ProtoMember(1)]
    public int AccountId { get; set; }
    [ProtoMember(2)]
    public string AccountName { get; set; }=string.Empty;
    [ProtoMember(3)]
    public double TradedLimit {  get; set; }
    [ProtoMember(4)]
    public double ProvidedLimit { get; set; }
    [ProtoMember(5)]
    public ushort AccountType { get; set; }
}