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
    IEmailTemplateService emailTemplateService,
    ILogger<IEmailService> logger) : IEmailService
{
    private readonly IConfiguration _emailSettings = configuration.GetSection("EmailSettings");
    private readonly MimeMessage _message = new();
    private readonly SmtpClient _client = new();


    public string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private void ConfigureEmail(string toEmail, string subject)
    {
        _message.From.Add(new MailboxAddress("EduInsights", _emailSettings["FromEmail"]));
        _message.To.Add(new MailboxAddress("", toEmail));
        _message.Subject = subject;
    }

    private async Task ConfigureClientAndSend()
    {
        await _client.ConnectAsync(_emailSettings["SmtpServer"],
            int.Parse(_emailSettings["SmtpPort"]!),
            SecureSocketOptions.StartTls);
        await _client.AuthenticateAsync(_emailSettings["SmtpUsername"], _emailSettings["SmtpPassword"]);
        await _client.SendAsync(_message);
        await _client.DisconnectAsync(true);
    }

    public async Task<ApiResponse<string>> VerifyEmailAsync(string email, string verificationCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return ApiResponse<string>.ErrorResult("Email code does not exist.", HttpStatusCode.NotFound);

            if (string.IsNullOrWhiteSpace(verificationCode))
                return ApiResponse<string>.ErrorResult("Verification code does not exist.", HttpStatusCode.NotFound);

            var storedHashedCode = await redisService.GetAsync(email);
            if (storedHashedCode == null)
                return ApiResponse<string>.ErrorResult("Stored Verification code does not exist.",
                    HttpStatusCode.NotFound);

            if (!BCrypt.Net.BCrypt.Verify(verificationCode, storedHashedCode))
                return ApiResponse<string>.ErrorResult("Invalid verification code.", HttpStatusCode.BadRequest,
                    ErrorCode.InvalidCredentials);

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

            ConfigureEmail(userResult.Data!.Email, "Welcome to EduInsights! Get Started Today");

            var replacements = new Dictionary<string, string>
            {
                { "UserName", userResult.Data!.FirstName },
                { "CurrentYear", DateTime.Now.Year.ToString() }
            };
            var getHtmlBodyResponse =
                await emailTemplateService.GetEmailTemplateAsync(TemplatePaths.Welcome, replacements);
            if (!getHtmlBodyResponse.Success)
                return ApiResponse<string>.ErrorResult(getHtmlBodyResponse.Message, getHtmlBodyResponse.StatusCode);

            var bodyBuilder = getHtmlBodyResponse.Data!;

            _message.Body = bodyBuilder.ToMessageBody();

            await ConfigureClientAndSend();

            return ApiResponse<string>.SuccessResult(null!, HttpStatusCode.Ok, "Email verified successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError("Error when verifying Email: {ex.ex}", ex.Message);
            return ApiResponse<string>.ErrorResult("Error when verifying Email", HttpStatusCode.InternalServerError);
        }
    }


    public async Task<ApiResponse<string>> SendVerificationCodeAsync(string toEmail)
    {
        ConfigureEmail(toEmail, "Your Verification Code");

        var expiry = TimeSpan.FromMinutes(10);
        var verificationCode = GenerateVerificationCode();

        var replacements = new Dictionary<string, string>
        {
            { "VerificationCode", verificationCode },
            { "CurrentYear", DateTime.Now.Year.ToString() }
        };

        var getHtmlBodyResponse =
            await emailTemplateService.GetEmailTemplateAsync(TemplatePaths.VerificationCode, replacements);
        if (!getHtmlBodyResponse.Success)
            return ApiResponse<string>.ErrorResult(getHtmlBodyResponse.Message, getHtmlBodyResponse.StatusCode);

        var bodyBuilder = getHtmlBodyResponse.Data!;

        _message.Body = bodyBuilder.ToMessageBody();

        try
        {
            await ConfigureClientAndSend();

            // store the code as hashed
            var hashedCode = BCrypt.Net.BCrypt.HashPassword(verificationCode);
            var redisSetResult = await redisService.SetAsync(toEmail, hashedCode, expiry);
            return !redisSetResult
                ? ApiResponse<string>.ErrorResult("Error when caching the verification code",
                    HttpStatusCode.InternalServerError)
                : ApiResponse<string>.SuccessResult(null!, HttpStatusCode.Ok, "Verification code sent successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when sending Verification code email: {ex.Message}", ex.Message);
            return ApiResponse<string>.ErrorResult("Error when sending Verification code email",
                HttpStatusCode.InternalServerError);
        }
    }
}