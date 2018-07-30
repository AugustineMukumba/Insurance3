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
            string path = Server.MapPath("~/Content/Countries.txt");
            var countries = System.IO.File.ReadAllText(path);
            var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(countries);
            ViewBag.Countries = resultt.countries;

            string paths = Server.MapPath("~/Content/Cities.txt");
            var cities = System.IO.File.ReadAllText(paths);
            var resultts = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObjects>(cities);
            ViewBag.Cities = resultts.cities;


            if (userLoggedin)
            {
                var customerModel = new CustomerModel();
                var _User = UserManager.FindById(User.Identity.GetUserId().ToString());


                var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{User.Identity.GetUserId().ToString()}'").FirstOrDefault();
                var customerData = (CustomerModel)Session["CustomerDataModal"];

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
                    customerModel.NationalIdentificationNumber = customerData.NationalIdentificationNumber;
                    customerModel.DateOfBirth = customerData.DateOfBirth;
                    customerModel.EmailAddress = customerData.EmailAddress;
                    customerModel.FirstName = customerData.FirstName;
                    customerModel.LastName = customerData.LastName;
                    customerModel.CountryCode = customerData.CountryCode;
                }
                else
                {
                    customerModel.AddressLine1 = _customerData.AddressLine1;
                    customerModel.AddressLine2 = _customerData.AddressLine2;
                    customerModel.City = _customerData.City;
                    customerModel.Id = _customerData.Id;
                    customerModel.Country = _customerData.Country;
                    customerModel.Zipcode = _customerData.Zipcode;
                    customerModel.Gender = _customerData.Gender;
                    customerModel.PhoneNumber = _User.PhoneNumber;
                    customerModel.NationalIdentificationNumber = _customerData.NationalIdentificationNumber;
                    customerModel.DateOfBirth = _customerData.DateOfBirth;
                    customerModel.EmailAddress = _User.Email;
                    customerModel.FirstName = _customerData.FirstName;
                    customerModel.LastName = _customerData.LastName;
                    customerModel.CountryCode = _customerData.Countrycode;
                    customerModel.CustomerId = _customerData.CustomerId;
                    customerModel.IsActive = _customerData.IsActive;
                    customerModel.UserID = _customerData.UserID;
                }
                customerModel.Zipcode = "00263";
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
                    customerModel.NationalIdentificationNumber = customerData.NationalIdentificationNumber;
                    customerModel.DateOfBirth = customerData.DateOfBirth;
                    customerModel.EmailAddress = customerData.EmailAddress;
                    customerModel.FirstName = customerData.FirstName;
                    customerModel.LastName = customerData.LastName;
                    customerModel.CountryCode = customerData.CountryCode;
                }
                return View(customerModel);
            }




        }


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
        public ActionResult RiskDetail(int? id = 0)
        {

            int RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
            var PolicyData = (PolicyDetail)Session["PolicyData"];
            //Id is policyid from Policy detail table
            var viewModel = new RiskDetailModel();
            var service = new VehicleService();
            if (PolicyData != null)
            {
                viewModel.CoverStartDate = PolicyData.StartDate;
                viewModel.CoverEndDate = PolicyData.EndDate;
                ViewBag.VehicleUsage = service.GetVehicleUsage(PolicyData.PolicyName);
            }
            else
            {
                ViewBag.VehicleUsage = service.GetAllVehicleUsage();
            }

            viewModel.NumberofPersons = 0;
            viewModel.AddThirdPartyAmount = 0.00m;
            viewModel.RadioLicenseCost = Convert.ToDecimal(RadioLicenseCosts);
            var makers = service.GetMakers();
            ViewBag.CoverType = service.GetCoverType();
            ViewBag.AgentCommission = service.GetAgentCommission();
            ViewBag.Makers = makers;
            viewModel.isUpdate = false;
            //TempData["Policy"] = service.GetPolicy(id);
            if (makers.Count > 0)
            {
                var model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = model;

            }

            viewModel.NoOfCarsCovered = 1;
            if (Session["VehicleDetails"] != null)
            {
                var list = (List<RiskDetailModel>)Session["VehicleDetails"];
                viewModel.NoOfCarsCovered = list.Count + 1;
            }

            if (id > 0)
            {
                var list = (List<RiskDetailModel>)Session["VehicleDetails"];
                var data = (RiskDetailModel)list[Convert.ToInt32(id - 1)];
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
                    viewModel.NoOfCarsCovered = id;
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
                    viewModel.Id = data.Id;
                    viewModel.ZTSCLevy = data.ZTSCLevy;
                    viewModel.NumberofPersons = data.NumberofPersons;
                    viewModel.PassengerAccidentCover = data.PassengerAccidentCover;
                    viewModel.IsLicenseDiskNeeded = data.IsLicenseDiskNeeded;
                    viewModel.ExcessBuyBack = data.ExcessBuyBack;
                    viewModel.RoadsideAssistance = data.RoadsideAssistance;
                    viewModel.MedicalExpenses = data.MedicalExpenses;
                    viewModel.Addthirdparty = data.Addthirdparty;
                    viewModel.AddThirdPartyAmount = data.AddThirdPartyAmount;
                    viewModel.isUpdate = true;
                    viewModel.vehicleindex = Convert.ToInt32(id);

                    var ser = new VehicleService();
                    var model = ser.GetModel(data.MakeId);
                    ViewBag.Model = model;
                }
            }


            return View(viewModel);
        }

        [HttpPost]
        public ActionResult GenerateQuote(RiskDetailModel model)
        {

            if (model.NumberofPersons == null)
            {
                model.NumberofPersons = 0;
            }

            if (model.AddThirdPartyAmount == null)
            {
                model.AddThirdPartyAmount = 0.00m;
            }

            if (model.isUpdate)
            {
                if (ModelState.IsValid)
                {
                    List<RiskDetailModel> listriskdetailmodel = new List<RiskDetailModel>();
                    if (Session["VehicleDetails"] != null)
                    {
                        List<RiskDetailModel> listriskdetails = (List<RiskDetailModel>)Session["VehicleDetails"];
                        if (listriskdetails != null && listriskdetails.Count > 0)
                        {
                            listriskdetailmodel = listriskdetails;
                        }
                    }

                    listriskdetailmodel[model.vehicleindex - 1] = model;
                    Session["VehicleDetails"] = listriskdetailmodel;
                }

                return RedirectToAction("RiskDetail");
            }
            else
            {

                if (model.chkAddVehicles)
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
                        List<RiskDetailModel> listriskdetailmodel = new List<RiskDetailModel>();
                        if (Session["VehicleDetails"] != null)
                        {
                            List<RiskDetailModel> listriskdetails = (List<RiskDetailModel>)Session["VehicleDetails"];
                            if (listriskdetails != null && listriskdetails.Count > 0)
                            {
                                listriskdetailmodel = listriskdetails;
                            }
                        }

                        listriskdetailmodel.Add(model);
                        Session["VehicleDetails"] = listriskdetailmodel;

                    }

                    return RedirectToAction("RiskDetail");
                }
                else
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
                        List<RiskDetailModel> listriskdetailmodel = new List<RiskDetailModel>();
                        if (Session["VehicleDetails"] != null)
                        {
                            List<RiskDetailModel> listriskdetails = (List<RiskDetailModel>)Session["VehicleDetails"];
                            if (listriskdetails != null && listriskdetails.Count > 0)
                            {
                                listriskdetailmodel = listriskdetails;
                            }
                        }

                        listriskdetailmodel.Add(model);
                        Session["VehicleDetails"] = listriskdetailmodel;

                    }

                    return RedirectToAction("SummaryDetail");
                }
            }
        }

        [HttpPost]
        public JsonResult DeleteVehicle(int? index)
        {

            try
            {
                if (Session["VehicleDetails"] != null)
                {
                    var list = (List<RiskDetailModel>)Session["VehicleDetails"];

                    list.RemoveAt(Convert.ToInt32(index) - 1);

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }


        }

        [HttpGet]
        public JsonResult getVehicleList()
        {
            try
            {
                if (Session["VehicleDetails"] != null)
                {
                    var list = (List<RiskDetailModel>)Session["VehicleDetails"];
                    List<VehicleListModel> vehiclelist = new List<VehicleListModel>();

                    foreach (var item in list)
                    {
                        VehicleListModel obj = new VehicleListModel();
                        obj.make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'").ShortDescription;
                        obj.model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ShortDescription;
                        obj.covertype = InsuranceContext.CoverTypes.Single(item.CoverTypeId).Name;
                        obj.premium = item.Premium.ToString();
                        obj.suminsured = item.SumInsured.ToString();
                        vehiclelist.Add(obj);
                    }

                    return Json(vehiclelist, JsonRequestBehavior.AllowGet);
                }
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpGet]
        public JsonResult getVehicleListbyID(int index)
        {
            try
            {
                if (Session["VehicleDetails"] != null)
                {
                    var list = (List<RiskDetailModel>)Session["VehicleDetails"];
                    List<VehicleListModel> vehiclelist = new List<VehicleListModel>();

                    return Json(vehiclelist[index - 1], JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult SummaryDetail()
        {
            Session["issummaryformvisited"] = true;
            var summarydetail = (SummaryDetailModel)Session["SummaryDetailed"];
            SummaryDetailService SummaryDetailServiceObj = new SummaryDetailService();

            if (summarydetail != null)
            {
                return View(summarydetail);
            }
            var model = new SummaryDetailModel();
            var summary = new SummaryDetailService();
            var vehicle = (List<RiskDetailModel>)Session["VehicleDetails"];// summary.GetVehicleInformation(id);
            model.CarInsuredCount = vehicle.Count;
            model.DebitNote = "INV" + Convert.ToString(SummaryDetailServiceObj.getNewDebitNote());
            model.PaymentMethodId = 1;
            model.PaymentTermId = 1;
            model.ReceiptNumber = "";
            model.SMSConfirmation = false;
            model.TotalPremium = vehicle.Sum(item => item.Premium);// + vehicle.StampDuty + vehicle.ZTSCLevy;
            model.TotalRadioLicenseCost = vehicle.Sum(item => item.RadioLicenseCost);
            model.TotalStampDuty = vehicle.Sum(item => item.StampDuty);
            model.TotalSumInsured = vehicle.Sum(item => item.SumInsured);
            model.TotalZTSCLevies = vehicle.Sum(item => item.ZTSCLevy);
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
            string SummeryofReinsurance = "";
            string SummeryofVehicleInsured = "";
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
            else
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                //var objCustomer = InsuranceContext.Customers.Single(where: $"Userid=@0", parms: new object[] { User.Identity.GetUserId() });
                var number = user.PhoneNumber;
                if (number != customer.PhoneNumber)
                {
                    user.PhoneNumber = customer.PhoneNumber;
                    UserManager.Update(user);
                }
                customer.UserID = User.Identity.GetUserId().ToString();
                var customerdata = Mapper.Map<CustomerModel, Customer>(customer);
                InsuranceContext.Customers.Update(customerdata);
            }


            var policy = (PolicyDetail)Session["PolicyData"];
            if (policy != null)
            {
                if (policy.Id == null || policy.Id == 0)
                {
                    policy.CustomerId = customer.Id;
                    InsuranceContext.PolicyDetails.Insert(policy);
                    Session["PolicyData"] = policy;
                }
                else
                {
                    PolicyDetail policydata = InsuranceContext.PolicyDetails.All(policy.Id.ToString()).FirstOrDefault();
                    policydata.BusinessSourceId = policy.BusinessSourceId;
                    policydata.CurrencyId = policy.CurrencyId;
                    policydata.CustomerId = policy.CustomerId;
                    policydata.EndDate = policy.EndDate;
                    policydata.InsurerId = policy.InsurerId;
                    policydata.IsActive = policy.IsActive;
                    policydata.PolicyName = policy.PolicyName;
                    policydata.PolicyNumber = policy.PolicyNumber;
                    policydata.PolicyStatusId = policy.PolicyStatusId;
                    policydata.RenewalDate = policy.RenewalDate;
                    policydata.StartDate = policy.StartDate;
                    policydata.TransactionDate = policy.TransactionDate;
                    InsuranceContext.PolicyDetails.Update(policydata);
                }

            }
            var Id = 0;
            var listReinsuranceTransaction = new List<ReinsuranceTransaction>();
            var vehicle = (List<RiskDetailModel>)Session["VehicleDetails"];
            if (vehicle != null && vehicle.Count > 0)
            {
                foreach (var item in vehicle.ToList())
                {
                    var _item = item;

                    if (item.Id == null || item.Id == 0)
                    {
                        var service = new RiskDetailService();
                        _item.CustomerId = customer.Id;
                        _item.PolicyId = policy.Id;
                        //var vehical = Mapper.Map<RiskDetailModel, RiskDetailModel>(vehicle);
                        _item.Id = service.AddVehicleInformation(_item);
                        var vehicles = (List<RiskDetailModel>)Session["VehicleDetails"];
                        vehicles[Convert.ToInt32(_item.NoOfCarsCovered) - 1] = _item;
                        Session["VehicleDetails"] = vehicles;

                        ///Reinsurance                      

                        var ReinsuranceCases = InsuranceContext.Reinsurances.All(where: $"Type='Reinsurance'").ToList();
                        var ownRetention = InsuranceContext.Reinsurances.All().Where(x => x.TreatyCode == "OR001").Select(x => x.MaxTreatyCapacity).SingleOrDefault();
                        var ReinsuranceCase = new Reinsurance();

                        foreach (var Reinsurance in ReinsuranceCases)
                        {
                            if (Reinsurance.MinTreatyCapacity <= item.SumInsured && item.SumInsured <= Reinsurance.MaxTreatyCapacity)
                            {
                                ReinsuranceCase = Reinsurance;
                                break;
                            }
                        }

                        if (ReinsuranceCase != null && ReinsuranceCase.MaxTreatyCapacity != null)
                        {
                            var ReinsuranceBroker = InsuranceContext.ReinsuranceBrokers.Single(where: $"ReinsuranceBrokerCode='{ReinsuranceCase.ReinsuranceBrokerCode}'");

                            var reinsurance = new ReinsuranceTransaction();
                            reinsurance.ReinsuranceAmount = _item.SumInsured - ownRetention;
                            reinsurance.ReinsurancePremium = ((reinsurance.ReinsuranceAmount / item.SumInsured) * item.Premium);
                            reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                            reinsurance.ReinsuranceCommission = ((reinsurance.ReinsurancePremium * reinsurance.ReinsuranceCommissionPercentage) / 100);
                            reinsurance.VehicleId = item.Id;
                            reinsurance.ReinsuranceBrokerId = ReinsuranceBroker.Id;
                            reinsurance.TreatyName = ReinsuranceCase.TreatyName;
                            reinsurance.TreatyCode = ReinsuranceCase.TreatyCode;

                            InsuranceContext.ReinsuranceTransactions.Insert(reinsurance);

                            listReinsuranceTransaction.Add(reinsurance);


                            Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                            VehicleModel vehiclemodel = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                            VehicleMake vehiclemake = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'");

                            string vehicledescription = vehiclemodel.ModelDescription + " / " + vehiclemake.MakeDescription;
                            SummeryofReinsurance += "<tr><td>" + Convert.ToString(reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";
                            SummeryofVehicleInsured += "<tr><td>" + vehicledescription + "</td><td>" + item.RegistrationNo + "</td><td>" + (item.CoverTypeId == 1 ? eCoverType.Comprehensive.ToString() : (item.CoverTypeId == 2 ? eCoverType.ThirdParty.ToString() : eCoverType.FullThirdParty.ToString())) + "</td><td>" + Convert.ToString(item.SumInsured) + "</td><td>" + Convert.ToString(item.Premium) + "</td><td>" + Convert.ToString(reinsurance.Id) + "</td></tr>";


                        }

                    }
                    else
                    {
                        VehicleDetail Vehicledata = InsuranceContext.VehicleDetails.All(item.Id.ToString()).FirstOrDefault();
                        Vehicledata.AgentCommissionId = item.AgentCommissionId;
                        Vehicledata.ChasisNumber = item.ChasisNumber;
                        Vehicledata.CoverEndDate = item.CoverEndDate;
                        Vehicledata.CoverNoteNo = item.CoverNoteNo;
                        Vehicledata.CoverStartDate = item.CoverStartDate;
                        Vehicledata.CoverTypeId = item.CoverTypeId;
                        Vehicledata.CubicCapacity = item.CubicCapacity;
                        Vehicledata.EngineNumber = item.EngineNumber;
                        Vehicledata.Excess = item.Excess;
                        Vehicledata.ExcessType = item.ExcessType;
                        Vehicledata.MakeId = item.MakeId;
                        Vehicledata.ModelId = item.ModelId;
                        Vehicledata.NoOfCarsCovered = item.NoOfCarsCovered;
                        Vehicledata.OptionalCovers = item.OptionalCovers;
                        Vehicledata.PolicyId = item.PolicyId;
                        Vehicledata.Premium = item.Premium;
                        Vehicledata.RadioLicenseCost = item.RadioLicenseCost;
                        Vehicledata.Rate = item.Rate;
                        Vehicledata.RegistrationNo = item.RegistrationNo;
                        Vehicledata.StampDuty = item.StampDuty;
                        Vehicledata.SumInsured = item.SumInsured;
                        Vehicledata.VehicleColor = item.VehicleColor;
                        Vehicledata.VehicleUsage = item.VehicleUsage;
                        Vehicledata.VehicleYear = item.VehicleYear;
                        Vehicledata.ZTSCLevy = item.ZTSCLevy;
                        Vehicledata.Addthirdparty = item.Addthirdparty;
                        Vehicledata.AddThirdPartyAmount = item.AddThirdPartyAmount;
                        Vehicledata.PassengerAccidentCover = item.PassengerAccidentCover;
                        Vehicledata.ExcessBuyBack = item.ExcessBuyBack;
                        Vehicledata.RoadsideAssistance = item.RoadsideAssistance;
                        Vehicledata.MedicalExpenses = item.MedicalExpenses;
                        Vehicledata.NumberofPersons = item.NumberofPersons;
                        Vehicledata.IsLicenseDiskNeeded = item.IsLicenseDiskNeeded;



                        InsuranceContext.VehicleDetails.Update(Vehicledata);
                        var _summary = (SummaryDetailModel)Session["SummaryDetailed"];


                        var ReinsuranceCases = InsuranceContext.Reinsurances.All(where: $"Type='Reinsurance'").ToList();
                        var ownRetention = InsuranceContext.Reinsurances.All().Where(x => x.TreatyCode == "OR001").Select(x => x.MaxTreatyCapacity).SingleOrDefault();
                        var ReinsuranceCase = new Reinsurance();

                        foreach (var Reinsurance in ReinsuranceCases)
                        {
                            if (Reinsurance.MinTreatyCapacity <= item.SumInsured && item.SumInsured <= Reinsurance.MaxTreatyCapacity)
                            {
                                ReinsuranceCase = Reinsurance;
                                break;
                            }
                        }

                        if (ReinsuranceCase != null && ReinsuranceCase.MaxTreatyCapacity != null)
                        {
                            var ReinsuranceBroker = InsuranceContext.ReinsuranceBrokers.Single(where: $"ReinsuranceBrokerCode='{ReinsuranceCase.ReinsuranceBrokerCode}'");

                            var summaryid = _summary.Id;
                            var vehicleid = item.Id;
                            var ReinsuranceTransactions = InsuranceContext.ReinsuranceTransactions.Single(where: $"SummaryDetailId={_summary.Id} and VehicleId={item.Id}");
                            //var _reinsurance = new ReinsuranceTransaction();
                            ReinsuranceTransactions.ReinsuranceAmount = _item.SumInsured - ownRetention;
                            ReinsuranceTransactions.ReinsurancePremium = ((ReinsuranceTransactions.ReinsuranceAmount / item.SumInsured) * item.Premium);
                            ReinsuranceTransactions.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                            ReinsuranceTransactions.ReinsuranceCommission = ((ReinsuranceTransactions.ReinsurancePremium * ReinsuranceTransactions.ReinsuranceCommissionPercentage) / 100);//Convert.ToDecimal(defaultReInsureanceBroker.Commission);
                            ReinsuranceTransactions.ReinsuranceBrokerId = ReinsuranceBroker.Id;

                            InsuranceContext.ReinsuranceTransactions.Update(ReinsuranceTransactions);
                        }
                        else
                        {
                            var ReinsuranceTransactions = InsuranceContext.ReinsuranceTransactions.Single(where: $"SummaryDetailId={_summary.Id} and VehicleId={item.Id}");
                            if (ReinsuranceTransactions != null)
                            {
                                InsuranceContext.ReinsuranceTransactions.Delete(ReinsuranceTransactions);
                            }
                        }


                    }
                }
            }

            var summary = (SummaryDetailModel)Session["SummaryDetailed"];
            var DbEntry = Mapper.Map<SummaryDetailModel, SummaryDetail>(model);



            if (summary != null)
            {

                if (summary.Id == null || summary.Id == 0)
                {
                    DbEntry.PaymentTermId = Convert.ToInt32(Session["policytermid"]);
                    //DbEntry.VehicleDetailId = vehicle[0].Id;
                    DbEntry.CustomerId = vehicle[0].CustomerId;
                    InsuranceContext.SummaryDetails.Insert(DbEntry);
                    model.Id = DbEntry.Id;
                    Session["SummaryDetailed"] = model;
                }
                else
                {
                    SummaryDetail summarydata = InsuranceContext.SummaryDetails.All(summary.Id.ToString()).FirstOrDefault();

                    summarydata.PaymentTermId = Convert.ToInt32(Session["policytermid"]);
                    //summarydata.VehicleDetailId = vehicle[0].Id;
                    summarydata.CustomerId = vehicle[0].CustomerId;

                    InsuranceContext.SummaryDetails.Update(summarydata);
                }



                if (listReinsuranceTransaction != null && listReinsuranceTransaction.Count > 0)
                {
                    foreach (var item in listReinsuranceTransaction)
                    {
                        var InsTransac = InsuranceContext.ReinsuranceTransactions.Single(item.Id);
                        InsTransac.SummaryDetailId = summary.Id;
                        InsuranceContext.ReinsuranceTransactions.Update(InsTransac);
                    }
                }

            }


            if (vehicle != null && vehicle.Count > 0 && summary.Id != null && summary.Id > 0)
            {
                var SummaryDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summary.Id}").ToList();

                if (SummaryDetails != null && SummaryDetails.Count > 0)
                {
                    foreach (var item in SummaryDetails)
                    {
                        InsuranceContext.SummaryVehicleDetails.Delete(item);
                    }
                }

                foreach (var item in vehicle.ToList())
                {

                    var summarydetails = new SummaryVehicleDetail();
                    summarydetails.SummaryDetailId = summary.Id;
                    summarydetails.VehicleDetailsId = item.Id;
                    InsuranceContext.SummaryVehicleDetails.Insert(summarydetails);


                }

            }

            if (listReinsuranceTransaction != null && listReinsuranceTransaction.Count > 0)
            {
                var user = UserManager.FindById(customer.UserID);
                Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
                var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
                var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == summary.PaymentTermId);
                string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/Reinsurance_Admin.cshtml";
                string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
                var Body = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", policy.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", policy.StartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##SummeryofReinsurance##", SummeryofReinsurance).Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured).Replace("##PostalAddress##", customer.Zipcode);
                objEmailService.SendEmail("ankit.dhiman@kindlebit.com", "", "", "Reinsurance Case: " + policy.PolicyNumber.ToString(), Body, null);
            }



            if (model.PaymentMethodId == 1)
                return RedirectToAction("SaveDetailList", "Paypal", new { id = DbEntry.Id });
            if (model.PaymentMethodId == 3)
                return RedirectToAction("InitiatePaynowTransaction", "Paypal", new { id = DbEntry.Id, TotalPremium = Convert.ToString(summary.TotalPremium), PolicyNumber = policy.PolicyNumber, Email = customer.EmailAddress });
            else
                return RedirectToAction("PaymentDetail", new { id = DbEntry.Id });
        }

        [HttpPost]
        public JsonResult CalculatePremium(int vehicleUsageId, decimal sumInsured, int coverType, int excessType, decimal excess, decimal? AddThirdPartyAmount, int NumberofPersons, Boolean Addthirdparty, Boolean PassengerAccidentCover, Boolean ExcessBuyBack, Boolean RoadsideAssistance, Boolean MedicalExpenses, decimal? RadioLicenseCost, Boolean IncludeRadioLicenseCost)
        {
            var policytermid = (int)Session["policytermid"];
            JsonResult json = new JsonResult();
            var quote = new QuoteLogic();
            var typeCover = eCoverType.Comprehensive;
            if (coverType == 2)
            {
                typeCover = eCoverType.ThirdParty;
            }
            if (coverType == 3)
            {
                typeCover = eCoverType.FullThirdParty;
            }
            var eexcessType = eExcessType.Percentage;
            if (excessType == 2)
            {
                eexcessType = eExcessType.FixedAmount;
            }
            var premium = quote.CalculatePremium(vehicleUsageId, sumInsured, typeCover, eexcessType, excess, policytermid, AddThirdPartyAmount, NumberofPersons, Addthirdparty, PassengerAccidentCover, ExcessBuyBack, RoadsideAssistance, MedicalExpenses, RadioLicenseCost, IncludeRadioLicenseCost);
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
            var cardDetails = (CardDetailModel)Session["CardDetail"];
            if (cardDetails == null)
            {
                cardDetails = new CardDetailModel();
            }
            cardDetails.SummaryDetailId = id;
            return View(cardDetails);
        }
        public PartialViewResult Addvehicle(int id = 0)
        {



            return PartialView();
        }



        public class Country
        {
            public string code { get; set; }
            public string name { get; set; }
        }

        public class RootObject
        {
            public List<Country> countries { get; set; }
        }

        public class City
        {
            public string name { get; set; }
        }

        public class RootObjects
        {
            public List<City> cities { get; set; }
        }


    }
}



