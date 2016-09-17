using System;

namespace RedisMicroservices.Domain
{
   public interface IEntity
    {
        Guid Id { get; set; }
    }
}
