using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
namespace SharedX.Core.Entities;
public class BaseEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
}
