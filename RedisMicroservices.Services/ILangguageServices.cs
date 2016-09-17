using RedisMicroservices.Core.Services;
using RedisMicroservices.Domain;
using RedisMicroservices.Domain.DataModel;

namespace RedisMicroservices.Services
{
    public interface ILangguageServices : IServicesEngine<LanguageData>
    {
        void DoSomething(LanguageData data);
    }
}