using StackExchange.Redis;

namespace Boilerplate.Infra.Data.Redis
{
    public interface IRedisConnection
    {
        IDatabase Database { get; }
    }
}
