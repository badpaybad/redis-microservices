using RedisMicroservices.Core.Repository;

namespace RedisMicroservices.Core.Distributed
{
    public class DistributedCommandDataModel<T>:DistributedCommand<T> where T : class, IDataModel
    {
        public EntityAction EntityAction { get; set; }
      
        public DistributedCommandDataModel(T entity, EntityAction entityAction, CommandBehavior commandBehavior = CommandBehavior.Queue) : base(entity,commandBehavior)
        {
            EntityAction = entityAction;
            Data = entity;
            CommandBehavior = commandBehavior;
        }

    }
}