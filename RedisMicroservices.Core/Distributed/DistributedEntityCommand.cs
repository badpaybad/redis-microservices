using RedisMicroservices.Core.Repository;

namespace RedisMicroservices.Core.Distributed
{
    public class DistributedCommandEntity<T> : DistributedCommand<T> where T : class, IEntity
    {
        public EntityAction EntityAction { get; set; }

        public DistributedCommandEntity(T entity, EntityAction entityAction, DataBehavior dataBehavior = DataBehavior.Queue) : base(entity,dataBehavior)
        {
            EntityAction = entityAction;
            Data = entity;
            DataBehavior = dataBehavior;
        }

    }
}