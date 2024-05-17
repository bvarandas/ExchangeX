using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ProtoBuf;
using QuickFix;
using SharedX.Core.Entities;
using System.Text.Json.Serialization;

namespace SharedX.Core.Matching.DropCopy;
[ProtoContract]
[JsonDerivedType(typeof(ReportFix), typeDiscriminator: "DropCopyReport")]
[JsonDerivedType(typeof(OrderMassCancelReportFix), typeDiscriminator: "TradeCaptureReport")]
[JsonDerivedType(typeof(OrderCancelRejectFix), typeDiscriminator: "ExecutionReport")]
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
