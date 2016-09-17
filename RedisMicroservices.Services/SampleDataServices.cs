using System;
using System.Threading;
using RedisMicroservices.Core.Distributed;
using RedisMicroservices.Core.Repository;
using RedisMicroservices.Domain;

namespace RedisMicroservices.Services
{
    public class SampleDataServices : ISampleDataServices
    {
        private ILangguageServices _langguageServices;
        private IDistributedServices _distributedServices;

        public SampleDataServices(ILangguageServices langguageServices)
        {
            _langguageServices = langguageServices;
        }

        public void ResiterDistributed(IDistributedServices distributedServices)
        {
            _distributedServices = distributedServices;
            distributedServices.SubscribeDataModel<SampleData>(((c, v) =>
            {
                switch (v.EntityAction)
                {
                    case EntityAction.Insert:
                        Create(v.Data);
                        break;
                    case EntityAction.Update:
                        break;
                    case EntityAction.Delete:
                        break;
                }
            }));
        }

        public void Create(SampleData data)
        {
            //call other business
            _langguageServices.DoSomething(new LanguageData()
            {
                Code= data.LanguageCode
            });

            data.CreatedDate = DateTime.Now;
            Thread.Sleep(1000); //sleep to simulate something to do 
            //after all push to message queue using distributed services

            //push command to store by repository engine
            _distributedServices.PublishEntity(new DistributedCommandEntity<Sample>(new Sample()
            {
                Id = data.Id,
                Version = data.Version,
                Name= data.Name
            }, EntityAction.Insert));
        }

        public void DoSomething(SampleData data)
        {
            throw new NotImplementedException();
        }
    }
}