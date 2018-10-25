using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InsuranceClaim.Models;
using Insurance.Domain;
using AutoMapper;

namespace InsuranceClaim.Controllers
{
    public class ServiceProviderController : Controller
    {
        // GET: ServiceProvider
        [HttpGet]
        public ActionResult SaveServiceProviders()
        {
            ViewBag.ProviderTypes = InsuranceContext.ServiceProviderTypes.All().ToList();
            return View();
        }

        [HttpPost]
        public ActionResult SaveServiceProviders(ServiceProviderModel model)
        {
            if (ModelState.IsValid)
            {
                var dbModel = Mapper.Map<ServiceProviderModel, ServiceProvider>(model);
                dbModel.CreatedOn = DateTime.Now;
                dbModel.IsDeleted = true;
                InsuranceContext.ServiceProviders.Insert(dbModel);
                return RedirectToAction("ProvidersList");              
            }
            return View();
        }
        //[Authorize(Roles = "Staff")]
        [HttpGet]
        public ActionResult ProvidersList()
        {
            InsuranceClaim.Models.ServiceProviderModel obj = new InsuranceClaim.Models.ServiceProviderModel();
            List<Insurance.Domain.ServiceProvider> objList = new List<Insurance.Domain.ServiceProvider>();
            objList = InsuranceContext.ServiceProviders.All(where: "IsDeleted = 'True' or IsDeleted is null").ToList();
            return View(objList);
        }

        [HttpGet]
        public ActionResult EditProviders(int Id)
        {
            ViewBag.ProviderTypes = InsuranceContext.ServiceProviderTypes.All().ToList();
            var record = InsuranceContext.ServiceProviders.All(where: $"Id ={Id}").FirstOrDefault();
            var model = Mapper.Map<ServiceProvider, ServiceProviderModel>(record);
            return View(model);
        }

        [HttpPost]
        public ActionResult EditProviders(ServiceProviderModel model)
        {
            if (ModelState.IsValid)
            {
                var data = Mapper.Map<ServiceProviderModel, ServiceProvider>(model);
                data.CreatedOn = DateTime.Now;
                data.IsDeleted = true;
                InsuranceContext.ServiceProviders.Update(data);
                return RedirectToAction("ProvidersList");
            }
            return View();
        }

        public ActionResult DeleteProviders(int Id)
        {
            string query = $"update ServiceProvider set IsDeleted = 0 where Id = {Id}";
            InsuranceContext.ServiceProviders.Execute(query);
            return RedirectToAction("ProvidersList");
        }
    }
}
