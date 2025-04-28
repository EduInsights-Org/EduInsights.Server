namespace EduInsights.Server.Contracts;

public class BatchGpaResponse
{
    public string BatchId { get; set; } = null!;
    public string BatchName { get; set; } = null!;
    public double AverageGpa { get; set; }
    public int StudentCount { get; set; }
    public int TotalStudentsInBatch { get; set; }
}