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
using PayPal.Api;
using System.Web.Script.Serialization;

namespace InsuranceClaim.Controllers
{
    public class RenewController : Controller
    {
        private ApplicationUserManager _userManager;
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

        //public ActionResult Index(int? Id)
        //{
        //    bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;



        //    //if (userLoggedin)
        //    //{
        //    //    var customerModel = new CustomerModel();
        //    //    var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
        //    //    var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{User.Identity.GetUserId().ToString()}'").FirstOrDefault();

        //    //    customerModel.AddressLine1 = _customerData.AddressLine1;
        //    //    customerModel.AddressLine2 = _customerData.AddressLine2;
        //    //    customerModel.City = _customerData.City;
        //    //    customerModel.Id = _customerData.Id;
        //    //    customerModel.Country = _customerData.Country;
        //    //    customerModel.Zipcode = _customerData.Zipcode;
        //    //    customerModel.Gender = _customerData.Gender;
        //    //    customerModel.PhoneNumber = _User.PhoneNumber;
        //    //    customerModel.NationalIdentificationNumber = _customerData.NationalIdentificationNumber;
        //    //    customerModel.DateOfBirth = _customerData.DateOfBirth;
        //    //    customerModel.EmailAddress = _User.Email;./
        //    //    customerModel.FirstName = _customerData.FirstName;
        //    //    customerModel.LastName = _customerData.LastName;
        //    //    customerModel.CountryCode = _customerData.Countrycode;
        //    //    customerModel.CustomerId = _customerData.CustomerId;
        //    //    customerModel.IsActive = _customerData.IsActive;
        //    //    customerModel.UserID = _customerData.UserID;

        //    //    Session["CustomerData"] = customerModel;
        //    //}

        //    return RedirectToAction("ProductDetail");

        //}



        //public ActionResult ProductDetail()
        //{
        //    var lapsedvehicleid = (int)Session["LapsedVehicleId"];





        //    return RedirectToAction("RiskDetail");
        //}

        public ActionResult RiskDetail(int? Id)
        {
            Session["LapsedVehicleId"] = Id;

            var policyid = InsuranceContext.VehicleDetails.Single(Id).PolicyId;
            var policy = InsuranceContext.PolicyDetails.Single(policyid);

            Session["LapsedVehiclePolicy"] = policy;

            var lapsedvehicleid = (int)Session["LapsedVehicleId"];
            var SummaryDetailId = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId = {lapsedvehicleid}").SummaryDetailId;
            var summary = InsuranceContext.SummaryDetails.Single(SummaryDetailId);
            //Session["policytermid"] = summary.PaymentTermId;
            int RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
            var LapsedVehiclePolicy = (PolicyDetail)Session["LapsedVehiclePolicy"];
            //Id is policyid from Policy detail table
            var viewModel = new RiskDetailModel();
            var service = new VehicleService();
            if (LapsedVehiclePolicy != null)
            {
                viewModel.CoverStartDate = LapsedVehiclePolicy.RenewalDate.Value.AddDays(1);
                viewModel.CoverEndDate = viewModel.CoverStartDate.Value.AddMonths((summary.PaymentTermId == 1 ? 12 : Convert.ToInt32(summary.PaymentTermId)));
                ViewBag.VehicleUsage = service.GetVehicleUsage(LapsedVehiclePolicy.PolicyName);
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
            //if (Session["VehicleDetails"] != null)
            //{
            //    //var list = (List<RiskDetailModel>)Session["VehicleDetails"];
            //    //viewModel.NoOfCarsCovered = list.Count + 1;
            //}

            if (Id > 0)
            {
                var data = InsuranceContext.VehicleDetails.Single(Id);
                //var data = (RiskDetailModel)list[Convert.ToInt32(id - 1)];
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
                    viewModel.NoOfCarsCovered = 0;
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
                    viewModel.IsLicenseDiskNeeded = Convert.ToBoolean(data.IsLicenseDiskNeeded);
                    viewModel.ExcessBuyBack = data.ExcessBuyBack;
                    viewModel.RoadsideAssistance = data.RoadsideAssistance;
                    viewModel.MedicalExpenses = data.MedicalExpenses;
                    viewModel.Addthirdparty = data.Addthirdparty;
                    viewModel.AddThirdPartyAmount = data.AddThirdPartyAmount;
                    viewModel.isUpdate = true;
                    viewModel.vehicleindex = Convert.ToInt32(0);

                    var ser = new VehicleService();
                    var model = ser.GetModel(data.MakeId);
                    ViewBag.Model = model;
                }
            }


            return View(viewModel);
        }


