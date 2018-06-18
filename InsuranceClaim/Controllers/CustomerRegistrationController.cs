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
            var InsService = new InsurerService();
            ViewBag.Currency = InsuranceContext.Currencies.All().ToList();
            ViewBag.PoliCyStatus = InsuranceContext.PolicyStatuses.All().ToList();
            ViewBag.BusinessSource = InsuranceContext.BusinessSources.All().ToList();
            ViewBag.Products = InsuranceContext.Products.All().ToList();
            ViewBag.Insurer = InsService.GetInsurers();
            var objList = InsuranceContext.PolicyDetails.All(orderBy: "Id desc").FirstOrDefault();
            if (objList != null)
            {
                string number = objList.PolicyNumber.Split('-')[0].Substring(4, objList.PolicyNumber.Length - 6);
                long pNumber = Convert.ToInt64(number.Substring(2, number.Length - 2)) + 1;
                string policyNumber = string.Empty;
                int length = 7;
                length = length - pNumber.ToString().Length;
                for (int i = 0; i < length; i++)
                {
                    policyNumber += "0";
                }
                policyNumber += pNumber;
                ViewBag.PolicyNumber = "GMCC" + DateTime.Now.Year.ToString().Substring(2, 2) + policyNumber + "-1";
            }
            else
            {
                ViewBag.PolicyNumber = ConfigurationManager.AppSettings["PolicyNumber"] + "-1";
            }
            var objCustomerData = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).ToList();
            if (objCustomerData.Count > 0)
            {
                ViewBag.InsurerId = objCustomerData.FirstOrDefault().Id;
                ViewBag.InsurerName = objCustomerData.FirstOrDefault().FirstName;
            }
            return View();
        }

        public ActionResult RiskDetail(int id)
        {
            //Id is policyid from Policy detail table
            var viewModel = new RiskDetailModel();
            viewModel.PolicyId = id;
            var service = new VehicleService();
            var makers = service.GetMakers();
            ViewBag.CoverType = service.GetCoverType();
            ViewBag.AgentCommission = service.GetAgentCommission();
            ViewBag.Makers = makers;
            ViewBag.VehicleUsage = service.GetVehicleUsage(id);
            TempData["Policy"] = service.GetPolicy(id);
            if (makers.Count > 0)
            {
                var model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = model;
            }
            service = null;
            return View(viewModel);
        }
        [HttpPost]
        public JsonResult CalculatePremium(int vehicleUsageId, decimal sumInsured, int coverType, int excessType, decimal excess)
        {
            JsonResult json = new JsonResult();
            var quote = new QuoteLogic();
            var typeCover = eCoverType.Comprehensive;
            if (coverType == 2)
            {
                typeCover = eCoverType.ThirdParty;
            }
            var eexcessType = eExcessType.Percentage;
            if (excessType == 2)
            {
                eexcessType = eExcessType.FixedAmount;
            }
            var premium = quote.CalculatePremium(vehicleUsageId, sumInsured, typeCover, eexcessType, excess);
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            json.Data = premium;
            return json;
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
        public ActionResult SummaryDetail(int id)
        {
            var model = new SummaryDetailModel();
            var summary = new SummaryDetailService();
            var vehicle = summary.GetVehicleInformation(id);
            TempData["VehicleDetail"] = vehicle;
            model.CarInsuredCount = vehicle.NoOfCarsCovered;
            model.DebitNote = "INV" + DateTime.Now.Ticks;
            model.PaymentMethodId = 1;
            model.PaymentTermId = 1;
            model.ReceiptNumber = "";
            model.SMSConfirmation = false;
            model.TotalPremium = vehicle.Premium + vehicle.StampDuty + vehicle.ZTSCLevy;
            model.TotalRadioLicenseCost = vehicle.RadioLicenseCost;
            model.TotalStampDuty = vehicle.StampDuty;
            model.TotalSumInsured = vehicle.SumInsured;
            model.TotalZTSCLevies = vehicle.ZTSCLevy;
            return View(model);
        }

        public ActionResult PaymentDetail(int id)
        {
            var model = new CardDetailModel();
            model.SummaryDetailId = id;
            return View(model);
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
                        Session["CustomerId"] = customer.Id;
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
            JsonResult json = new JsonResult();
            var response = new Response();
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
                policy.CustomerId = Session["CustomerId"] == null ? 0 : Convert.ToInt32(Session["CustomerId"]);
                InsuranceContext.PolicyDetails.Insert(policy);
                response.Id = policy.Id;
                response.Message = "Success";
                response.Status = true;
                json.Data = response;
                json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return json;
            }
            catch (Exception ex)
            {
                response.Id = 0;
                response.Message = ex.Message;
                response.Status = false;
                json.Data = response;
                json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return json;
            }

        }

        [HttpPost]
        public ActionResult GenerateQuote(RiskDetailModel model)
        {
            DateTimeFormatInfo usDtfi = new CultureInfo("en-US", false).DateTimeFormat;
            var service = new RiskDetailService();
            var startDate = Request.Form["CoverStartDate"];
            var endDate = Request.Form["CoverEndDate"];
            if (!string.IsNullOrEmpty(startDate))
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
                var policy = TempData["Policy"] as PolicyDetail;
                model.CustomerId = policy.CustomerId;
                var Id = service.AddVehicleInformation(model);
                return RedirectToAction("SummaryDetail", new { id = Id });
            }

            return View("RiskDetail");
        }
        [HttpPost]
        public ActionResult SubmitPlan(SummaryDetailModel model)
        {
            var vehicle= TempData["VehicleDetail"] as VehicleDetail;

            var DbEntry = Mapper.Map<SummaryDetailModel, SummaryDetail>(model);
            DbEntry.VehicleDetailId = vehicle.Id;
            DbEntry.CustomerId=vehicle.CustomerId;
            InsuranceContext.SummaryDetails.Insert(DbEntry);
            return RedirectToAction("PaymentDetail", new { id = DbEntry.Id });
        }
    }
}