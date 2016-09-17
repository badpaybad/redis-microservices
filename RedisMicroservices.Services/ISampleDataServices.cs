using RedisMicroservices.Core.Services;
using RedisMicroservices.Domain;

namespace RedisMicroservices.Services
{
    public interface ISampleDataServices : IServicesEngine<SampleData>
    {
        void DoSomething(SampleData data);
    }
}