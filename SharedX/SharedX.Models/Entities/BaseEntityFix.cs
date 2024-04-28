using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using QuickFix;

namespace SharedX.Core.Entities;

public class BaseEntityFix
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public SessionID SessionID { get; set; } = null!;
}
