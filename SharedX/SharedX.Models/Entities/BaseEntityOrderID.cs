using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SharedX.Core.Entities;
public class BaseEntityOrderID
{
    [BsonId]
    public string Id { get; set; } = string.Empty;
}
