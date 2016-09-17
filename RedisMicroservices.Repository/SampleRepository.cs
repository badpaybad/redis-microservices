using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;
using RedisMicroservices.Core.Distributed;
using RedisMicroservices.Core.Repository;
using RedisMicroservices.Domain;
using RedisMicroservices.Domain.StorageMock;

namespace RedisMicroservices.Repository
{
    public class SampleRepository : IRepositoryEngine<Sample>,ISampleRepository
    {
           
       public void Insert(Sample entity)
        {
            //using eneity framework to insert into db
            SampleMockStorage.Add(entity);
        }

        public void Update(Sample entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(Sample entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Delete(Expression<Func<Sample, bool>> whereExpression)
        {
            throw new NotImplementedException();
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

        
    }
}