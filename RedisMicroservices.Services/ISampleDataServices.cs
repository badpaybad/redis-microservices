using RedisMicroservices.Core.Services;
using RedisMicroservices.Domain;
using RedisMicroservices.Domain.DataModel;

namespace RedisMicroservices.Services
{
    public interface ISampleDataServices : IServicesEngine<SampleData>
    {
        void DoSomething(SampleData data);
    }
}