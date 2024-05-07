using ProtoBuf;
using SharedX.Core.Matching;
namespace OrderEntryX.Core.Entities;
[ProtoContract]
public class OrderEntry : OrderEng
{
    public string OrderFix {  get; set; }=string.Empty;
}
