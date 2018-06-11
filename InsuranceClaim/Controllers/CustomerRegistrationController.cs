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
            var objList = InsuranceContext.PolicyDetails.All(orderBy:"Id desc",top:1).ToList();
            if (objList.Count > 0)
            {
                ViewBag.PolicyNumber = objList.FirstOrDefault().PolicyNumber;
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
            return View();
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
            string custId = string.Empty;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.EmailAddress, Email = model.EmailAddress, PhoneNumber = model.PhoneNumber };
                var result = await UserManager.CreateAsync(user, "Kindle@123");
                if (result.Succeeded)
                {
                    var objCustomerId = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).ToList();
                    if (objCustomerId.Count > 0)
                    {
                        custId = Convert.ToString(objCustomerId.FirstOrDefault().CustomerId);
                    }
                    else
                    {
                        custId = ConfigurationManager.AppSettings["CustomerId"];
                    }

                    model.UserID = user.Id;
                    var customer = Mapper.Map<CustomerModel, Customer>(model);
                    InsuranceContext.Customers.Insert(customer);
                }

            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SavePolicyData(PolicyDetailModel model)
        {
            if (ModelState.IsValid)
            {

                var policy = Mapper.Map<PolicyDetailModel, PolicyDetail>(model);
                InsuranceContext.PolicyDetails.Insert(policy);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}