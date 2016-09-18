using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RedisMicroservices.Core.Distributed;
using RedisMicroservices.Domain;
using RedisMicroservices.Domain.DataModel;
using RedisMicroservices.Domain.Ef6;
using RedisMicroservices.Domain.Entity;

namespace RedisMicroservices.SampleWeb.Controllers
{
    public class HomeController : Controller
    {
        IDistributedServices _distributedServices = new DistributedServices();

        public ActionResult Index()
        {
            //we can use direct select, join ... to create view model ...
            //we dont use exec command to do with db
            //because username in connection string only had permission to select from db
            List<Sample> allSample;

            using (var db = new SampleDbContext())
            {
                allSample = db.Samples.ToList();
            }
            return View(allSample);
        }

        [HttpPost]
        public ActionResult CreateRandom(SampleData data)
        {
            //we dont use exec command to do with db
            //we push command to manipulate with db
            // push data (SampleData obj) to ServicesEngine if want to do complex business
            data.CreatedDate = DateTime.Now;

            _distributedServices.PublishDataModel(new DistributedCommandDataModel<SampleData>(
                data, EntityAction.Insert, DataBehavior.Queue));

            // push data (Sample obj) to RepositoryEngine if want to do simple manipulate with db

            //_distributedServices.PublishEntity(new DistributedCommandEntity<Sample>(
            // entity, EntityAction.Insert, DataBehavior.Queue ));

            return RedirectToAction("Index");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}