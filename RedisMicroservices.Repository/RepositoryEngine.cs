using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedisMicroservices.Core.Distributed;

namespace RedisMicroservices.Repository
{
    public static class RepositoryEngine
    {
        public static IDistributedServices DistributedServices;
        public static ISampleRepository SampleRepository;
        static RepositoryEngine()
        {
            DistributedServices=new DistributedServices();
            SampleRepository= new SampleRepository();
        }

        public static void Boot()
        {
            SampleRepository.RegisterDistributed(DistributedServices);
        }
    }
}