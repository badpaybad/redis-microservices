using System.Collections.Generic;
using RedisMicroservices.Core.Distributed;

namespace RedisMicroservices.Core.Services
{
    public interface IDataModelServices<T> where T : IDataModel
    {
        void ResiterDistributed(IDistributedServices distributedServices);
        void Create(T data);
        void DoSomething(T data);
    }
}