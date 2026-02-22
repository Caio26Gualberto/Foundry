namespace Boilerplate.Infra.Data.Services.Caching
{
    public interface ICacheService
    {
        Task<T?> GetTAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        Task RemoveAsync(string key);
    }
}
