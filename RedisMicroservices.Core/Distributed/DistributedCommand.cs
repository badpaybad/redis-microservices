using System;
using Newtonsoft.Json;

namespace RedisMicroservices.Core.Distributed
{
    public class DistributedCommand<T>: BaseDistributedCommand where T : class
    {
        public  override Guid Id { get;  set; }
        public  override DataBehavior DataBehavior { get; set; }

        public T Data { get; set; }

        public  override string DataType { get; set; }

        public DistributedCommand() { } 

        public DistributedCommand(T data, DataBehavior dataBehavior = DataBehavior.Queue)
        {
            Id = Guid.NewGuid();
            Data = data;
            DataType = data.GetType().FullName;
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