using System;
using Newtonsoft.Json;
using RedisMicroservices.Core.Redis;
using RedisMicroservices.Core.Repository;
using RedisMicroservices.Domain;
using StackExchange.Redis;

namespace RedisMicroservices.Core.Distributed
{
    public class DistributedServices : IDistributedServices
    {
        public void Publish<T>(DistributedCommand<T> cmd) where T : class
        {
            var redisChannel = typeof(T).FullName;
            var redisValue = cmd.ToJson();
            switch (cmd.DataBehavior)
            {
                case DataBehavior.Queue:
                case DataBehavior.Stack:
                    RedisServices.RedisDatabase.ListRightPush(cmd.DataBehavior + redisChannel, redisValue);

                    break;
                case DataBehavior.PubSub:
                    break;
            }

            RedisServices.RedisSubscriber.Publish(redisChannel, redisValue);

            LogCommand<T>(cmd.Id, redisValue);

            Console.WriteLine("Pushed:" + redisValue);
        }

        public void Subscribe<T>(Action<string, DistributedCommand<T>> callBack) where T : class
        {
            var redisChannel = typeof(T).FullName;

            RedisServices.RedisSubscriber.Subscribe(redisChannel, (channel, value) =>
            {
                var cmd = JsonConvert.DeserializeObject<DistributedCommand<T>>(value);
                try
                {
                     bool isDone = false;
                    switch (cmd.DataBehavior)
                    {
                        case DataBehavior.Queue:
                            var qv = RedisServices.RedisDatabase.ListLeftPop(cmd.DataBehavior + redisChannel);
                            if (qv.HasValue)
                            {
                                var val = JsonConvert.DeserializeObject<DistributedCommand<T>>(qv);
                                callBack(redisChannel, val);
                                isDone = true;
                            }
                            break;
                        case DataBehavior.Stack:
                            var sv = RedisServices.RedisDatabase.ListRightPop(cmd.DataBehavior + redisChannel);
                            if (sv.HasValue)
                            {
                                var val = JsonConvert.DeserializeObject<DistributedCommand<T>>(sv);
                                callBack(redisChannel, val);
                                isDone = true;
                            }
                            break;
                        case DataBehavior.PubSub:
                            callBack(redisChannel, cmd);
                            isDone = true;
                            break;
                    }
                    if (isDone)
                    {
                        LogSuccess<T>(cmd.Id,value);
                        Console.WriteLine("Done:" + value);
                    }
                }
                catch (Exception ex)
                {
                    LogError<T>(cmd.Id,value);
                    Console.WriteLine(ex);
                }
            });
        }


        public void PublishEntity<T>(DistributedCommandEntity<T> cmd) where T : class, IEntity
        {
            var redisChannel = typeof(T).FullName;
            var redisValue = cmd.ToJson();
            switch (cmd.DataBehavior)
            {
                case DataBehavior.Queue:
                case DataBehavior.Stack:
                    RedisServices.RedisDatabase.ListRightPush(cmd.DataBehavior + redisChannel, redisValue);

                    break;
                case DataBehavior.PubSub:
                    break;
            }

            RedisServices.RedisSubscriber.Publish(redisChannel, redisValue);
            LogCommand<T>(cmd.Id,redisValue);
            Console.WriteLine("Pushed:" + redisValue);
        }

