using System;
using System.Collections.Generic;
using System.Linq;
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
        private static Thread _findingResendCmd;
        private static Thread _doResendCmdError;
        const string juljulCommandLogPushed = "juljul_command_log_pushed";
        const string juljulCommandLogPendding = "juljul_command_log_pendding";
        const string juljulCommandLogError = "juljul_command_log_error";
        const string juljulCommandLogSucess = "juljul_command_log_sucess";
        const string juljulChannelByDataType = "juljul_channel_log_by_data_type";
        const string juljulLock = "juljul_lock";

        const string juljulCommandResend = "juljul_command_resend";

        static DistributedServices()
        {
            _findingResendCmd = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var lckok = RedisServices.RedisDatabase.StringGet(juljulLock);
                        if (lckok.HasValue && (bool)lckok )
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                        RedisServices.RedisDatabase.StringSet(juljulLock, true);

                        var allCmdPushed =
                            RedisServices.RedisDatabase.HashGetAll(juljulCommandLogPushed).ToList();

                        var resendItems = new List<HashEntry>();
                        foreach (var cmdP in allCmdPushed)
                        {
                            var pending = RedisServices.RedisDatabase.HashGet(juljulCommandLogPendding, cmdP.Name);
                            var success = RedisServices.RedisDatabase.HashGet(juljulCommandLogSucess, cmdP.Name);

                            if (pending.HasValue) continue;
                            if (success.HasValue) continue;
                            
                            resendItems.Add(cmdP);
                        }

                        foreach (var itm in resendItems)
                        {
                            var redisValue = itm.Value;
                            var cmd = JsonConvert.DeserializeObject<BaseDistributedCommand>(redisValue);

                            var pending = RedisServices.RedisDatabase.HashGet(juljulCommandLogPendding,
                                cmd.Id.ToString());

                            if (pending.HasValue) continue;

                            var success = RedisServices.RedisDatabase.HashGet(juljulCommandLogSucess, cmd.Id.ToString());

                            if (success.HasValue) continue;

                            SendAnCmd(cmd, redisValue);
                        }
                        allCmdPushed.Clear();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                        RedisServices.RedisDatabase.StringSet(juljulLock, false);
                    }
                }
            });

            _doResendCmdError = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var allCmdPushed =
                            RedisServices.RedisDatabase.HashGetAll(juljulCommandLogPushed).ToList();
                        foreach (var cmdP in allCmdPushed)
                        {
                            var error = RedisServices.RedisDatabase.ListRightPop(cmdP.Name.ToString());
                            if (!error.HasValue) continue;

                            var cmd = JsonConvert.DeserializeObject<BaseDistributedCommand>(error);

                            var success = RedisServices.RedisDatabase.HashGet(juljulCommandLogSucess, cmd.Id.ToString());

                            if (success.HasValue) continue;

                            SendAnCmd(cmd, error);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }
            });

            _findingResendCmd.Start();
            //_doResendCmdError.Start();
        }

        private static void SendAnCmd(BaseDistributedCommand cmd, RedisValue redisValue)
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
                    RedisServices.RedisDatabase.HashSet(cmd.DataBehavior + dataType, cmd.Id.ToString(),
                        redisValue);
                    break;
            }

            RedisServices.RedisDatabase.Publish(dataType, redisValue);
        }

        public void Publish<T>(DistributedCommand<T> cmd, Action<Exception> errorCallback = null) where T : class
        {
            try
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
                        RedisServices.RedisDatabase.HashSet(cmd.DataBehavior + redisChannel, cmd.Id.ToString(),
                            redisValue);
                        break;
                }
                SendAnCmd(cmd, redisValue);
                RedisServices.RedisSubscriber.Publish(redisChannel, redisValue);

                cmd.CommandStatus = CommandStatus.Pushed;
                LogPushed<T>(cmd.Id, cmd.ToJson());

                Console.WriteLine("Pushed:" + redisValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (errorCallback != null) errorCallback(ex);
            }
        }

        public void Subscribe<T>(Action<string, DistributedCommand<T>> callBack, Action<Exception> errorCallback = null)
            where T : class
        {
            try
            {
                var redisChannel = typeof (T).FullName;

                RedisServices.RedisSubscriber.Subscribe(redisChannel, (channel, value) =>
                {
                    var cmd = JsonConvert.DeserializeObject<DistributedCommand<T>>(value);
                    try
                    {
                        cmd.CommandStatus= CommandStatus.Pedding;
                        LogPendding<T>(cmd.Id, cmd.ToJson());
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
                                var da = RedisServices.RedisDatabase.HashGet(cmd.DataBehavior + redisChannel,
                                    cmd.Id.ToString());
                                if (da.HasValue)
                                {
                                    var val = JsonConvert.DeserializeObject<DistributedCommand<T>>(da);
                                    callBack(redisChannel, val);
                                    isDone = true;
                                }
                                break;
                        }
                        if (isDone)
                        {
                            cmd.CommandStatus = CommandStatus.Success;
                            LogSuccess<T>(cmd.Id, cmd.ToJson());
                            Console.WriteLine("Done:" + value);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError<T>(cmd.Id, cmd.ToJson());

                        Console.WriteLine(ex);
                        if (errorCallback != null) errorCallback(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (errorCallback != null) errorCallback(ex);
            }
        }


        public void PublishEntity<T>(DistributedCommandEntity<T> cmd, Action<Exception> errorCallback = null)
            where T : class, IEntity
        {
            try
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
                        RedisServices.RedisDatabase.HashSet(cmd.DataBehavior + redisChannel, cmd.Id.ToString(),
                            redisValue);

                        break;
                }
                RedisServices.RedisSubscriber.Publish(redisChannel, redisValue);

                cmd.CommandStatus = CommandStatus.Pushed;
                LogPushed<T>(cmd.Id, cmd.ToJson());

                Console.WriteLine("Pushed:" + redisValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (errorCallback != null) errorCallback(ex);
            }
        }

        public void SubscribeEntity<T>(Action<string, DistributedCommandEntity<T>> callBack,
            Action<Exception> errorCallback = null) where T : class, IEntity
        {
            try
            {
                var redisChannel = typeof (T).FullName;
                RedisServices.RedisSubscriber.Subscribe(redisChannel, (channel, value) =>
                {
                    var cmd = JsonConvert.DeserializeObject<DistributedCommandEntity<T>>(value);
                    try
                    {
                        cmd.CommandStatus = CommandStatus.Pedding;
                        LogPendding<T>(cmd.Id, cmd.ToJson());
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
                                var da = RedisServices.RedisDatabase.HashGet(cmd.DataBehavior + redisChannel,
                                    cmd.Id.ToString());
                                if (da.HasValue)
                                {
                                    var val = JsonConvert.DeserializeObject<DistributedCommandEntity<T>>(da);
                                    callBack(redisChannel, val);
                                    isDone = true;
                                }
                                break;
                        }
                        if (isDone)
                        {
                            cmd.CommandStatus = CommandStatus.Success;
                            LogSuccess<T>(cmd.Id, cmd.ToJson());
                            Console.WriteLine("Done:" + value);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError<T>(cmd.Id, cmd.ToJson());

                        Console.WriteLine(ex);
                        if (errorCallback != null) errorCallback(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (errorCallback != null) errorCallback(ex);
            }
        }

        public void PublishDataModel<T>(DistributedCommandDataModel<T> cmd, Action<Exception> errorCallback = null)
            where T : class, IDataModel
        {
            try
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
                        RedisServices.RedisDatabase.HashSet(cmd.DataBehavior + redisChannel, cmd.Id.ToString(),
                            redisValue);

                        break;
                }

                RedisServices.RedisSubscriber.Publish(redisChannel, redisValue);

                cmd.CommandStatus = CommandStatus.Pushed;
                LogPushed<T>(cmd.Id, cmd.ToJson());

                Console.WriteLine("Pushed:" + redisValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (errorCallback != null) errorCallback(ex);
            }
        }

        public void SubscribeDataModel<T>(Action<string, DistributedCommandDataModel<T>> callBack,
            Action<Exception> errorCallback = null)
            where T : class, IDataModel
        {
            try
            {
                var redisChannel = typeof (T).FullName;

                RedisServices.RedisSubscriber.Subscribe(redisChannel, (channel, value) =>
                {
                    var cmd = JsonConvert.DeserializeObject<DistributedCommandDataModel<T>>(value);
                    try
                    {
                        cmd.CommandStatus = CommandStatus.Pedding;
                        LogPendding<T>(cmd.Id, cmd.ToJson());

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
                                
                                var da = RedisServices.RedisDatabase.HashGet(cmd.DataBehavior + redisChannel,
                                    cmd.Id.ToString());
                                if (da.HasValue)
                                {
                                    var val = JsonConvert.DeserializeObject<DistributedCommandDataModel<T>>(da);
                                    callBack(redisChannel, val);
                                    isDone = true;
                                }
                                break;
                        }
                        if (isDone)
                        {
                            cmd.CommandStatus = CommandStatus.Success;
                            LogSuccess<T>(cmd.Id, cmd.ToJson());
                            Console.WriteLine("Done:" + value);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError<T>(cmd.Id, cmd.ToJson());
                        Console.WriteLine(ex);
                        if (errorCallback != null) errorCallback(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (errorCallback != null) errorCallback(ex);
            }
        }

        private void LogPendding<T>(Guid cmdId, RedisValue cmdJson)
        {
            RedisServices.RedisDatabase.HashSet(juljulCommandLogPendding, cmdId.ToString(), cmdJson);
        }

        void LogPushed<T>(Guid cmdId, RedisValue cmdJson) where T : class
        {
            var dataType = typeof (T).FullName;
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