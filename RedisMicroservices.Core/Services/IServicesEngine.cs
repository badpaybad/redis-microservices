using System.Collections.Generic;
using RedisMicroservices.Core.Distributed;
using RedisMicroservices.Domain;
using RedisMicroservices.Domain.DataModel;

namespace RedisMicroservices.Core.Services
{
    public interface IServicesEngine<T> where T : IDataModel
    {
        void ResiterDistributed(IDistributedServices distributedServices);
     
    }
}