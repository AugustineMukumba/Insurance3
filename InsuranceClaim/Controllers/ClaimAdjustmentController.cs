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
    public class ClaimAdjustmentController : Controller
    {
        // GET: ClaimAdjustment
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult SaveClaimAdjustment(ClaimAdjustmentModel model)
        {
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                var data = Mapper.Map<ClaimAdjustmentModel, ClaimAdjustment>(model);
                data.CreatedOn = DateTime.Now;
                data.CreatedBy = 1;
                data.IsActive = true;
                InsuranceContext.ClaimAdjustments.Insert(data);
            }
            return RedirectToAction("ListClaimAdjustment");
        }
        public ActionResult ListClaimAdjustment()
        {

            var ListClaimAdjust = InsuranceContext.ClaimAdjustments.All(where: $"IsActive = 'True' or IsActive is null");

            return View(ListClaimAdjust);
        }
        [HttpGet]
        public ActionResult EditClaimAdjustment(int Id)
        {
            var claimadjustdata = InsuranceContext.ClaimAdjustments.Single(where: $"Id ='{Id}'");
            var model = Mapper.Map<ClaimAdjustment, ClaimAdjustmentModel>(claimadjustdata);

            return View(model);
        }
    }
}