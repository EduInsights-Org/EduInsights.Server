using EduInsights.Server.Contracts;
using EduInsights.Server.Enums;
using EduInsights.Server.Interfaces;
using MimeKit;

namespace EduInsights.Server.Services;

public class EmailTemplateService(
    ILogger<IEmailTemplateService> logger
) : IEmailTemplateService
{
    public required string TemplatePath;
    private const string TemplatesDirectory = "EmailTemplates";

    public async Task<ApiResponse<BodyBuilder>> GetEmailTemplateAsync(TemplatePaths templateFileName,
        Dictionary<string, string> replacements)
    {
        var baseDirectory = AppContext.BaseDirectory;
        TemplatePath = Path.Combine(baseDirectory, TemplatesDirectory, templateFileName.Value);

        if (!File.Exists(TemplatePath))
        {
            logger.LogError("Template file not found at: {templatePath}", TemplatePath);
            return ApiResponse<BodyBuilder>.ErrorResult($"Template file not found", HttpStatusCode.NotFound);
        }

        var htmlBody = await File.ReadAllTextAsync(TemplatePath);

        foreach (var replacement in replacements)
        {
            htmlBody = htmlBody.Replace($"{{{{{replacement.Key}}}}}", replacement.Value);
        }

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        return ApiResponse<BodyBuilder>.SuccessResult(bodyBuilder);
    }
}