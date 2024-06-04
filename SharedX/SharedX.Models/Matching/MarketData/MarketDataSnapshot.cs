using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;
namespace SharedX.Core.Matching.MarketData;

[ProtoContract]
public class MarketDataSnapshot
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [ProtoMember(1)]
    public string Id { get; set; }=string.Empty;
    [ProtoMember(2)]
    public string Symbol { get; set; } = string.Empty;
    [ProtoMember(2)]
    public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    [ProtoMember(4)]
    public IList<MarketDataSnapshotDetail> Details { get; set; } = null!;
}

[ProtoContract]
public class MarketDataSnapshotDetail
{
    [ProtoMember(1)]
    public char EntryType { get; set; }
    [ProtoMember(2)]
    public decimal EntryPx { get; set; }
    [ProtoMember(3)]
    public decimal EntrySize { get; set; }
    [ProtoMember(4)]
    public int PriceLevel { get; set; }
    [ProtoMember(5)]
    public string SecurityID { get; set; } = string.Empty;
    [ProtoMember(6)]
    public char SecurityIDSource { get; set; }
    [ProtoMember(7)]
    public char EntryId { get; set; }
    [ProtoMember(8)]
    public int NumberOfOrders { get; set; }
}