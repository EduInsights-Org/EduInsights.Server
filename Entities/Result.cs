using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EduInsights.Server.Entities;

public class Result
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("student_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string StudentId { get; set; } = null!;
    
    [BsonElement("subject_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SubjectId { get; set; } = null!;
    
    [BsonElement("semester_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SemesterId { get; set; } = null!;

    [BsonElement("grade")] public string Grade { get; set; } = null!;
}