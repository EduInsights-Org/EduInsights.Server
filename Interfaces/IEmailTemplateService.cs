using EduInsights.Server.Contracts;
using EduInsights.Server.Enums;
using MimeKit;

namespace EduInsights.Server.Interfaces;

public interface IEmailTemplateService
{
    Task<ApiResponse<BodyBuilder>>
        GetEmailTemplateAsync(TemplatePaths templateName, Dictionary<string, string> replacements);
}