using AutoMapper;
using Insurance.Domain;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class VehicleUsageController : Controller
    {
        // GET: VehicleUsage
        public ActionResult Index()
        {
            var obj = new InsuranceClaim.Models.VehicleUsageModel();
           var objList = InsuranceContext.VehicleUsages.All().ToList();
            ViewBag.Products = InsuranceContext.Products.All().ToList();
            return View(obj);
        }
        [HttpPost]
        public ActionResult SaveVehicalUsage(VehicleUsageModel model)
        {
            var dbModel = Mapper.Map<VehicleUsageModel,VehicleUsage>(model);
            InsuranceContext.VehicleUsages.Insert(dbModel);
            //return View(dbModel);
            return RedirectToAction("VehicalUserList");
        }
        public ActionResult VehicalUserList()
        {
            var UserList = InsuranceContext.VehicleUsages.All().ToList();

            return View(UserList);
        }

    }
}