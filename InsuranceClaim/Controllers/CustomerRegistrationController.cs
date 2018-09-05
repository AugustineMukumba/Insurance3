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
using System.Web.Configuration;

namespace InsuranceClaim.Controllers
{
    public class CustomerRegistrationController : Controller
    {
        private ApplicationUserManager _userManager;
        string AdminEmail = WebConfigurationManager.AppSettings["AdminEmail"];
        string ZimnatEmail = WebConfigurationManager.AppSettings["ZimnatEmail"];
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
            ViewBag.Countries = resultt.countries.OrderBy(x => x.code.Replace("+", ""));

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
                customerModel.Zipcode = "00263";
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
            var model = new PolicyDetailModel();
            var InsService = new InsurerService();
            model.CurrencyId = InsuranceContext.Currencies.All().FirstOrDefault().Id;
            model.PolicyStatusId = InsuranceContext.PolicyStatuses.All().FirstOrDefault().Id;
            model.BusinessSourceId = InsuranceContext.BusinessSources.All().FirstOrDefault().Id;
            //model.Products = InsuranceContext.Products.All().ToList();
            model.InsurerId = InsService.GetInsurers().FirstOrDefault().Id;
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
                model.PolicyNumber = ViewBag.PolicyNumber;
            }
            else
            {
                ViewBag.PolicyNumber = ConfigurationManager.AppSettings["PolicyNumber"] + "-1";
                model.PolicyNumber = ViewBag.PolicyNumber;
            }

            model.BusinessSourceId = 3;

            Session["PolicyData"] = Mapper.Map<PolicyDetailModel, PolicyDetail>(model);

            if (User != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Staff"))
                {
                    return RedirectToAction("RiskDetail", "ContactCentre");
                }
            }


