using AutoMapper;
using Insurance.Domain;
using Insurance.Service;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
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
        public ActionResult Index(int? id, string PolicyNumber, int? claimnumber)
        {

            var ePaymentDetail = from ePayeeBankDetails e in Enum.GetValues(typeof(ePayeeBankDetails))
                                 select new
                                 {
                                     ID = (int)e,
                                     Name = e.ToString()
                                 };

            ViewBag.ePaymentDetailData = new SelectList(ePaymentDetail, "ID", "Name");

            if (id != 0 && id != null)
            {
                var model = new ClaimAdjustmentModel();
                var data = InsuranceContext.ClaimRegistrations.Single(where: $"PolicyNumber= '{PolicyNumber}' and ClaimNumber ='{claimnumber}'");
                if (data != null && data.Count() > 0)
                {
                    
                    var policy = InsuranceContext.PolicyDetails.Single(where: $"PolicyNumber='{data.PolicyNumber}'");
                    var summery = InsuranceContext.SummaryDetails.Single(where: $"CustomerId = '{policy.CustomerId}'");
                    
                    var custmo = InsuranceContext.Customers.Single(where: $"Id = '{policy.CustomerId}'");
                    model.EstimatedLoss =data.EstimatedValueOfLoss;
                    model.ClaimNumber = Convert.ToInt32(data.ClaimNumber);
                    model.PolicyNumber = data.PolicyNumber;
                    model.FirstName = custmo.FirstName;
                    model.LastName = custmo.LastName;
                    model.TotalSuminsure = Convert.ToDecimal(summery.TotalPremium);
                    return View(model);

                }
            }
            return View();
        }
        public ActionResult SaveClaimAdjustment(ClaimAdjustmentModel model)
        {
            ModelState.Remove("Id");
            if (model.IsDriverUnder25==null)
            {
                ModelState.Remove("IsDriverUnder25");
            }
            if (model.DriverIsUnder21 == null)
            {
                ModelState.Remove("DriverIsUnder21");
            }

            foreach (ModelState modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    var s = error;
                }
            }


            if (ModelState.IsValid)
            {
                string userid = "";
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                if (userLoggedin)
                {
                    var claimadjust = InsuranceContext.ClaimAdjustments.Single(where: $"ClaimNumber = '{model.ClaimNumber}'");
                    userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");
                    var data = Mapper.Map<ClaimAdjustmentModel, ClaimAdjustment>(model);
                    data.CreatedOn = DateTime.Now;
                    data.CreatedBy = customer.Id;
                    data.IsActive = true;
                    //insert
                    InsuranceContext.ClaimAdjustments.Insert(data);
                    return RedirectToAction("ListClaimAdjustment");
                }
            }
            return View("Index");
        }
        public ActionResult ListClaimAdjustment()
        {

            var ListClaimAdjust = InsuranceContext.ClaimAdjustments.All(where: $"IsActive = 'True' or IsActive is null").OrderByDescending(x => x.Id);

            return View(ListClaimAdjust);
        }
        [HttpGet]
        public ActionResult EditClaimAdjustment(int Id)
        {
            var claimadjustdata = InsuranceContext.ClaimAdjustments.Single(where: $"Id ='{Id}'");
            var model = Mapper.Map<ClaimAdjustment, ClaimAdjustmentModel>(claimadjustdata);

            return View(model);
        }
        public ActionResult EditClaimAdjustment(ClaimAdjustmentModel model)
        {

            if (ModelState.IsValid)
            {
                string userid = "";
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                if (userLoggedin)
                {
                    userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");
                    var claimadjustdata = Mapper.Map<ClaimAdjustmentModel, ClaimAdjustment>(model);
                    var record = InsuranceContext.ClaimAdjustments.Single(where: $"Id = '{model.Id}'");
                    claimadjustdata.ModifiedOn = DateTime.Now;
                    claimadjustdata.ModifiedBy = customer.Id;
                    claimadjustdata.IsActive = true;
                    claimadjustdata.CreatedOn = record.CreatedOn;
                    InsuranceContext.ClaimAdjustments.Update(claimadjustdata);
                    return RedirectToAction("ListClaimAdjustment");
                }
            }
            return View("EditClaimAdjustment");
        }
        public ActionResult DeleteClaimAdjustment(int Id)
        {
            string query = $"update ClaimAdjustment set IsActive = 0 where Id={Id}";
            InsuranceContext.ClaimAdjustments.Execute(query);
            return RedirectToAction("ListClaimAdjustment");
        }
        [HttpPost]
        public JsonResult CalculateClaimPremium(decimal sumInsured,int? IsPartialLoss,int? IsLossInZimbabwe,int? IsStolen,int? Islicensedless60months,int? DriverIsUnder21,Boolean PrivateCar,Boolean CommercialCar,int? IsDriver25,int? IsSoundSystem)
        {
            JsonResult json = new JsonResult();
            var ClaimQuoteL = new ClaimQuoteLogic();
            var excess = ClaimQuoteL.CalculateClaimPremium(sumInsured, IsPartialLoss, IsLossInZimbabwe, IsStolen, Islicensedless60months, DriverIsUnder21, PrivateCar, CommercialCar, IsDriver25, IsSoundSystem);
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            json.Data = excess;
            return json;
        }
    }
}