        public ActionResult SummaryDetail()
        {
            var Id = (int)Session["LapsedVehicleId"];
            var summaryID = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId={Id}").SummaryDetailId;
            var summary = InsuranceContext.SummaryDetails.Single(summaryID);
            Session["LapsedVehicleSummary"] = summary;
            var vehicle = InsuranceContext.VehicleDetails.Single(Id);
            Session["LapsedVehicle"] = vehicle;
            SummaryDetailService SummaryDetailServiceObj = new SummaryDetailService();


            var model = new SummaryDetailModel();
            //var summary = new SummaryDetailService();
            //var vehicle = (RiskDetailModel)Session["VehicleDetail"];// summary.GetVehicleInformation(id);
            model.CarInsuredCount = 1;
            model.DebitNote = summary.DebitNote;
            model.PaymentMethodId = summary.PaymentMethodId;
            model.PaymentTermId = summary.PaymentTermId;
            model.ReceiptNumber = "";
            model.SMSConfirmation = false;
            model.TotalPremium = vehicle.Premium;// + vehicle.StampDuty + vehicle.ZTSCLevy;
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

            var customer = InsuranceContext.Customers.Single(where: $"UserId='{User.Identity.GetUserId().ToString()}'");

            var policy = (PolicyDetail)Session["LapsedVehiclePolicy"];
            //var vehicle = (RiskDetailModel)Session["LapsedVehicle"];
            var summary = (SummaryDetail)Session["LapsedVehicleSummary"];

            if (model.PaymentMethodId == 1)
                return RedirectToAction("SaveDetailList", "Renew", new { id = summary.Id });
            if (model.PaymentMethodId == 3)
                return RedirectToAction("InitiatePaynowTransaction", "Renew", new { id = summary.Id, TotalPremium = Convert.ToString(summary.TotalPremium), PolicyNumber = policy.PolicyNumber, Email = User.Identity.GetUserName() });
            else
                return RedirectToAction("PaymentDetail", new { id = summary.Id });
        }

        [HttpPost]
        public JsonResult CalculatePremium(int vehicleUsageId, decimal sumInsured, int coverType, int excessType, decimal excess, decimal? AddThirdPartyAmount, int NumberofPersons, Boolean Addthirdparty, Boolean PassengerAccidentCover, Boolean ExcessBuyBack, Boolean RoadsideAssistance, Boolean MedicalExpenses, decimal? RadioLicenseCost, Boolean IncludeRadioLicenseCost,int policytermid)
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

