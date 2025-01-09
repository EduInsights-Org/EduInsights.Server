using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EduInsights.Server.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("institute_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string InstituteId { get; set; } = null!;

    [BsonElement("first_name")] public string FirstName { get; set; } = null!;

    [BsonElement("last_name")] public string LastName { get; set; } = null!;

    [BsonElement("user_name")] public string UserName { get; set; } = null!;
    [BsonElement("email")] public string Email { get; set; } = null!;

    [BsonElement("password_hash")] public string PasswordHash { get; set; } = null!;

    [BsonElement("role")] public string Role { get; set; } = "user";

    [BsonElement("created_at")] public DateTime CreatedAt { get; set; }
}