using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EduInsights.Server.Entities;

public class Semester
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("year")]
    public string Year { get; set; } = null!;

    [BsonElement("semester")]
    public string Sem { get; set; } = null!;
    
    [BsonElement("institute_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string InstituteId { get; set; } = null!;
}