using RedisMicroservices.Core.Repository;
using RedisMicroservices.Domain;
using RedisMicroservices.Domain.DataModel;
using RedisMicroservices.Domain.Entity;

namespace RedisMicroservices.Core.Distributed
{
    public class DistributedCommandDataModel<T>:DistributedCommand<T> where T : class, IDataModel
    {
        public EntityAction EntityAction { get; set; }
      
        public DistributedCommandDataModel() { } 

        public DistributedCommandDataModel(T entity, EntityAction entityAction, DataBehavior dataBehavior = DataBehavior.Queue) : base(entity,dataBehavior)
        {
            EntityAction = entityAction;
            Data = entity;
            DataBehavior = dataBehavior;
        }

    }
}