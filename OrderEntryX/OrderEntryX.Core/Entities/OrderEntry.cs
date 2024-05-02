using ProtoBuf;
using SharedX.Core.Matching;
namespace OrderEntryX.Core.Entities;
[ProtoContract]
public class OrderEntry : Order
{
    public string OrderFix {  get; set; }=string.Empty;
}
