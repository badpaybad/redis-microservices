using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RedisMicroservices.Core.Distributed;
using RedisMicroservices.DataAccess.Ef6;
using RedisMicroservices.Domain;
using RedisMicroservices.Domain.DataModel;
using RedisMicroservices.Domain.Entity;
using RedisMicroservices.SampleWeb.Business;

namespace RedisMicroservices.SampleWeb.Controllers
{
    public class HomeController : Controller
    {
        SampleManager _sampleManager = new SampleManager();

        public ActionResult Index()
        {
            return View(_sampleManager.ListAll());
        }

        [HttpPost]
        public ActionResult CreateRandom(SampleData data)
        {
             data.CreatedDate = DateTime.Now;

            _sampleManager.Create(data);

         
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