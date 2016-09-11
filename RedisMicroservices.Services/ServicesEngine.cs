using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedisMicroservices.Core.Distributed;

namespace RedisMicroservices.Services
{
    public static class ServicesEngine
    {
        public static IDistributedServices DistributedServices;
        public static SampleDataServices SampleDataServices;
        public static LangguageServices LangguageServices;

        static ServicesEngine()
        {
            DistributedServices = new DistributedServices();
            LangguageServices = new LangguageServices();
            SampleDataServices = new SampleDataServices(LangguageServices);
        }

        public static void Boot()
        {
            LangguageServices.ResiterDistributed(DistributedServices);
            SampleDataServices.ResiterDistributed(DistributedServices);
        }

    }
}