using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedisMicroservices.Core.Distributed;
using RedisMicroservices.Domain.DataModel;
using RedisMicroservices.Domain.Entity;
using RedisMicroservices.Repository;
using RedisMicroservices.Services;

namespace RedisMicroservices.ConsolePushTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //RepositoryEngine.Boot();
            //ServicesEngine.Boot();

            var ds = new DistributedServices();

            var sampleData = new SampleData()
            {
                Id=Guid.NewGuid(),
                Name = "",
                Version = ""
            };
            ds.PublishDataModel(new DistributedCommandDataModel<SampleData>(sampleData, EntityAction.Insert));
            Console.WriteLine(sampleData.Id);
            Console.ReadLine();
        }
    }
}
