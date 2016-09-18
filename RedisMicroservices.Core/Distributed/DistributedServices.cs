using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using RedisMicroservices.Core.Redis;
using RedisMicroservices.Core.Repository;
using RedisMicroservices.Domain;
using RedisMicroservices.Domain.DataModel;
using RedisMicroservices.Domain.Entity;
using StackExchange.Redis;

namespace RedisMicroservices.Core.Distributed
{
    public class DistributedServices : IDistributedServices
    {
        private static Thread _resendCmd;
        const string juljulCommandLogPushed = "juljul_command_log_pushed";
        const string juljulCommandLogPendding = "juljul_command_log_pendding";
        const string juljulCommandLogError = "juljul_command_log_error";
        const string juljulCommandLogSucess = "juljul_command_log_sucess";
        const string juljulChannelByDataType = "juljul_channel_log_by_data_type";

        static DistributedServices()
        {
            _resendCmd = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var allCmdPushed =
                            RedisServices.RedisDatabase.HashGetAll(juljulCommandLogPushed).ToDictionary();
                        var allCmdSuccess =
                            RedisServices.RedisDatabase.HashGetAll(juljulCommandLogSucess).ToDictionary();
                        var allCmdPending =
                            RedisServices.RedisDatabase.HashGetAll(juljulCommandLogPendding).ToDictionary();

                        var resendItems = new List<HashEntry>();
                        foreach (var cmdP in allCmdPushed)
                        {
                            var error = RedisServices.RedisDatabase.ListRightPop(cmdP.Key.ToString());
                            if (error.HasValue)
                            {
                                resendItems.Add(new HashEntry(cmdP.Key, error));
                            }

                            if (!allCmdPending.ContainsKey(cmdP.ToString()))
                            {
                                resendItems.Add(cmdP);
                            }
                        }

                        foreach (var itm in resendItems)
                        {
                            var redisValue = itm.Value;
                            var cmd = JsonConvert.DeserializeObject<BaseDistributedCommand>(redisValue);
                            if (!allCmdSuccess.ContainsKey(cmd.Id.ToString()))
                            {
                                var dataType = cmd.DataType;

                                switch (cmd.DataBehavior)
                                {
                                    case DataBehavior.Queue:
                                    case DataBehavior.Stack:
                                        RedisServices.RedisDatabase.ListRightPush(cmd.DataBehavior + dataType,
                                            redisValue);

                                        break;
                                    case DataBehavior.PubSub:
                                        break;
                                }

                                RedisServices.RedisDatabase.Publish(dataType, redisValue);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
            });
            _resendCmd.Start();
        }

        public void Publish<T>(DistributedCommand<T> cmd) where T : class
        {
            var redisChannel = typeof (T).FullName;
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

            LogPushed<T>(cmd.Id, redisValue);

            Console.WriteLine("Pushed:" + redisValue);
        }

        public void Subscribe<T>(Action<string, DistributedCommand<T>> callBack) where T : class
        {
            var redisChannel = typeof (T).FullName;

            RedisServices.RedisSubscriber.Subscribe(redisChannel, (channel, value) =>
            {
                var cmd = JsonConvert.DeserializeObject<DistributedCommand<T>>(value);
                try
                {
                    LogPendding<T>(cmd.Id, value);
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
                        LogSuccess<T>(cmd.Id, value);
                        Console.WriteLine("Done:" + value);
                    }
                }
                catch (Exception ex)
                {
                    LogError<T>(cmd.Id, value);
                    Console.WriteLine(ex);
                }
            });
        }


        public void PublishEntity<T>(DistributedCommandEntity<T> cmd) where T : class, IEntity
        {
            var redisChannel = typeof (T).FullName;
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
            LogPushed<T>(cmd.Id, redisValue);
            Console.WriteLine("Pushed:" + redisValue);
        }

        public void SubscribeEntity<T>(Action<string, DistributedCommandEntity<T>> callBack) where T : class, IEntity
        {
            var redisChannel = typeof (T).FullName;
            RedisServices.RedisSubscriber.Subscribe(redisChannel, (channel, value) =>
            {
                var cmd = JsonConvert.DeserializeObject<DistributedCommandEntity<T>>(value);
                try
                {
                    LogPendding<T>(cmd.Id, value);
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
                        LogSuccess<T>(cmd.Id, value);
                        Console.WriteLine("Done:" + value);
                    }
                }
                catch (Exception ex)
                {
                    LogError<T>(cmd.Id, value);
                    Console.WriteLine(ex);
                }
            });
        }

        public void PublishDataModel<T>(DistributedCommandDataModel<T> cmd) where T : class, IDataModel
        {
            var redisChannel = typeof (T).FullName;
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
            LogPushed<T>(cmd.Id, redisValue);
            Console.WriteLine("Pushed:" + redisValue);
        }

        public void SubscribeDataModel<T>(Action<string, DistributedCommandDataModel<T>> callBack)
            where T : class, IDataModel
        {
            var redisChannel = typeof (T).FullName;

            RedisServices.RedisSubscriber.Subscribe(redisChannel, (channel, value) =>
            {
                var cmd = JsonConvert.DeserializeObject<DistributedCommandDataModel<T>>(value);
                try
                {
                    LogPendding<T>(cmd.Id, value);

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
                        LogSuccess<T>(cmd.Id, value);
                        Console.WriteLine("Done:" + value);
                    }
                }
                catch (Exception ex)
                {
                    LogError<T>(cmd.Id, value);
                    Console.WriteLine(ex);
                }
            });
        }

        private void LogPendding<T>(Guid cmdId, RedisValue cmdJson)
        {
            RedisServices.RedisDatabase.HashSet(juljulCommandLogPendding, cmdId.ToString(), cmdJson);
        }

        void LogPushed<T>(Guid cmdId, RedisValue cmdJson) where T : class
        {
            var dataType = typeof (T).ToString();
            RedisServices.RedisDatabase.HashSet(juljulChannelByDataType, dataType, cmdJson);
            RedisServices.RedisDatabase.HashSet(juljulCommandLogPushed, cmdId.ToString(), cmdJson);
        }

        void LogError<T>(Guid cmdId, RedisValue cmdJson) where T : class
        {
            RedisServices.RedisDatabase.ListRightPush(cmdId.ToString(), cmdJson);
            RedisServices.RedisDatabase.HashSet(juljulCommandLogError, cmdId.ToString(), cmdJson);
        }

        void LogSuccess<T>(Guid cmdId, RedisValue cmdJson) where T : class
        {
            RedisServices.RedisDatabase.HashSet(juljulCommandLogSucess, cmdId.ToString(), cmdJson);
        }
    }
}