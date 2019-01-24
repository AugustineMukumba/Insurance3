﻿using System;
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
using PayPal.Api;
using System.Web.Script.Serialization;
using System.Web.Configuration;

namespace InsuranceClaim.Controllers
{
    public class RenewController : Controller
    {
        private ApplicationUserManager _userManager;

        string AdminEmail = WebConfigurationManager.AppSettings["AdminEmail"];
        string ZimnatEmail = WebConfigurationManager.AppSettings["ZimnatEmail"];
        public RenewController()
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

        public ActionResult Index(int? vehicleid)
        {
            Session["RenewVehicleId"] = vehicleid;
            CustomerModel custdata = new CustomerModel();
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            string path = Server.MapPath("~/Content/Countries.txt");
            var countries = System.IO.File.ReadAllText(path);
            var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(countries);
            ViewBag.Cities = InsuranceContext.Cities.All();
            ViewBag.Countries = resultt.countries.OrderBy(x => x.code.Replace("+", ""));



            var vehicledetails = InsuranceContext.VehicleDetails.Single(where: $"Id = '{vehicleid}'");
            var customerdetail = InsuranceContext.Customers.Single(where: $"Id= '{vehicledetails.CustomerId}'");
            if (customerdetail != null)
            {

                custdata = Mapper.Map<Customer, CustomerModel>(customerdetail);
                var dbUser = UserManager.Users.FirstOrDefault(c => c.Id == customerdetail.UserID);
                if (dbUser != null)
                {
                    custdata.EmailAddress = dbUser.Email;
                }

            }

            return View(custdata);
        }

        [HttpPost]
        public async Task<JsonResult> SaveCustomerData(CustomerModel model, string buttonUpdate)
        {
            if (ModelState.IsValid)
            {
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                if (userLoggedin)
                {
                    var email = LoggedUserEmail();

                    if (email == model.EmailAddress)
                    {
                        return Json(new { IsError = false, error = "Staff and customer email can not be same" }, JsonRequestBehavior.AllowGet);
                    }

                    //After test  Change the Role(web User)
                    if (User.IsInRole("Administrator"))
                    {
                        Session["ReCustomerDataModal"] = model;
                        return Json(new { IsError = true, error = "" }, JsonRequestBehavior.AllowGet);

                    }
                }
            }
            return Json(new { IsError = false, error = TempData["ErrorMessage"].ToString() }, JsonRequestBehavior.AllowGet);
        }
        public string LoggedUserEmail()
        {
            string email = "";
            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            if (_userLoggedin)
            {
                var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                email = _User.Email;
            }


            return email;

        }


        public ActionResult RiskDetail(int? Id)
        {

            var vehicleId = (Int32)Session["RenewVehicleId"];
            CustomerModel custdata = new CustomerModel();
            //get All PolicyDetail in session
            var policyid = InsuranceContext.VehicleDetails.Single(vehicleId).PolicyId;
            var policy = InsuranceContext.PolicyDetails.Single(policyid);
            Session["RenewVehiclePolicy"] = policy;
            //VehicleDetailId
            var lapsedvehicleid = (Int32)Session["RenewVehicleId"];
            var SummaryDetailId = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId = {lapsedvehicleid}").SummaryDetailId;
            var summary = InsuranceContext.SummaryDetails.Single(SummaryDetailId);
            int RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
            var LapsedVehiclePolicy = (PolicyDetail)Session["RenewVehiclePolicy"];
            //Id is policyid from Policy detail table
            var viewModels = new RiskDetailModel();
            //HistoryVehicleDetailModel viewRenewModel = new HistoryVehicleDetailModel();
            var service = new VehicleService();

            ViewBag.VehicleUsage = service.GetAllVehicleUsage();

            viewModels.NumberofPersons = 0;
            viewModels.AddThirdPartyAmount = 0.00m;
            viewModels.RadioLicenseCost = Convert.ToDecimal(RadioLicenseCosts);
            var makers = service.GetMakers();
            ViewBag.CoverType = service.GetCoverType();

            ViewBag.AgentCommission = service.GetAgentCommission();
            var data1 = (from p in InsuranceContext.BusinessSources.All().ToList()
                         join f in InsuranceContext.SourceDetails.All().ToList()
                         on p.Id equals f.BusinessId
                         select new
                         {
                             Value = f.Id,
                             Text = f.FirstName + " " + f.LastName + " - " + p.Source
                         }).ToList();

            List<SelectListItem> listdata = new List<SelectListItem>();
            foreach (var item in data1)
            {
                SelectListItem sli = new SelectListItem();
                sli.Value = Convert.ToString(item.Value);
                sli.Text = item.Text;
                listdata.Add(sli);
            }
            ViewBag.Sources = new SelectList(listdata, "Value", "Text");



            ViewBag.Currencies = InsuranceContext.Currencies.All();

            ViewBag.Makers = makers;
            viewModels.isUpdate = false;
            ViewBag.Products = InsuranceContext.Products.All(where: "Active = 'True' or Active is null").ToList();
            ViewBag.PaymentTermId = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is null").ToList();

            var eExcessTypeData = from eExcessType e in Enum.GetValues(typeof(eExcessType))
                                  select new
                                  {
                                      ID = (int)e,
                                      Name = e.ToString()
                                  };

            ViewBag.eExcessTypeData = new SelectList(eExcessTypeData, "ID", "Name");
            if (makers.Count > 0)
            {
                var model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = model;

            }

            viewModels.NoOfCarsCovered = 1;

            if (vehicleId > 0)
            {
                var data = InsuranceContext.VehicleDetails.Single(vehicleId);

                if (data != null)
                {
                    viewModels.AgentCommissionId = data.AgentCommissionId;
                    viewModels.ChasisNumber = data.ChasisNumber;
                    viewModels.CoverEndDate = data.CoverEndDate;
                    viewModels.CoverNoteNo = data.CoverNoteNo;
                    viewModels.CoverStartDate = data.CoverStartDate;
                    viewModels.CoverTypeId = data.CoverTypeId;
                    viewModels.CubicCapacity = (int)Math.Round(data.CubicCapacity.Value, 0);
                    viewModels.CustomerId = data.CustomerId;
                    viewModels.EngineNumber = data.EngineNumber;
                    viewModels.Excess = (int)Math.Round(data.Excess, 0);
                    viewModels.ExcessType = data.ExcessType;
                    viewModels.MakeId = data.MakeId;
                    viewModels.ModelId = data.ModelId;

                    viewModels.NoOfCarsCovered = data.NoOfCarsCovered;
                    viewModels.OptionalCovers = data.OptionalCovers;
                    viewModels.PolicyId = data.PolicyId;
                    viewModels.Premium = data.Premium;
                    viewModels.RadioLicenseCost = (int)Math.Round(data.RadioLicenseCost == null ? 0 : data.RadioLicenseCost.Value, 0);
                    viewModels.Rate = data.Rate;
                    viewModels.RegistrationNo = data.RegistrationNo;
                    viewModels.StampDuty = data.StampDuty;
                    viewModels.SumInsured = (int)Math.Round(data.SumInsured == null ? 0 : data.SumInsured.Value, 0);
                    viewModels.VehicleColor = data.VehicleColor;
                    viewModels.VehicleUsage = data.VehicleUsage;
                    viewModels.VehicleYear = data.VehicleYear;


                    viewModels.ZTSCLevy = data.ZTSCLevy;
                    viewModels.NumberofPersons = data.NumberofPersons;
                    viewModels.PassengerAccidentCover = data.PassengerAccidentCover;
                    viewModels.IsLicenseDiskNeeded = Convert.ToBoolean(data.IsLicenseDiskNeeded);
                    viewModels.ExcessBuyBack = data.ExcessBuyBack;
                    viewModels.RoadsideAssistance = data.RoadsideAssistance;
                    viewModels.MedicalExpenses = data.MedicalExpenses;
                    viewModels.Addthirdparty = data.Addthirdparty;
                    viewModels.InsuranceId = data.InsuranceId;

                    viewModels.AddThirdPartyAmount = Math.Round(Convert.ToDecimal(data.AddThirdPartyAmount), 2);
                    viewModels.ExcessAmount = Math.Round(Convert.ToDecimal(data.ExcessAmount), 2);
                    viewModels.ExcessBuyBackAmount = Math.Round(Convert.ToDecimal(data.ExcessBuyBackAmount), 2);
                    viewModels.MedicalExpensesAmount = Math.Round(Convert.ToDecimal(data.MedicalExpensesAmount), 2);
                    viewModels.PassengerAccidentCoverAmount = Math.Round(Convert.ToDecimal(data.PassengerAccidentCoverAmount), 2);
                    viewModels.PassengerAccidentCoverAmountPerPerson = Math.Round(Convert.ToDecimal(data.PassengerAccidentCoverAmountPerPerson), 2);
                    viewModels.RoadsideAssistanceAmount = Math.Round(Convert.ToDecimal(data.RoadsideAssistanceAmount), 2);

                    //viewRenewModel.ExcessAmount = Math.Round(Convert.ToDecimal(data.ExcessAmount), 2);
                    viewModels.MedicalExpensesPercentage = Math.Round(Convert.ToDecimal(data.MedicalExpensesPercentage), 2);
                    viewModels.ExcessBuyBackPercentage = Math.Round(Convert.ToDecimal(data.ExcessBuyBackPercentage), 2);
                    viewModels.RoadsideAssistancePercentage = Math.Round(Convert.ToDecimal(data.RoadsideAssistancePercentage), 2);
                    viewModels.isUpdate = false;

                    viewModels.vehicleindex = Convert.ToInt32(vehicleId);
                    viewModels.PaymentTermId = data.PaymentTermId;
                    viewModels.ProductId = data.ProductId;
                    viewModels.IncludeRadioLicenseCost = Convert.ToBoolean(data.IncludeRadioLicenseCost);
                    viewModels.RenewalDate = Convert.ToDateTime(data.RenewalDate);
                    viewModels.CustomerId = data.CustomerId;
                    viewModels.PolicyId = data.PolicyId;
                    viewModels.AnnualRiskPremium = data.AnnualRiskPremium;
                    viewModels.TermlyRiskPremium = data.TermlyRiskPremium;
                    viewModels.QuaterlyRiskPremium = data.QuaterlyRiskPremium;
                    viewModels.Discount = data.Discount;
                    viewModels.VehicleLicenceFee = Convert.ToDecimal(data.VehicleLicenceFee);
                    viewModels.BalanceAmount = data.BalanceAmount;
                    viewModels.TransactionDate = Convert.ToDateTime(data.TransactionDate);
                    viewModels.Id = data.Id;

                    viewModels.BusinessSourceDetailId = data.BusinessSourceDetailId;
                    viewModels.CurrencyId = data.CurrencyId;

                    var ser = new VehicleService();
                    var model = ser.GetModel(data.MakeId);
                    ViewBag.Model = model;
                }
            }


            return View(viewModels);
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
                // model.Id = 0;


                // List<RiskDetailModel> listriskdetailmodel = new List<RiskDetailModel>();
                if (Session["RenewVehicleDetails"] != null)
                {
                    //List<RiskDetailModel> listriskdetails = (List<RiskDetailModel>)Session["RenewVehicleDetails"];
                    //if (listriskdetails != null && listriskdetails.Count > 0)
                    //{
                    //    listriskdetailmodel = listriskdetails;
                    //}
                }
                //model.Id = 0;
                //listriskdetailmodel.Add(model);
                Session["RenewVehicleDetails"] = model;

            }
            return RedirectToAction("SummaryDetail");
        }

