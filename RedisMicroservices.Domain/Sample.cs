using System;

namespace RedisMicroservices.Domain
{
    public class Sample : IEntity
    {
        public Guid Id { get; set; }

        public string Version { get; set; }
    }
}