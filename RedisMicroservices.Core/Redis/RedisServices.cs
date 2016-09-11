using StackExchange.Redis;

namespace RedisMicroservices.Core.Redis
{
    public static class RedisServices
    {
        static IServer _server;
        static SocketManager _socketManager;
        static IConnectionMultiplexer _connectionMultiplexer;

        public static IConnectionMultiplexer RedisConnectionMultiplexer
        {
            get
            {
                if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
                    return _connectionMultiplexer;

                if (_connectionMultiplexer != null && !_connectionMultiplexer.IsConnected)
                {
                    _connectionMultiplexer.Dispose();
                }

                _connectionMultiplexer = GetConnection();

                return _connectionMultiplexer;
            }
        }

        public static IDatabase RedisDatabase
        {
            get { return RedisConnectionMultiplexer.GetDatabase(); }
        }

        public static ISubscriber RedisSubscriber
        {
            get { return RedisConnectionMultiplexer.GetSubscriber(); }
        }

        static RedisServices()
        {
            _socketManager = new SocketManager("JulJulCore", true);
        }

        public static ConnectionMultiplexer GetConnection()
        {
           
            var options = new ConfigurationOptions
            {
                EndPoints =
                {
                    {"badpaybad.info", 6379}
                },
                Password = "badpaybad.info",
                AllowAdmin = false,
                SyncTimeout = 5*1000,
                SocketManager = _socketManager,
                AbortOnConnectFail = false,
                ConnectTimeout = 5*1000,
            };

            return ConnectionMultiplexer.Connect(options);
        }
    }
}