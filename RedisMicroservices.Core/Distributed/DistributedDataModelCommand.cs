using RedisMicroservices.Core.Repository;
using RedisMicroservices.Domain;

namespace RedisMicroservices.Core.Distributed
{
    public class DistributedCommandDataModel<T>:DistributedCommand<T> where T : class, IDataModel
    {
        public EntityAction EntityAction { get; set; }
      
        public DistributedCommandDataModel(T entity, EntityAction entityAction, DataBehavior dataBehavior = DataBehavior.Queue) : base(entity,dataBehavior)
        {
            EntityAction = entityAction;
            Data = entity;
            DataBehavior = dataBehavior;
        }

    }
}