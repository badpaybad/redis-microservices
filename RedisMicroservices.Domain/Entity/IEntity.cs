using System;

namespace RedisMicroservices.Domain.Entity
{
   public interface IEntity
    {
        Guid Id { get; set; }
    }
}
