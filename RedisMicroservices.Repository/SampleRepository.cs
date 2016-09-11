using System.Collections.Generic;
using System.Security.Cryptography;
using RedisMicroservices.Core.Distributed;
using RedisMicroservices.Core.Domain;
using RedisMicroservices.Core.Domain.StorageMock;
using RedisMicroservices.Core.Repository;

namespace RedisMicroservices.Repository
{
    public class SampleRepository : IRepository<Sample>,ISampleRepository
    {
        
        void Insert(Sample entity)
        {
            //using eneity framework to insert into db
            SampleMockStorage.Add(entity);
        }

        public void RegisterDistributed(IDistributedServices distributedServices)
        {
            distributedServices.SubscribeEntity<Sample>((c, v) =>
            {
                switch (v.EntityAction)
                {
                        case EntityAction.Insert:
                            Insert(v.Data);
                        break;
                        case EntityAction.Update:
                        //
                        break;
                        case EntityAction.Delete:
                        //
                        break;
                }
            });
        }

        public List<Sample> SelectAll()
        {
            //   //using eneity framework to insert into db
            return SampleMockStorage.All();
        }
    }
}