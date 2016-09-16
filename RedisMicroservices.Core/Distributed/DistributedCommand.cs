﻿using System;
using Newtonsoft.Json;

namespace RedisMicroservices.Core.Distributed
{
    public class DistributedCommand<T> where T : class 
    {
        public DataBehavior DataBehavior { get; set; }

        public T Data { get; set; }

        public Type DataType { get { return Data.GetType(); } }

        public DistributedCommand(T data,  DataBehavior dataBehavior = DataBehavior.Queue)
        {
            Data = data;
            DataBehavior = dataBehavior;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public DistributedCommand<T> FromJson(string json)
        {
            return JsonConvert.DeserializeObject<DistributedCommand<T>>(json);
        }
    }
}