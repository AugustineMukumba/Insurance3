using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Insurance.Domain;
using InsuranceClaim.Models;
using AutoMapper;

namespace InsuranceClaim.Controllers
{
    public class CommissionController : Controller
    {
        // GET: Commission
        public ActionResult Index()
        {
            InsuranceClaim.Models.AgentCommissionModel obj = new InsuranceClaim.Models.AgentCommissionModel();
            List<Insurance.Domain.AgentCommission> objList = new List<Insurance.Domain.AgentCommission>();
            objList = InsuranceContext.AgentCommissions.All().ToList();
            obj.CommissionAmount = null;
            obj.ManagementCommission = null;

            return View(obj);

    }
    [HttpPost]
        public ActionResult CommissionSave(AgentCommissionModel model)
        {

                var data = Mapper.Map<AgentCommissionModel, AgentCommission>(model);
            InsuranceContext.AgentCommissions.Insert(data);
                return RedirectToAction("CommissionList");
            
        }
        public ActionResult CommissionList()
        {
            var db = InsuranceContext.AgentCommissions.All(where:"IsActive='True' Or IsActive is null").ToList();


            return View(db);
        }
        public ActionResult CommissionEdit(int Id)
        {
            var record = InsuranceContext.AgentCommissions.All(where: $"Id ={Id}").FirstOrDefault();
            AgentCommissionModel obj = new AgentCommissionModel();
            obj.Id = record.Id;
            obj.CommissionName = record.CommissionName;
            obj.CommissionAmount = record.CommissionAmount;
            obj.ManagementCommission = record.ManagementCommission;

            return View(obj);
        }
        [HttpPost]

        public ActionResult CommissionEdit(AgentCommissionModel model )
        {
            if (ModelState.IsValid)
            {
                var db = InsuranceContext.AgentCommissions.Single(where: $"Id = {model.Id}");
                db.CommissionName = model.CommissionName;
                db.CommissionAmount = model.CommissionAmount;
                db.ManagementCommission = model.ManagementCommission;
                InsuranceContext.AgentCommissions.Update(db);
            }

            return RedirectToAction("CommissionList");

        }
        public ActionResult DeleteCommission(int Id)
        {
            string query = $"update AgentCommission set IsActive = 0 where Id ={Id}";
            InsuranceContext.AgentCommissions.Execute(query);


            return RedirectToAction("CommissionList");
        }

    }
}
