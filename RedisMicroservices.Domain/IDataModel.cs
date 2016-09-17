using System;

namespace RedisMicroservices.Domain
{
    public interface IDataModel
    {
        Guid Id { get; set; }
    }
}