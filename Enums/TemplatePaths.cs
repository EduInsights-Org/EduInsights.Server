namespace EduInsights.Server.Enums;

public class TemplatePaths
{
    private TemplatePaths(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static TemplatePaths VerificationCode => new("VerificationCode.html");
    public static TemplatePaths Welcome => new("Welcome.html");

    public override string ToString()
    {
        return Value;
    }
}