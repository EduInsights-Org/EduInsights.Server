using EduInsights.Server.Contracts;

namespace EduInsights.Server.Interfaces;

public interface IEmailService
{
    Task<ApiResponse<string>> SendVerificationCodeAsync(string toEmail);
    public string GenerateVerificationCode();
}