        public ActionResult SummaryDetail(int summaryDetailId = 0)
        {
            var Id = (Int32)Session["RenewVehicleId"];
            ViewBag.vehicleid = Id;
            var vehicle = (RiskDetailModel)Session["RenewVehicleDetails"];// summary.GetVehicleInformation(id);
            var model = new SummaryDetailModel();
            var summarydetail = (SummaryDetailModel)Session["ReSummaryDetailed"];
            SummaryDetailService SummaryDetailServiceObj = new SummaryDetailService();

            ViewBag.SummaryDetailId = summaryDetailId;
            var role = "";
            if (System.Web.HttpContext.Current.User.Identity.GetUserId() != null)
            {
                role = UserManager.GetRoles(System.Web.HttpContext.Current.User.Identity.GetUserId()).FirstOrDefault();
            }

            ViewBag.CurrentUserRole = role;

            var DiscountSettings = InsuranceContext.Settings.Single(where: $"keyname='Discount On Renewal'");
            model.CarInsuredCount = 1;
            model.DebitNote = "INV" + Convert.ToString(SummaryDetailServiceObj.getNewDebitNote());
            //default selection 

            model.PaymentMethodId = 1;

            //if (User.IsInRole("Staff"))
            //{
            //    model.PaymentMethodId = 1;
            //}
            //else
            //{
            //    model.PaymentMethodId = 2;
            //}

            model.PaymentTermId = 1;
            model.ReceiptNumber = "";
            model.SMSConfirmation = false;
            model.TotalPremium = 0.00m;
            model.TotalRadioLicenseCost = 0.00m;
            model.Discount = 0.00m;


            //foreach (var item in vehicle)
            //{
            //    model.TotalPremium += item.Premium + item.ZTSCLevy + item.StampDuty + item.VehicleLicenceFee;
            //    if (item.IncludeRadioLicenseCost)
            //    {
            //        model.TotalPremium += item.RadioLicenseCost;
            //        model.TotalRadioLicenseCost += item.RadioLicenseCost;
            //    }
            //    model.Discount += item.Discount;
            //}
            model.TotalPremium = vehicle.Premium + vehicle.ZTSCLevy + vehicle.StampDuty + vehicle.VehicleLicenceFee;
            if (vehicle.IncludeRadioLicenseCost)
            {
                model.TotalPremium = vehicle.RadioLicenseCost;
                model.TotalRadioLicenseCost = vehicle.RadioLicenseCost;
            }
            model.Discount = vehicle.Discount;
            model.TotalRadioLicenseCost = Math.Round(Convert.ToDecimal(model.TotalRadioLicenseCost), 2);
            model.Discount = Math.Round(Convert.ToDecimal(model.Discount), 2);
            model.TotalPremium = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);

            model.TotalStampDuty = vehicle.StampDuty;
            model.TotalSumInsured = vehicle.SumInsured;
            model.TotalZTSCLevies = vehicle.ZTSCLevy;
            model.ExcessBuyBackAmount = vehicle.ExcessBuyBackAmount;
            model.MedicalExpensesAmount = vehicle.MedicalExpensesAmount;
            model.PassengerAccidentCoverAmount = vehicle.PassengerAccidentCoverAmount;
            model.RoadsideAssistanceAmount = vehicle.RoadsideAssistanceAmount;
            model.ExcessAmount = vehicle.ExcessAmount;


