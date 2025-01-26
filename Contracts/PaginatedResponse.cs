namespace EduInsights.Server.Contracts;

public record PaginatedResponse<T>(T Data, long TotalRecords, int CurrentPage, int PageSize);