        public async Task<ActionResult> SaveDetailList(Int32 id)
        {
            var vehicleId = (int)Session["LapsedVehicleId"];
            var PaymentId = Session["PaymentId"];
            var InvoiceId = Session["InvoiceId"];
            var summary = (SummaryDetail)Session["LapsedVehicleSummary"];
            //var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={id}").ToList();
            //var vehicle = InsuranceContext.VehicleDetails.Single(summary[0].VehicleDetailsId);
            var policy = (PolicyDetail)Session["LapsedVehiclePolicy"];
            var customer = InsuranceContext.Customers.Single(summary.CustomerId);
            var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));
            var currency = InsuranceContext.Currencies.Single(policy.CurrencyId);
            //var paymentInformations = InsuranceContext.PaymentInformations.SingleCustome(id);
            var user = UserManager.FindById(customer.UserID);
            //var user = Microsoft.AspNet.Identity.GetUserManager<ApplicationUserManager>();
            var DebitNote = summary.DebitNote;
            PaymentInformation objSaveDetailListModel = new PaymentInformation();
            objSaveDetailListModel.CurrencyId = policy.CurrencyId;
            objSaveDetailListModel.PolicyId = policy.Id;
            objSaveDetailListModel.VehicleDetailId = vehicleId;
            objSaveDetailListModel.CustomerId = summary.CustomerId.Value;
            objSaveDetailListModel.SummaryDetailId = id;
            objSaveDetailListModel.DebitNote = summary.DebitNote;
            objSaveDetailListModel.ProductId = product.Id;
            objSaveDetailListModel.PaymentId = PaymentId == null ? "" : PaymentId.ToString();
            objSaveDetailListModel.InvoiceId = InvoiceId == null ? "" : InvoiceId.ToString();
            InsuranceContext.PaymentInformations.Insert(objSaveDetailListModel);

            //Session.Remove("policytermid");
            Session.Remove("LapsedVehicleId");
            Session.Remove("PaymentId");
            Session.Remove("InvoiceId");
            Session.Remove("LapsedVehicleSummary");
            Session.Remove("LapsedVehiclePolicy");
            Session.Remove("LapsedVehicle");
            
            //if (paymentInformations == null)
            //{
            //    Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
            //    string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            //    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
            //    bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            //    InsuranceContext.PaymentInformations.Insert(objSaveDetailListModel);

            //    if (!userLoggedin)
            //    {

            //        string emailTemplatePath = "/Views/Shared/EmaiTemplates/UserRegisteration.cshtml";
            //        string EmailBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(emailTemplatePath));
            //        var Body = EmailBody.Replace(" #PolicyNumber#", policy.PolicyNumber).Replace("#TodayDate#", DateTime.Now.ToShortDateString()).Replace("#FirstName#", customer.FirstName).Replace("#LastName#", customer.LastName).Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2).Replace("#Email#", user.Email).Replace("#change#", callbackUrl);
            //        objEmailService.SendEmail(user.Email, "", "", "Account Creation", Body, null);



            //        Insurance.Service.smsService objsmsService = new Insurance.Service.smsService();

            //        string body = "Hello " + customer.FirstName + "\nWelcome to the GENE-INSURE family, we would like to simplify your life." + "\nYour policy number is : " + policy.PolicyNumber + "\nUsername is : " + user.Email + "\nYour Password : Geneinsure@123" + "\nPlease reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>" + "\nThank you once again.";
            //        var result = await objsmsService.SendSMS(customer.Countrycode.Replace("+", "") + user.PhoneNumber, body);

            //        SmsLog objsmslog = new SmsLog()
            //        {
            //            Sendto = user.PhoneNumber,
            //            Body = body,
            //            Response = result
            //        };

            //        InsuranceContext.SmsLogs.Insert(objsmslog);
            //    }

            //    var data = (List<Item>)Session["itemData"];
            //    if (data != null)
            //    {
            //        var totalprem = data.Sum(x => Convert.ToDecimal(x.price));


            //        string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/UserPaymentEmail.cshtml";
            //        string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(userRegisterationEmailPath));
            //        var Body2 = EmailBody2.Replace("#DATE#", DateTime.Now.ToShortDateString()).Replace("#FirstName#", customer.FirstName).Replace("#LastName#", customer.LastName).Replace("#AccountName#", customer.FirstName + ", " + customer.LastName).Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2).Replace("#Amount#", Convert.ToString(totalprem)).Replace("#PaymentDetails#", "New Premium").Replace("#ReceiptNumber#", policy.PolicyNumber).Replace("#PaymentType#", (summaryDetail.PaymentMethodId == 1 ? "Cash" : (summaryDetail.PaymentMethodId == 2 ? "PayPal" : "PayNow")));
            //        objEmailService.SendEmail(user.Email, "", "", "Payment", Body2, null);
            //    }



            //    decimal totalpaymentdue = 0.00m;