            //model.TotalStampDuty = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.StampDuty)), 2);
            //model.TotalSumInsured = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.SumInsured)), 2);
            //model.TotalZTSCLevies = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.ZTSCLevy)), 2);
            //model.ExcessBuyBackAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.ExcessBuyBackAmount)), 2);
            //model.MedicalExpensesAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.MedicalExpensesAmount)), 2);
            //model.PassengerAccidentCoverAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.PassengerAccidentCoverAmount)), 2);
            //model.RoadsideAssistanceAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.RoadsideAssistanceAmount)), 2);
            //model.ExcessAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.ExcessAmount)), 2);
            model.AmountPaid = 0.00m;
            model.MaxAmounttoPaid = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);
            //var vehiclewithminpremium = vehicle.OrderBy(x => x.Premium).FirstOrDefault();
            model.AmountPaid = Convert.ToDecimal(model.TotalPremium);
            model.BalancePaidDate = DateTime.Now;
            model.Notes = "";
            if (Session["RenewVehiclePolicy"] != null)
            {
                var PolicyData = (PolicyDetail)Session["RenewVehiclePolicy"];
                model.InvoiceNumber = PolicyData.PolicyNumber;
            }

            if (summarydetail != null)
            {
                model.Id = summarydetail.Id;
            }

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
        public async Task<ActionResult> SubmitPlan1(SummaryDetailModel model)
        {
            try
            {
                if (model != null)
                {
                    //if (ModelState.IsValid && (model.AmountPaid >= model.MinAmounttoPaid && model.AmountPaid <= model.MaxAmounttoPaid))

                    int CustomerUniquId = 0;
                    //if (User.IsInRole("Administrator"))
                    //{
                    //    TempData["SucessMsg"] = "Admin can not create policy.";
                    //    return RedirectToAction("SummaryDetail");
                    //}


                    TempData["ErroMsg"] = null;
                    if (User.IsInRole("Staff") && model.PaymentMethodId == 1)
                    {
                        //  ModelState.Remove("InvoiceNumber");
                        if (string.IsNullOrEmpty(model.InvoiceNumber))
                        {
                            TempData["ErroMsg"] = "Please enter invoice number.";
                            return RedirectToAction("SummaryDetail");
                        }
                    }


                    if (ModelState.IsValid)
                    {
                        Insurance.Service.ICEcashService ICEcashService = new Insurance.Service.ICEcashService();
                        List<RiskDetailModel> list = new List<RiskDetailModel>();
                        string PartnerToken = "";

                        #region update  TPIQuoteUpdate
                        var customerDetails = new Customer();

                        var policyDetils = new PolicyDetail();

                        var customerEmail = "";
                        var policyNum = "";
                        var InsuranceID = "";
                        var vichelDetails = new VehicleDetail();


                        if (model.Id != 0)
                        {
                            model.CustomSumarryDetilId = model.Id;
                        }

                        #endregion

                        #region Add All info to database

                        //var vehicle = (RiskDetailModel)Session["VehicleDetail"];
                        Session["ReSummaryDetailed"] = model;
                        string SummeryofReinsurance = "";
                        string SummeryofVehicleInsured = "";
                        bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                        var customer = (CustomerModel)Session["ReCustomerDataModal"];


                        var role = "";

                        if (System.Web.HttpContext.Current.User.Identity.GetUserId() != null)
                        {
                            role = UserManager.GetRoles(System.Web.HttpContext.Current.User.Identity.GetUserId()).FirstOrDefault();

                        }

                        var userDetials = UserManager.FindByEmail(customer.EmailAddress);

                        if (userDetials == null)
                        {
                            customer.Id = 0;
                        }

                        //if user staff

                        if (role == "Staff" || role == "Administrator")
                        {
                            // check if email id exist in user table
                            var user = UserManager.FindByEmail(customer.EmailAddress);

                            // if exist - get customer id from xcustomer table and set customer.Id in Customer object
                            if (user != null && user.Id != null)
                            {
                                var customerDetials = InsuranceContext.Customers.Single(where: $"UserID = '" + user.Id + "'");

                                if (customerDetials != null)
                                {
                                    customer.Id = customerDetials.Id;

                                    CustomerUniquId = customerDetials.Id;


                                    // need to do work
                                    //if (btnSendQuatation != "" && model.Id != 0)
                                    //{
                                    //    var SummaryDetails = InsuranceContext.SummaryDetails.Single(where: $"CustomerId={customer.Id} and isQuotation = 'True'");
                                    //    if (SummaryDetails != null)
                                    //    {
                                    //        TempData["SucessMsg"] = customer.FirstName + " " + customer.LastName + " Quotation alredy exist, please edit existing.";
                                    //        return RedirectToAction("SummaryDetail");
                                    //    }
                                    //}


                                }

                            }
                        }


                        if (!userLoggedin)  // create new user without logged in
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
                                        try
                                        {
                                            var roleresult = UserManager.AddToRole(user.Id, "Web Customer"); // for web user
                                        }
                                        catch (Exception ex)
                                        {
                                        }

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
                        else if (userLoggedin && userDetials == null) //  when user is logged in
                        {

                            if (customer.Id == null || customer.Id == 0)
                            {
                                decimal custId = 0;


                                var user = new ApplicationUser { UserName = customer.EmailAddress, Email = customer.EmailAddress, PhoneNumber = customer.PhoneNumber };
                                var result = await UserManager.CreateAsync(user, "Geninsure@123");
                                if (result.Succeeded)
                                {
                                    try
                                    {
                                        var roleresult = UserManager.AddToRole(user.Id, "Customer");
                                    }
                                    catch (Exception ex)
                                    {
                                    }

                                    var objCustomer = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).FirstOrDefault();



                                    //Query
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
                        else if (userLoggedin && userDetials != null && customer.Id == 0) //  when user is logged in
                        {
                            decimal custId = 0;

                            var objCustomer = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).FirstOrDefault();
                            //Query
                            if (objCustomer != null)
                            {
                                custId = objCustomer.CustomerId + 1;
                            }
                            else
                            {
                                custId = Convert.ToDecimal(ConfigurationManager.AppSettings["CustomerId"]);
                            }

                            customer.UserID = userDetials.Id;
                            customer.CustomerId = custId;
                            var customerdata = Mapper.Map<CustomerModel, Customer>(customer);
                            InsuranceContext.Customers.Insert(customerdata);
                            customer.Id = customerdata.Id;


                        }
                        else
                        {
                            //  var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                            var user = UserManager.FindByEmail(customer.EmailAddress);
                            //var objCustomer = InsuranceContext.Customers.Single(where: $"Userid=@0", parms: new object[] { User.Identity.GetUserId() });

                            if (user != null)
                            {
                                var number = user.PhoneNumber;
                                if (number != customer.PhoneNumber)
                                {
                                    user.PhoneNumber = customer.PhoneNumber;
                                    UserManager.Update(user);
                                }
                                // customer.UserID = User.Identity.GetUserId().ToString();

                                var customerDetials = InsuranceContext.Customers.Single(where: $"UserID = '" + user.Id + "'");

                                if (customerDetials != null)
                                {
                                    customer.UserID = user.Id;
                                    customer.CustomerId = customerDetials.CustomerId;
                                    var customerdata = Mapper.Map<CustomerModel, Customer>(customer);

                                    if (customerdata.CustomerId == 0) // if exting record belong to 0
                                    {
                                        customerdata.CustomerId = customerdata.Id;
                                    }


                                    InsuranceContext.Customers.Update(customerdata);
                                }

                            }
                        }


                        var policy = (PolicyDetail)Session["RenewVehiclePolicy"];


                        // Genrate new policy number


                        // end genrate policy number


                        if (policy != null)
                        {
                            if (policy.Id == null || policy.Id == 0)
                            {
                                policy.CustomerId = customer.Id;
                                policy.StartDate = null;
                                policy.EndDate = null;
                                policy.TransactionDate = null;
                                policy.RenewalDate = null;
                                policy.RenewalDate = null;
                                policy.StartDate = null;
                                policy.TransactionDate = null;
                                policy.CreatedBy = customer.Id;
                                policy.CreatedOn = DateTime.Now;
                                InsuranceContext.PolicyDetails.Insert(policy);

                                Session["RenewVehiclePolicy"] = policy;
                            }
                            else
                            {
                                PolicyDetail policydata = InsuranceContext.PolicyDetails.All(policy.Id.ToString()).FirstOrDefault();
                                policydata.BusinessSourceId = policy.BusinessSourceId;
                                policydata.CurrencyId = policy.CurrencyId;
                                // policydata.CustomerId = policy.CustomerId;
                                policydata.CustomerId = customer.Id;
                                policydata.EndDate = null;
                                policydata.InsurerId = policy.InsurerId;
                                policydata.IsActive = policy.IsActive;
                                policydata.PolicyName = policy.PolicyName;
                                policydata.PolicyNumber = policy.PolicyNumber;
                                policydata.PolicyStatusId = policy.PolicyStatusId;
                                policydata.RenewalDate = null;
                                policydata.StartDate = null;
                                policydata.TransactionDate = null;
                                policy.ModifiedBy = customer.Id;
                                policy.ModifiedOn = DateTime.Now;
                                InsuranceContext.PolicyDetails.Update(policydata);
                            }

                        }
                        var Id = 0;
                        var listReinsuranceTransaction = new List<ReinsuranceTransaction>();
                        var vehicle = (RiskDetailModel)Session["RenewVehicleDetails"];
                        var summary = (SummaryDetailModel)Session["ReSummaryDetailed"];


                        if (vehicle != null)
                        {
                            var _item = vehicle;

                            //List<RiskDetailModel> objVehicles = new List<RiskDetailModel>();
                            ////objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo });
                            //objVehicles.Add(new RiskDetailModel { RegistrationNo = _item.RegistrationNo, PaymentTermId = Convert.ToInt32(_item.PaymentTermId) });
                            //var  tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
                            //ResultRootObject quoteresponse = ICEcashService.checkVehicleExists(objVehicles, tokenObject.Response.PartnerToken, tokenObject.PartnerReference);

                            //if (quoteresponse.Response.Result == 0)
                            //{
                            //    response.message = quoteresponse.Response.Quotes[0].Message;
                            //}
                            //else
                            //{
                            //    response.Data = quoteresponse;
                            //}


                            var vehicelDetails = InsuranceContext.VehicleDetails.Single(where: $"policyid= '{policy.Id}' and RegistrationNo= '{_item.RegistrationNo}'");

                            if (vehicelDetails != null)
                            {
                                // item.Id = vehicelDetails.Id;
                                vehicle.Id = 0;
                                vehicelDetails.IsActive = false;
                                vehicelDetails.RenewPolicyNumber = policy.PolicyNumber;
                                vehicelDetails.isLapsed = true;
                                InsuranceContext.VehicleDetails.Update(vehicelDetails);


                                var SummaryVehicalDetails = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId={vehicelDetails.Id}");
                                if (SummaryVehicalDetails != null)
                                    summary.Id = SummaryVehicalDetails.SummaryDetailId;

                            }


                            // Get renew policy number

                            int policyLastSequence = 0;
                            string[] splitPolicyNumber = policy.PolicyNumber.Split('-');

                            if (splitPolicyNumber.Length > 1)
                            {
                                policyLastSequence = Convert.ToInt32(splitPolicyNumber[1]);
                                policyLastSequence += 1;
                            }
                            string reNewPolicyNumber  = splitPolicyNumber[0] + "-" + policyLastSequence;


                            if (vehicle.Id == null || vehicle.Id == 0)
                            {
                                var service = new RiskDetailService();
                                _item.CustomerId = customer.Id;
                                _item.PolicyId = policy.Id;
                                _item.RenewPolicyNumber = reNewPolicyNumber;
                                //   _item.InsuranceId = model.InsuranceId;
                                //if (model.AmountPaid < model.TotalPremium)
                                //{
                                //    _item.BalanceAmount = (_item.Premium + _item.ZTSCLevy + _item.StampDuty + (_item.IncludeRadioLicenseCost ? _item.RadioLicenseCost : 0.00m) - _item.Discount) - (model.AmountPaid / vehicle.Count);
                                //}

                                _item.Id = service.AddVehicleInformation(_item);
                                var vehicles = (RiskDetailModel)Session["RenewVehicleDetails"];
                                // vehicles[Convert.ToInt32(_item.NoOfCarsCovered) - 1] = _item;
                                vehicles = _item;
                                Session["RenewVehicleDetails"] = vehicles;


                                // Delivery Address Save
                                var LicenseAddress = new LicenceDiskDeliveryAddress();
                                LicenseAddress.Address1 = _item.LicenseAddress1;
                                LicenseAddress.Address2 = _item.LicenseAddress2;
                                LicenseAddress.City = _item.LicenseCity;
                                LicenseAddress.VehicleId = _item.Id;
                                LicenseAddress.CreatedBy = customer.Id;
                                LicenseAddress.CreatedOn = DateTime.Now;
                                LicenseAddress.ModifiedBy = customer.Id;
                                LicenseAddress.ModifiedOn = DateTime.Now;

                                InsuranceContext.LicenceDiskDeliveryAddresses.Insert(LicenseAddress);


                                ///Licence Ticket
                                if (_item.IsLicenseDiskNeeded)
                                {

                                    var LicenceTicket = new LicenceTicket();
                                    var Licence = InsuranceContext.LicenceTickets.All(orderBy: "Id desc").FirstOrDefault();

                                    if (Licence != null)
                                    {
                                        string number = Licence.TicketNo.Substring(3);

                                        long tNumber = Convert.ToInt64(number) + 1;
                                        string TicketNo = string.Empty;
                                        int length = 6;
                                        length = length - tNumber.ToString().Length;

                                        for (int i = 0; i < length; i++)
                                        {
                                            TicketNo += "0";
                                        }
                                        TicketNo += tNumber;
                                        var ticketnumber = "GEN" + TicketNo;

                                        LicenceTicket.TicketNo = ticketnumber;
                                    }
                                    else
                                    {
                                        var TicketNo = ConfigurationManager.AppSettings["TicketNo"];
                                        LicenceTicket.TicketNo = TicketNo;
                                    }

                                    LicenceTicket.VehicleId = _item.Id;
                                    LicenceTicket.CloseComments = "";
                                    LicenceTicket.ReopenComments = "";
                                    LicenceTicket.DeliveredTo = "";
                                    LicenceTicket.CreatedDate = DateTime.Now;
                                    LicenceTicket.CreatedBy = customer.Id;
                                    LicenceTicket.IsClosed = false;
                                    LicenceTicket.PolicyNumber = policy.PolicyNumber;

                                    InsuranceContext.LicenceTickets.Insert(LicenceTicket);
                                }

                                ///Reinsurance                      

                                var ReinsuranceCases = InsuranceContext.Reinsurances.All(where: $"Type='Reinsurance'").ToList();
                                var ownRetention = InsuranceContext.Reinsurances.All().Where(x => x.TreatyCode == "OR001").Select(x => x.MaxTreatyCapacity).SingleOrDefault();
                                var ReinsuranceCase = new Reinsurance();

                                foreach (var Reinsurance in ReinsuranceCases)
                                {
                                    if (Reinsurance.MinTreatyCapacity <= vehicle.SumInsured && vehicle.SumInsured <= Reinsurance.MaxTreatyCapacity)
                                    {
                                        ReinsuranceCase = Reinsurance;
                                        break;
                                    }
                                }

                                if (ReinsuranceCase != null && ReinsuranceCase.MaxTreatyCapacity != null)
                                {
                                    var basicPremium = vehicle.Premium;
                                    var ReinsuranceBroker = InsuranceContext.ReinsuranceBrokers.Single(where: $"ReinsuranceBrokerCode='{ReinsuranceCase.ReinsuranceBrokerCode}'");
                                    var AutoFacSumInsured = 0.00m;
                                    var AutoFacPremium = 0.00m;
                                    var FacSumInsured = 0.00m;
                                    var FacPremium = 0.00m;

                                    if (ReinsuranceCase.MinTreatyCapacity > 200000)
                                    {
                                        var autofaccase = ReinsuranceCases.FirstOrDefault();
                                        var autofacSumInsured = autofaccase.MaxTreatyCapacity - ownRetention;
                                        var autofacReinsuranceBroker = InsuranceContext.ReinsuranceBrokers.Single(where: $"ReinsuranceBrokerCode='{autofaccase.ReinsuranceBrokerCode}'");

                                        var _reinsurance = new ReinsuranceTransaction();
                                        _reinsurance.ReinsuranceAmount = autofacSumInsured;
                                        AutoFacSumInsured = Convert.ToDecimal(_reinsurance.ReinsuranceAmount);
                                        _reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((_reinsurance.ReinsuranceAmount / vehicle.SumInsured) * basicPremium), 2);
                                        AutoFacPremium = Convert.ToDecimal(_reinsurance.ReinsurancePremium);
                                        _reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(autofacReinsuranceBroker.Commission);
                                        _reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((_reinsurance.ReinsurancePremium * _reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                        _reinsurance.VehicleId = vehicle.Id;
                                        _reinsurance.ReinsuranceBrokerId = autofacReinsuranceBroker.Id;
                                        _reinsurance.TreatyName = autofaccase.TreatyName;
                                        _reinsurance.TreatyCode = autofaccase.TreatyCode;
                                        _reinsurance.CreatedOn = DateTime.Now;
                                        _reinsurance.CreatedBy = customer.Id;

                                        InsuranceContext.ReinsuranceTransactions.Insert(_reinsurance);

                                        SummeryofReinsurance += "<tr><td>" + Convert.ToString(_reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(_reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(_reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";

                                        listReinsuranceTransaction.Add(_reinsurance);

                                        var __reinsurance = new ReinsuranceTransaction();
                                        __reinsurance.ReinsuranceAmount = _item.SumInsured - ownRetention - autofacSumInsured;
                                        FacSumInsured = Convert.ToDecimal(__reinsurance.ReinsuranceAmount);
                                        __reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((__reinsurance.ReinsuranceAmount / vehicle.SumInsured) * basicPremium), 2);
                                        FacPremium = Convert.ToDecimal(__reinsurance.ReinsurancePremium);
                                        __reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                                        __reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((__reinsurance.ReinsurancePremium * __reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                        __reinsurance.VehicleId = vehicle.Id;
                                        __reinsurance.ReinsuranceBrokerId = ReinsuranceBroker.Id;
                                        __reinsurance.TreatyName = ReinsuranceCase.TreatyName;
                                        __reinsurance.TreatyCode = ReinsuranceCase.TreatyCode;
                                        __reinsurance.CreatedOn = DateTime.Now;
                                        __reinsurance.CreatedBy = customer.Id;

                                        InsuranceContext.ReinsuranceTransactions.Insert(__reinsurance);

                                        //SummeryofReinsurance += "<tr><td>" + Convert.ToString(__reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(__reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(__reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";

                                        listReinsuranceTransaction.Add(__reinsurance);
                                    }
                                    else
                                    {

                                        var reinsurance = new ReinsuranceTransaction();
                                        reinsurance.ReinsuranceAmount = _item.SumInsured - ownRetention;
                                        AutoFacSumInsured = Convert.ToDecimal(reinsurance.ReinsuranceAmount);
                                        reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((reinsurance.ReinsuranceAmount / vehicle.SumInsured) * basicPremium), 2);
                                        AutoFacPremium = Convert.ToDecimal(reinsurance.ReinsurancePremium);
                                        reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                                        reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((reinsurance.ReinsurancePremium * reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                        reinsurance.VehicleId = vehicle.Id;
                                        reinsurance.ReinsuranceBrokerId = ReinsuranceBroker.Id;
                                        reinsurance.TreatyName = ReinsuranceCase.TreatyName;
                                        reinsurance.TreatyCode = ReinsuranceCase.TreatyCode;
                                        reinsurance.CreatedOn = DateTime.Now;
                                        reinsurance.CreatedBy = customer.Id;

                                        InsuranceContext.ReinsuranceTransactions.Insert(reinsurance);

                                        //SummeryofReinsurance += "<tr><td>" + Convert.ToString(reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";

                                        listReinsuranceTransaction.Add(reinsurance);
                                    }


                                    Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                                    VehicleModel vehiclemodel = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{vehicle.ModelId}'");
                                    VehicleMake vehiclemake = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{vehicle.MakeId}'");

                                    string vehicledescription = vehiclemodel.ModelDescription + " / " + vehiclemake.MakeDescription;

                                    // SummeryofVehicleInsured += "<tr><td>" + vehicledescription + "</td><td>" + Convert.ToString(item.SumInsured) + "</td><td>" + Convert.ToString(item.Premium) + "</td><td>" + AutoFacSumInsured.ToString() + "</td><td>" + AutoFacPremium.ToString() + "</td><td>" + FacSumInsured.ToString() + "</td><td>" + FacPremium.ToString() + "</td></tr>";

                                    SummeryofVehicleInsured += "<tr><td style='padding:7px 10px; font-size:14px'><font size='2'>" + vehicledescription + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + Convert.ToString(vehicle.SumInsured) + " </font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + Convert.ToString(vehicle.Premium) + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + AutoFacSumInsured.ToString() + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + AutoFacPremium.ToString() + "</ font ></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + FacSumInsured.ToString() + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + FacPremium.ToString() + "</font></td></tr>";



                                }

                            }


                        }




                        //   var item = vehicle;

                        try
                        {
                            var summarydetails = new SummaryVehicleDetail();
                            summarydetails.SummaryDetailId = summary.Id;
                            summarydetails.VehicleDetailsId = vehicle.Id;
                            summarydetails.CreatedBy = customer.Id;
                            summarydetails.CreatedOn = DateTime.Now;
                            InsuranceContext.SummaryVehicleDetails.Insert(summarydetails);
                        }
                        catch (Exception ex)
                        {
                            Insurance.Service.EmailService log = new Insurance.Service.EmailService();
                            log.WriteLog("exception during insert vehicel :" + ex.Message);

                        }


                        var DbEntry = Mapper.Map<SummaryDetailModel, SummaryDetail>(model);

                        if (summary != null)
                        {
                            // SummaryDetail summarydata = InsuranceContext.SummaryDetails.All(summary.Id.ToString()).FirstOrDefault(); // on 05-oct for updatig qutation

                            SummaryDetailsCalculation(summary);

                            var summarydata = Mapper.Map<SummaryDetailModel, SummaryDetail>(model);
                            summarydata.Id = summary.Id;
                            summarydata.CreatedOn = DateTime.Now;


                            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                            if (_userLoggedin)
                            {
                                var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                                var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();

                                if (_customerData != null)
                                {
                                    summarydata.CreatedBy = _customerData.Id;
                                }
                            }


                            summarydata.ModifiedBy = customer.Id;
                            summarydata.ModifiedOn = DateTime.Now;
                            if (summarydata.BalancePaidDate.Value.Year == 0001)
                            {
                                summarydata.BalancePaidDate = DateTime.Now;
                            }
                            if (DbEntry.Notes == null)
                            {
                                summarydata.Notes = "";
                            }
                            //summarydata.CustomerId = vehicle[0].CustomerId;

                            summarydata.CustomerId = customer.Id;
                            InsuranceContext.SummaryDetails.Update(summarydata);



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

                        if (vehicle != null && summary.Id != null && summary.Id > 0)
                        {
                            //var SummaryDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summary.Id}").ToList();

                            //if (SummaryDetails != null && SummaryDetails.Count > 0)
                            //{
                            //    foreach (var item1 in SummaryDetails)
                            //    {
                            //        InsuranceContext.SummaryVehicleDetails.Delete(item1);
                            //    }
                            //}



                        }
                        MiscellaneousService.UpdateBalanceForVehicles(summary.AmountPaid, summary.Id, Convert.ToDecimal(summary.TotalPremium), false);


                        if (listReinsuranceTransaction != null && listReinsuranceTransaction.Count > 0)
                        {
                            string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
                            int _vehicleId = 0;
                            int count = 0;
                            bool MailSent = false;
                            foreach (var item in listReinsuranceTransaction)
                            {
                                count++;
                                if (_vehicleId == 0)
                                {
                                    SummeryofReinsurance = "<tr><td>" + Convert.ToString(item.Id) + "</td><td>" + item.TreatyCode + "</td><td>" + item.TreatyName + "</td><td>" + Convert.ToString(item.ReinsuranceAmount) + "</td><td>" + MiscellaneousService.GetReinsuranceBrokerNamebybrokerid(item.ReinsuranceBrokerId) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(item.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(item.ReinsuranceCommissionPercentage) + "%</td></tr>";
                                    _vehicleId = item.VehicleId;
                                    MailSent = false;
                                }
                                else
                                {
                                    if (_vehicleId == item.VehicleId)
                                    {
                                        SummeryofReinsurance += "<tr><td>" + Convert.ToString(item.Id) + "</td><td>" + item.TreatyCode + "</td><td>" + item.TreatyName + "</td><td>" + Convert.ToString(item.ReinsuranceAmount) + "</td><td>" + MiscellaneousService.GetReinsuranceBrokerNamebybrokerid(item.ReinsuranceBrokerId) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(item.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(item.ReinsuranceCommissionPercentage) + "%</td></tr>";
                                        var user = UserManager.FindById(customer.UserID);
                                        Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
                                        var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
                                        var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == summary.PaymentTermId);
                                        string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/Reinsurance_Admin.cshtml";
                                        string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
                                        var Body = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##path##", filepath).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured);

                                        var attachementPath = MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case");

                                        List<string> attachements = new List<string>();
                                        attachements.Add(attachementPath);

                                        objEmailService.SendEmail(ZimnatEmail, "", "", "Reinsurance Case: " + policy.PolicyNumber.ToString(), Body, attachements);
                                        MailSent = true;
                                    }
                                    else
                                    {
                                        SummeryofReinsurance = "<tr><td>" + Convert.ToString(item.Id) + "</td><td>" + item.TreatyCode + "</td><td>" + item.TreatyName + "</td><td>" + Convert.ToString(item.ReinsuranceAmount) + "</td><td>" + MiscellaneousService.GetReinsuranceBrokerNamebybrokerid(item.ReinsuranceBrokerId) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(item.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(item.ReinsuranceCommissionPercentage) + "%</td></tr>";
                                        MailSent = false;
                                    }
                                    _vehicleId = item.VehicleId;
                                }


                                if (count == listReinsuranceTransaction.Count && !MailSent)
                                {
                                    var user = UserManager.FindById(customer.UserID);
                                    Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
                                    var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
                                    var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == summary.PaymentTermId);
                                    string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/Reinsurance_Admin.cshtml";
                                    string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
                                    var Body = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##paath##", filepath).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured);

                                    var attacehMentFilePath = MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case");

                                    List<string> _attachements = new List<string>();
                                    _attachements.Add(attacehMentFilePath);
                                    objEmailService.SendEmail(ZimnatEmail, "", "", "Reinsurance Case: " + policy.PolicyNumber.ToString(), Body, _attachements);
                                    //MiscellaneousService.ScheduleMotorPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case- " + policy.PolicyNumber.ToString(), item.VehicleId);
                                }
                            }
                        }

                        #endregion

                        Session["RenewVehicleId"] = vehicle.Id;
                        // return RedirectToAction("InitiatePaynowTransaction", "Paypal", new { id = DbEntry.Id, TotalPremiumPaid = Convert.ToString(model.AmountPaid), PolicyNumber = policy.PolicyNumber, Email = customer.EmailAddress });

                        if (model.PaymentMethodId == 1)
                            return RedirectToAction("SaveDetailList", "Renew", new { id = DbEntry.Id, invoiceNumer = model.InvoiceNumber });
                        if (model.PaymentMethodId == 3)
                        {

                            //return RedirectToAction("InitiatePaynowTransaction", "Paypal", new { id = DbEntry.Id, TotalPremiumPaid = Convert.ToString(model.AmountPaid), PolicyNumber = policy.PolicyNumber, Email = customer.EmailAddress });
                            TempData["PaymentMethodId"] = model.PaymentMethodId;
                            return RedirectToAction("makepayment", new { id = DbEntry.Id, TotalPremiumPaid = Convert.ToString(model.AmountPaid) });
                        }
                        else
                            return RedirectToAction("PaymentDetail", new { id = DbEntry.Id });
                    }
                    else
                    {
                        return RedirectToAction("SummaryDetail");
                    }
                }
                else
                {
                    return RedirectToAction("SummaryDetail");
                }

            }
            catch (Exception ex)
            {
                return RedirectToAction("SummaryDetail");
            }
        }

        public ActionResult VehicleHistory()
        {
            //List<VehicleDetail> vehicles = new List<VehicleDetail>();
            //vehicles = InsuranceContext.VehicleDetails.All().Where(x => x.IsActive == false).ToList();


            //   var list = InsuranceContext.Query("select PolicyId, RegistrationNo,Premium, VehicleMake.MakeDescription as makeId, VehicleModel.modeldescription as modelId from vehicledetail join VehicleMake on VehicleDetail.MakeId = VehicleMake.Makecode join VehicleModel on vehicledetail.modelId = vehiclemodel.modelcode where vehicledetail.Isactive=0")
            var list = InsuranceContext.Query("select PolicyId, RegistrationNo,Premium, VehicleMake.MakeDescription as makeId, VehicleModel.modeldescription as modelId, PolicyDetail.PolicyNumber, Customer.FirstName,Customer.LastName from vehicledetail join VehicleMake on VehicleDetail.MakeId = VehicleMake.Makecode join VehicleModel on vehicledetail.modelId = vehiclemodel.modelcode join Policydetail on vehicledetail.PolicyId=Policydetail.Id join customer on vehicledetail.customerId=customer.Id where vehicledetail.Isactive=0")
            .Select(x => new VehicleDetail()
            {
                EngineNumber = x.PolicyNumber,
                ChasisNumber = x.FirstName + " " + x.LastName,
                RegistrationNo = x.RegistrationNo,
                MakeId = x.makeId,
                ModelId = x.modelId,
                Premium = x.Premium,
            }).ToList();

            return View(list);
        }
        public SummaryDetailModel SummaryDetailsCalculation(SummaryDetailModel model)
        {

            var summary = new SummaryDetailService();

            SummaryDetailService SummaryDetailServiceObj = new SummaryDetailService();
            List<RiskDetailModel> vehicleList = new List<RiskDetailModel>();
            if (model.Id != 0)
            {
                model.CustomSumarryDetilId = model.Id;
                //vehicle = summary.GetVehicleInformation(id);
                var summaryVichalList = InsuranceContext.SummaryVehicleDetails.All(where: $" SummaryDetailId='{model.Id}'");

                foreach (var item in summaryVichalList)
                {
                    //  var vehicleDetails = InsuranceContext.VehicleDetails.Single(item.VehicleDetailsId);
                    var vehicleDetails = InsuranceContext.VehicleDetails.Single(where: $"Id='{ item.VehicleDetailsId }' and IsActive<>0 ");

                    if (vehicleDetails != null)
                    {
                        RiskDetailModel vehicleModel = Mapper.Map<VehicleDetail, RiskDetailModel>(vehicleDetails);
                        vehicleList.Add(vehicleModel);
                    }
                }
            }

            var DiscountSettings = InsuranceContext.Settings.Single(where: $"keyname='Discount On Renewal'");
            model.CarInsuredCount = vehicleList.Count;
            model.DebitNote = "INV" + Convert.ToString(SummaryDetailServiceObj.getNewDebitNote());

            model.PaymentMethodId = model.PaymentTermId;

            //default selection 
            //if (User.IsInRole("Staff"))
            //{
            //    model.PaymentMethodId = 1;
            //}
            //else
            //{
            //    model.PaymentMethodId = 2;
            //}


            model.PaymentTermId = 1;
            model.ReceiptNumber = "";
            model.SMSConfirmation = false;
            //model.TotalPremium = vehicle.Sum(item => item.Premium + item.ZTSCLevy + item.StampDuty + item.RadioLicenseCost);
            model.TotalPremium = 0.00m;
            model.TotalRadioLicenseCost = 0.00m;
            model.Discount = 0.00m;
            foreach (var item in vehicleList)
            {
                model.TotalPremium += item.Premium + item.ZTSCLevy + item.StampDuty + item.VehicleLicenceFee;
                if (item.IncludeRadioLicenseCost)
                {
                    model.TotalPremium += item.RadioLicenseCost;
                    model.TotalRadioLicenseCost += item.RadioLicenseCost;
                }
                model.Discount += item.Discount;

            }
            model.TotalRadioLicenseCost = Math.Round(Convert.ToDecimal(model.TotalRadioLicenseCost), 2);
            model.Discount = Math.Round(Convert.ToDecimal(model.Discount), 2);
            model.TotalPremium = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);
            model.TotalStampDuty = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.StampDuty)), 2);
            model.TotalSumInsured = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.SumInsured)), 2);
            model.TotalZTSCLevies = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.ZTSCLevy)), 2);
            model.ExcessBuyBackAmount = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.ExcessBuyBackAmount)), 2);
            model.MedicalExpensesAmount = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.MedicalExpensesAmount)), 2);
            model.PassengerAccidentCoverAmount = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.PassengerAccidentCoverAmount)), 2);
            model.RoadsideAssistanceAmount = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.RoadsideAssistanceAmount)), 2);
            model.ExcessAmount = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.ExcessAmount)), 2);
            model.AmountPaid = 0.00m;
            model.MaxAmounttoPaid = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);
            var vehiclewithminpremium = vehicleList.OrderBy(x => x.Premium).FirstOrDefault();

            if (vehiclewithminpremium != null)
            {
                model.MinAmounttoPaid = Math.Round(Convert.ToDecimal(vehiclewithminpremium.Premium + vehiclewithminpremium.StampDuty + vehiclewithminpremium.ZTSCLevy + (Convert.ToBoolean(vehiclewithminpremium.IncludeRadioLicenseCost) ? vehiclewithminpremium.RadioLicenseCost : 0.00m)), 2);
            }

            model.AmountPaid = Convert.ToDecimal(model.TotalPremium);
            model.BalancePaidDate = DateTime.Now;
            model.Notes = "";
            model.Id = model.Id;

            if (Session["RePolicyData"] != null)
            {
                var PolicyData = (PolicyDetail)Session["RePolicyData"];
                model.InvoiceNumber = PolicyData.PolicyNumber;
            }

            return model;
        }


        [HttpPost]
        public JsonResult CalculatePremium(int vehicleUsageId, decimal sumInsured, int coverType, int excessType, decimal excess, decimal? AddThirdPartyAmount, int NumberofPersons, Boolean Addthirdparty, Boolean PassengerAccidentCover, Boolean ExcessBuyBack, Boolean RoadsideAssistance, Boolean MedicalExpenses, decimal? RadioLicenseCost, Boolean IncludeRadioLicenseCost, int policytermid)
        {
            //var policytermid = (int)Session["policytermid"];
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
            var premium = quote.CalculatePremium(vehicleUsageId, sumInsured, typeCover, eexcessType, excess, policytermid, AddThirdPartyAmount, NumberofPersons, Addthirdparty, PassengerAccidentCover, ExcessBuyBack, RoadsideAssistance, MedicalExpenses, RadioLicenseCost, IncludeRadioLicenseCost, false, "", "", "");
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

        public ActionResult PaymentDetail(int id, string erroMsg = null)
        {
            var cardDetails = (CardDetailModel)Session["CardDetail"];
            if (cardDetails == null)
            {
                cardDetails = new CardDetailModel();
            }
            cardDetails.SummaryDetailId = id;

            TempData["ErrorMsg"] = erroMsg;

            return View(cardDetails);
        }
        public async Task<ActionResult> SaveDetailList(Int32 id)
        {
            var vehicleId = (Int32)Session["RenewVehicleId"];
            var PaymentId = Session["PaymentId"];
            var InvoiceId = Session["InvoiceId"];
            var summary = InsuranceContext.SummaryDetails.Single(id);
            var policy = (PolicyDetail)Session["RenewVehiclePolicy"];
            var DebitNote = summary.DebitNote;
            var vehicle = InsuranceContext.VehicleDetails.Single(vehicleId);
            PaymentInformation objSaveDetailListModel = new PaymentInformation();
            string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
            if (Session["RenewVehicleDetails"] != null)
            {

                //vehicle.isLapsed = true;
                //InsuranceContext.VehicleDetails.Update(vehicle);

                //var summaryvehicledetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"SummaryDetailId={summary.Id} and VehicleDetailsId={vehicleId}");               
                //InsuranceContext.SummaryVehicleDetails.Delete(summaryvehicledetail);

                var _item = (RiskDetailModel)Session["RenewVehicleDetails"];
                //var product = InsuranceContext.Products.Single(Convert.ToInt32(_item.ProductId));

                objSaveDetailListModel.CurrencyId = policy.CurrencyId;
                objSaveDetailListModel.PolicyId = policy.Id;
                objSaveDetailListModel.VehicleDetailId = _item.Id;
                objSaveDetailListModel.CustomerId = summary.CustomerId.Value;
                objSaveDetailListModel.SummaryDetailId = id;
                objSaveDetailListModel.DebitNote = summary.DebitNote;
                objSaveDetailListModel.ProductId = _item.ProductId;
                objSaveDetailListModel.PaymentId = PaymentId == null ? "" : PaymentId.ToString();
                objSaveDetailListModel.InvoiceId = InvoiceId == null ? "" : InvoiceId.ToString();
                objSaveDetailListModel.InvoiceNumber = policy.PolicyNumber;
                InsuranceContext.PaymentInformations.Insert(objSaveDetailListModel);

                MiscellaneousService.AddLoyaltyPoints(summary.CustomerId.Value, policy.Id, _item);
            }
            else
            {

                DateTime NewRenewalDate = DateTime.Now;

                switch (vehicle.PaymentTermId)
                {
                    case 1:
                        NewRenewalDate = vehicle.RenewalDate.Value.AddYears(1);
                        break;
                    case 3:
                        NewRenewalDate = vehicle.RenewalDate.Value.AddMonths(3);
                        break;
                    case 4:
                        NewRenewalDate = vehicle.RenewalDate.Value.AddMonths(4);
                        break;
                }

                vehicle.RenewalDate = NewRenewalDate;
                InsuranceContext.VehicleDetails.Update(vehicle);

                //var product = InsuranceContext.Products.Single(Convert.ToInt32(vehicle.ProductId));

                objSaveDetailListModel.CurrencyId = policy.CurrencyId;
                objSaveDetailListModel.PolicyId = policy.Id;
                objSaveDetailListModel.VehicleDetailId = vehicleId;
                objSaveDetailListModel.CustomerId = summary.CustomerId.Value;
                objSaveDetailListModel.SummaryDetailId = id;
                objSaveDetailListModel.DebitNote = summary.DebitNote;
                objSaveDetailListModel.ProductId = vehicle.ProductId;
                objSaveDetailListModel.PaymentId = PaymentId == null ? "" : PaymentId.ToString();
                objSaveDetailListModel.InvoiceId = InvoiceId == null ? "" : InvoiceId.ToString();
                InsuranceContext.PaymentInformations.Insert(objSaveDetailListModel);
                MiscellaneousService.AddLoyaltyPoints(summary.CustomerId.Value, policy.Id, Mapper.Map<VehicleDetail, RiskDetailModel>(vehicle));
            }

            var customer = InsuranceContext.Customers.Single(summary.CustomerId.Value);
            var user = UserManager.FindById(customer.UserID);


            Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();

            //var data = (List<Item>)Session["itemData"];
            //if (data != null)
            //{
               // var totalprem = data.Sum(x => Convert.ToDecimal(x.price));

                string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/Reciept.cshtml";
                string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(userRegisterationEmailPath));

                var Body2 = EmailBody2.Replace("#DATE#", DateTime.Now.ToShortDateString()).Replace("##path##", filepath).Replace("#FirstName#", customer.FirstName).Replace("#LastName#", customer.LastName).Replace("#AccountName#", customer.FirstName + ", " + customer.LastName).Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2).Replace("#Amount#", Convert.ToString(summary.AmountPaid)).Replace("#PaymentDetails#", "New Premium").Replace("#ReceiptNumber#", policy.PolicyNumber).Replace("#PaymentType#", (summary.PaymentMethodId == 1 ? "Cash" : (summary.PaymentMethodId == 2 ? "PayPal" : "PayNow")));

                var attachementPath = MiscellaneousService.EmailPdf(Body2, policy.CustomerId, policy.PolicyNumber, "Renew Invoice ");

                List<string> _attachements = new List<string>();
                _attachements.Add(attachementPath);



                if (!customer.IsCustomEmail) // if customer has custom email
                {
                    objEmailService.SendEmail(LoggedUserEmail(), "", "", "Renew " + policy.PolicyNumber + " : Invoice", Body2, _attachements);

                }
                else
                {
                    objEmailService.SendEmail(user.Email, "", "", "Renew " + policy.PolicyNumber + " : Invoice", Body2, _attachements);


                }




            //}


            decimal totalpaymentdue = 0.00m;

            //if (vehicle.PaymentTermId == 1)
            //{
            //    totalpaymentdue = (decimal)summary.TotalPremium;
            //}
            //else if (vehicle.PaymentTermId == 4)
            //{
            //    totalpaymentdue = (decimal)summary.TotalPremium * 3;
            //}
            //else if (vehicle.PaymentTermId == 3)
            //{
            //    totalpaymentdue = (decimal)summary.TotalPremium * 4;
            //}


          //  var SummaryVehicleDetail = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summary.Id}").ToList();

            string Summeryofcover = "";
            //for (int i = 0; i < SummaryVehicleDetail.Count; i++)
            //{


            if (Session["RenewVehicleDetails"] != null)
            {

                var _vehicle = (RiskDetailModel)Session["RenewVehicleDetails"];

                Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                VehicleModel model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{_vehicle.ModelId}'");
                VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{_vehicle.MakeId}'");

                string vehicledescription = model.ModelDescription + " / " + make.MakeDescription;


                Summeryofcover += "<tr><td>" + vehicledescription + "</td><td>$" + _vehicle.SumInsured + "</td><td>" + (_vehicle.CoverTypeId == 1 ? eCoverType.Comprehensive.ToString() : eCoverType.ThirdParty.ToString()) + "</td><td>" + InsuranceContext.VehicleUsages.All(Convert.ToString(_vehicle.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</td><td>$0.00</td><td>$" + Convert.ToString(_vehicle.Excess) + "</td><td>$" + Convert.ToString(_vehicle.Premium) + "</td></tr>";


            }
            else
            {
                var _Premium = vehicle.Premium - vehicle.PassengerAccidentCoverAmount - vehicle.ExcessAmount - vehicle.ExcessBuyBackAmount - vehicle.MedicalExpensesAmount - vehicle.RoadsideAssistanceAmount;
                Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                VehicleModel model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{vehicle.ModelId}'");
                VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{vehicle.MakeId}'");

                string vehicledescription = model.ModelDescription + " / " + make.MakeDescription;

                string coverType = "";

                if (vehicle.CoverTypeId == 1)
                    coverType = eCoverType.ThirdParty.ToString();
                if (vehicle.CoverTypeId == 2)
                    coverType = eCoverType.FullThirdParty.ToString();

                if (vehicle.CoverTypeId == 4)
                    coverType = eCoverType.Comprehensive.ToString();


                Summeryofcover += "<tr><td>" + vehicle.RegistrationNo + "</td> <td>" + vehicledescription + "</td><td>$" + vehicle.SumInsured + "</td><td>" + coverType + "</td><td>" + InsuranceContext.VehicleUsages.All(Convert.ToString(vehicle.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</td><td>$0.00</td><td>" + vehicle.PaymentTermId.ToString()=="1"? "Yearly": vehicle.PaymentTermId + " Months" + "</td><td>$" + Convert.ToString(_Premium) + "</td></tr>";

            }
            //var Premium = vehicle.Premium + vehicle.PassengerAccidentCoverAmount + vehicle.ExcessAmount + vehicle.ExcessBuyBackAmount + vehicle.MedicalExpensesAmount + vehicle.RoadsideAssistanceAmount ;


            var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
            var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == summary.PaymentTermId);
            string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/ScheduleMotorRenew.cshtml";
            string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
            //var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (summary.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + summary.PaymentTermId.ToString() + "Months)")).Replace("##TotalPremiumDue##", Convert.ToString(totalpaymentdue)).Replace("##StampDuty##", Convert.ToString(summary.TotalStampDuty)).Replace("##MotorLevy##", Convert.ToString(summary.TotalZTSCLevies)).Replace("##PremiumDue##", Convert.ToString(summary.TotalPremium)).Replace("##PostalAddress##", customer.Zipcode);

            var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##NINumber##", customer.NationalIdentificationNumber).Replace("##ReNewPolicyNo##", vehicle.RenewPolicyNumber).Replace("##path##", filepath).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (summary.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + summary.PaymentTermId.ToString() + "Months)")).Replace("##TotalPremiumDue##", Convert.ToString(summary.TotalPremium)).Replace("##StampDuty##", Convert.ToString(summary.TotalStampDuty)).Replace("##MotorLevy##", Convert.ToString(summary.TotalZTSCLevies)).Replace("##PremiumDue##", Convert.ToString(vehicle.Premium)).Replace("##PostalAddress##", customer.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(vehicle.ExcessBuyBackAmount)).Replace("##MedicalExpenses##", Convert.ToString(vehicle.MedicalExpensesAmount)).Replace("##PassengerAccidentCover##", Convert.ToString(vehicle.PassengerAccidentCoverAmount)).Replace("##RoadsideAssistance##", Convert.ToString(vehicle.RoadsideAssistanceAmount).Replace("##RadioLicence##", Convert.ToString(vehicle.RadioLicenseCost))
                .Replace("##Discount##", Convert.ToString(vehicle.Discount)).Replace("##ExcessAmount##", Convert.ToString(vehicle.ExcessAmount)));

            var attachemetPath = MiscellaneousService.EmailPdf(Bodyy, policy.CustomerId, policy.PolicyNumber, "Renew Schedule-motor");
            List<string> attachements = new List<string>();

            attachements.Add(attachemetPath);

            var Atter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
            attachements.Add(Atter);



            if (!customer.IsCustomEmail) // if customer has custom email
                objEmailService.SendEmail(LoggedUserEmail(), "", "", "Renew " + policy.PolicyNumber + " :Renew-Schedule-motor", Bodyy, attachements);
            else
                objEmailService.SendEmail(user.Email, "", "", "Renew " + policy.PolicyNumber + " :Renew-Schedule-motor", Bodyy, attachements);




            //MiscellaneousService.ScheduleMotorPdf(Bodyy, policy.CustomerId, policy.PolicyNumber, "Renew-Schedule-motor");




            //Session.Remove("policytermid");
            Session.Remove("RenewVehicleId");
            Session.Remove("RenewPaymentId");
            Session.Remove("RenewInvoiceId");
            Session.Remove("RenewVehicleSummary");
            Session.Remove("RenewVehiclePolicy");
            //Session.Remove("RenewVehicle");
            Session.Remove("RenewVehicleDetails");
            Session.Remove("RenewCardDetail");




            return View(objSaveDetailListModel);
        }

        public JsonResult GetLicenseAddress()
        {
            var customerData = (CustomerModel)Session["CustomerDataModal"];
            //LicenseAddress licenseAddress = new LicenseAddress();
            RiskDetailModel riskDetailModel = new RiskDetailModel();
            riskDetailModel.LicenseAddress1 = customerData.AddressLine1;
            riskDetailModel.LicenseAddress2 = customerData.AddressLine2;
            riskDetailModel.LicenseCity = customerData.City;
            return Json(riskDetailModel, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> InitiatePaynowTransaction(Int32 id, string TotalPremiumPaid, string PolicyNumber, string Email)
        {
            var summaryDetail = InsuranceContext.SummaryDetails.Single(id);
            var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={id}").ToList();
            //var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
            var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
            var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
            var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));

            List<Item> itms = new List<Item>();

            foreach (var vehicledetail in SummaryVehicleDetails.ToList())
            {
                var _vehicle = InsuranceContext.VehicleDetails.Single(vehicledetail.VehicleDetailsId);
                Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                VehicleModel _model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{_vehicle.ModelId}'");
                VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{_vehicle.MakeId}'");

                Item item = new Item();
                item.name = make.MakeDescription + "/" + _model.ModelDescription;
                item.currency = "USD";
                item.price = Convert.ToString(_vehicle.Premium);
                item.quantity = "1";
                item.sku = _vehicle.RegistrationNo;

                itms.Add(item);
            }

            Session["itemData"] = itms;

            Insurance.Service.PaynowService paynowservice = new Insurance.Service.PaynowService();
            PaynowResponse paynowresponse = new PaynowResponse();

            paynowresponse = await paynowservice.initiateTransaction(Convert.ToString(id), TotalPremiumPaid, PolicyNumber, Email, true);

            if (paynowresponse.status == "Ok")
            {
                string strScript = "location.href = '" + paynowresponse.browserurl + "';";
                ViewBag.strScript = "<script type='text/javascript'>$(document).ready(function(){" + strScript + "});</script>";
            }
            else
            {
                ViewBag.strScript = "<script type='text/javascript'>$(document).ready(function(){$('#errormsg').text('" + paynowresponse.error + "');});</script>";
            }

            return View();
            //return RedirectToAction("SaveDetailList", "Paypal", new { id = id });
        }

        public ActionResult PaymentWithCreditCard(CardDetailModel model)
        {
            Session["RenewCardDetail"] = model;

            var Vehicle = new RiskDetailModel();
            if (Session["RenewVehicleDetails"] != null)
            {
                Vehicle = (RiskDetailModel)Session["RenewVehicleDetails"];
            }
            else
            {
                var Id = (Int32)Session["RenewVehicleId"];
                var _vehicle = InsuranceContext.VehicleDetails.Single(Id);
                Vehicle = Mapper.Map<VehicleDetail, RiskDetailModel>(_vehicle);
            }

            //create and item for which you are taking payment
            //if you need to add more items in the list
            //Then you will need to create multiple item objects or use some loop to instantiate object
            var summaryDetail = (SummaryDetail)Session["RenewVehicleSummary"];
            //var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={model.SummaryDetailId}").ToList();
            //var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
            //var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
            var customer = InsuranceContext.Customers.Single(Vehicle.CustomerId);
            //var summaryDetail = (SummaryDetailModel)Session["SummaryDetailed"];
            //var vehicle = (RiskDetailModel)Session["VehicleDetail"];
            //var policy = (PolicyDetail)Session["PolicyData"];
            //var customer = (CustomerModel)Session["CustomerDataModal"];
            //var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));
            //var currency = InsuranceContext.Currencies.Single(policy.CurrencyId);

            //var paymentInformations = InsuranceContext.PaymentInformations.SingleCustome(model.SummaryDetailId);

            double totalPremium = Convert.ToDouble(Vehicle.Premium + Vehicle.StampDuty + Vehicle.ZTSCLevy + (Convert.ToBoolean(Vehicle.IncludeRadioLicenseCost) ? Vehicle.RadioLicenseCost : 0.00m));

            //check if single decimal place
            string zeros = string.Empty;
            try
            {
                var percision = totalPremium.ToString().Split('.');
                var length = 2 - percision[1].Length;
                for (int i = 0; i < length; i++)
                {
                    zeros += "0";
                }
            }
            catch
            {
                zeros = ".00";

            }

            List<Item> itms = new List<Item>();

            Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
            VehicleModel _model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{Vehicle.ModelId}'");
            VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{Vehicle.MakeId}'");

            Item item = new Item();
            item.name = make.MakeDescription + "/" + _model.ModelDescription;
            item.currency = "USD";
            item.price = Convert.ToString(Vehicle.Premium + Vehicle.StampDuty + Vehicle.ZTSCLevy + (Convert.ToBoolean(Vehicle.IncludeRadioLicenseCost) ? Vehicle.RadioLicenseCost : 0.00m));
            item.quantity = "1";
            item.sku = Vehicle.RegistrationNo;

            itms.Add(item);


            Session["itemData"] = itms;

            ItemList itemList = new ItemList();
            itemList.items = itms;

            Address billingAddress = new Address();
            billingAddress.city = customer.City;
            billingAddress.country_code = "US";
            billingAddress.line1 = customer.AddressLine1 == string.Empty ? customer.AddressLine2 : customer.AddressLine1;
            billingAddress.line2 = customer.AddressLine2 == string.Empty ? customer.AddressLine1 : customer.AddressLine2;

            if (customer.Zipcode == null)
            {
                billingAddress.postal_code = "00263";
            }
            else
            {
                billingAddress.postal_code = customer.Zipcode;
            }

            billingAddress.state = customer.NationalIdentificationNumber;

            PayPal.Api.CreditCard crdtCard = new PayPal.Api.CreditCard();
            crdtCard.billing_address = billingAddress;
            crdtCard.cvv2 = model.CVC;
            crdtCard.expire_month = Convert.ToInt32(model.ExpiryDate.Split('/')[0]);
            crdtCard.expire_year = Convert.ToInt32(model.ExpiryDate.Split('/')[1]);

            //crdtCard.first_name = "fgdfg";
            //crdtCard.last_name = "rffd";

            var name = model.NameOnCard.Split(' ');
            if (name.Length == 1)
            {
                crdtCard.first_name = name[0];
                crdtCard.last_name = null;
            }
            if (name.Length == 2)
            {
                crdtCard.first_name = name[0];
                crdtCard.last_name = name[1];
            }

            crdtCard.number = model.CardNumber; //use some other test number if it fails
            crdtCard.type = CreditCardUtility.GetTypeName(model.CardNumber).ToLower();

            Details details = new Details();
            details.tax = "0";
            details.shipping = "0";
            details.subtotal = totalPremium.ToString() + zeros;

            Amount amont = new Amount();
            amont.currency = "USD";
            amont.total = totalPremium.ToString() + zeros;
            amont.details = details;

            Transaction tran = new Transaction();
            tran.amount = amont;
            tran.description = "trnx desc";
            tran.item_list = itemList;

            List<Transaction> transactions = new List<Transaction>();
            transactions.Add(tran);

            FundingInstrument fundInstrument = new FundingInstrument();
            fundInstrument.credit_card = crdtCard;

            List<FundingInstrument> fundingInstrumentList = new List<FundingInstrument>();
            fundingInstrumentList.Add(fundInstrument);

            var User = UserManager.FindById(customer.UserID);
            PayerInfo pi = new PayerInfo();
            pi.email = User.Email;
            pi.first_name = customer.FirstName;
            pi.last_name = customer.LastName;
            pi.shipping_address = new ShippingAddress
            {
                city = customer.City,
                country_code = "US",
                line1 = customer.AddressLine1 == string.Empty ? customer.AddressLine2 : customer.AddressLine1,
                line2 = customer.AddressLine2 == string.Empty ? customer.AddressLine1 : customer.AddressLine2,
                postal_code = customer.Zipcode,
                state = customer.NationalIdentificationNumber,
            };

            Payer payr = new Payer();
            payr.funding_instruments = fundingInstrumentList;
            payr.payment_method = "credit_card";
            payr.payer_info = pi;

            Payment pymnt = new Payment();
            pymnt.intent = "sale";
            pymnt.payer = payr;
            pymnt.transactions = transactions;

            try
            {
                //getting context from the paypal, basically we are sending the clientID and clientSecret key in this function 
                //to the get the context from the paypal API to make the payment for which we have created the object above.

                //Code for the configuration class is provided next

                // Basically, apiContext has a accesstoken which is sent by the paypal to authenticate the payment to facilitator account. An access token could be an alphanumeric string

                APIContext apiContext = InsuranceClaim.Models.Configuration.GetAPIContext();

                // Create is a Payment class function which actually sends the payment details to the paypal API for the payment. The function is passed with the ApiContext which we received above.

                Payment createdPayment = pymnt.Create(apiContext);
                //paymentInformations.PaymentTransctionId = createdPayment.id;
                Session["RenewPaymentId"] = createdPayment.id;

                //if the createdPayment.State is "approved" it means the payment was successfull else not
                creatInvoice(User, customer);
                if (createdPayment.state.ToLower() != "approved")
                {
                    ModelState.AddModelError("PaymentError", "Payment not approved");
                    return RedirectToAction("PaymentDetail", "CustomerRegistration", new { id = model.SummaryDetailId });
                }
            }
            catch (PayPal.PayPalException ex)
            {
                Logger.Log("Error: " + ex.Message);
                ModelState.AddModelError("PaymentError", ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();

                var error = json_serializer.DeserializeObject(((PayPal.ConnectionException)ex).Response);
                return RedirectToAction("PaymentDetail", "CustomerRegistration", new { id = model.SummaryDetailId });
            }

            return RedirectToAction("SaveDetailList", "Renew", new { id = model.SummaryDetailId });
        }

        private ActionResult creatInvoice(ApplicationUser User, Customer customer)
        {
            APIContext apiContext = InsuranceClaim.Models.Configuration.GetAPIContext();

            var data = (List<Item>)Session["itemData"];

            var invoice = new Invoice()
            {

                merchant_info = new MerchantInfo()
                {
                    email = "ankit.dhiman-facilitator@kindlebit.com",
                    first_name = "Genetic Financial Services",
                    last_name = "11 Routledge Street Milton Park",
                    business_name = "Insurance Claim",
                    website = "insuranceclaim.com",
                    //tax_id = "47-4559942",

                    phone = new Phone()
                    {
                        country_code = "001",
                        national_number = "08677007491"
                    },
                    address = new InvoiceAddress()
                    {
                        line1 = customer.AddressLine1,
                        city = customer.AddressLine2,
                        state = customer.City + "/ " + customer.NationalIdentificationNumber,
                        postal_code = customer.Zipcode,
                        country_code = "US"

                    }
                },

                billing_info = new List<BillingInfo>()
                            {
                                new BillingInfo()
                                {

                                    email = User.Email,//"amit.kamal@kindlebit.com",
                                    first_name=customer.FirstName,
                                    last_name=customer.LastName
                                }
                            },

                items = new List<InvoiceItem>()
                            {
                                new InvoiceItem()
                                {
                                    name = data[0].name,
                                    quantity = 1,
                                    unit_price = new PayPal.Api.Currency()
                                    {
                                        currency = "USD",
                                        value =data[0].price

                                    },
                                },
                            },
                note = "Your  Invoce has been created successfully.",

                shipping_info = new ShippingInfo()
                {
                    first_name = customer.FirstName,
                    last_name = customer.LastName,
                    business_name = "InsuranceClaim",
                    address = new InvoiceAddress()
                    {
                        //line1 = userdata.State.ToString(),
                        city = customer.City,
                        state = customer.City + "/" + customer.NationalIdentificationNumber,
                        postal_code = customer.Zipcode,
                        country_code = "US"
                    }
                }
            };
            var createdInvoice = invoice.Create(apiContext);
            Session["RenewInvoiceId"] = createdInvoice.id;
            createdInvoice.Send(apiContext);

            return null;
        }

        [HttpPost]
        public JsonResult checkVRNwithICEcash(string regNo, string PaymentTerm)
        {
            CustomerRegistrationController.checkVRNwithICEcashResponse response = new CustomerRegistrationController.checkVRNwithICEcashResponse();

            JsonResult json = new JsonResult();
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            //json.Data = "";

            Insurance.Service.ICEcashService ICEcashService = new Insurance.Service.ICEcashService();
            var tokenObject = new ICEcashTokenResponse();

            #region get ICE cash token
            //if (Session["ICEcashToken"] != null)
            //{
            //    ICEcashService.getToken();
            //    tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
            //}
            //else
            //{
            //    ICEcashService.getToken();
            //    tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
            //}

            if (Session["ICEcashToken"] != null)
            {
                var icevalue = (ICEcashTokenResponse)Session["ICEcashToken"];
                string format = "yyyyMMddHHmmss";
                var IceDateNowtime = DateTime.Now;
                var IceExpery = DateTime.ParseExact(icevalue.Response.ExpireDate, format, CultureInfo.InvariantCulture);
                if (IceDateNowtime > IceExpery)
                {
                    ICEcashService.getToken();
                }

                tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
            }
            else
            {
                ICEcashService.getToken();
                tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
            }



            #endregion

            List<RiskDetailModel> objVehicles = new List<RiskDetailModel>();
            //objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo });
            objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo, PaymentTermId = Convert.ToInt32(PaymentTerm) });

            if (tokenObject.Response.PartnerToken != "")
            {
                ResultRootObject quoteresponse = ICEcashService.checkVehicleExists(objVehicles, tokenObject.Response.PartnerToken, tokenObject.PartnerReference);
                response.result = quoteresponse.Response.Result;
                if (response.result == 0)
                {
                    response.message = quoteresponse.Response.Quotes[0].Message;
                }
                else
                {
                    response.Data = quoteresponse;
                }
            }

            json.Data = response;

            return json;
        }

        [HttpPost]
        public JsonResult getPolicyDetailsFromICEcash(string regNo, string PaymentTerm, string SumInsured, string make, string model, int VehicleYear, int CoverTypeId, int VehicleUsage)
        {
            CustomerRegistrationController.checkVRNwithICEcashResponse response = new CustomerRegistrationController.checkVRNwithICEcashResponse();
            JsonResult json = new JsonResult();
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            //json.Data = "";

            Insurance.Service.ICEcashService ICEcashService = new Insurance.Service.ICEcashService();
            var tokenObject = new ICEcashTokenResponse();

            #region get ICE cash token
            if (Session["ICEcashToken"] != null)
            {
                ICEcashService.getToken();
                tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
            }
            else
            {
                ICEcashService.getToken();
                tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
            }
            #endregion

            List<RiskDetailModel> objVehicles = new List<RiskDetailModel>();
            //objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo });
            objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo, PaymentTermId = Convert.ToInt32(PaymentTerm) });

            if (tokenObject.Response.PartnerToken != "")
            {
                ResultRootObject quoteresponse = ICEcashService.RequestQuote(tokenObject.Response.PartnerToken, regNo, SumInsured, make, model, Convert.ToInt32(PaymentTerm), VehicleYear, CoverTypeId, VehicleUsage, tokenObject.PartnerReference);
                response.result = quoteresponse.Response.Result;
                if (response.result == 0)
                {
                    response.message = quoteresponse.Response.Quotes[0].Message;
                }
                else
                {
                    response.Data = quoteresponse;
                }
            }

            json.Data = response;

            return json;
        }
        [HttpGet]
        public JsonResult getVehicleList(int summaryDetailId = 0)
        {
            try
            {

                List<RiskDetailModel> vehicleList = new List<RiskDetailModel>();
                if (summaryDetailId != 0)
                {
                    //vehicle = summary.GetVehicleInformation(id);
                    var summaryVehicleList = InsuranceContext.SummaryVehicleDetails.All(where: $" SummaryDetailId='{summaryDetailId}'");

                    foreach (var item in summaryVehicleList)
                    {
                        var vehicleDetails = InsuranceContext.VehicleDetails.Single(where: $" Id='{item.VehicleDetailsId}'");
                        RiskDetailModel vehicleModel = Mapper.Map<VehicleDetail, RiskDetailModel>(vehicleDetails);

                        vehicleModel.ZTSCLevy = vehicleDetails.ZTSCLevy;
                        vehicleList.Add(vehicleModel);
                    }

                    Session["RenewVehicleDetails"] = vehicleList;

                }



                if (Session["RenewVehicleDetails"] != null)
                {
                    var list = (List<RiskDetailModel>)Session["RenewVehicleDetails"];
                    List<VehicleListModel> vehiclelist = new List<VehicleListModel>();


                    foreach (var item in list)
                    {



                        VehicleListModel obj = new VehicleListModel();
                        obj.make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'").MakeDescription;
                        obj.model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ModelDescription;
                        obj.covertype = InsuranceContext.CoverTypes.Single(item.CoverTypeId).Name;
                        obj.premium = item.Premium.ToString();
                        obj.suminsured = item.SumInsured.ToString();
                        obj.RegistrationNo = item.RegistrationNo;


                        if (item.IncludeRadioLicenseCost == true)
                        {
                            obj.radio_license_fee = item.RadioLicenseCost == null ? "0" : item.RadioLicenseCost.ToString();
                        }
                        else
                        {
                            obj.radio_license_fee = "0";
                        }


                        obj.excess = item.ExcessAmount == null ? "0" : item.ExcessAmount.ToString();
                        obj.vehicle_license_fee = item.VehicleLicenceFee == 0 ? "0" : item.VehicleLicenceFee.ToString();
                        obj.stampDuty = item.StampDuty == null ? "0" : item.StampDuty.ToString();

                        decimal? radioLicenseCost = 0;
                        if (item.IncludeRadioLicenseCost)
                        {
                            radioLicenseCost = item.RadioLicenseCost;
                        }

                        // var calculationAmount = item.Premium + radioLicenseCost + item.Excess + item.VehicleLicenceFee + item.StampDuty + item.ZTSCLevy;

                        var calculationAmount = item.Premium + radioLicenseCost + item.VehicleLicenceFee + item.StampDuty + item.ZTSCLevy;


                        obj.total = calculationAmount.ToString();
                        obj.ZTSCLevy = item.ZTSCLevy == null ? "0" : Convert.ToString(item.ZTSCLevy);


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


        public class Country
        {
            public string code { get; set; }
            public string name { get; set; }
            public string DisplayName { get; set; }
        }

        public class RootObject
        {
            public List<Country> countries { get; set; }
        }
    }




}