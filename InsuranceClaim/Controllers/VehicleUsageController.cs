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
            var dbModel = Mapper.Map<VehicleUsageModel, VehicleUsage>(model);
            InsuranceContext.VehicleUsages.Insert(dbModel);
            //return View(dbModel);
            return RedirectToAction("VehicalUsageList");
        }
        public ActionResult VehicalUsageList()
        {
            var UserList = InsuranceContext.VehicleUsages.All(where: "IsActive='True' or IsActive is null").ToList();

            return View(UserList);

        }
        public ActionResult EditVehicalUsege(int Id)
        {
            var record = InsuranceContext.VehicleUsages.All(where: $"Id ={Id}").FirstOrDefault();
            VehicleUsageModel obj = new VehicleUsageModel();
            obj.Id = record.Id;
            ViewBag.Products = InsuranceContext.Products.All().ToList();
            obj.ProductId = record.ProductId;
            obj.VehUsage = record.VehUsage;
            obj.ComprehensiveRate = record.ComprehensiveRate;
            obj.MinCompAmount = record.MinCompAmount;
            obj.ThirdPartyRate = record.ThirdPartyRate;
            obj.MinThirdAmount = record.MinThirdAmount;
            obj.FTPAmount = record.FTPAmount;
            obj.AnnualTPAmount = record.AnnualTPAmount;
            return View(obj);
        }
        [HttpPost]
        public ActionResult EditVehicalUsege(VehicleUsageModel model)
        {
            if (ModelState.IsValid)
            {
                var obj = InsuranceContext.VehicleUsages.Single(where: $"Id = {model.Id}");
                obj.ProductId = model.ProductId;
                obj.VehUsage = model.VehUsage;
                obj.ComprehensiveRate = model.ComprehensiveRate;
                obj.MinCompAmount = model.MinCompAmount;
                obj.ThirdPartyRate = model.ThirdPartyRate;
                obj.MinThirdAmount = model.MinThirdAmount;
                obj.FTPAmount = model.FTPAmount;
                obj.AnnualTPAmount = model.AnnualTPAmount;
                InsuranceContext.VehicleUsages.Update(obj);
            }

            return RedirectToAction("VehicalUsageList");
        }
        public ActionResult DeleteVehicalUsage(int Id)
        {

            //var record = InsuranceContext.VehicleUsages.All(where: $"Id ={Id}").FirstOrDefault();

            string query = $"update VehicleUsage set IsActive=0 where Id={Id}";

            InsuranceContext.VehicleUsages.Execute(query);
            return RedirectToAction("VehicalUsageList");
        }
    }
}