        public void SubscribeEntity<T>(Action<string, DistributedCommandEntity<T>> callBack) where T : class, IEntity
        {
            var redisChannel = typeof(T).FullName;

            RedisServices.RedisSubscriber.Subscribe(redisChannel, (channel, value) =>
            {
                var cmd = JsonConvert.DeserializeObject<DistributedCommandEntity<T>>(value);
                try
                {
                    bool isDone = false;
                    switch (cmd.DataBehavior)
                    {
                        case DataBehavior.Queue:
                            var qv = RedisServices.RedisDatabase.ListLeftPop(cmd.DataBehavior + redisChannel);
                            if (qv.HasValue)
                            {
                                var val = JsonConvert.DeserializeObject<DistributedCommandEntity<T>>(qv);
                                callBack(redisChannel, val);
                                isDone = true;
                            }
                            break;
                        case DataBehavior.Stack:
                            var sv = RedisServices.RedisDatabase.ListRightPop(cmd.DataBehavior + redisChannel);
                            if (sv.HasValue)
                            {
                                var val = JsonConvert.DeserializeObject<DistributedCommandEntity<T>>(sv);
                                callBack(redisChannel, val);
                                isDone = true;
                            }
                            break;
                        case DataBehavior.PubSub:
                            callBack(redisChannel, cmd);
                            isDone = true;
                            break;
                    }
                    if (isDone)
                    {
                        LogSuccess<T>(cmd.Id,value);
                        Console.WriteLine("Done:" + value);
                    }
                }
                catch (Exception ex)
                {
                    LogError<T>(cmd.Id,value);
                    Console.WriteLine(ex);
                }
            });
        }


        public void PublishDataModel<T>(DistributedCommandDataModel<T> cmd) where T : class, IDataModel
        {
            var redisChannel = typeof(T).FullName;
            var redisValue = cmd.ToJson();
            switch (cmd.DataBehavior)
            {
                case DataBehavior.Queue:
                case DataBehavior.Stack:
                    RedisServices.RedisDatabase.ListRightPush(cmd.DataBehavior + redisChannel, redisValue);

                    break;
                case DataBehavior.PubSub:
                    break;
            }

            RedisServices.RedisSubscriber.Publish(redisChannel, redisValue);
            LogCommand<T>(cmd.Id,redisValue);
            Console.WriteLine("Pushed:" + redisValue);
        }

        public void SubscribeDataModel<T>(Action<string, DistributedCommandDataModel<T>> callBack)
            where T : class, IDataModel
        {
            var redisChannel = typeof(T).FullName;

            RedisServices.RedisSubscriber.Subscribe(redisChannel, (channel, value) =>
            {
                var cmd = JsonConvert.DeserializeObject<DistributedCommandDataModel<T>>(value);
                try
                {
                    bool isDone = false;
                    switch (cmd.DataBehavior)
                    {
                        case DataBehavior.Queue:
                            var qv = RedisServices.RedisDatabase.ListLeftPop(cmd.DataBehavior + redisChannel);
                            if (qv.HasValue)
                            {
                                var val = JsonConvert.DeserializeObject<DistributedCommandDataModel<T>>(qv);
                                callBack(redisChannel, val);
                                isDone = true;
                            }
                            break;
                        case DataBehavior.Stack:
                            var sv = RedisServices.RedisDatabase.ListRightPop(cmd.DataBehavior + redisChannel);
                            if (sv.HasValue)
                            {
                                var val = JsonConvert.DeserializeObject<DistributedCommandDataModel<T>>(sv);
                                callBack(redisChannel, val);
                                isDone = true;
                            }
                            break;
                        case DataBehavior.PubSub:
                            //how to log success for all subcribers
                            callBack(redisChannel, cmd);
                            isDone = true;
                            break;
                    }
                    if (isDone)
                    {
                        LogSuccess<T>(cmd.Id,value);
                        Console.WriteLine("Done:" + value);
                    }
                }
                catch (Exception ex)
                {
                    LogError<T>(cmd.Id,value);
                    Console.WriteLine(ex);
                }
            });
        }

        void LogCommand<T>(Guid cmdId, RedisValue cmdJson) where T : class
        {
            RedisServices.RedisDatabase.HashSet("juljul_command_log_pushed",cmdId.ToString(),  cmdJson,When.Always,CommandFlags.FireAndForget);
        }

        void LogError<T>(Guid cmdId,RedisValue cmdJson) where T : class
        {
            RedisServices.RedisDatabase.HashSet("juljul_command_log_error",cmdId.ToString(),  cmdJson, When.Always, CommandFlags.FireAndForget);
        }

        void LogSuccess<T>(Guid cmdId, RedisValue cmdJson) where T : class
        {
            RedisServices.RedisDatabase.HashSet("juljul_command_log_sucess",cmdId.ToString(), cmdJson, When.Always, CommandFlags.FireAndForget);
        }
    }
}