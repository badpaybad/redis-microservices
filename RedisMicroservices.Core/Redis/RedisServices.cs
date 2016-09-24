using System;
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
                if (!_connectionMultiplexer.IsConnected)
                {
                    var exception = new Exception("Can not connect to redis");
                    Console.WriteLine(exception);
                    throw exception;
                }
                return _connectionMultiplexer;
            }
        }

        public static IDatabase RedisDatabase
        {
            get
            {
                var redisDatabase = RedisConnectionMultiplexer.GetDatabase();
               
                return redisDatabase;
            }
        }

        public static ISubscriber RedisSubscriber
        {
            get
            {
                var redisSubscriber = RedisConnectionMultiplexer.GetSubscriber();
               
                return redisSubscriber;
            }
        }

        static RedisServices()
        {
            _socketManager = new SocketManager("JulJulCore");
        }

        public static ConnectionMultiplexer GetConnection()
        {
           
            var options = new ConfigurationOptions
            {
                EndPoints =
                {
                    {"127.0.0.1", 6379}
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