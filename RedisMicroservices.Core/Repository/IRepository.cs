using System.Collections.Generic;
using RedisMicroservices.Core.Distributed;

namespace RedisMicroservices.Core.Repository
{
  public  interface IRepository<T> where T: IEntity
  {
      void RegisterDistributed(IDistributedServices distributedServices);

  }
}
