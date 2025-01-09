using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EduInsights.Server.Entities;

public class Institute
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("name")]
    public string Name { get; set; } = null!;
}