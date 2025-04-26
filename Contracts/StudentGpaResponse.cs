namespace EduInsights.Server.Contracts;

public class StudentGpaResponse
{
    public string IndexNumber { get; set; } = null!;
    public string Batch { get; set; } = null!;
    public double Gpa { get; set; }
}