using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EduInsights.Server.Entities;

public class Subject
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("name")] public string Name { get; set; } = null!;

    [BsonElement("code")] public string Code { get; set; } = null!;

    [BsonElement("credit")] public string Credit { get; set; } = null!;

    [BsonElement("institute_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string InstituteId { get; set; } = null!;
}