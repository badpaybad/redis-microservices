using RedisMicroservices.Core.Repository;
using RedisMicroservices.Domain;
using RedisMicroservices.Domain.Entity;

namespace RedisMicroservices.Core.Distributed
{
    public class DistributedCommandEntity<T> : DistributedCommand<T> where T : class, IEntity
    {
        public EntityAction EntityAction { get; set; }

        public DistributedCommandEntity() { } 

        public DistributedCommandEntity(T entity, EntityAction entityAction, DataBehavior dataBehavior = DataBehavior.Queue) : base(entity,dataBehavior)
        {
            EntityAction = entityAction;
            Data = entity;
            DataBehavior = dataBehavior;
        }

    }
}