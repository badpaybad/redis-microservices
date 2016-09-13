using System;
using Newtonsoft.Json;
using RedisMicroservices.Core.Redis;
using RedisMicroservices.Core.Repository;
using StackExchange.Redis;

namespace RedisMicroservices.Core.Distributed
{
    public class DistributedServices : IDistributedServices
    {
        public void Publish<T>(DistributedCommand<T> cmd) where T : class
        {
            var redisChannel = typeof(T).FullName;
            var redisValue = cmd.ToJson();
            switch (cmd.CommandBehavior)
            {
                case CommandBehavior.Queue:
                case CommandBehavior.Stack:
                    RedisServices.RedisDatabase.ListRightPush(cmd.CommandBehavior + redisChannel, redisValue);

                    break;
                case CommandBehavior.PubSub:
                    break;
            }

            RedisServices.RedisSubscriber.Publish(redisChannel, redisValue);

            Console.WriteLine("Pushed:" + redisValue);
        }

        public void Subscribe<T>(Action<string, DistributedCommand<T>> callBack) where T : class
        {
            var redisChannel = typeof(T).FullName;

            RedisServices.RedisSubscriber.Subscribe(redisChannel, (channel, value) =>
            {
                try
                {
                    var cmd = JsonConvert.DeserializeObject<DistributedCommand<T>>(value);
                    bool isDone = false;
                    switch (cmd.CommandBehavior)
                    {
                        case CommandBehavior.Queue:
                            var qv = RedisServices.RedisDatabase.ListLeftPop(cmd.CommandBehavior + redisChannel);
                            if (qv.HasValue)
                            {
                                var val = JsonConvert.DeserializeObject<DistributedCommand<T>>(qv);
                                callBack(redisChannel, val);
                                isDone = true;
                            }
                            break;
                        case CommandBehavior.Stack:
                            var sv = RedisServices.RedisDatabase.ListRightPop(cmd.CommandBehavior + redisChannel);
                            if (sv.HasValue)
                            {
                                var val = JsonConvert.DeserializeObject<DistributedCommand<T>>(sv);
                                callBack(redisChannel, val);
                                isDone = true;
                            }
                            break;
                        case CommandBehavior.PubSub:
                            callBack(redisChannel, cmd);
                            isDone = true;
                            break;
                    }
                    if (isDone)
                    {
                        LogSuccess<T>(value);
                        Console.WriteLine("Done:" + value);
                    }
                }
                catch (Exception ex)
                {
                    LogError<T>(value);
                    Console.WriteLine(ex);
                }
            });
        }


        public void PublishEntity<T>(DistributedCommandEntity<T> cmd) where T : class, IEntity
        {
            var redisChannel = typeof(T).FullName;
            var redisValue = cmd.ToJson();
            switch (cmd.CommandBehavior)
            {
                case CommandBehavior.Queue:
                case CommandBehavior.Stack:
                    RedisServices.RedisDatabase.ListRightPush(cmd.CommandBehavior + redisChannel, redisValue);

                    break;
                case CommandBehavior.PubSub:
                    break;
            }

            RedisServices.RedisSubscriber.Publish(redisChannel, redisValue);

            Console.WriteLine("Pushed:" + redisValue);
        }

        public void SubscribeEntity<T>(Action<string, DistributedCommandEntity<T>> callBack) where T : class, IEntity
        {
            var redisChannel = typeof(T).FullName;

            RedisServices.RedisSubscriber.Subscribe(redisChannel, (channel, value) =>
            {
                try
                {
                    var cmd = JsonConvert.DeserializeObject<DistributedCommandEntity<T>>(value);
                    bool isDone = false;
                    switch (cmd.CommandBehavior)
                    {
                        case CommandBehavior.Queue:
                            var qv = RedisServices.RedisDatabase.ListLeftPop(cmd.CommandBehavior + redisChannel);
                            if (qv.HasValue)
                            {
                                var val = JsonConvert.DeserializeObject<DistributedCommandEntity<T>>(qv);
                                callBack(redisChannel, val);
                                isDone = true;
                            }
                            break;
                        case CommandBehavior.Stack:
                            var sv = RedisServices.RedisDatabase.ListRightPop(cmd.CommandBehavior + redisChannel);
                            if (sv.HasValue)
                            {
                                var val = JsonConvert.DeserializeObject<DistributedCommandEntity<T>>(sv);
                                callBack(redisChannel, val);
                                isDone = true;
                            }
                            break;
                        case CommandBehavior.PubSub:
                            callBack(redisChannel, cmd);
                            isDone = true;
                            break;
                    }
                    if (isDone)
                    {
                        LogSuccess<T>(value);
                        Console.WriteLine("Done:" + value);
                    }
                }
                catch (Exception ex)
                {
                    LogError<T>(value);
                    Console.WriteLine(ex);
                }
            });
        }


        public void PublishDataModel<T>(DistributedCommandDataModel<T> cmd) where T : class, IDataModel
        {
            var redisChannel = typeof(T).FullName;
            var redisValue = cmd.ToJson();
            switch (cmd.CommandBehavior)
            {
                case CommandBehavior.Queue:
                case CommandBehavior.Stack:
                    RedisServices.RedisDatabase.ListRightPush(cmd.CommandBehavior + redisChannel, redisValue);

                    break;
                case CommandBehavior.PubSub:
                    break;
            }

            RedisServices.RedisSubscriber.Publish(redisChannel, redisValue);

            Console.WriteLine("Pushed:" + redisValue);
        }

        public void SubscribeDataModel<T>(Action<string, DistributedCommandDataModel<T>> callBack)
            where T : class, IDataModel
        {
            var redisChannel = typeof(T).FullName;

            RedisServices.RedisSubscriber.Subscribe(redisChannel, (channel, value) =>
            {
                try
                {
                    var cmd = JsonConvert.DeserializeObject<DistributedCommandDataModel<T>>(value);
                    bool isDone = false;
                    switch (cmd.CommandBehavior)
                    {
                        case CommandBehavior.Queue:
                            var qv = RedisServices.RedisDatabase.ListLeftPop(cmd.CommandBehavior + redisChannel);
                            if (qv.HasValue)
                            {
                                var val = JsonConvert.DeserializeObject<DistributedCommandDataModel<T>>(qv);
                                callBack(redisChannel, val);
                                isDone = true;
                            }
                            break;
                        case CommandBehavior.Stack:
                            var sv = RedisServices.RedisDatabase.ListRightPop(cmd.CommandBehavior + redisChannel);
                            if (sv.HasValue)
                            {
                                var val = JsonConvert.DeserializeObject<DistributedCommandDataModel<T>>(sv);
                                callBack(redisChannel, val);
                                isDone = true;
                            }
                            break;
                        case CommandBehavior.PubSub:
                            //how to log success for all subcribers
                            callBack(redisChannel, cmd);
                            isDone = true;
                            break;
                    }
                    if (isDone)
                    {
                        LogSuccess<T>(value);
                        Console.WriteLine("Done:" + value);
                    }
                }
                catch (Exception ex)
                {
                    LogError<T>(value);
                    Console.WriteLine(ex);
                }
            });
        }


        void LogError<T>(RedisValue val)
        {
            RedisServices.RedisDatabase.HashSet("juljul_command_log_error", typeof(T).FullName, val);
        }

        void LogSuccess<T>(RedisValue val)
        {
            RedisServices.RedisDatabase.HashSet("juljul_command_log_sucess", typeof(T).FullName, val);
        }
    }
}