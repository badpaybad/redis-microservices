using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RedisMicroservices.Core.Redis;
using RedisMicroservices.Core.Repository;
using RedisMicroservices.Core.Utils;
using RedisMicroservices.Domain;
using RedisMicroservices.Domain.DataModel;
using RedisMicroservices.Domain.Entity;
using StackExchange.Redis;

namespace RedisMicroservices.Core.Distributed
{
    public class DistributedServices : IDistributedServices
    {
        private static Thread _commandQueue;
        private static Thread _findingResendCmd;
        private static Thread _doResendCmdError;
        const string juljulCommandLogPushed = "juljul_command_log_pushed";
        const string juljulCommandLogPendding = "juljul_command_log_pendding";
        const string juljulCommandLogError = "juljul_command_log_error";
        const string juljulCommandLogSucess = "juljul_command_log_sucess";
        const string juljulChannelByDataType = "juljul_channel_log_by_data_type";

        const string juljulCommandQueueWaitToSend = "juljul_command_queue_wait_to_send";

        const string juljulLock = "juljul_lock";

        const string juljulCommandResend = "juljul_command_resend";

        static DistributedServices()
        {
            _commandQueue = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var redisValue = RedisServices.RedisDatabase.ListLeftPop(juljulCommandQueueWaitToSend);
                        if (redisValue.HasValue)
                        {
                            var cmd = JsonConvert.DeserializeObject<BaseDistributedCommand>(redisValue);
                            PublishAnCmd(cmd, redisValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        Thread.Sleep(100);
                    }
                }
            });

            _findingResendCmd = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var allCmdPushed =
                            RedisServices.RedisDatabase.HashGetAll(juljulCommandLogPushed).ToList();

                        foreach (var cmdP in allCmdPushed)
                        {
                            var pending = RedisServices.RedisDatabase.HashGet(juljulCommandLogPendding, cmdP.Name);
                            var success = RedisServices.RedisDatabase.HashGet(juljulCommandLogSucess, cmdP.Name);

                            if (pending.HasValue) continue;
                            if (success.HasValue) continue;

                            var redisValue = cmdP.Value;
                            var cmd = JsonConvert.DeserializeObject<BaseDistributedCommand>(redisValue);

                            RedisServices.RedisDatabase.ListRightPush(juljulCommandQueueWaitToSend, redisValue);

                            Console.WriteLine("--resend cmd: " + cmd.Id);
                            Console.WriteLine("--------data: " + redisValue);
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
                            var error = RedisServices.RedisDatabase.HashGet(juljulCommandLogError, cmdP.Name);
                            if (!error.HasValue) continue;

                            var success = RedisServices.RedisDatabase.HashGet(juljulCommandLogSucess, cmdP.Name);

                            if (success.HasValue) continue;

                            RedisServices.RedisDatabase.ListRightPush(juljulCommandQueueWaitToSend, error);
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
            _commandQueue.Start();
        }

        private static void PublishAnCmd(BaseDistributedCommand cmd, RedisValue cmdInJson)
        {
            var dataType = cmd.DataType;

            switch (cmd.DataBehavior)
            {
                case DataBehavior.Queue:
                case DataBehavior.Stack:
                    RedisServices.RedisDatabase.ListRightPush(cmd.DataBehavior + dataType,
                        cmdInJson);
                    break;
                case DataBehavior.PubSub:
                    RedisServices.RedisDatabase.HashSet(cmd.DataBehavior + dataType, cmd.Id.ToString(),
                        cmdInJson);
                    break;
            }

            RedisServices.RedisDatabase.Publish(dataType, cmdInJson);

            cmd.CommandStatus = CommandStatus.Pushed;
            LogPushed(dataType, cmd.Id, cmdInJson);

            Console.WriteLine("Pushed:" + cmdInJson);
        }

        public void Publish<T>(DistributedCommand<T> cmd, Action<Exception> errorCallback = null) where T : class
        {
            try
            {
                var redisValue = cmd.ToJson();

                RedisServices.RedisDatabase.ListRightPush(juljulCommandQueueWaitToSend, redisValue);
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
                    TryCatchLog(() =>
                    {
                        var cmd = JsonConvert.DeserializeObject<DistributedCommand<T>>(value);
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
                            cmd.CommandStatus = CommandStatus.Error;
                            var redisValue = cmd.ToJson();
                            LogError<T>(cmd.Id, redisValue);
                            Console.WriteLine("----cmdid: " + cmd.Id);
                            Console.WriteLine("---data  : " + redisValue);
                            Console.WriteLine(ex);
                            if (errorCallback != null) errorCallback(ex);
                        }
                    });
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
                var redisValue = cmd.ToJson();

                RedisServices.RedisDatabase.ListRightPush(juljulCommandQueueWaitToSend, redisValue);
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
                    TryCatchLog(() =>
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
                            cmd.CommandStatus = CommandStatus.Error;
                            var redisValue = cmd.ToJson();
                            LogError<T>(cmd.Id, redisValue);
                            Console.WriteLine("----cmdid: " + cmd.Id);
                            Console.WriteLine("---data  : " + redisValue);
                            Console.WriteLine(ex);
                            if (errorCallback != null) errorCallback(ex);
                        }
                    });
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
                var redisValue = cmd.ToJson();
                RedisServices.RedisDatabase.ListRightPush(juljulCommandQueueWaitToSend, redisValue);
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
                    TryCatchLog(() =>
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
                            cmd.CommandStatus = CommandStatus.Error;
                            var redisValue = cmd.ToJson();
                            LogError<T>(cmd.Id, redisValue);
                            Console.WriteLine("----cmdid: " + cmd.Id);
                            Console.WriteLine("---data  : " + redisValue);
                            Console.WriteLine(ex);
                            if (errorCallback != null) errorCallback(ex);
                        }
                    });
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

        static void LogPushed(string dataType, Guid cmdId, RedisValue cmdJson)
        {
            RedisServices.RedisDatabase.HashSet(juljulChannelByDataType, dataType, cmdJson);
            RedisServices.RedisDatabase.HashSet(juljulCommandLogPushed, cmdId.ToString(), cmdJson);
        }

        void LogError<T>(Guid cmdId, RedisValue cmdJson) where T : class
        {
            RedisServices.RedisDatabase.HashSet(juljulCommandLogError, cmdId.ToString(), cmdJson);
        }

        void LogSuccess<T>(Guid cmdId, RedisValue cmdJson) where T : class
        {
            RedisServices.RedisDatabase.HashSet(juljulCommandLogSucess, cmdId.ToString(), cmdJson);
        }

        void TryCatchLog(Action a)
        {
            try
            {
                a();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}