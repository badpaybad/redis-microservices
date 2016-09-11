using System;

namespace RedisMicroservices.Core.Repository
{
   public interface IEntity
    {
        Guid Id { get; set; }
    }
}
