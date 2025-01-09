using EduInsights.Server.Entities;

namespace EduInsights.Server.Contracts;

public record GetUserResponse(User? User, bool IsSuccess, string Message);