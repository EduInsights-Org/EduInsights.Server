namespace EduInsights.Server.Interfaces;

public interface IRedisService
{
    Task<bool> SetAsync(string key, string value, TimeSpan expiry);
    Task<string?> GetAsync(string key);
    Task<bool> RemoveAsync(string key);
}