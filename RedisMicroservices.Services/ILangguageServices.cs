using RedisMicroservices.Core.Services;
using RedisMicroservices.Domain;

namespace RedisMicroservices.Services
{
    public interface ILangguageServices : IServicesEngine<LanguageData>
    {
        void DoSomething(LanguageData data);
    }
}