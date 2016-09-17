using System;

namespace RedisMicroservices.Domain.DataModel
{
    public interface IDataModel
    {
        Guid Id { get; set; }
    }
}