            return RedirectToAction("RiskDetail");
        }
        [HttpPost]
        public JsonResult SavePolicyData(PolicyDetailModel model)
        {

            JsonResult json = new JsonResult();
            var response = new Response();
            try
            {


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

            if (Session["CustomerDataModal"] == null)
            {
                // return RedirectToAction("Index", "CustomerRegistration");
                return Redirect("/CustomerRegistration/Index");
            }


            ViewBag.Products = InsuranceContext.Products.All().ToList();
            //var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm))
            //                       select new
            //                       {
            //                           ID = (int)e,
            //                           Name = e.ToString()
            //                       };

            //ViewBag.ePaymentTermData = new SelectList(ePaymentTermData, "ID", "Name");

            // ViewBag.PaymentTermId = InsuranceContext.PaymentTerms.All().ToList();
            ViewBag.PaymentTermId = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is null").ToList();

            int RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
            var PolicyData = (PolicyDetail)Session["PolicyData"];
            //Id is policyid from Policy detail table
            var viewModel = new RiskDetailModel();
            var service = new VehicleService();

            ViewBag.VehicleUsage = service.GetAllVehicleUsage();

            viewModel.VehicleUsage = 0;
            viewModel.NumberofPersons = 0;
            viewModel.AddThirdPartyAmount = 0.00m;
            viewModel.RadioLicenseCost = Convert.ToDecimal(RadioLicenseCosts);
            var makers = service.GetMakers();
            ViewBag.CoverType = service.GetCoverType().Where(x => x.Name.Contains("Third Party")).ToList();
            ViewBag.AgentCommission = service.GetAgentCommission();
            ViewBag.Makers = makers;
            viewModel.isUpdate = false;
            viewModel.isWebUser = true;

            viewModel.AnnualRiskPremium = 0.00m;
            viewModel.TermlyRiskPremium = 0.00m;
            viewModel.QuaterlyRiskPremium = 0.00m;

            viewModel.ChasisNumber = "0";
            viewModel.CubicCapacity = 0;
            viewModel.EngineNumber = "0";
            viewModel.Excess = 0.00m;
            viewModel.ExcessAmount = 0.00m;
            viewModel.ExcessBuyBack = false;
            viewModel.ExcessBuyBackAmount = 0.00m;
            viewModel.ExcessBuyBackPercentage = 0.00m;
            viewModel.ExcessType = 0;
            viewModel.MedicalExpenses = false;
            viewModel.MedicalExpensesAmount = 0.00m;
            viewModel.MedicalExpensesPercentage = 0.00m;
            viewModel.PassengerAccidentCover = false;
            viewModel.PassengerAccidentCoverAmount = 0.00m;
            viewModel.PassengerAccidentCoverAmountPerPerson = 0.00m;
            viewModel.RoadsideAssistance = false;
            viewModel.RoadsideAssistanceAmount = 0.00m;
            viewModel.RoadsideAssistancePercentage = 0.00m;

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
                if (list != null && list.Count > 0 && (list.Count >= id))
                {
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
                        viewModel.StampDuty = Math.Round(Convert.ToDecimal(data.StampDuty), 2);
                        viewModel.SumInsured = (int)Math.Round(data.SumInsured == null ? 0 : data.SumInsured.Value, 0);
                        viewModel.VehicleColor = data.VehicleColor;
                        viewModel.VehicleUsage = data.VehicleUsage;
                        viewModel.VehicleYear = data.VehicleYear;
                        viewModel.Id = data.Id;
                        viewModel.ZTSCLevy = Math.Round(Convert.ToDecimal(data.ZTSCLevy), 2);
                        viewModel.NumberofPersons = data.NumberofPersons;
                        viewModel.PassengerAccidentCover = data.PassengerAccidentCover;
                        viewModel.IsLicenseDiskNeeded = data.IsLicenseDiskNeeded;
                        viewModel.ExcessBuyBack = data.ExcessBuyBack;
                        viewModel.RoadsideAssistance = data.RoadsideAssistance;
                        viewModel.MedicalExpenses = data.MedicalExpenses;
                        viewModel.Addthirdparty = data.Addthirdparty;
                        viewModel.AddThirdPartyAmount = Math.Round(Convert.ToDecimal(data.AddThirdPartyAmount), 2);
                        viewModel.ExcessAmount = Math.Round(Convert.ToDecimal(data.ExcessAmount), 2);
                        viewModel.ExcessAmount = Math.Round(Convert.ToDecimal(data.ExcessAmount), 2);
                        viewModel.ExcessBuyBackAmount = Math.Round(Convert.ToDecimal(data.ExcessBuyBackAmount), 2);
                        viewModel.MedicalExpensesAmount = Math.Round(Convert.ToDecimal(data.MedicalExpensesAmount), 2);
                        viewModel.MedicalExpensesPercentage = Math.Round(Convert.ToDecimal(data.MedicalExpensesPercentage), 2);
                        viewModel.PassengerAccidentCoverAmount = Math.Round(Convert.ToDecimal(data.PassengerAccidentCoverAmount), 2);
                        viewModel.PassengerAccidentCoverAmountPerPerson = Math.Round(Convert.ToDecimal(data.PassengerAccidentCoverAmountPerPerson), 2);
                        viewModel.PaymentTermId = data.PaymentTermId;
                        viewModel.ProductId = data.ProductId;
                        viewModel.IncludeRadioLicenseCost = data.IncludeRadioLicenseCost;
                        viewModel.RenewalDate = data.RenewalDate;
                        viewModel.TransactionDate = data.TransactionDate;
                        viewModel.AnnualRiskPremium = Math.Round(Convert.ToDecimal(data.AnnualRiskPremium), 2);
                        viewModel.TermlyRiskPremium = Math.Round(Convert.ToDecimal(data.TermlyRiskPremium), 2);
                        viewModel.QuaterlyRiskPremium = Math.Round(Convert.ToDecimal(data.QuaterlyRiskPremium), 2);
                        viewModel.Discount = Math.Round(Convert.ToDecimal(data.Discount), 2);
                        viewModel.VehicleLicenceFee = Convert.ToDecimal(data.VehicleLicenceFee);

                        viewModel.isUpdate = true;
                        viewModel.vehicleindex = Convert.ToInt32(id);

                        var ser = new VehicleService();
                        var model = ser.GetModel(data.MakeId);
                        ViewBag.Model = model;
                    }
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
                model.Id = 0;

                //if (!model.IncludeRadioLicenseCost)
                //{
                //    model.RadioLicenseCost = 0.00m;
                //}

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
                        model.Id = 0;

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
                        model.Id = 0;

                        //if (!model.IncludeRadioLicenseCost)
                        //{
                        //    model.RadioLicenseCost = 0.00m;
                        //}


                        List<RiskDetailModel> listriskdetailmodel = new List<RiskDetailModel>();
                        if (Session["VehicleDetails"] != null)
                        {
                            List<RiskDetailModel> listriskdetails = (List<RiskDetailModel>)Session["VehicleDetails"];
                            if (listriskdetails != null && listriskdetails.Count > 0)
                            {
                                listriskdetailmodel = listriskdetails;
                            }
                        }
                        model.Id = 0;
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
            if (Session["CustomerDataModal"] == null)
            {
                // return RedirectToAction("Index", "CustomerRegistration");
                return Redirect("/CustomerRegistration/Index");
            }

            if (Session["VehicleDetails"] == null)
            {
                //return RedirectToAction("RiskDetail", "CustomerRegistration");
                return Redirect("/CustomerRegistration/RiskDetail");
            }



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
            var DiscountSettings = InsuranceContext.Settings.Single(where: $"keyname='Discount On Renewal'");
            model.CarInsuredCount = vehicle.Count;
            model.DebitNote = "INV" + Convert.ToString(SummaryDetailServiceObj.getNewDebitNote());
            model.PaymentMethodId = 1;
            model.PaymentTermId = 1;
            model.ReceiptNumber = "";
            model.SMSConfirmation = false;
            //model.TotalPremium = vehicle.Sum(item => item.Premium + item.ZTSCLevy + item.StampDuty + item.RadioLicenseCost);
            model.TotalPremium = 0.00m;
            model.TotalRadioLicenseCost = 0.00m;
            model.Discount = 0.00m;
            foreach (var item in vehicle)
            {
                model.TotalPremium += item.Premium + item.ZTSCLevy + item.StampDuty;
                if (item.IncludeRadioLicenseCost)
                {
                    model.TotalPremium += item.RadioLicenseCost;
                    model.TotalRadioLicenseCost += item.RadioLicenseCost;
                }
                model.Discount += item.Discount;
                //if (DiscountSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                //{
                //    var amountToCalculateDiscount = 0.00m;
                //    switch (item.PaymentTermId)
                //    {
                //        case 1:
                //            amountToCalculateDiscount = Convert.ToDecimal(item.AnnualRiskPremium);
                //            break;
                //        case 3:
                //            amountToCalculateDiscount = Convert.ToDecimal(item.QuaterlyRiskPremium);
                //            break;
                //        case 4:
                //            amountToCalculateDiscount = Convert.ToDecimal(item.TermlyRiskPremium);
                //            break;
                //    }
                //    model.Discount += ((Convert.ToDecimal(DiscountSettings.value) * amountToCalculateDiscount) / 100);
                //}
                //if (DiscountSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                //{
                //    model.Discount += Convert.ToDecimal(DiscountSettings.value);
                //}
            }
            model.TotalRadioLicenseCost = Math.Round(Convert.ToDecimal(model.TotalRadioLicenseCost), 2);
            model.Discount = Math.Round(Convert.ToDecimal(model.Discount), 2);
            model.TotalPremium = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);
            model.TotalStampDuty = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.StampDuty)), 2);
            model.TotalSumInsured = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.SumInsured)), 2);
            model.TotalZTSCLevies = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.ZTSCLevy)), 2);
            model.ExcessBuyBackAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.ExcessBuyBackAmount)), 2);
            model.MedicalExpensesAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.MedicalExpensesAmount)), 2);
            model.PassengerAccidentCoverAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.PassengerAccidentCoverAmount)), 2);
            model.RoadsideAssistanceAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.RoadsideAssistanceAmount)), 2);
            model.ExcessAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.ExcessAmount)), 2);
            model.AmountPaid = 0.00m;
            model.MaxAmounttoPaid = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);
            var vehiclewithminpremium = vehicle.OrderBy(x => x.Premium).FirstOrDefault();
            model.MinAmounttoPaid = Math.Round(Convert.ToDecimal(vehiclewithminpremium.Premium + vehiclewithminpremium.StampDuty + vehiclewithminpremium.ZTSCLevy + (Convert.ToBoolean(vehiclewithminpremium.IncludeRadioLicenseCost) ? vehiclewithminpremium.RadioLicenseCost : 0.00m)), 2);
            model.AmountPaid = Convert.ToDecimal(model.TotalPremium);
            model.BalancePaidDate = DateTime.Now;
            model.Notes = "";
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
            if (model != null)
            {
                //if (ModelState.IsValid && (model.AmountPaid >= model.MinAmounttoPaid && model.AmountPaid <= model.MaxAmounttoPaid))
                if (ModelState.IsValid)
                {
                    #region Add All info to database

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
                                    try
                                    {
                                        var roleresult = UserManager.AddToRole(user.Id, "Customer");
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


                    // Genrate new policy number
                    string policyNumber = string.Empty;

                    var objList = InsuranceContext.PolicyDetails.All(orderBy: "Id desc").FirstOrDefault();
                    if (objList != null)
                    {
                        string number = objList.PolicyNumber.Split('-')[0].Substring(4, objList.PolicyNumber.Length - 6);
                        long pNumber = Convert.ToInt64(number.Substring(2, number.Length - 2)) + 1;
                       
                        int length = 7;
                        length = length - pNumber.ToString().Length;
                        for (int i = 0; i < length; i++)
                        {
                            policyNumber += "0";
                        }
                        policyNumber += pNumber;
                        policy.PolicyNumber = "GMCC" + DateTime.Now.Year.ToString().Substring(2, 2) + policyNumber + "-1";
                       
                    }
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
                            Session["PolicyData"] = policy;
                        }
                        else
                        {
                            PolicyDetail policydata = InsuranceContext.PolicyDetails.All(policy.Id.ToString()).FirstOrDefault();
                            policydata.BusinessSourceId = policy.BusinessSourceId;
                            policydata.CurrencyId = policy.CurrencyId;
                            policydata.CustomerId = policy.CustomerId;
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
                                //if (model.AmountPaid < model.TotalPremium)
                                //{
                                //    _item.BalanceAmount = (_item.Premium + _item.ZTSCLevy + _item.StampDuty + (_item.IncludeRadioLicenseCost ? _item.RadioLicenseCost : 0.00m) - _item.Discount) - (model.AmountPaid / vehicle.Count);
                                //}

                                _item.Id = service.AddVehicleInformation(_item);
                                var vehicles = (List<RiskDetailModel>)Session["VehicleDetails"];
                                vehicles[Convert.ToInt32(_item.NoOfCarsCovered) - 1] = _item;
                                Session["VehicleDetails"] = vehicles;


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
                                    if (Reinsurance.MinTreatyCapacity <= item.SumInsured && item.SumInsured <= Reinsurance.MaxTreatyCapacity)
                                    {
                                        ReinsuranceCase = Reinsurance;
                                        break;
                                    }
                                }

                                if (ReinsuranceCase != null && ReinsuranceCase.MaxTreatyCapacity != null)
                                {
                                    var basicPremium = item.Premium;
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
                                        _reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((_reinsurance.ReinsuranceAmount / item.SumInsured) * basicPremium), 2);
                                        AutoFacPremium = Convert.ToDecimal(_reinsurance.ReinsurancePremium);
                                        _reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(autofacReinsuranceBroker.Commission);
                                        _reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((_reinsurance.ReinsurancePremium * _reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                        _reinsurance.VehicleId = item.Id;
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
                                        __reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((__reinsurance.ReinsuranceAmount / item.SumInsured) * basicPremium), 2);
                                        FacPremium = Convert.ToDecimal(__reinsurance.ReinsurancePremium);
                                        __reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                                        __reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((__reinsurance.ReinsurancePremium * __reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                        __reinsurance.VehicleId = item.Id;
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
                                        reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((reinsurance.ReinsuranceAmount / item.SumInsured) * basicPremium), 2);
                                        AutoFacPremium = Convert.ToDecimal(reinsurance.ReinsurancePremium);
                                        reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                                        reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((reinsurance.ReinsurancePremium * reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                        reinsurance.VehicleId = item.Id;
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
                                    VehicleModel vehiclemodel = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                                    VehicleMake vehiclemake = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'");

                                    string vehicledescription = vehiclemodel.ModelDescription + " / " + vehiclemake.MakeDescription;

                                    SummeryofVehicleInsured += "<tr><td>" + vehicledescription + "</td><td>" + Convert.ToString(item.SumInsured) + "</td><td>" + Convert.ToString(item.Premium) + "</td><td>" + AutoFacSumInsured.ToString() + "</td><td>" + AutoFacPremium.ToString() + "</td><td>" + FacSumInsured.ToString() + "</td><td>" + FacPremium.ToString() + "</td></tr>";


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
                                Vehicledata.RadioLicenseCost = (item.IsLicenseDiskNeeded ? item.RadioLicenseCost : 0.00m);
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
                                Vehicledata.AnnualRiskPremium = item.AnnualRiskPremium;
                                Vehicledata.TermlyRiskPremium = item.TermlyRiskPremium;
                                Vehicledata.QuaterlyRiskPremium = item.QuaterlyRiskPremium;

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
                            //DbEntry.PaymentTermId = Convert.ToInt32(Session["policytermid"]);
                            //DbEntry.VehicleDetailId = vehicle[0].Id;
                            DbEntry.CustomerId = vehicle[0].CustomerId;
                            DbEntry.CreatedBy = customer.Id;
                            DbEntry.CreatedOn = DateTime.Now;
                            if (DbEntry.BalancePaidDate.Value.Year == 0001)
                            {
                                DbEntry.BalancePaidDate = DateTime.Now;
                            }
                            if (DbEntry.Notes == null)
                            {
                                DbEntry.Notes = "";
                            }
                            InsuranceContext.SummaryDetails.Insert(DbEntry);
                            model.Id = DbEntry.Id;
                            Session["SummaryDetailed"] = model;
                        }
                        else
                        {
                            SummaryDetail summarydata = InsuranceContext.SummaryDetails.All(summary.Id.ToString()).FirstOrDefault();

                            //summarydata.PaymentTermId = Convert.ToInt32(Session["policytermid"]);
                            //summarydata.VehicleDetailId = vehicle[0].Id;
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
                            summarydetails.CreatedBy = customer.Id;
                            summarydetails.CreatedOn = DateTime.Now;
                            InsuranceContext.SummaryVehicleDetails.Insert(summarydetails);


                        }

                        MiscellaneousService.UpdateBalanceForVehicles(summary.AmountPaid, summary.Id, Convert.ToDecimal(summary.TotalPremium), false);

                    }

                    if (listReinsuranceTransaction != null && listReinsuranceTransaction.Count > 0)
                    {
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
                                    var Body = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured);
                                    objEmailService.SendEmail(ZimnatEmail, "", "", "Reinsurance Case: " + policy.PolicyNumber.ToString(), Body, null);
                                    MailSent = true;
                                    MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case");
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
                                var Body = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured);
                                objEmailService.SendEmail(ZimnatEmail, "", "", "Reinsurance Case: " + policy.PolicyNumber.ToString(), Body, null);
                                MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case");
                                //MiscellaneousService.ScheduleMotorPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case- " + policy.PolicyNumber.ToString(), item.VehicleId);
                            }
                        }
                    }

                    #endregion

                    if (model.PaymentMethodId == 1)
                        return RedirectToAction("SaveDetailList", "Paypal", new { id = DbEntry.Id });
                    if (model.PaymentMethodId == 3)
                        return RedirectToAction("InitiatePaynowTransaction", "Paypal", new { id = DbEntry.Id, TotalPremiumPaid = Convert.ToString(model.AmountPaid), PolicyNumber = policy.PolicyNumber, Email = customer.EmailAddress });
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

        [HttpPost]
        public JsonResult CalculatePremium(int vehicleUsageId, decimal sumInsured, int coverType, int excessType, decimal excess, decimal? AddThirdPartyAmount, int NumberofPersons, Boolean Addthirdparty, Boolean PassengerAccidentCover, Boolean ExcessBuyBack, Boolean RoadsideAssistance, Boolean MedicalExpenses, decimal? RadioLicenseCost, Boolean IncludeRadioLicenseCost, int policytermid, Boolean isVehicleRegisteredonICEcash, string BasicPremium, string StampDuty, string ZTSCLevy)
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
            var premium = quote.CalculatePremium(vehicleUsageId, sumInsured, typeCover, eexcessType, excess, policytermid, AddThirdPartyAmount, NumberofPersons, Addthirdparty, PassengerAccidentCover, ExcessBuyBack, RoadsideAssistance, MedicalExpenses, RadioLicenseCost, IncludeRadioLicenseCost, isVehicleRegisteredonICEcash, BasicPremium, StampDuty, ZTSCLevy);
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            json.Data = premium;
            return json;
        }

        [HttpPost]
        public JsonResult CheckDuplicateRegisterationNumberExist(string regNo)
        {
            JsonResult json = new JsonResult();
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            json.Data = false;

            var list = (List<RiskDetailModel>)Session["VehicleDetails"];

            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {
                    if (item.RegistrationNo.Trim().ToLower() == regNo.Trim().ToLower())
                        json.Data = true;
                }
            }

            return json;
        }

        [HttpPost]
        public JsonResult checkVRNwithICEcash(string regNo, string PaymentTerm)
        {
            checkVRNwithICEcashResponse response = new checkVRNwithICEcashResponse();
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
                ResultRootObject quoteresponse = ICEcashService.checkVehicleExists(objVehicles, tokenObject.Response.PartnerToken);
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
            checkVRNwithICEcashResponse response = new checkVRNwithICEcashResponse();
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
                ResultRootObject quoteresponse = ICEcashService.RequestQuote(tokenObject.Response.PartnerToken, regNo, SumInsured, make, model, Convert.ToInt32(PaymentTerm), VehicleYear, CoverTypeId, VehicleUsage);
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
        public JsonResult GetVehicleModel(string makeCode)
        {
            var service = new VehicleService();
            var model = service.GetModel(makeCode);
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = model;
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public JsonResult GetVehicleUsage(string ProductId)
        {
            var service = new VehicleService();
            var model = service.GetVehicleUsage(ProductId).Select(x => new VehicleUsageModel { VehUsage = x.VehUsage, Id = x.Id }).ToList();
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

        public JsonResult getproductidbyvehicleusage(int vehicleusageId)
        {
            try
            {
                var ProductId = InsuranceContext.VehicleUsages.Single(vehicleusageId).ProductId;
                return Json(ProductId, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
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

        //public class LicenseAddress
        //{
        //    public string Address1 { get; set; }
        //    public string Address2 { get; set; }
        //    public string City { get; set; }
        //}



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

        public class City
        {
            public string name { get; set; }
        }

        public class RootObjects
        {
            public List<City> cities { get; set; }
        }

        public class checkVRNwithICEcashResponse
        {
            public int result { get; set; }
            public string message { get; set; }
            public ResultRootObject Data { get; set; }
        }

    }
}



