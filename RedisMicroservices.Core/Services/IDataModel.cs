using System;

namespace RedisMicroservices.Core
{
    public interface IDataModel
    {
        Guid Id { get; set; }
    }
}