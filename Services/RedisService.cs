using StackExchange.Redis;
using EduInsights.Server.Interfaces;

namespace EduInsights.Server.Services;

public class RedisService(IConnectionMultiplexer redis) : IRedisService
{
    private readonly IDatabase _redisDb = redis.GetDatabase();

    public async Task<bool> SetAsync(string key, string value, TimeSpan expiry)
    {
        return await _redisDb.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> GetAsync(string key)
    {
        return await _redisDb.StringGetAsync(key);
    }

    public async Task<bool> RemoveAsync(string key)
    {
        return await _redisDb.KeyDeleteAsync(key);
    }
}