            //    if (summaryDetail.PaymentTermId == 1)
            //    {
            //        totalpaymentdue = (decimal)summaryDetail.TotalPremium;
            //    }
            //    else if (summaryDetail.PaymentTermId == 4)
            //    {
            //        totalpaymentdue = (decimal)summaryDetail.TotalPremium * 3;
            //    }
            //    else if (summaryDetail.PaymentTermId == 3)
            //    {
            //        totalpaymentdue = (decimal)summaryDetail.TotalPremium * 4;
            //    }


            //    string Summeryofcover = "";
            //    for (int i = 0; i < SummaryVehicleDetails.Count; i++)
            //    {
            //        var _vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[i].VehicleDetailsId);
            //        Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
            //        VehicleModel model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{_vehicle.ModelId}'");
            //        VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{_vehicle.MakeId}'");

            //        string vehicledescription = model.ModelDescription + " / " + make.MakeDescription;


            //        Summeryofcover += "<tr><td>" + vehicledescription + "</td><td>$" + _vehicle.SumInsured + "</td><td>" + (_vehicle.CoverTypeId == 1 ? eCoverType.Comprehensive.ToString() : eCoverType.ThirdParty.ToString()) + "</td><td>" + InsuranceContext.VehicleUsages.All(_vehicle.VehicleUsage).Select(x => x.VehUsage).FirstOrDefault() + "</td><td>$0.00</td><td>$" + Convert.ToString(_vehicle.Excess) + "</td><td>$" + Convert.ToString(_vehicle.Premium) + "</td></tr>";
            //    }

            //    //TempData["Summeryofcover"] = Summeryofcover;

            //    var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
            //    var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == summaryDetail.PaymentTermId);
            //    string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/SeheduleMotor.cshtml";
            //    string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
            //    var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", policy.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", policy.StartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (summaryDetail.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + summaryDetail.PaymentTermId.ToString() + "Months)")).Replace("##TotalPremiumDue##", Convert.ToString(totalpaymentdue)).Replace("##StampDuty##", Convert.ToString(summaryDetail.TotalStampDuty)).Replace("##MotorLevy##", Convert.ToString(summaryDetail.TotalZTSCLevies)).Replace("##PremiumDue##", Convert.ToString(summaryDetail.TotalPremium)).Replace("##PostalAddress##", customer.Zipcode);
            //    objEmailService.SendEmail(user.Email, "", "", "Schedule-motor", Bodyy, null);
            //}



            return View(objSaveDetailListModel);
        }

        public async Task<ActionResult> InitiatePaynowTransaction(Int32 id, string TotalPremium, string PolicyNumber, string Email)
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

            paynowresponse = await paynowservice.initiateTransaction(Convert.ToString(id), TotalPremium, PolicyNumber, Email, true);

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
            Session["CardDetail"] = model;
            //create and item for which you are taking payment
            //if you need to add more items in the list
            //Then you will need to create multiple item objects or use some loop to instantiate object
            var summaryDetail = InsuranceContext.SummaryDetails.Single(model.SummaryDetailId);
            var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={model.SummaryDetailId}").ToList();
            var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
            var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
            var customer = InsuranceContext.Customers.Single(summaryDetail.CustomerId);
            //var summaryDetail = (SummaryDetailModel)Session["SummaryDetailed"];
            //var vehicle = (RiskDetailModel)Session["VehicleDetail"];
            //var policy = (PolicyDetail)Session["PolicyData"];
            //var customer = (CustomerModel)Session["CustomerDataModal"];
            var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));
            var currency = InsuranceContext.Currencies.Single(policy.CurrencyId);

            //var paymentInformations = InsuranceContext.PaymentInformations.SingleCustome(model.SummaryDetailId);

            double totalPremium = Convert.ToDouble(summaryDetail.TotalPremium);

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
                Session["PaymentId"] = createdPayment.id;

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
            Session["InvoiceId"] = createdInvoice.id;
            createdInvoice.Send(apiContext);

            return null;
        }
    }



}