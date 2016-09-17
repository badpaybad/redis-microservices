using RedisMicroservices.Core.Repository;
using RedisMicroservices.Domain;
using RedisMicroservices.Domain.Entity;

namespace RedisMicroservices.Repository
{
    public interface ISampleRepository:IRepositoryEngine<Sample>
    {
        
    }
}