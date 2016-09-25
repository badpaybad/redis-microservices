using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RedisMicroservices.Core.Distributed;
using RedisMicroservices.DataAccess.Ef6;
using RedisMicroservices.Domain.DataModel;
using RedisMicroservices.Domain.Entity;

namespace RedisMicroservices.SampleWeb.Business
{
    public class SampleManager
    {
        DistributedServices _distributedServices=new DistributedServices();
        public void Create(SampleData data)
        {
           _distributedServices.PublishDataModel(new DistributedCommandDataModel<SampleData>(data,EntityAction.Insert));
            //we dont use exec command to do with db
            //we push command to manipulate with db
            // push data (SampleData obj) to ServicesEngine if want to do complex business

            // push data (Sample obj) to RepositoryEngine if want to do simple manipulate with db

            //_distributedServices.PublishEntity(new DistributedCommandEntity<Sample>(
            // entity, EntityAction.Insert, DataBehavior.Queue ));
        }

        public List<Sample> ListAll()
        {
            //we can use direct select, join ... to create view model ...
            //we dont use exec command to do with db
            //because username in connection string only had permission to select from db
            List<Sample> allSample;

            using (var db = new SampleDbContext())
            {
                allSample = db.Samples.ToList();
            }

            return allSample;
        } 
    }
}