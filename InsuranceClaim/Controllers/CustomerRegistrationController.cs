using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Insurance.Domain;
using AutoMapper;
using System.Configuration;
using System.Globalization;
using Insurance.Service;

namespace InsuranceClaim.Controllers
{
    public class CustomerRegistrationController : Controller
    {
        private ApplicationUserManager _userManager;
        public CustomerRegistrationController()
        {
            // UserManager = userManager;
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // GET: CustomerRegistration
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ProductDetail()
        {
            ViewBag.Currency = InsuranceContext.Currencies.All().ToList();
            ViewBag.PoliCyStatus = InsuranceContext.PolicyStatuses.All().ToList();
            ViewBag.BusinessSource = InsuranceContext.BusinessSources.All().ToList();
            ViewBag.Products = InsuranceContext.Products.All().ToList();
            var objList = InsuranceContext.PolicyDetails.All(orderBy: "Id desc").FirstOrDefault();
            if (objList != null)
            {
                ViewBag.PolicyNumber = Convert.ToDecimal(objList.PolicyNumber) + 1;
            }
            else
            {
                ViewBag.PolicyNumber = ConfigurationManager.AppSettings["PolicyNumber"];
            }
            var objCustomerData = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).ToList();
            if (objCustomerData.Count > 0)
            {
                ViewBag.InsurerId = objCustomerData.FirstOrDefault().Id;
                ViewBag.InsurerName = objCustomerData.FirstOrDefault().FirstName;
            }
            return View();
        }

        public ActionResult RiskDetail()
        {
            var service = new VehicleService();
            var makers = service.GetMakers();
            ViewBag.Makers = makers;
            if (makers.Count > 0)
            {
                var model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = model;
            }
            service = null;
            return View();
        }
        public JsonResult GetVehicleModel(string makeCode)
        {
            var service = new VehicleService();
            var model = service.GetModel(makeCode);
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = model;
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
        public ActionResult SummaryDetail()
        {
            return View();
        }

        public ActionResult PaymentDetail()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> SaveCustomerData(CustomerModel model)
        {
            decimal custId = 0;
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new ApplicationUser { UserName = model.EmailAddress, Email = model.EmailAddress, PhoneNumber = model.PhoneNumber };
                    var result = await UserManager.CreateAsync(user, "Kindle@123");
                    if (result.Succeeded)
                    {
                        var objCustomer = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).FirstOrDefault();
                        if (objCustomer != null)
                        {
                            custId = objCustomer.CustomerId + 1;
                        }
                        else
                        {
                            custId = Convert.ToDecimal(ConfigurationManager.AppSettings["CustomerId"]);
                        }

                        model.UserID = user.Id;
                        model.CustomerId = custId;
                        var customer = Mapper.Map<CustomerModel, Customer>(model);
                        InsuranceContext.Customers.Insert(customer);
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {

                    return Json(false, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SavePolicyData(PolicyDetailModel model)
        {
            try
            {
                DateTimeFormatInfo usDtfi = new CultureInfo("en-US", false).DateTimeFormat;
                var startDate = Request.Form["StartDate"];
                var endDate = Request.Form["EndDate"];
                var renewDate = Request.Form["RenewalDate"];
                var transactionDate = Request.Form["TransactionDate"];
                if (startDate != null)
                {
                    model.StartDate = Convert.ToDateTime(startDate, usDtfi);
                }
                if (endDate != null)
                {
                    model.EndDate = Convert.ToDateTime(endDate, usDtfi);
                }
                if (renewDate != null)
                {
                    model.RenewalDate = Convert.ToDateTime(renewDate, usDtfi);
                }
                if (transactionDate != null)
                {
                    model.TransactionDate = Convert.ToDateTime(transactionDate, usDtfi);
                }

                var policy = Mapper.Map<PolicyDetailModel, PolicyDetail>(model);
                InsuranceContext.PolicyDetails.Insert(policy);

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult GenerateQuote(RiskDetailModel model)
        {
            DateTimeFormatInfo usDtfi = new CultureInfo("en-US", false).DateTimeFormat;
            var service = new RiskDetailService();
            var startDate = Request.Form["CoverStartDate"];
            var endDate = Request.Form["CoverEndDate"];
            if(!string.IsNullOrEmpty(startDate))
            {
                ModelState.Remove("CoverStartDate");
                model.CoverStartDate = Convert.ToDateTime(startDate, usDtfi);
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                ModelState.Remove("CoverEndDate");
                model.CoverEndDate = Convert.ToDateTime(endDate, usDtfi);
            }          
            if (ModelState.IsValid)
            {
                service.AddVehicleInformation(model);
            }
          
            return View();
        }
    }
}