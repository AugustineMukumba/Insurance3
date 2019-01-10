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

        public ActionResult RiskDetail(int? Id)
        {
            Session["RenewVehicleId"] = Id;

            var policyid = InsuranceContext.VehicleDetails.Single(Id).PolicyId;
            var policy = InsuranceContext.PolicyDetails.Single(policyid);

            Session["RenewVehiclePolicy"] = policy;

            var lapsedvehicleid = (int)Session["RenewVehicleId"];
            var SummaryDetailId = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId = {lapsedvehicleid}").SummaryDetailId;
            var summary = InsuranceContext.SummaryDetails.Single(SummaryDetailId);
            //Session["policytermid"] = summary.PaymentTermId;
            int RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
            var LapsedVehiclePolicy = (PolicyDetail)Session["RenewVehiclePolicy"];
            //Id is policyid from Policy detail table
            var viewModel = new RiskDetailModel();
            var service = new VehicleService();

            ViewBag.VehicleUsage = service.GetAllVehicleUsage();

            viewModel.NumberofPersons = 0;
            viewModel.AddThirdPartyAmount = 0.00m;
            viewModel.RadioLicenseCost = Convert.ToDecimal(RadioLicenseCosts);
            var makers = service.GetMakers();
            ViewBag.CoverType = service.GetCoverType();
            ViewBag.AgentCommission = service.GetAgentCommission();
            ViewBag.Makers = makers;
            viewModel.isUpdate = false;
            ViewBag.Products = InsuranceContext.Products.All().ToList();
            //var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm))
            //                       select new
            //                       {
            //                           ID = (int)e,
            //                           Name = e.ToString()
            //                       };

            //ViewBag.ePaymentTermData = new SelectList(ePaymentTermData, "ID", "Name");


            ViewBag.PaymentTermId = InsuranceContext.PaymentTerms.All().ToList();

            var eExcessTypeData = from eExcessType e in Enum.GetValues(typeof(eExcessType))
                                  select new
                                  {
                                      ID = (int)e,
                                      Name = e.ToString()
                                  };

            ViewBag.eExcessTypeData = new SelectList(eExcessTypeData, "ID", "Name");




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
                    viewModel.VehicleUsage = Convert.ToInt32(data.VehicleUsage);
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
                    //viewModel.isUpdate = true;
                    viewModel.isUpdate = false;

                    viewModel.vehicleindex = Convert.ToInt32(0);
                    viewModel.PaymentTermId = data.PaymentTermId;
                    viewModel.ProductId = data.ProductId;
                    viewModel.IncludeRadioLicenseCost = Convert.ToBoolean(data.IncludeRadioLicenseCost);
                    viewModel.RenewalDate = Convert.ToDateTime(data.RenewalDate);
                    viewModel.CustomerId = data.CustomerId;
                    viewModel.PolicyId = data.PolicyId;
                    viewModel.AnnualRiskPremium = data.AnnualRiskPremium;
                    viewModel.TermlyRiskPremium = data.TermlyRiskPremium;
                    viewModel.QuaterlyRiskPremium = data.QuaterlyRiskPremium;
                    viewModel.Discount = data.Discount;
                    viewModel.VehicleLicenceFee = Convert.ToDecimal(data.VehicleLicenceFee);

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

            DateTimeFormatInfo usDtfi = new CultureInfo("en-US", false).DateTimeFormat;
            var service = new RiskDetailService();

            if (ModelState.IsValid)
            {
                model.Id = 0;
                if (model.RenewalDate < model.CoverEndDate)
                {
                    model.RenewalDate = model.CoverEndDate.Value.AddDays(1);
                }
                Session["RenewVehicleDetails"] = model;
            }

            return RedirectToAction("SummaryDetail");
        }

        public ActionResult SummaryDetail()
        {
            var Id = (int)Session["RenewVehicleId"];
            var summaryID = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId={Id}").SummaryDetailId;
            var summary = InsuranceContext.SummaryDetails.Single(summaryID);
            if (Session["RenewVehicleSummary"] == null)
            {
                Session["RenewVehicleSummary"] = summary;
            }
            var vehicle = InsuranceContext.VehicleDetails.Single(Id);
            Session["RenewdVehicle"] = vehicle;
            SummaryDetailService SummaryDetailServiceObj = new SummaryDetailService();

            var model = new SummaryDetailModel();

            if (Session["RenewVehicleDetails"] != null)
            {
                var _vehicle = (RiskDetailModel)Session["RenewVehicleDetails"];

                model.CarInsuredCount = 1;
                model.DebitNote = summary.DebitNote;
                if (Session["RenewVehicleSummary"] != null)
                {
                    model.PaymentMethodId = ((SummaryDetail)Session["RenewVehicleSummary"]).PaymentMethodId;
                }
                else
                {
                    model.PaymentMethodId = 1;
                }
                model.PaymentTermId = summary.PaymentTermId;
                model.ReceiptNumber = "";
                model.SMSConfirmation = false;
                model.TotalPremium = _vehicle.Premium + _vehicle.ZTSCLevy + _vehicle.StampDuty + (Convert.ToBoolean(_vehicle.IncludeRadioLicenseCost) ? _vehicle.RadioLicenseCost : 0.00m);// - _vehicle.Discount;// + vehicle.StampDuty + vehicle.ZTSCLevy;
                model.TotalRadioLicenseCost = _vehicle.RadioLicenseCost;
                model.TotalStampDuty = _vehicle.StampDuty;
                model.TotalSumInsured = _vehicle.SumInsured;
                model.TotalZTSCLevies = _vehicle.ZTSCLevy;
                model.ExcessBuyBackAmount = _vehicle.ExcessBuyBackAmount;
                model.MedicalExpensesAmount = _vehicle.MedicalExpensesAmount;
                model.PassengerAccidentCoverAmount = _vehicle.PassengerAccidentCoverAmount;
                model.RoadsideAssistanceAmount = _vehicle.RoadsideAssistanceAmount;
                model.Discount = _vehicle.Discount;
                model.ExcessAmount = _vehicle.ExcessAmount;
            }
            else
            {
                model.CarInsuredCount = 1;
                model.DebitNote = summary.DebitNote;
                if (Session["RenewVehicleSummary"] != null)
                {
                    model.PaymentMethodId = ((SummaryDetail)Session["RenewVehicleSummary"]).PaymentMethodId;
                }
                else
                {
                    model.PaymentMethodId = 1;
                }
                model.PaymentTermId = summary.PaymentTermId;
                model.ReceiptNumber = "";
                model.SMSConfirmation = false;
                model.TotalPremium = vehicle.Premium + vehicle.ZTSCLevy + vehicle.StampDuty + (Convert.ToBoolean(vehicle.IncludeRadioLicenseCost) ? vehicle.RadioLicenseCost : 0.00m);// - vehicle.Discount;// + vehicle.StampDuty + vehicle.ZTSCLevy;
                model.TotalRadioLicenseCost = vehicle.RadioLicenseCost;
                model.TotalStampDuty = vehicle.StampDuty;
                model.TotalSumInsured = vehicle.SumInsured;
                model.TotalZTSCLevies = vehicle.ZTSCLevy;
                model.ExcessBuyBackAmount = vehicle.ExcessBuyBackAmount;
                model.MedicalExpensesAmount = vehicle.MedicalExpensesAmount;
                model.PassengerAccidentCoverAmount = vehicle.PassengerAccidentCoverAmount;
                model.RoadsideAssistanceAmount = vehicle.RoadsideAssistanceAmount;
                model.Discount = vehicle.Discount;
                model.ExcessAmount = vehicle.ExcessAmount;
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
        public async Task<ActionResult> SubmitPlan(SummaryDetailModel model)
        {

            var customer = InsuranceContext.Customers.Single(where: $"UserId='{User.Identity.GetUserId().ToString()}'");

            var policy = (PolicyDetail)Session["RenewVehiclePolicy"];
            //var vehicle = (RiskDetailModel)Session["LapsedVehicle"];
            var summary = (SummaryDetail)Session["RenewVehicleSummary"];
            summary.PaymentMethodId = model.PaymentMethodId;
            Session["RenewVehicleSummary"] = summary;

            if (Session["RenewVehicleDetails"] != null)
            {
                var service = new RiskDetailService();
                var item = (RiskDetailModel)Session["RenewVehicleDetails"];
                item.CustomerId = summary.CustomerId;
                item.PolicyId = item.PolicyId;
                //var vehical = Mapper.Map<RiskDetailModel, RiskDetailModel>(vehicle);
                item.Id = service.AddVehicleInformation(item);
                Session["NewVehicleId"] = item.Id;
                Session["RenewVehicleDetails"] = item;

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

                    if (ReinsuranceCase.MinTreatyCapacity > 200000)
                    {
                        var autofaccase = ReinsuranceCases.FirstOrDefault();
                        var autofacSumInsured = autofaccase.MaxTreatyCapacity - ownRetention;
                        var autofacReinsuranceBroker = InsuranceContext.ReinsuranceBrokers.Single(where: $"ReinsuranceBrokerCode='{autofaccase.ReinsuranceBrokerCode}'");

                        var _reinsurance = new ReinsuranceTransaction();
                        _reinsurance.ReinsuranceAmount = autofacSumInsured;
                        _reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((_reinsurance.ReinsuranceAmount / item.SumInsured) * basicPremium), 2);
                        _reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(autofacReinsuranceBroker.Commission);
                        _reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((_reinsurance.ReinsurancePremium * _reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                        _reinsurance.VehicleId = item.Id;
                        _reinsurance.ReinsuranceBrokerId = autofacReinsuranceBroker.Id;
                        _reinsurance.TreatyName = autofaccase.TreatyName;
                        _reinsurance.TreatyCode = autofaccase.TreatyCode;
                        _reinsurance.SummaryDetailId = summary.Id;

                        InsuranceContext.ReinsuranceTransactions.Insert(_reinsurance);

                        //SummeryofReinsurance += "<tr><td>" + Convert.ToString(_reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(_reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(_reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";

                        //listReinsuranceTransaction.Add(_reinsurance);

                        var __reinsurance = new ReinsuranceTransaction();
                        __reinsurance.ReinsuranceAmount = item.SumInsured - ownRetention - autofacSumInsured;
                        __reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((__reinsurance.ReinsuranceAmount / item.SumInsured) * basicPremium), 2);
                        __reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                        __reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((__reinsurance.ReinsurancePremium * __reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                        __reinsurance.VehicleId = item.Id;
                        __reinsurance.ReinsuranceBrokerId = ReinsuranceBroker.Id;
                        __reinsurance.TreatyName = ReinsuranceCase.TreatyName;
                        __reinsurance.TreatyCode = ReinsuranceCase.TreatyCode;
                        __reinsurance.SummaryDetailId = summary.Id;

                        InsuranceContext.ReinsuranceTransactions.Insert(__reinsurance);

                        //SummeryofReinsurance += "<tr><td>" + Convert.ToString(__reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(__reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(__reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";

                        //listReinsuranceTransaction.Add(__reinsurance);
                    }
                    else
                    {

                        var reinsurance = new ReinsuranceTransaction();
                        reinsurance.ReinsuranceAmount = item.SumInsured - ownRetention;
                        reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((reinsurance.ReinsuranceAmount / item.SumInsured) * basicPremium), 2);
                        reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                        reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((reinsurance.ReinsurancePremium * reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                        reinsurance.VehicleId = item.Id;
                        reinsurance.ReinsuranceBrokerId = ReinsuranceBroker.Id;
                        reinsurance.TreatyName = ReinsuranceCase.TreatyName;
                        reinsurance.TreatyCode = ReinsuranceCase.TreatyCode;
                        reinsurance.SummaryDetailId = summary.Id;

                        InsuranceContext.ReinsuranceTransactions.Insert(reinsurance);

                        //SummeryofReinsurance += "<tr><td>" + Convert.ToString(reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";

                        //listReinsuranceTransaction.Add(reinsurance);
                    }


                    Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                    VehicleModel vehiclemodel = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                    VehicleMake vehiclemake = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'");

                    string vehicledescription = vehiclemodel.ModelDescription + " / " + vehiclemake.MakeDescription;

                    //SummeryofVehicleInsured += "<tr><td>" + vehicledescription + "</td><td>" + item.RegistrationNo + "</td><td>" + (item.CoverTypeId == 1 ? eCoverType.Comprehensive.ToString() : (item.CoverTypeId == 2 ? eCoverType.ThirdParty.ToString() : eCoverType.FullThirdParty.ToString())) + "</td><td>" + Convert.ToString(item.SumInsured) + "</td><td>" + Convert.ToString(item.Premium) + "</td></tr>";


                }

                SummaryVehicleDetail objSummaryVehicleDetails = new SummaryVehicleDetail();
                objSummaryVehicleDetails.SummaryDetailId = summary.Id;
                objSummaryVehicleDetails.VehicleDetailsId = item.Id;
                InsuranceContext.SummaryVehicleDetails.Insert(objSummaryVehicleDetails);
            }

            if (model.PaymentMethodId == 1)
                return RedirectToAction("SaveDetailList", "Renew", new { id = summary.Id });
            if (model.PaymentMethodId == 3)
                return RedirectToAction("InitiatePaynowTransaction", "Renew", new { id = summary.Id, TotalPremiumPaid = Convert.ToString(summary.TotalPremium), PolicyNumber = policy.PolicyNumber, Email = User.Identity.GetUserName() });
            else
                return RedirectToAction("PaymentDetail", new { id = summary.Id });
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
            var vehicleId = (int)Session["RenewVehicleId"];
            var PaymentId = Session["PaymentId"];
            var InvoiceId = Session["InvoiceId"];
            var summary = (SummaryDetail)Session["RenewVehicleSummary"];
            var policy = (PolicyDetail)Session["RenewVehiclePolicy"];
            var DebitNote = summary.DebitNote;
            var vehicle = InsuranceContext.VehicleDetails.Single(vehicleId);
            PaymentInformation objSaveDetailListModel = new PaymentInformation();
            string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
            if (Session["RenewVehicleDetails"] != null)
            {

                vehicle.isLapsed = true;
                InsuranceContext.VehicleDetails.Update(vehicle);

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

            var data = (List<Item>)Session["itemData"];
            if (data != null)
            {
                var totalprem = data.Sum(x => Convert.ToDecimal(x.price));


                string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/UserPaymentEmail.cshtml";
                string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(userRegisterationEmailPath));

                var Body2 = EmailBody2.Replace("#DATE#", DateTime.Now.ToShortDateString()).Replace("##path##",filepath).Replace("#FirstName#", customer.FirstName).Replace("#LastName#", customer.LastName).Replace("#AccountName#", customer.FirstName + ", " + customer.LastName).Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2).Replace("#Amount#", Convert.ToString(summary.AmountPaid)).Replace("#PaymentDetails#", "New Premium").Replace("#ReceiptNumber#", policy.PolicyNumber).Replace("#PaymentType#", (summary.PaymentMethodId == 1 ? "Cash" : (summary.PaymentMethodId == 2 ? "PayPal" : "PayNow")));

              var attachementPath=  MiscellaneousService.EmailPdf(Body2, policy.CustomerId, policy.PolicyNumber, "Renew Payment ");

                List<string> _attachements = new List<string>();
                _attachements.Add(attachementPath);
                objEmailService.SendEmail(user.Email, "", "", "Renew " + policy.PolicyNumber + " : Payment", Body2, _attachements);
              
            }


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


            var SummaryVehicleDetail = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summary.Id}").ToList();

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


                Summeryofcover += "<tr><td>" + vehicledescription + "</td><td>$" + vehicle.SumInsured + "</td><td>" + (vehicle.CoverTypeId == 1 ? eCoverType.Comprehensive.ToString() : eCoverType.ThirdParty.ToString()) + "</td><td>" + InsuranceContext.VehicleUsages.All(Convert.ToString(vehicle.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</td><td>$0.00</td><td>$" + Convert.ToString(vehicle.Excess) + "</td><td>$" + Convert.ToString(_Premium) + "</td></tr>";

            }
            //var Premium = vehicle.Premium + vehicle.PassengerAccidentCoverAmount + vehicle.ExcessAmount + vehicle.ExcessBuyBackAmount + vehicle.MedicalExpensesAmount + vehicle.RoadsideAssistanceAmount ;


            var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
            var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == summary.PaymentTermId);
            string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/SeheduleMotor.cshtml";
            string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
            //var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (summary.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + summary.PaymentTermId.ToString() + "Months)")).Replace("##TotalPremiumDue##", Convert.ToString(totalpaymentdue)).Replace("##StampDuty##", Convert.ToString(summary.TotalStampDuty)).Replace("##MotorLevy##", Convert.ToString(summary.TotalZTSCLevies)).Replace("##PremiumDue##", Convert.ToString(summary.TotalPremium)).Replace("##PostalAddress##", customer.Zipcode);

            var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##path##",filepath).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (summary.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + summary.PaymentTermId.ToString() + "Months)")).Replace("##TotalPremiumDue##", Convert.ToString(summary.TotalPremium)).Replace("##StampDuty##", Convert.ToString(summary.TotalStampDuty)).Replace("##MotorLevy##", Convert.ToString(summary.TotalZTSCLevies)).Replace("##PremiumDue##", Convert.ToString(vehicle.Premium)).Replace("##PostalAddress##", customer.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(vehicle.ExcessBuyBackAmount)).Replace("##MedicalExpenses##", Convert.ToString(vehicle.MedicalExpensesAmount)).Replace("##PassengerAccidentCover##", Convert.ToString(vehicle.PassengerAccidentCoverAmount)).Replace("##RoadsideAssistance##", Convert.ToString(vehicle.RoadsideAssistanceAmount).Replace("##RadioLicence##", Convert.ToString(vehicle.RadioLicenseCost)).Replace("##Discount##", Convert.ToString(vehicle.Discount)));

           var attachemetPath= MiscellaneousService.EmailPdf(Bodyy, policy.CustomerId, policy.PolicyNumber, "Renew Schedule-motor");
            List<string> attachements = new List<string>();
            attachements.Add(attachemetPath);
            objEmailService.SendEmail(user.Email, "", "", "Renew " + policy.PolicyNumber + " : Schedule-motor", Bodyy, attachements);
          
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
                var Id = (int)Session["RenewVehicleId"];
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
    }



}