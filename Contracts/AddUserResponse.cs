using System;

namespace EduInsights.Server.Contracts;

public record class AddUserResponse(
    string Message,
    bool IsSuccess
);