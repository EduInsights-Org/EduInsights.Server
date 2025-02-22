using EduInsights.Server.Contracts;
using EduInsights.Server.Interfaces;

namespace EduInsights.Server.Services;

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

public class EmailService(IConfiguration configuration, ILogger<IEmailService> logger) : IEmailService
{
    public string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    public async Task<ApiResponse<string>> SendVerificationCodeAsync(string toEmail)
    {
        var emailSettings = configuration.GetSection("EmailSettings");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("EduInsights", emailSettings["FromEmail"]));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "Your Verification Code";
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

            return ApiResponse<string>.SuccessResult(null!, 200, "Verification code sent successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when sending Verification code email: {ex.Message}", ex.Message);
            return ApiResponse<string>.ErrorResult("Error when sending Verification code email", 500);
        }
    }
}