using ProtoBuf;
using SharedX.Core.Matching.OrderEngine;
namespace OrderEntryX.Core.Entities;
[ProtoContract]
public class OrderEntry : OrderEngine
{
    public string OrderFix {  get; set; }=string.Empty;
}
