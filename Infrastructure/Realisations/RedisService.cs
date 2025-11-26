using Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Realisations;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisService> _logger;

    public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<T> GetAsync<T>(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            var cachedData = await db.StringGetAsync(key);

            if(cachedData.HasValue)
            {
                return JsonSerializer.Deserialize<T>(cachedData);
            }

            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting data from Redis for key: {key}");

            return default(T);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, $"Error removing data from Redis for key: {key}");
        }
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan expiry)
    {
        try
        {
            var db = _redis.GetDatabase();
            var serialisedData = JsonSerializer.Serialize(data);
            await db.StringSetAsync(key, serialisedData, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting data to Redis for key: ${key}");
        }
    }
}
