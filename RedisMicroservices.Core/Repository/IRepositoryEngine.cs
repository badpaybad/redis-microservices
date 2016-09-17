using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RedisMicroservices.Core.Distributed;
using RedisMicroservices.Domain;

namespace RedisMicroservices.Core.Repository
{
  public  interface IRepositoryEngine<T> where T: IEntity
  {
      void RegisterDistributed(IDistributedServices distributedServices);

      void Insert(T entity);
      void Update(T entity);
      void Delete(T entity);
      void Delete(Guid id);
      void Delete(Expression<Func<T, bool>> whereExpression);
  }
}
