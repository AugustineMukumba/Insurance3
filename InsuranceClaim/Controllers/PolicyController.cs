using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InsuranceClaim.Models;
using AutoMapper;

namespace InsuranceClaim.Controllers
{
    public class PolicyController : Controller
    {
        // GET: Policy
        public ActionResult Index()
        {
            InsuranceClaim.Models.PolicyInsurerModel obj = new InsuranceClaim.Models.PolicyInsurerModel();
            List<Insurance.Domain.PolicyInsurer> objList = new List<Insurance.Domain.PolicyInsurer>();
            objList = InsuranceContext.PolicyInsurers.All().ToList();
            return View(obj);
        }
        [HttpPost]
        public ActionResult PolicySave(PolicyInsurerModel model)
        {
            var data = Mapper.Map<PolicyInsurerModel, PolicyInsurer>(model);
            InsuranceContext.PolicyInsurers.Insert(data);
            return RedirectToAction("PolicyList");
        }
        public ActionResult PolicyList()
        {
            var db = InsuranceContext.PolicyInsurers.All().ToList();

            return View(db);
        }
        public ActionResult EditPolicy(int Id)
        {
            var record = InsuranceContext.PolicyInsurers.All(where: $"Id ={Id}").FirstOrDefault();
            PolicyInsurerModel obj = new PolicyInsurerModel();
            obj.Id = record.Id;
            obj.InsurerName = record.InsurerName;
            obj.InsurerCode = record.InsurerCode;
            obj.InsurerAddress = record.InsurerAddress;

            return View(obj);
        }
            [HttpPost]
        public ActionResult EditPolicy(PolicyInsurerModel model)
        {

            if (ModelState.IsValid)
            {
                var db = InsuranceContext.PolicyInsurers.Single(where: $"Id = {model.Id}");
                db.InsurerName = model.InsurerName;
                db.InsurerCode = model.InsurerCode;
                db.InsurerAddress = model.InsurerAddress;
                InsuranceContext.PolicyInsurers.Update(db);
            }
            return RedirectToAction("PolicyList");
        }
    }

}

