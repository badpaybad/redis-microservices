using System.Collections.Generic;
using RedisMicroservices.Core.Distributed;
using RedisMicroservices.Domain;

namespace RedisMicroservices.Core.Services
{
    public interface IServicesEngine<T> where T : IDataModel
    {
        void ResiterDistributed(IDistributedServices distributedServices);
     
    }
}