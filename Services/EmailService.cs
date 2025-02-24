using EduInsights.Server.Contracts;
using EduInsights.Server.Enums;
using EduInsights.Server.Interfaces;

namespace EduInsights.Server.Services;

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

public class EmailService(
    IConfiguration configuration,
    IRedisService redisService,
    IUserService userService,
    ILogger<IEmailService> logger) : IEmailService
{
    public string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    public async Task<ApiResponse<string>> VerifyEmailAsync(string email, string verificationCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return ApiResponse<string>.ErrorResult("Email code does not exist.", 404);

            if (string.IsNullOrWhiteSpace(verificationCode))
                return ApiResponse<string>.ErrorResult("Verification code does not exist.", 404);

            var storedHashedCode = await redisService.GetAsync(email);
            if (storedHashedCode == null)
                return ApiResponse<string>.ErrorResult("Stored Verification code does not exist.", 404);

            if (!BCrypt.Net.BCrypt.Verify(verificationCode, storedHashedCode))
                return ApiResponse<string>.ErrorResult("Invalid verification code.", 400, ErrorCode.InvalidCredentials);

            var userResult = await userService.FindUserByEmail(email);
            if (!userResult.Success) return ApiResponse<string>.ErrorResult(userResult.Message, userResult.StatusCode);

            var updatedUser = new UpdateUserRequest
            {
                IsEMailVerified = true
            };

            var updatedUserResult = await userService.UpdateUserAsync(userResult.Data!.Id, updatedUser);
            if (!updatedUserResult.Success)
                return ApiResponse<string>.ErrorResult(updatedUserResult.Message, updatedUserResult.StatusCode);

            var removeRedisResult = await redisService.RemoveAsync(email);
            if (!removeRedisResult)
            {
                logger.LogError("Failed to remove email from cache.");
                throw new Exception();
            }

            return ApiResponse<string>.SuccessResult(null!, 200, "Email verified successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError("Error when verifying Email: {ex.ex}", ex.Message);
            return ApiResponse<string>.ErrorResult("Error when verifying Email", 500);
        }
    }

    public async Task<ApiResponse<string>> SendVerificationCodeAsync(string toEmail)
    {
        var emailSettings = configuration.GetSection("EmailSettings");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("EduInsights", emailSettings["FromEmail"]));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "Your Verification Code";
        var expiry = TimeSpan.FromMinutes(10);
        var verificationCode = GenerateVerificationCode();

        var baseDirectory = AppContext.BaseDirectory;

        var templatePath = Path.Combine(baseDirectory, "Templates", "EmailTemplate.html");

        if (!File.Exists(templatePath))
        {
            logger.LogError("Template file not found at: {templatePath}", templatePath);
            return ApiResponse<string>.ErrorResult($"Template file not found", 404);
        }

        var htmlBody = await File.ReadAllTextAsync(templatePath);

        htmlBody = htmlBody
            .Replace("{{VerificationCode}}", verificationCode)
            .Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        message.Body = bodyBuilder.ToMessageBody();
        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(emailSettings["SmtpServer"],
                int.Parse(emailSettings["SmtpPort"]!),
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(emailSettings["SmtpUsername"], emailSettings["SmtpPassword"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            // store the code as hashed
            var hashedCode = BCrypt.Net.BCrypt.HashPassword(verificationCode);
            var redisSetResult = await redisService.SetAsync(toEmail, hashedCode, expiry);
            return !redisSetResult
                ? ApiResponse<string>.ErrorResult("Error when caching the verification code", 500)
                : ApiResponse<string>.SuccessResult(null!, 200, "Verification code sent successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when sending Verification code email: {ex.Message}", ex.Message);
            return ApiResponse<string>.ErrorResult("Error when sending Verification code email", 500);
        }
    }
}