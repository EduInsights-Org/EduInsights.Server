using EduInsights.Server.Contracts;
using EduInsights.Server.Interfaces;

namespace EduInsights.Server.EndPoints;

public static class EmailsEndpoints
{
    private const string StudentsEndpointsName = "/api/v1/emails";

    public static void MapEmailsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(StudentsEndpointsName).WithTags("EduInsights endpoints");

        group.MapPost("/send-verification-code",
            async (IEmailService emailService, string email) =>
            {
                var result = await emailService.SendVerificationCodeAsync(email);
                return Results.Json(result, statusCode: result.StatusCode);
            });

        group.MapPost("/verify-email",
            async (IEmailService emailService, VerifyEmailRequest request) =>
            {
                var result = await emailService.VerifyEmailAsync(request.Email, request.Code);
                return Results.Json(result, statusCode: result.StatusCode);
            });
    }
}