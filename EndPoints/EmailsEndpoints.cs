using EduInsights.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduInsights.Server.EndPoints;

public static class EmailsEndpoints
{
    private const string StudentsEndpointsName = "/api/v1/emails";

    public static void MapEmailsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(StudentsEndpointsName).WithTags("EduInsights endpoints");

        group.MapPost("/send-verification-code",
            async ([FromServices] IEmailService emailService, [FromQuery] string email) =>
            {
                var result = await emailService.SendVerificationCodeAsync(email);
                return Results.Json(result, statusCode: result.StatusCode);
            });
    }
}