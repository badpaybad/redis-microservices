using RedisMicroservices.Core.Repository;

namespace RedisMicroservices.Core.Distributed
{
    public class DistributedCommandEntity<T> : DistributedCommand<T> where T : class, IEntity
    {
        public EntityAction EntityAction { get; set; }

        public DistributedCommandEntity(T entity, EntityAction entityAction, CommandBehavior commandBehavior = CommandBehavior.Queue) : base(entity,commandBehavior)
        {
            EntityAction = entityAction;
            Data = entity;
            CommandBehavior = commandBehavior;
        }

    }
}