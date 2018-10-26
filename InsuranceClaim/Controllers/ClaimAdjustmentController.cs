using AutoMapper;
using Insurance.Domain;
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
        public ActionResult Index(int? id,string PolicyNumber,int? claimnumber)
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
                var adjustment = InsuranceContext.ClaimAdjustments.Single(where:$"PolicyNumber= '{PolicyNumber}' and ClaimNumber = '{claimnumber}'");
               

                var model = new ClaimAdjustmentModel();
                if (adjustment != null && adjustment.Count() > 0)
                {

                    model.AmountToPay = adjustment.AmountToPay;
                    model.EstimatedLoss = adjustment.EstimatedLoss;
                    model.FirstName = adjustment.FirstName;
                    model.LastName = adjustment.LastName;
                    model.ExcessesAmount = adjustment.ExcessesAmount;
                    model.PayeeName = adjustment.PayeeName;
                    model.PolicyholderName = adjustment.PolicyholderName;
                    model.PayeeBankDetails = adjustment.PayeeBankDetails;
                    model.DriverIsUnder21 = adjustment.DriverIsUnder21;
                    model.Islicensedless60months = adjustment.Islicensedless60months;
                    model.IsLossInZimbabwe = adjustment.IsLossInZimbabwe;
                    model.IsPartialLoss = adjustment.IsPartialLoss;
                    model.IsStolen = adjustment.IsStolen;
                    model.PolicyNumber = adjustment.PolicyNumber;
                    model.PhoneNumber = adjustment.PhoneNumber;
                    model.ClaimNumber = adjustment.ClaimNumber;

                    return View(model);
                }
                else
                {

                    var data = InsuranceContext.ClaimRegistrations.Single(where:$"PolicyNumber= '{PolicyNumber}' and ClaimNumber ='{claimnumber}'");
                    var policy = InsuranceContext.PolicyDetails.Single(where: $"PolicyNumber='{data.PolicyNumber}'");
                    var custmo = InsuranceContext.Customers.Single(where: $"Id = '{policy.CustomerId}'");
                    model.EstimatedLoss = Convert.ToInt32(data.EstimatedValueOfLoss);
                    model.ClaimNumber =Convert.ToInt32(data.ClaimNumber);
                    model.PolicyNumber =data.PolicyNumber;
                    model.FirstName = custmo.FirstName;
                    model.LastName = custmo.LastName;
                    model.DriverIsUnder21 = true;
                    model.Islicensedless60months = true;
                    model.IsLossInZimbabwe = true;
                    model.IsPartialLoss = true;
                    model.IsStolen = true;
                    return View(model);
                }
            }
           





            return View();
        }
        public ActionResult SaveClaimAdjustment(ClaimAdjustmentModel model)
        {
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                string userid = "";
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                if (userLoggedin)
                {
                    var claimadjust = InsuranceContext.ClaimAdjustments.Single(where: $"PolicyNumber = '{model.PolicyNumber}'");
                    if (claimadjust != null && claimadjust.Count() >0)
                    {


                        userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");

                        var data = Mapper.Map<ClaimAdjustmentModel, ClaimAdjustment>(model);
                        data.ModifiedOn = DateTime.Now;
                        data.ModifiedBy = customer.Id;
                        data.IsActive = true;
                        InsuranceContext.ClaimAdjustments.Update(data);

                        return RedirectToAction("ListClaimAdjustment");
                    }
                    else
                    {
                        userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");

                        var data = Mapper.Map<ClaimAdjustmentModel, ClaimAdjustment>(model);
                        data.CreatedOn = DateTime.Now;
                        data.CreatedBy = customer.Id;
                        data.IsActive = true;
                        InsuranceContext.ClaimAdjustments.Insert(data);
                        return RedirectToAction("ListClaimAdjustment");
                    }
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
    }
}