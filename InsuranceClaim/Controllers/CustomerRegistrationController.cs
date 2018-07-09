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

        public ActionResult Index()
        {
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            if (userLoggedin)
            {
                var customerModel = new CustomerModel();
                var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{User.Identity.GetUserId().ToString()}'").FirstOrDefault();

                if (_customerData != null)
                {
                    customerModel.AddressLine1 = _customerData.AddressLine1;
                    customerModel.AddressLine2 = _customerData.AddressLine2;
                    customerModel.City = _customerData.City;
                    customerModel.Id = _customerData.Id;
                    customerModel.Country = _customerData.Country;
                    customerModel.Zipcode = _customerData.Zipcode;
                    customerModel.Gender = _customerData.Gender;
                    customerModel.PhoneNumber = _User.PhoneNumber;
                    customerModel.State = _customerData.State;
                    customerModel.DateOfBirth = _customerData.DateOfBirth;
                    customerModel.EmailAddress = _User.Email;
                    customerModel.FirstName = _customerData.FirstName;
                    customerModel.LastName = _customerData.LastName;
                }

                return View(customerModel);
            }
            else
            {
                var customerData = (CustomerModel)Session["CustomerDataModal"];
                var customerModel = new CustomerModel();
                if (customerData != null)
                {
                    var User = UserManager.FindById(customerData.UserID);
                    customerModel.AddressLine1 = customerData.AddressLine1;
                    customerModel.AddressLine2 = customerData.AddressLine2;
                    customerModel.City = customerData.City;
                    customerModel.Id = customerData.Id;
                    customerModel.Country = customerData.Country;
                    customerModel.Zipcode = customerData.Zipcode;
                    customerModel.Gender = customerData.Gender;
                    customerModel.PhoneNumber = customerData.PhoneNumber;
                    customerModel.State = customerData.State;
                    customerModel.DateOfBirth = customerData.DateOfBirth;
                    customerModel.EmailAddress = customerData.EmailAddress;
                    customerModel.FirstName = customerData.FirstName;
                    customerModel.LastName = customerData.LastName;
                }
                return View(customerModel);
            }




        }

        //// GET: CustomerRegistration
        //public ActionResult Index()
        //{
        //    var customerData = (CustomerModel)Session["CustomerDataModal"];

        //    var customerModel = new CustomerModel();
        //    if (customerData != null)
        //    {
        //        var User = UserManager.FindById(customerData.UserID);
        //        customerModel.AddressLine1 = customerData.AddressLine1;
        //        customerModel.AddressLine2 = customerData.AddressLine2;
        //        customerModel.City = customerData.City;
        //        customerModel.Id = customerData.Id;
        //        customerModel.Country = customerData.Country;
        //        customerModel.Zipcode = customerData.Zipcode;
        //        customerModel.Gender = customerData.Gender;
        //        customerModel.PhoneNumber = customerData.PhoneNumber;
        //        customerModel.State = customerData.State;
        //        customerModel.DateOfBirth = customerData.DateOfBirth;
        //        customerModel.EmailAddress = customerData.EmailAddress;
        //        customerModel.FirstName = customerData.FirstName;
        //        customerModel.LastName = customerData.LastName;
        //    }
        //    return View(customerModel);
        //}
        [HttpPost]
        public async Task<JsonResult> SaveCustomerData(CustomerModel model)
        {
            if (ModelState.IsValid)
            {
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                if (userLoggedin)
                {
                    Session["CustomerDataModal"] = model;
                    return Json(new { IsError = true, error = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    var AllUsers = UserManager.Users.ToList();//.FirstOrDefault(p=>p.Email== model.EmailAddress);
                    var isExist = AllUsers.Any(p => p.Email.ToLower() == model.EmailAddress.ToLower() || p.UserName.ToLower() == model.EmailAddress);
                    if (isExist)
                    {
                        return Json(new { IsError = false, error = "Email " + model.EmailAddress + " already exists." }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Session["CustomerDataModal"] = model;
                        return Json(new { IsError = true, error = "" }, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            return Json(new { IsError = false, error = TempData["ErrorMessage"].ToString() }, JsonRequestBehavior.AllowGet);
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

            var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm))
                                   select new
                                   {
                                       ID = (int)e,
                                       Name = e.ToString()
                                   };

            ViewBag.ePaymentTermData = new SelectList(ePaymentTermData, "ID", "Name");

            var model = new PolicyDetailModel();
            model.BusinessSourceId = 3;

            var data = (PolicyDetail)Session["PolicyData"];
            if (data != null)
            {
                model.EndDate = data.EndDate;
                model.PaymentTermId = Convert.ToInt32(Session["policytermid"]);
                model.StartDate = data.StartDate;
                model.PolicyName = data.PolicyName;
                model.RenewalDate = data.RenewalDate;
                model.RenewalDate = data.RenewalDate;
                model.PolicyNumber = data.PolicyNumber;
                model.PolicyStatusId = data.PolicyStatusId;
                model.BusinessSourceId = data.BusinessSourceId;
                model.CurrencyId = data.CurrencyId;
            }
            return View(model);
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
                if (startDate != null && startDate != "")
                {
                    model.StartDate = Convert.ToDateTime(startDate, usDtfi);
                }
                if (endDate != null && endDate != "")
                {
                    model.EndDate = Convert.ToDateTime(endDate, usDtfi);
                }
                if (renewDate != null && renewDate != "")
                {
                    model.RenewalDate = Convert.ToDateTime(renewDate, usDtfi);
                }
                if (transactionDate != null && transactionDate != "")
                {
                    model.TransactionDate = Convert.ToDateTime(transactionDate, usDtfi);
                }
                Session["policytermid"] = model.PaymentTermId;
                Session["PolicyData"] = Mapper.Map<PolicyDetailModel, PolicyDetail>(model);
                //policy.CustomerId = Session["CustomerId"] == null ? 0 : Convert.ToInt32(Session["CustomerId"]);
                //if (Session["PolicyId"] != null)
                //{
                //    var id = Convert.ToInt32(Session["PolicyId"].ToString());
                //    //var data = InsuranceContext.PolicyDetails.Single(Convert.ToInt32(id));
                //    policy.Id = id;

                //    InsuranceContext.PolicyDetails.Update(policy);

                //}
                //else
                //{
                //    InsuranceContext.PolicyDetails.Insert(policy);
                //}

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
        public ActionResult RiskDetail()
        {
            var PolicyData = (PolicyDetail)Session["PolicyData"];
            //Id is policyid from Policy detail table
            var viewModel = new RiskDetailModel();
            viewModel.CoverStartDate = PolicyData.StartDate;
            viewModel.CoverEndDate = PolicyData.EndDate;
            var service = new VehicleService();
            var makers = service.GetMakers();
            ViewBag.CoverType = service.GetCoverType();
            ViewBag.AgentCommission = service.GetAgentCommission();
            ViewBag.Makers = makers;

            ViewBag.VehicleUsage = service.GetVehicleUsage(PolicyData.PolicyName);
            //TempData["Policy"] = service.GetPolicy(id);
            if (makers.Count > 0)
            {
                var model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = model;

            }
            var data = (RiskDetailModel)Session["VehicleDetail"];
            if (data != null)
            {
                viewModel.AgentCommissionId = data.AgentCommissionId;
                viewModel.ChasisNumber = data.ChasisNumber;
                viewModel.CoverEndDate = data.CoverEndDate;
                viewModel.CoverNoteNo = data.CoverNoteNo;
                viewModel.CoverStartDate = data.CoverStartDate;
                viewModel.CoverTypeId = data.CoverTypeId;
                viewModel.CubicCapacity = (int)Math.Round(data.CubicCapacity.Value, 0);
                viewModel.CustomerId = data.CustomerId;
                viewModel.EngineNumber = data.EngineNumber;
                //viewModel.Equals = data.Equals;
                viewModel.Excess = (int)Math.Round(data.Excess, 0);
                viewModel.ExcessType = data.ExcessType;
                viewModel.MakeId = data.MakeId;
                viewModel.ModelId = data.ModelId;
                viewModel.NoOfCarsCovered = data.NoOfCarsCovered;
                viewModel.OptionalCovers = data.OptionalCovers;
                viewModel.PolicyId = data.PolicyId;
                viewModel.Premium = data.Premium;
                viewModel.RadioLicenseCost = (int)Math.Round(data.RadioLicenseCost == null ? 0 : data.RadioLicenseCost.Value, 0);
                viewModel.Rate = data.Rate;
                viewModel.RegistrationNo = data.RegistrationNo;
                viewModel.StampDuty = data.StampDuty;
                viewModel.SumInsured = (int)Math.Round(data.SumInsured == null ? 0 : data.SumInsured.Value, 0);
                viewModel.VehicleColor = data.VehicleColor;
                viewModel.VehicleUsage = data.VehicleUsage;
                viewModel.VehicleYear = data.VehicleYear;
                viewModel.ZTSCLevy = data.ZTSCLevy;
                var ser = new VehicleService();
                var model = ser.GetModel(data.MakeId);
                ViewBag.Model = model;
            }
            return View(viewModel);
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
                Session["VehicleDetail"] = model;
                //var policy = TempData["Policy"] as PolicyDetail;
                //model.CustomerId = policy.CustomerId;
                //if (Session["VehicalId"] == null)
                //{
                //    var Id = service.AddVehicleInformation(model);
                //    Session["VehicalId"] = Id;
                //}
                //else
                //{
                //    var id = Convert.ToInt32(Session["VehicalId"]);

                //    var vehical = Mapper.Map<RiskDetailModel, VehicleDetail>(model);
                //    //var data = InsuranceContext.VehicleDetails.Single(id);
                //    vehical.Id = id;
                //    InsuranceContext.VehicleDetails.Update(vehical);
                //}

                return RedirectToAction("SummaryDetail");
            }

            return View("RiskDetail");
        }
        public ActionResult SummaryDetail()
        {
            SummaryDetailService SummaryDetailServiceObj = new SummaryDetailService();

            var model = new SummaryDetailModel();
            var summary = new SummaryDetailService();
            var vehicle = (RiskDetailModel)Session["VehicleDetail"];// summary.GetVehicleInformation(id);
            model.CarInsuredCount = vehicle.NoOfCarsCovered;
            model.DebitNote = "INV" + Convert.ToString(SummaryDetailServiceObj.getNewDebitNote());
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

        public static string CreateRandomPassword()
        {
            string _allowedChars = "0123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ";
            Random randNum = new Random();
            char[] chars = new char[8];
            int allowedCharCount = _allowedChars.Length;
            for (int i = 0; i < 8; i++)
            {
                chars[i] = _allowedChars[(int)((_allowedChars.Length) * randNum.NextDouble())];
            }
            return new string(chars);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitPlan(SummaryDetailModel model)
        {
            //var vehicle = (RiskDetailModel)Session["VehicleDetail"];
            Session["SummaryDetailed"] = model;
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            var customer = (CustomerModel)Session["CustomerDataModal"];
            if (!userLoggedin)
            {
                if (customer != null)
                {
                    if (customer.Id == null || customer.Id == 0)
                    {
                        decimal custId = 0;
                        var user = new ApplicationUser { UserName = customer.EmailAddress, Email = customer.EmailAddress, PhoneNumber = customer.PhoneNumber };
                        var result = await UserManager.CreateAsync(user, "Geninsure@123");
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

                            customer.UserID = user.Id;
                            customer.CustomerId = custId;
                            var customerdata = Mapper.Map<CustomerModel, Customer>(customer);
                            InsuranceContext.Customers.Insert(customerdata);
                            customer.Id = customerdata.Id;
                        }
                    }
                }
            }
            var policy = (PolicyDetail)Session["PolicyData"];
            if (policy != null)
            {
                if (policy.Id == null || policy.Id == 0)
                {
                    policy.CustomerId = customer.Id;
                    InsuranceContext.PolicyDetails.Insert(policy);
                }
            }
            var Id = 0;
            var vehicle = (RiskDetailModel)Session["VehicleDetail"];
            if (vehicle != null)
            {
                var service = new RiskDetailService();
                vehicle.CustomerId = customer.Id;
                vehicle.PolicyId = policy.Id;
                //var vehical = Mapper.Map<RiskDetailModel, RiskDetailModel>(vehicle);
                Id = service.AddVehicleInformation(vehicle);

            }

            var DbEntry = Mapper.Map<SummaryDetailModel, SummaryDetail>(model);
            DbEntry.PaymentTermId = Convert.ToInt32(Session["policytermid"]);
            DbEntry.VehicleDetailId = Id;
            DbEntry.CustomerId = vehicle.CustomerId;
            InsuranceContext.SummaryDetails.Insert(DbEntry);
            return RedirectToAction("PaymentDetail", new { id = DbEntry.Id });
        }






        //[HttpPost]
        //public async Task<ActionResult> SubmitPlan(SummaryDetailModel model)
        //{
        //    //var vehicle = (RiskDetailModel)Session["VehicleDetail"];
        //    Session["SummaryDetailed"] = model;
        //    var DbEntry = new SummaryDetail();
        //    var customer = (CustomerModel)Session["CustomerDataModal"];
        //    if (customer != null)
        //    {
        //        if (customer.Id == null || customer.Id == 0)
        //        {
        //            decimal custId = 0;
        //            var user = new ApplicationUser { UserName = customer.EmailAddress, Email = customer.EmailAddress, PhoneNumber = customer.PhoneNumber };
        //            var result = await UserManager.CreateAsync(user, "Kindle@123");
        //            if (result.Succeeded)
        //            {
        //                var objCustomer = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).FirstOrDefault();
        //                if (objCustomer != null)
        //                {
        //                    custId = objCustomer.CustomerId + 1;
        //                }
        //                else
        //                {
        //                    custId = Convert.ToDecimal(ConfigurationManager.AppSettings["CustomerId"]);
        //                }

        //                customer.UserID = user.Id;
        //                customer.CustomerId = custId;
        //                var customerdata = Mapper.Map<CustomerModel, Customer>(customer);
        //                InsuranceContext.Customers.Insert(customerdata);
        //                customer.Id = customerdata.Id;
        //            }
        //        }
        //        var policy = (PolicyDetail)Session["PolicyData"];
        //        if (policy != null)
        //        {
        //            if (policy.Id == null || policy.Id == 0)
        //            {
        //                policy.CustomerId = customer.Id;
        //                InsuranceContext.PolicyDetails.Insert(policy);
        //            }
        //        }
        //        var Id = 0;
        //        var vehicle = (RiskDetailModel)Session["VehicleDetail"];
        //        if (vehicle != null)
        //        {
        //            var service = new RiskDetailService();
        //            vehicle.CustomerId = customer.Id;
        //            vehicle.PolicyId = policy.Id;
        //            //var vehical = Mapper.Map<RiskDetailModel, RiskDetailModel>(vehicle);
        //            Id = service.AddVehicleInformation(vehicle);

        //        }

        //        DbEntry = Mapper.Map<SummaryDetailModel, SummaryDetail>(model);
        //        DbEntry.PaymentTermId = Convert.ToInt32(Session["policytermid"]);
        //        DbEntry.VehicleDetailId = Id;
        //        DbEntry.CustomerId = vehicle.CustomerId;
        //        InsuranceContext.SummaryDetails.Insert(DbEntry);
        //    }

        //    return RedirectToAction("PaymentDetail", new { id = DbEntry.Id });
        //}
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


        public ActionResult PaymentDetail(int id)
        {
            var model = new CardDetailModel();
            model.SummaryDetailId = id;
            return View(model);
        }







    }
}