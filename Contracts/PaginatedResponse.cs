namespace EduInsights.Server.Contracts;

public record PaginatedResponse<T>
{
    public required T Data { get; set; }
    public required long TotalRecords { get; set; }
    public required int CurrentPage { get; set; }
    public required int PageSize { get; set; }
};