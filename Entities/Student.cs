using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EduInsights.Server.Entities;

public class Student
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    [BsonElement("batch_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string BatchId { get; set; } = null!;
    
    [BsonElement("institute_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string InstituteId { get; set; } = null!;

    [BsonElement("index_number")] public string IndexNumber { get; set; } = null!;
}