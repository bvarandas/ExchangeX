using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ProtoBuf;
using QuickFix;
using System.Text.Json.Serialization;

namespace SharedX.Core.Matching.DropCopy;
[ProtoContract]
[JsonDerivedType(typeof(DropCopyReport), typeDiscriminator: "DropCopyReport")]
[JsonDerivedType(typeof(TradeCaptureReport), typeDiscriminator: "TradeCaptureReport")]
[JsonDerivedType(typeof(ExecutionReport), typeDiscriminator: "ExecutionReport")]
public class DropCopyReport
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [ProtoMember(1)]
    public SessionID SessionID { get; set; } = null!;

    [ProtoMember(2)]
    public long TradeId { get; set; }
}
