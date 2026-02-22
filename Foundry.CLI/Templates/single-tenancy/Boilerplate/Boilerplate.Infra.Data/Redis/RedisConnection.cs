using StackExchange.Redis;

namespace Boilerplate.Infra.Data.Redis
{
    public sealed class RedisConnection : IRedisConnection
    {
        private readonly IConnectionMultiplexer _multiplexer;
        public RedisConnection(IConnectionMultiplexer connectionMultiplexer)
        {
            _multiplexer = connectionMultiplexer;
        }
        public IDatabase Database => _multiplexer.GetDatabase();
    }
}
