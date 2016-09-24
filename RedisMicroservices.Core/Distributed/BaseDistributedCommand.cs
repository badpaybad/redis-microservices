using System;

namespace RedisMicroservices.Core.Distributed
{
    public  class BaseDistributedCommand
    {
        public virtual Guid Id { get; set; }
        public virtual DataBehavior DataBehavior { get; set; }
        public virtual string DataType { get; set; }

        //public virtual CommandStatus CommandStatus { get; set; }

        public BaseDistributedCommand()
        {
            
        }
    }
}