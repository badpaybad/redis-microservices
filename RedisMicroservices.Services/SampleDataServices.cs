using System;
using System.Threading;
using Newtonsoft.Json;
using RedisMicroservices.Core.Distributed;
using RedisMicroservices.Core.Repository;
using RedisMicroservices.Domain;
using RedisMicroservices.Domain.DataModel;
using RedisMicroservices.Domain.Entity;

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
                Code = data.LanguageCode
            });

            data.CreatedDate = DateTime.Now;
            Thread.Sleep(1000); //sleep to simulate something to do 
            //after all push to message queue using distributed services

            //push command to store by repository engine
            var entity = new Sample()
            {
                Id = data.Id,
                Version = data.Version,
                Name = data.Name
            };
            var distributedCommandEntity = new DistributedCommandEntity<Sample>(entity, EntityAction.Insert);

            _distributedServices.PublishEntity(distributedCommandEntity);
        }

        public void DoSomething(SampleData data)
        {
            throw new NotImplementedException();
        }
    }
}