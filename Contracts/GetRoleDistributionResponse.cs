namespace EduInsights.Server.Contracts;

public record GetRoleDistributionResponse
{
    public int SuperAdmin { get; set; } = 0;
    public int Admin { get; set; } = 0;
    public int DataEntry { get; set; } = 0;
    public int Student { get; set; } = 0;
}