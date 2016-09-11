using System;
using RedisMicroservices.Core.Repository;

namespace RedisMicroservices.Core.Domain
{
    public class Sample : IEntity
    {
        public Guid Id { get; set; }

        public string Version { get; set; }
    }
}