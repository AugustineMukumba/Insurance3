﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InsuranceClaim.Models;
using Insurance.Domain;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Insurance.Service;
using System.Configuration;

namespace InsuranceClaim.Controllers
{
    public class ReportController : Controller
    {

        SummaryDetailService _summaryDetailService = new SummaryDetailService();
        private ApplicationUserManager _userManager;
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
        // GET: Report
        public ActionResult ZTSCLevyReport()
        {
            //List<ZTSCLevyReportModels> listZTSCLevyreport = new List<ZTSCLevyReportModels>();
            //ListZTSCLevyReportModels _listZTSCLevyreport = new ListZTSCLevyReportModels();
            //_listZTSCLevyreport.ListZTSCreportdata = new List<ZTSCLevyReportModels>();

            //ZTSCLevyReportSeachModels model = new ZTSCLevyReportSeachModels();

            //var currencyList = _summaryDetailService.GetAllCurrency();


            //var vehicledetail = InsuranceContext.VehicleDetails.All(where: "IsActive=1").ToList();
            //foreach (var item in vehicledetail)
            //{
            //    ZTSCLevyReportModels obj = new ZTSCLevyReportModels();
            //    var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
            //    var customer = InsuranceContext.Customers.Single(item.CustomerId);
            //    var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

            //    obj.Currency = _summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId);
            //    obj.Customer_Name = customer.FirstName + " " + customer.LastName;
            //    obj.Policy_Number = policy.PolicyNumber;
            //    obj.Premium_due = Convert.ToDecimal(item.Premium);
            //    obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("dd/MM/yyy");
            //    obj.ZTSCLevy = Convert.ToDecimal(item.ZTSCLevy);

            //    listZTSCLevyreport.Add(obj);
            //}

            //model.ListZTSCreportdata = listZTSCLevyreport.OrderByDescending(x => x.Transaction_date).ToList();

            var query = "SELECT top 100 PolicyNumber, VEHICLEDETAIL.TransactionDate,Premium AS PREMIUMDUE,ZTSCLevy, FirstName AS CUSTOMERNAME,Name AS CURRENCY FROM VEHICLEDETAIL left JOIN CUSTOMER ON CUSTOMER.ID = VehicleDetail.CUSTOMERID left  JOIN POLICYDETAIL ON POLICYDETAIL.ID = VehicleDetail.POLICYID left JOIN CURRENCY ON CURRENCY.ID = VehicleDetail.CurrencyId WHERE VEHICLEDETAIL.IsActive = 1";


            ZTSCLevyReportSeachModels model = new ZTSCLevyReportSeachModels();
            List<ZTSCLevyReportModels> listZTSCLevyreport = new List<ZTSCLevyReportModels>();
            var vehicledetails = InsuranceContext.Query(query)
                           .Select(x => new ZTSCLevyReportModels
                           {
                               Policy_Number = x.PolicyNumber,
                               Customer_Name = x.CUSTOMERNAME,
                               Premium_due = x.PREMIUMDUE,
                               Transaction_date = Convert.ToDateTime(x.TransactionDate).ToString("dd/MM/yyy"),
                               ZTSCLevy = Convert.ToDecimal(x.ZTSCLevy),
                               Currency = x.CURRENCY == null ? "USD" : x.CURRENCY,
                           }).ToList();
            listZTSCLevyreport = vehicledetails;
            model.ListZTSCreportdata = listZTSCLevyreport.OrderByDescending(x => x.Transaction_date).ToList();

            return View(model);
        }


        public ActionResult InsuredVehical()
        {
            var results = new List<RiskDetailModel>();
            try
            {
                //results = (from vehicalDetials in InsuranceContext.VehicleDetails.All()
                //           join customer in InsuranceContext.Customers.All()
                //           on vehicalDetials.CustomerId equals customer.Id
                //           join make in InsuranceContext.VehicleMakes.All()
                //           on vehicalDetials.MakeId equals make.MakeCode
                //           join vehicalModel in InsuranceContext.VehicleModels.All()
                //           on vehicalDetials.ModelId equals vehicalModel.ModelCode
                //           join coverType in InsuranceContext.CoverTypes.All().ToList()
                //           on vehicalDetials.CoverTypeId equals coverType.Id
                //           join user in UserManager.Users
                //           on customer.UserID equals user.Id

                //           select new RiskDetailModel
                //           {
                //               PolicyExpireDate = vehicalDetials.CoverEndDate.Value.ToShortDateString(),
                //               CoverTypeName = coverType.Name,
                //               SumInsured = vehicalDetials.SumInsured,
                //               VechicalMake = make.MakeDescription,
                //               VechicalModel = vehicalModel.ModelDescription,
                //               VehicleYear = vehicalDetials.VehicleYear,
                //               CustomerDetails = new CustomerModel { FirstName = customer.FirstName, LastName = customer.LastName, PhoneNumber = customer.PhoneNumber, EmailAddress = user.Email }


                //           }).ToList();



                var strQuery = "select Email, customer.PhoneNumber, LastName,  FirstName, VehicleYear, ModelDescription, MakeDescription, CoverEndDate, coverType.Name, SumInsured from VehicleDetail join Customer on VehicleDetail.CustomerId=Customer.Id" +
" join VehicleMake on VehicleDetail.MakeId = VehicleMake.MakeCode " +
" join vehicleModel on VehicleDetail.ModelId = vehicleModel.ModelCode" +
" join CoverType on VehicleDetail.CoverTypeId = CoverType.Id " +
" join AspNetUsers on Customer.UserID = AspNetUsers.Id";

                results = InsuranceContext.Query(strQuery)
.Select(x => new RiskDetailModel()
{
    PolicyExpireDate = x.CoverEndDate.ToShortDateString(),
    CoverTypeName = x.Name,
    SumInsured = x.SumInsured,
    VechicalMake = x.MakeDescription,
    VechicalModel = x.ModelDescription,
    VehicleYear = x.VehicleYear,
    CustomerDetails = new CustomerModel { FirstName = x.FirstName, LastName = x.LastName, PhoneNumber = x.PhoneNumber, EmailAddress = x.Email }


}).ToList();





            }
            catch (Exception ex)
            {

            }


            return View(results);
        }






        public ActionResult SearchZtscReports(ZTSCLevyReportSeachModels Model)
        {

            List<ZTSCLevyReportModels> listZTSCLevyreport = new List<ZTSCLevyReportModels>();
            ListZTSCLevyReportModels _listZTSCLevyreport = new ListZTSCLevyReportModels();
            _listZTSCLevyreport.ListZTSCreportdata = new List<ZTSCLevyReportModels>();

            ZTSCLevyReportSeachModels model = new ZTSCLevyReportSeachModels();

            var vehicledetail = InsuranceContext.VehicleDetails.All(where: "IsActive = 1").ToList();

            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrEmpty(Model.FromDate) && !string.IsNullOrEmpty(Model.EndDate))
            {
                fromDate = Convert.ToDateTime(Model.FromDate);
                endDate = Convert.ToDateTime(Model.EndDate);
                ViewBag.fromdate = fromDate.ToString("dd/MM/yyyy");
                ViewBag.enddate = endDate.ToString("dd/MM/yyyy");
            }

            var currencyList = _summaryDetailService.GetAllCurrency();


            vehicledetail = vehicledetail.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();



            foreach (var item in vehicledetail)
            {
                // if (Model.FromDate ==Convert.ToString(item.TransactionDate) && Model.EndDate > Convert.ToString(item.TransactionDate))

                ZTSCLevyReportModels obj = new ZTSCLevyReportModels();
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

                obj.Currency = _summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId);
                obj.Customer_Name = customer.FirstName + " " + customer.LastName;
                obj.Policy_Number = policy.PolicyNumber;
                obj.Premium_due = Convert.ToDecimal(item.Premium);
                obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("dd/MM/yyy");
                obj.ZTSCLevy = Convert.ToDecimal(item.ZTSCLevy);

                listZTSCLevyreport.Add(obj);


            }

            //_listZTSCLevyreport.ListZTSCreportdata = listZTSCLevyreport.OrderByDescending(x => x.Transaction_date).ToList();
            model.ListZTSCreportdata = listZTSCLevyreport.OrderByDescending(x => x.Transaction_date).ToList();
            return View("ZTSCLevyReport", model);
        }
        public ActionResult StampDutyReport()
        {
            List<StampDutyReportModels> ListStampDutyReport = new List<StampDutyReportModels>();
            ListStampDutyReportModels _ListStampDutyReport = new ListStampDutyReportModels();
            _ListStampDutyReport.ListStampDutyReportdata = new List<StampDutyReportModels>();
            StampDutySearchReportModels model = new StampDutySearchReportModels();

            //    var currenyList = InsuranceContext.Currencies.All();
            var vehicledetail = InsuranceContext.VehicleDetails.All(where: " IsActive = 1").ToList();

            var currenyList = _summaryDetailService.GetAllCurrency();


            foreach (var item in vehicledetail.Take(100))
            {
                StampDutyReportModels obj = new StampDutyReportModels();
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                obj.Currency = _summaryDetailService.GetCurrencyName(currenyList, item.CurrencyId);

                obj.Customer_Name = customer.FirstName + " " + customer.LastName;
                obj.Policy_Number = policy.PolicyNumber;
                obj.Premium_due = Convert.ToDecimal(item.Premium);
                obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("dd/MM/yyy");
                obj.Stamp_duty = Convert.ToDecimal(item.StampDuty);

                ListStampDutyReport.Add(obj);
            }
            // _ListStampDutyReport.ListStampDutyReportdata = ListStampDutyReport.OrderBy(x => x.Customer_Name).ToList();
            model.ListStampDutyReportdata = ListStampDutyReport.OrderBy(x => x.Customer_Name).ToList();
            return View(model);
        }
        public ActionResult StampDutySearchReport(StampDutySearchReportModels Model)
        {
            List<StampDutyReportModels> ListStampDutyReport = new List<StampDutyReportModels>();
            ListStampDutyReportModels _ListStampDutyReport = new ListStampDutyReportModels();
            _ListStampDutyReport.ListStampDutyReportdata = new List<StampDutyReportModels>();
            StampDutySearchReportModels model = new StampDutySearchReportModels();

            var vehicledetail = InsuranceContext.VehicleDetails.All(where: "IsActive=1").ToList();


            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrEmpty(Model.FromDate) && !string.IsNullOrEmpty(Model.EndDate))
            {
                fromDate = Convert.ToDateTime(Model.FromDate);
                endDate = Convert.ToDateTime(Model.EndDate);
                ViewBag.fromdate = fromDate.ToString("dd/MM/yyyy");
                ViewBag.enddate = endDate.ToString("dd/MM/yyyy");
            }

            vehicledetail = vehicledetail.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();
            var currencyList = _summaryDetailService.GetAllCurrency();


            foreach (var item in vehicledetail)
            {
                StampDutyReportModels obj = new StampDutyReportModels();
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

                obj.Customer_Name = customer.FirstName + " " + customer.LastName;
                obj.Policy_Number = policy.PolicyNumber;
                obj.Premium_due = Convert.ToDecimal(item.Premium);
                obj.Currency = _summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId);

                obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("dd/MM/yyy");
                obj.Stamp_duty = Convert.ToDecimal(item.StampDuty);

                ListStampDutyReport.Add(obj);
            }
            // _ListStampDutyReport.ListStampDutyReportdata = ListStampDutyReport.OrderBy(x => x.Customer_Name).ToList();
            model.ListStampDutyReportdata = ListStampDutyReport.OrderBy(x => x.Customer_Name).ToList();

            return View("StampDutyReport", model);
        }


        public ActionResult VehicleRiskAboutExpire()
        {
            List<VehicleRiskAboutExpireModels> ListVehicleRiskAboutExpire = new List<VehicleRiskAboutExpireModels>();
            ListVehicleRiskAboutExpireModels _ListVehicleRiskAboutExpire = new ListVehicleRiskAboutExpireModels();
            _ListVehicleRiskAboutExpire.ListVehicleRiskAboutExpiredata = new List<VehicleRiskAboutExpireModels>();
            VehicleRiskAboutSearchExpireModels Model = new VehicleRiskAboutSearchExpireModels();
            List<VehicleDetail> vehicledetail = new List<VehicleDetail>();

            var makeList = InsuranceContext.VehicleMakes.All();
            var modelList = InsuranceContext.VehicleModels.All();
            var currenyList = InsuranceContext.Currencies.All();


            var query = "select top 100 VehicleDetail.Id,   VehicleDetail.RegistrationNo, Customer.FirstName + ' ' + Customer.LastName as FullName, Customer.PhoneNumber , PolicyDetail.PolicyNumber,  VehicleDetail.MakeId, VehicleDetail.ModelId, ";
            query += "  VehicleDetail.CoverStartDate, VehicleDetail.CoverEndDate, VehicleDetail.SumInsured, VehicleDetail.TransactionDate, VehicleDetail.Premium, VehicleDetail.CurrencyId from VehicleDetail ";
            query += " join PolicyDetail on VehicleDetail.PolicyId = PolicyDetail.Id ";
            query += " join Customer on VehicleDetail.CustomerId = Customer.Id order by  VehicleDetail.Id desc";



            ListVehicleRiskAboutExpire = InsuranceContext.Query(query)
        .Select(x => new VehicleRiskAboutExpireModels()
        {
            Customer_Name = x.FullName,
            Policy_Number = x.PhoneNumber,
            phone_number = x.PolicyNumber,
            Vehicle_makeandmodel = makeList.FirstOrDefault(c => c.MakeCode == x.MakeId)==null? "":   makeList.FirstOrDefault(c => c.MakeCode == x.MakeId).MakeDescription + " " + modelList.FirstOrDefault(c => c.ModelCode == x.ModelId)==null ? "" : modelList.FirstOrDefault(c => c.ModelCode == x.ModelId).ModelDescription,
            Vehicle_startdate = x.CoverStartDate.ToShortDateString(),
            Vehicle_enddate = x.CoverEndDate.ToShortDateString(),
            Premium_due = x.Premium,
            Transaction_date = x.TransactionDate.ToShortDateString(),
            Sum_Insured = x.SumInsured == null ? 0 : x.SumInsured,
            Currency = currenyList.FirstOrDefault(c => c.Id == x.CurrencyId) == null ? "USD" : currenyList.FirstOrDefault(c => c.Id == x.CurrencyId).Name,
            RegistrationNumber = x.RegistrationNo
        }).ToList();


            ////if (Date == null)
            //vehicledetail = InsuranceContext.VehicleDetails.All().ToList();
            ////else
            ////vehicledetail = InsuranceContext.VehicleDetails.All().Where(p => p.CoverEndDate.Value.ToShortDateString() == (Date == null ? DateTime.Now.ToShortDateString() : Date.Value.ToShortDateString())).ToList();
            //var policyDetails = InsuranceContext.PolicyDetails.All();
            //var customerDetails = InsuranceContext.Customers.All();
            //foreach (var item in vehicledetail)
            //{
            //    var obj = new VehicleRiskAboutExpireModels();
            //    var Vehicle = vehicledetail.Where(m => m.Id == item.Id).First();
            //    var policy = policyDetails.Where(m => m.Id == item.PolicyId).First();
            //    var customer = customerDetails.Where(m => m.Id == item.CustomerId).First();
            //    var make = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'");
            //    var model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
            //    obj.Customer_Name = customer.FirstName + " " + customer.LastName;
            //    obj.Policy_Number = policy.PolicyNumber;
            //    obj.phone_number = customer.PhoneNumber;
            //    obj.Vehicle_makeandmodel = make.MakeDescription + "/" + model.ModelDescription;
            //    //obj.Vehicle_startdate = Convert.ToDateTime(item.CoverStartDate).ToString("dd/MM/yyy");
            //    //obj.Vehicle_enddate = Convert.ToDateTime(item.CoverEndDate).ToString("dd/MM/yyy");
            //    obj.Vehicle_startdate = Convert.ToDateTime(item.CoverStartDate).ToString("MM/dd/yyyy");
            //    obj.Vehicle_enddate = Convert.ToDateTime(item.CoverEndDate).ToString("MM/dd/yyyy");
            //    obj.Premium_due = Convert.ToDecimal(item.Premium);
            //    //obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("dd/MM/yyy");
            //    obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("MM/dd/yyyy");
            //    obj.Sum_Insured = Convert.ToDecimal(item.SumInsured);
            //    ListVehicleRiskAboutExpire.Add(obj);
            //}







            //_ListVehicleRiskAboutExpire.ListVehicleRiskAboutExpiredata = ListVehicleRiskAboutExpire;
            Model.ListVehicleRiskAboutExpiredata = ListVehicleRiskAboutExpire;
            return View(Model);
        }
        public ActionResult VehicleRiskAboutSearchExpire(VehicleRiskAboutSearchExpireModels _Model)
        {
            List<VehicleRiskAboutExpireModels> ListVehicleRiskAboutExpire = new List<VehicleRiskAboutExpireModels>();
            ListVehicleRiskAboutExpireModels _ListVehicleRiskAboutExpire = new ListVehicleRiskAboutExpireModels();
            _ListVehicleRiskAboutExpire.ListVehicleRiskAboutExpiredata = new List<VehicleRiskAboutExpireModels>();
            VehicleRiskAboutSearchExpireModels Model = new VehicleRiskAboutSearchExpireModels();
            List<VehicleDetail> vehicledetail = new List<VehicleDetail>();
            //VehicleRiskAboutExpireModels obj = new VehicleRiskAboutExpireModels();

            //if (Date == null)
            vehicledetail = InsuranceContext.VehicleDetails.All(where: "IsActive = 1").ToList();

            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrEmpty(_Model.FromDate) && !string.IsNullOrEmpty(_Model.EndDate))
            {
                fromDate = Convert.ToDateTime(_Model.FromDate);
                endDate = Convert.ToDateTime(_Model.EndDate);
                ViewBag.fromdate = fromDate.ToString("dd/MM/yyyy");
                ViewBag.enddate = endDate.ToString("dd/MM/yyyy");
            }

            //vehicledetail = vehicledetail.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();
            //vehicledetail = vehicledetail.Where(c => c.CoverStartDate >= fromDate && c.CoverEndDate <= endDate).ToList();

            //vehicledetail = vehicledetail.Where(c => c.CoverStartDate >= fromDate && c.CoverEndDate <= endDate).ToList();

            //vehicledetail = vehicledetail.Where(c => c.CoverEndDate >= endDate).ToList();

            vehicledetail = vehicledetail.Where(c => c.CoverEndDate >= fromDate && c.CoverEndDate <= endDate).ToList();
            //var VehicleDetails = InsuranceContext.VehicleDetails.All().ToList();
            var policyDetails = InsuranceContext.PolicyDetails.All().ToList();
            var customerDetails = InsuranceContext.Customers.All().ToList();


            var currencyList = _summaryDetailService.GetAllCurrency();

            //else
            //vehicledetail = InsuranceContext.VehicleDetails.All().Where(p => p.CoverEndDate.Value.ToShortDateString() == (Date == null ? DateTime.Now.ToShortDateString() : Date.Value.ToShortDateString())).ToList();
            foreach (var item in vehicledetail)
            {
                var obj = new VehicleRiskAboutExpireModels();
                var Vehicle = vehicledetail.Where(m => m.Id == item.Id).First();
                var policy = policyDetails.Where(m => m.Id == item.PolicyId).First();
                var customer = customerDetails.Where(m => m.Id == item.CustomerId).First();
                var make = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'");
                var model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                obj.Customer_Name = customer.FirstName + " " + customer.LastName;
                obj.Policy_Number = policy.PolicyNumber;
                obj.phone_number = customer.PhoneNumber;
                obj.Vehicle_makeandmodel = make.MakeDescription + "/" + model.ModelDescription;
                //obj.Vehicle_startdate = Convert.ToDateTime(item.CoverStartDate).ToString("dd/MM/yyy");
                //obj.Vehicle_enddate = Convert.ToDateTime(item.CoverEndDate).ToString("dd/MM/yyy");
                obj.Vehicle_startdate = Convert.ToDateTime(item.CoverStartDate).ToString("MM/dd/yyyy");
                obj.Vehicle_enddate = Convert.ToDateTime(item.CoverEndDate).ToString("MM/dd/yyyy");
                obj.Premium_due = Convert.ToDecimal(item.Premium);
                //obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("dd/MM/yyy");
                obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("MM/dd/yyyy");
                obj.Sum_Insured = Convert.ToDecimal(item.SumInsured);
                obj.Currency = _summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId);


                ListVehicleRiskAboutExpire.Add(obj);
            }
            //_ListVehicleRiskAboutExpire.ListVehicleRiskAboutExpiredata = ListVehicleRiskAboutExpire;
            Model.ListVehicleRiskAboutExpiredata = ListVehicleRiskAboutExpire;



            return View("VehicleRiskAboutExpire", Model);
        }

        //public ActionResult GrossWrittenPremiumReport()
        //{
        //    List<GrossWrittenPremiumReportModels> ListGrossWrittenPremiumReport = new List<GrossWrittenPremiumReportModels>();
        //    ListGrossWrittenPremiumReportModels _ListGrossWrittenPremiumReport = new ListGrossWrittenPremiumReportModels();
        //    _ListGrossWrittenPremiumReport.ListGrossWrittenPremiumReportdata = new List<GrossWrittenPremiumReportModels>();


        //    //var customerList = InsuranceContext.Customers.All().ToList();
        //    //var makeList = InsuranceContext.VehicleMakes.All().ToList();
        //    //var modelList = InsuranceContext.VehicleModels.All().ToList();


        //    GrossWrittenPremiumReportSearchModels Model = new GrossWrittenPremiumReportSearchModels();
        //    //   var vehicledetail = InsuranceContext.VehicleDetails.All(where: $"IsActive='1'").ToList().Take(200);

        //    var vehicledetail = InsuranceContext.VehicleDetails.All().OrderByDescending(c => c.Id).ToList().Take(200);

        //    var currenyList = _summaryDetailService.GetAllCurrency();


        //    foreach (var item in vehicledetail)
        //    {
        //        var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
        //        GrossWrittenPremiumReportModels obj = new GrossWrittenPremiumReportModels();
        //        var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);

        //        //var customer = customerList.Single(c=>c.CustomerId==item.CustomerId);
        //        //var make = makeList.Single(c=>c.MakeCode==item.MakeId);
        //        //var model = modelList.Single(c => c.ModelCode == item.ModelId);


        //        obj.RenewPolicyNumber = item.RenewPolicyNumber;


        //        var customer = InsuranceContext.Customers.Single(item.CustomerId);
        //        var make = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'");
        //        var model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");


        //        var vehicleSUmmarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
        //        if (vehicleSUmmarydetail != null)
        //        {
        //            var summary = InsuranceContext.SummaryDetails.Single(vehicleSUmmarydetail.SummaryDetailId);
        //            if (summary != null)
        //            {
        //                if (summary.isQuotation != true)
        //                {
        //                    obj.ALMId = customer.ALMId;

        //                    obj.Payment_Term = InsuranceContext.PaymentTerms.Single(item.PaymentTermId).Name;
        //                    var paymentMethod = InsuranceContext.PaymentMethods.Single(summary.PaymentMethodId);

        //                    obj.Payment_Mode = paymentMethod == null ? "" : paymentMethod.Name;

        //                    if (customer != null)
        //                        obj.Customer_Name = customer.FirstName + " " + customer.LastName;
        //                    //10MAy D
        //                    obj.Id = item.Id;

        //                    obj.Policy_Number = policy.PolicyNumber;
        //                    obj.Policy_startdate = Convert.ToDateTime(item.CoverStartDate).ToString("dd/MM/yyy");
        //                    obj.Policy_endate = Convert.ToDateTime(item.CoverEndDate).ToString("dd/MM/yyy");

        //                    //8 Feb
        //                    obj.PolicyRenewalDate = Convert.ToDateTime(item.RenewalDate);
        //                    obj.IsActive = item.IsActive;
        //                    obj.IsLapsed = item.isLapsed;


        //                    var modelDescription = "";

        //                    if (model != null && model.ModelDescription != null)
        //                        modelDescription = model.ModelDescription;


        //                    obj.Vehicle_makeandmodel = make == null ? "" : make.MakeDescription + "/" + modelDescription;
        //                    obj.Stamp_duty = Convert.ToDecimal(item.StampDuty);
        //                    obj.ZTSC_Levy = Convert.ToDecimal(item.ZTSCLevy);
        //                    obj.Sum_Insured = Convert.ToDecimal(item.SumInsured);
        //                    obj.Zinara_License_Fee = Vehicle.VehicleLicenceFee;

        //                    var customerDetails = InsuranceContext.Customers.Single(summary.CreatedBy);

        //                    // var customerDetails = customerList.Single(c => c.CustomerId == summary.CreatedBy);

        //                    if (customerDetails != null)
        //                        obj.PolicyCreatedBy = customerDetails.FirstName + " " + customerDetails.LastName;


        //                    obj.Comission_percentage = 30;

        //                    if (Vehicle != null)
        //                    {
        //                        obj.Comission_Amount = Convert.ToDecimal(Vehicle.Premium * 30 / 100);
        //                    }


        //                    obj.Currency = _summaryDetailService.GetCurrencyName(currenyList, Vehicle.CurrencyId);


        //                    string converType = "";

        //                    if (item.CoverTypeId == (int)eCoverType.ThirdParty)
        //                        converType = eCoverType.ThirdParty.ToString();

        //                    if (item.CoverTypeId == (int)eCoverType.FullThirdParty)
        //                        converType = eCoverType.FullThirdParty.ToString();

        //                    if (item.CoverTypeId == (int)eCoverType.Comprehensive)
        //                        converType = eCoverType.Comprehensive.ToString();

        //                    obj.CoverType = converType;

        //                    obj.Net_Premium = item.Premium;
        //                    obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("dd/MM/yyy");

        //                    if (item.PaymentTermId == 1)
        //                    {
        //                        obj.Annual_Premium = Convert.ToDecimal(item.Premium);

        //                        obj.Premium_due = Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.ZTSCLevy) + Convert.ToDecimal(item.RadioLicenseCost);
        //                    }
        //                    if (item.PaymentTermId == 3)
        //                    {
        //                        obj.Premium_due = Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.ZTSCLevy) + Convert.ToDecimal(item.RadioLicenseCost);
        //                        obj.Annual_Premium = obj.Premium_due * 4;

        //                    }
        //                    if (item.PaymentTermId == 4)
        //                    {
        //                        obj.Premium_due = Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.ZTSCLevy) + Convert.ToDecimal(item.RadioLicenseCost);
        //                        obj.Annual_Premium = obj.Premium_due * 3;

        //                    }

        //                    obj.RadioLicenseCost = item.RadioLicenseCost;
        //                    ListGrossWrittenPremiumReport.Add(obj);

        //                }
        //            }
        //        }
        //    }
        //    //_ListGrossWrittenPremiumReport.ListGrossWrittenPremiumReportdata = ListGrossWrittenPremiumReport.OrderBy(p => p.Customer_Name).ThenBy(p => p.Payment_Term).ThenBy(p => p.Payment_Mode).ToList();
        //    // Model.ListGrossWrittenPremiumReportdata = ListGrossWrittenPremiumReport.OrderBy(p => p.Id).ThenBy(p => p.Customer_Name).ThenBy(p => p.Payment_Term).ThenBy(p => p.Payment_Mode).ToList();

        //    Model.ListGrossWrittenPremiumReportdata = ListGrossWrittenPremiumReport.OrderByDescending(p => p.Id).ToList();

        //    return View(Model);
        //}


        public ActionResult GrossWrittenPremiumReport()
        {
            List<GrossWrittenPremiumReportModels> ListGrossWrittenPremiumReport = new List<GrossWrittenPremiumReportModels>();
            ListGrossWrittenPremiumReportModels _ListGrossWrittenPremiumReport = new ListGrossWrittenPremiumReportModels();
            _ListGrossWrittenPremiumReport.ListGrossWrittenPremiumReportdata = new List<GrossWrittenPremiumReportModels>();


            var query = " select top 100 PolicyDetail.PolicyNumber as Policy_Number, Customer.ALMId, case when Customer.ALMId is null  then  [dbo].fn_GetUserCallCenterAgent(SummaryDetail.CreatedBy) else Customer.ALMId end  as PolicyCreatedBy, Customer.FirstName + ' ' + Customer.LastName as Customer_Name,VehicleDetail.TransactionDate as Transaction_date, ";
            query += "  case when Customer.id=SummaryDetail.CreatedBy then [dbo].fn_GetUserBranch(Customer.id) else [dbo].fn_GetUserBranch(SummaryDetail.CreatedBy) end as BranchName, ";
            query += " VehicleDetail.CoverNote as CoverNoteNum, PaymentMethod.Name as Payment_Mode, PaymentTerm.Name as Payment_Term,CoverType.Name as CoverType, Currency.Name as Currency, ";
            query += " VehicleDetail.Premium + VehicleDetail.StampDuty + VehicleDetail.ZTSCLevy as Premium_due, VehicleDetail.StampDuty as Stamp_duty, VehicleDetail.ZTSCLevy as ZTSC_Levy, ";
            query += " cast(VehicleDetail.Premium * 30 / 100 as decimal(10, 2))    as Comission_Amount, VehicleDetail.IncludeRadioLicenseCost, ";
            query += " CASE WHEN IncludeRadioLicenseCost = 1 THEN VehicleDetail.RadioLicenseCost else 0 end as RadioLicenseCost, VehicleDetail.VehicleLicenceFee as Zinara_License_Fee, ";
            query += " VehicleDetail.RenewalDate as PolicyRenewalDate, VehicleDetail.IsActive, VehicleDetail.RenewPolicyNumber as RenewPolicyNumber, ";
            query += " VehicleDetail.BusinessSourceDetailId, BusinessSource.Source as BusinessSourceName, SourceDetail.FirstName + ' ' + SourceDetail.LastName as SourceDetailName from PolicyDetail ";
            query += " join Customer on PolicyDetail.CustomerId = Customer.Id ";
            query += " join VehicleDetail on PolicyDetail.Id = VehicleDetail.PolicyId ";
            query += "join SummaryVehicleDetail on VehicleDetail.id = SummaryVehicleDetail.VehicleDetailsId ";
            query += " join SummaryDetail on SummaryDetail.id = SummaryVehicleDetail.SummaryDetailId ";
            query += " join PaymentMethod on SummaryDetail.PaymentMethodId = PaymentMethod.Id ";
            query += "join PaymentTerm on VehicleDetail.PaymentTermId = PaymentTerm.Id ";
            query += " join CoverType on VehicleDetail.CoverTypeId = CoverType.Id ";
            query += " left join Currency on VehicleDetail.CurrencyId = Currency.Id ";
            query += " left join BusinessSource on BusinessSource.Id = VehicleDetail.BusinessSourceDetailId ";
            query += " left   join SourceDetail on VehicleDetail.BusinessSourceDetailId = SourceDetail.Id join AspNetUsers on AspNetUsers.id=customer.UserID join AspNetUserRoles on AspNetUserRoles.UserId=AspNetUsers.Id ";
            query += " where VehicleDetail.IsActive = 1 and SummaryDetail.isQuotation=0  order by  VehicleDetail.Id desc ";



            ListGrossWrittenPremiumReport = InsuranceContext.Query(query).
                Select(x => new GrossWrittenPremiumReportModels()
                {

                    Policy_Number = x.Policy_Number,
                    BranchName = x.BranchName,
                    PolicyCreatedBy = x.PolicyCreatedBy,
                    Customer_Name = x.Customer_Name,
                    Transaction_date = x.Transaction_date.ToShortDateString(),
                    CoverNoteNum = x.CoverNoteNum,
                    Payment_Mode = x.Payment_Mode,
                    Payment_Term = x.Payment_Term,
                    CoverType = x.CoverType,
                    Currency = x.Currency,
                    Premium_due = x.Premium_due,
                    Stamp_duty = x.Stamp_duty,
                    ZTSC_Levy = x.ZTSC_Levy,
                    ALMId = x.ALMId,
                    Comission_Amount = x.Comission_Amount,
                    //IncludeRadioLicenseCost = x.IncludeRadioLicenseCost,
                    RadioLicenseCost = x.RadioLicenseCost,
                    Zinara_License_Fee = x.Zinara_License_Fee,
                    PolicyRenewalDate = x.PolicyRenewalDate,
                    IsActive = x.IsActive,
                    RenewPolicyNumber = x.RenewPolicyNumber,
                    BusinessSourceName = x.BusinessSourceName,
                    //IncludeRadioLicenseCost = x.IncludeRadioLicenseCost,
                    SourceDetailName = x.SourceDetailName,
                }).ToList();


            GrossWrittenPremiumReportSearchModels Model = new GrossWrittenPremiumReportSearchModels();
            //var VehicleList = InsuranceContext.VehicleDetails.All(where: $"IsActive='1'");
            //var vehicledetail = VehicleList.OrderByDescending(c => c.Id).ToList().Take(200);
            //var policyList = InsuranceContext.PolicyDetails.All();
            //var customerList = InsuranceContext.Customers.All();
            //var makelist = InsuranceContext.VehicleMakes.All();
            //var modelList = InsuranceContext.VehicleModels.All();
            //var vehicleSUmmarydetaillist = InsuranceContext.SummaryVehicleDetails.All();
            //var summarylist = InsuranceContext.SummaryDetails.All();
            //var currenyList = _summaryDetailService.GetAllCurrency();
            //var Payment_TermList = InsuranceContext.PaymentTerms.All();
            //var PaymentMethodsList = InsuranceContext.PaymentMethods.All();
            //var customerDetailsList = InsuranceContext.Customers.All();
            //var branchList = InsuranceContext.Branches.All();

            //var businessSourceList = InsuranceContext.BusinessSources.All();
            //var sourceList = InsuranceContext.SourceDetails.All();


            //foreach (var item in vehicledetail.Take(50))
            //{
            //    var Vehicle = VehicleList.FirstOrDefault(x => x.Id == item.Id);
            //    GrossWrittenPremiumReportModels obj = new GrossWrittenPremiumReportModels();
            //    var policy = policyList.FirstOrDefault(x => x.Id == item.PolicyId);



            //    obj.RenewPolicyNumber = item.RenewPolicyNumber;
            //    obj.CoverNoteNum = item.CoverNote;


            //    var businessSourceListDetails = sourceList.FirstOrDefault(c => c.Id == item.BusinessSourceDetailId);
            //    if (businessSourceListDetails != null)
            //    {
            //        obj.BusinessSourceName = businessSourceListDetails.FirstName + " " + businessSourceListDetails.LastName;


            //        var businessSourceDetails = businessSourceList.FirstOrDefault(c => c.Id == businessSourceListDetails.Id);
            //        if (businessSourceListDetails != null)
            //        {
            //            obj.SourceDetailName = businessSourceDetails.Source;
            //        }
            //    }


            //    var customer = customerList.FirstOrDefault(x => x.Id == item.CustomerId);
            //    var make = makelist.FirstOrDefault(x => x.MakeCode == item.MakeId);//  (where: $"MakeCode='{item.MakeId}'");
            //    var model = modelList.FirstOrDefault(x => x.ModelCode == item.ModelId);


            //    var vehicleSUmmarydetail = vehicleSUmmarydetaillist.FirstOrDefault(x => x.VehicleDetailsId == item.Id);
            //    if (vehicleSUmmarydetail != null)
            //    {
            //        var summary = summarylist.FirstOrDefault(x => x.Id == vehicleSUmmarydetail.SummaryDetailId);
            //        if (summary != null)
            //        {
            //            if (summary.isQuotation != true)
            //            {

            //                var branchDetails = branchList.FirstOrDefault(c => c.Id == customer.BranchId);
            //                if (branchDetails != null)
            //                {
            //                    obj.ALMId = branchDetails.AlmId;
            //                    obj.BranchName = branchDetails.BranchName;
            //                }

            //                obj.Payment_Term = Payment_TermList.FirstOrDefault(x => x.Id == item.PaymentTermId).Name;
            //                var paymentMethod = PaymentMethodsList.FirstOrDefault(x => x.Id == summary.PaymentMethodId);

            //                obj.Payment_Mode = paymentMethod == null ? "" : paymentMethod.Name;

            //                if (customer != null)
            //                    obj.Customer_Name = customer.FirstName + " " + customer.LastName;
            //                //10MAy D
            //                obj.Id = item.Id;

            //                obj.Policy_Number = policy.PolicyNumber;
            //                obj.Policy_startdate = Convert.ToDateTime(item.CoverStartDate).ToString("dd/MM/yyy");
            //                obj.Policy_endate = Convert.ToDateTime(item.CoverEndDate).ToString("dd/MM/yyy");

            //                //8 Feb
            //                obj.PolicyRenewalDate = Convert.ToDateTime(item.RenewalDate);
            //                obj.IsActive = item.IsActive;
            //                obj.IsLapsed = item.isLapsed;


            //                var modelDescription = "";

            //                if (model != null && model.ModelDescription != null)
            //                    modelDescription = model.ModelDescription;


            //                obj.Vehicle_makeandmodel = make == null ? "" : make.MakeDescription + "/" + modelDescription;
            //                obj.Stamp_duty = Convert.ToDecimal(item.StampDuty);
            //                obj.ZTSC_Levy = Convert.ToDecimal(item.ZTSCLevy);
            //                obj.Sum_Insured = Convert.ToDecimal(item.SumInsured);
            //                obj.Zinara_License_Fee = Vehicle.VehicleLicenceFee;


            //                var customerDetails = customerDetailsList.FirstOrDefault(x => x.Id == summary.CreatedBy);


            //                if (customerDetails != null)
            //                    obj.PolicyCreatedBy = customerDetails.FirstName + " " + customerDetails.LastName;


            //                obj.Comission_percentage = 30;

            //                if (Vehicle != null)
            //                {
            //                    obj.Comission_Amount = Convert.ToDecimal(Vehicle.Premium * 30 / 100);
            //                }


            //                obj.Currency = _summaryDetailService.GetCurrencyName(currenyList, Vehicle.CurrencyId);

            //                string converType = "";

            //                if (item.CoverTypeId == (int)eCoverType.ThirdParty)
            //                    converType = eCoverType.ThirdParty.ToString();

            //                if (item.CoverTypeId == (int)eCoverType.FullThirdParty)
            //                    converType = eCoverType.FullThirdParty.ToString();

            //                if (item.CoverTypeId == (int)eCoverType.Comprehensive)
            //                    converType = eCoverType.Comprehensive.ToString();

            //                obj.CoverType = converType;

            //                obj.Net_Premium = item.Premium;
            //                obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("dd/MM/yyy");

            //                obj.Annual_Premium = Convert.ToDecimal(item.Premium);

            //                decimal radioLicenseCost = 0;
            //                if (item.IncludeRadioLicenseCost.Value)
            //                {
            //                    radioLicenseCost = Convert.ToDecimal(item.RadioLicenseCost);
            //                }


            //                obj.Premium_due = Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.ZTSCLevy);




            //                obj.RadioLicenseCost = item.RadioLicenseCost;
            //                ListGrossWrittenPremiumReport.Add(obj);

            //            }
            //        }
            //    }
            //}


            Model.ListGrossWrittenPremiumReportdata = ListGrossWrittenPremiumReport.OrderByDescending(p => p.Id).ToList();

            return View(Model);
        }


        public ActionResult SearchGrossReports(GrossWrittenPremiumReportSearchModels _model)
        {

            List<GrossWrittenPremiumReportModels> ListGrossWrittenPremiumReport = new List<GrossWrittenPremiumReportModels>();
            ListGrossWrittenPremiumReportModels _ListGrossWrittenPremiumReport = new ListGrossWrittenPremiumReportModels();
            _ListGrossWrittenPremiumReport.ListGrossWrittenPremiumReportdata = new List<GrossWrittenPremiumReportModels>();

            GrossWrittenPremiumReportSearchModels Model = new GrossWrittenPremiumReportSearchModels();

            //  var vehicledetail = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();
            //  var vehicledetail = InsuranceContext.VehicleDetails.All(where: "IsActive=1").ToList();

            var vehicledetail = InsuranceContext.VehicleDetails.All(where: $"IsActive='1'").OrderByDescending(c => c.Id).ToList();
            var branchList = InsuranceContext.Branches.All();


            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrEmpty(_model.FormDate) && !string.IsNullOrEmpty(_model.EndDate))
            {
                fromDate = Convert.ToDateTime(_model.FormDate);
                endDate = Convert.ToDateTime(_model.EndDate);


                ViewBag.fromdate = fromDate.ToString("dd/MM/yyyy");
                ViewBag.enddate = endDate.ToString("dd/MM/yyyy");

            }


            vehicledetail = vehicledetail.Where(c => Convert.ToDateTime(c.TransactionDate.Value.ToShortDateString()) >= fromDate && Convert.ToDateTime(c.TransactionDate.Value.ToShortDateString()) <= endDate).ToList();


            var currencyList = _summaryDetailService.GetAllCurrency();

            //var customerList = InsuranceContext.Customers.All().ToList();
            //var makeList = InsuranceContext.VehicleMakes.All().ToList();
            //var modelList = InsuranceContext.VehicleModels.All().ToList();

            var businessSourceList = InsuranceContext.BusinessSources.All();
            var sourceList = InsuranceContext.SourceDetails.All();





            foreach (var item in vehicledetail)
            {
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                GrossWrittenPremiumReportModels obj = new GrossWrittenPremiumReportModels();
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);



                var customer = InsuranceContext.Customers.Single(item.CustomerId);
                var make = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'");
                var model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");

                obj.RenewPolicyNumber = item.RenewPolicyNumber;
                obj.CoverNoteNum = item.CoverNote;




                var businessSourceListDetails = sourceList.FirstOrDefault(c => c.Id == item.BusinessSourceDetailId);
                if (businessSourceListDetails != null)
                {
                    obj.BusinessSourceName = businessSourceListDetails.FirstName + " " + businessSourceListDetails.LastName;


                    var businessSourceDetails = businessSourceList.FirstOrDefault(c => c.Id == businessSourceListDetails.Id);
                    if (businessSourceListDetails != null)
                    {
                        obj.SourceDetailName = businessSourceDetails.Source;
                    }
                }


                var vehicleSUmmarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
                if (vehicleSUmmarydetail != null)
                {
                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSUmmarydetail.SummaryDetailId);
                    if (summary != null)
                    {

                        if (summary.isQuotation != true)
                        {
                            var branchDetails = branchList.FirstOrDefault(c => c.Id == customer.BranchId);
                            if (branchDetails != null)
                            {
                                obj.ALMId = branchDetails.AlmId;
                                obj.BranchName = branchDetails.BranchName;
                            }

                            obj.Payment_Term = InsuranceContext.PaymentTerms.Single(item.PaymentTermId)?.Name;
                            obj.Payment_Mode = InsuranceContext.PaymentMethods.Single(summary.PaymentMethodId)?.Name;
                            obj.Customer_Name = customer.FirstName + " " + customer.LastName;
                            obj.Policy_Number = policy.PolicyNumber;
                            obj.Policy_startdate = Convert.ToDateTime(item.CoverStartDate).ToString("dd/MM/yyy");
                            obj.Policy_endate = Convert.ToDateTime(item.CoverEndDate).ToString("dd/MM/yyy");

                            var makeDesc = make == null ? "" : make.MakeDescription;
                            var modelDes = model == null ? "" : model.ModelDescription;

                            obj.Vehicle_makeandmodel = makeDesc + "/" + modelDes;
                            obj.Stamp_duty = Convert.ToDecimal(item.StampDuty);
                            obj.ZTSC_Levy = Convert.ToDecimal(item.ZTSCLevy);
                            obj.Sum_Insured = Convert.ToDecimal(item.SumInsured);

                            //10 May D
                            obj.Id = item.Id;
                            //8 Feb
                            obj.IsLapsed = item.isLapsed;
                            obj.PolicyRenewalDate = Convert.ToDateTime(item.RenewalDate);
                            obj.IsActive = item.IsActive;

                            obj.Currency = _summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId);

                            var customerDetails = InsuranceContext.Customers.Single(summary.CreatedBy);

                            if (customerDetails != null)
                                obj.PolicyCreatedBy = customerDetails.FirstName + " " + customerDetails.LastName;

                            obj.Zinara_License_Fee = Vehicle.VehicleLicenceFee;


                            string converType = "";

                            if (item.CoverTypeId == (int)eCoverType.ThirdParty)
                                converType = eCoverType.ThirdParty.ToString();

                            if (item.CoverTypeId == (int)eCoverType.FullThirdParty)
                                converType = eCoverType.FullThirdParty.ToString();

                            if (item.CoverTypeId == (int)eCoverType.Comprehensive)
                                converType = eCoverType.Comprehensive.ToString();

                            obj.CoverType = converType;


                            obj.Comission_percentage = 30;

                            if (Vehicle != null)
                            {
                                obj.Comission_Amount = Convert.ToDecimal(Vehicle.Premium * 30 / 100);
                            }


                            obj.Net_Premium = item.Premium;
                            obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("dd/MM/yyy");


                            decimal radioLicenseCost = 0;
                            if (item.IncludeRadioLicenseCost.Value)
                            {
                                radioLicenseCost = Convert.ToDecimal(item.RadioLicenseCost);
                            }

                            //obj.Annual_Premium = Convert.ToDecimal(item.Premium);

                            //  obj.Premium_due = Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.ZTSCLevy) + Convert.ToDecimal(item.VehicleLicenceFee) + radioLicenseCost;

                            obj.Premium_due = Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.ZTSCLevy);


                            //if (item.PaymentTermId == 1)
                            //{
                            //    obj.Annual_Premium = Convert.ToDecimal(item.Premium);

                            //    obj.Premium_due = Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.ZTSCLevy) + Convert.ToDecimal(item.RadioLicenseCost);
                            //}
                            //if (item.PaymentTermId == 3)
                            //{
                            //    obj.Premium_due = Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.ZTSCLevy) + Convert.ToDecimal(item.RadioLicenseCost);
                            //    obj.Annual_Premium = obj.Premium_due * 4;

                            //}
                            //if (item.PaymentTermId == 4)
                            //{
                            //    obj.Premium_due = Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.ZTSCLevy) + Convert.ToDecimal(item.RadioLicenseCost);
                            //    obj.Annual_Premium = obj.Premium_due * 3;

                            //}

                            obj.RadioLicenseCost = item.RadioLicenseCost;
                            ListGrossWrittenPremiumReport.Add(obj);
                        }
                    }
                }

            }
            //_ListGrossWrittenPremiumReport.ListGrossWrittenPremiumReportdata = ListGrossWrittenPremiumReport.OrderBy(p => p.Customer_Name).ThenBy(p => p.Payment_Term).ThenBy(p => p.Payment_Mode).ToList();
            Model.ListGrossWrittenPremiumReportdata = ListGrossWrittenPremiumReport.OrderBy(p => p.Id).ThenBy(p => p.Customer_Name).ThenBy(c => c.Policy_Number).ThenBy(c => c.Policy_Number).ThenBy(p => p.Payment_Term).ThenBy(p => p.Payment_Mode).ToList();
            return View("GrossWrittenPremiumReport", Model);

        }
        public ActionResult ReinsuranceCommissionReport()
        {
            //ReinsuranceCommissionReportModel ReinsurancReportListmodel = new ReinsuranceCommissionReportModel();
            var ListReinsurancelist = new List<ReinsuranceCommissionReportModel>();

            ReinsuranceCommissionReportModel obj = new ReinsuranceCommissionReportModel();
            ReinsuranceCommissionSearchReportModel model = new ReinsuranceCommissionSearchReportModel();
            ListReinsurance _Reinsurance = new ListReinsurance();
            _Reinsurance.Reinsurance = new List<ReinsuranceCommissionReportModel>();
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();

            var currenyList = _summaryDetailService.GetAllCurrency();



            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                var ReinsuranceTransaction = InsuranceContext.ReinsuranceTransactions.All(where: $"VehicleId={Vehicle.Id}").ToList();

                var currencyDetails = currenyList.FirstOrDefault(c => c.Id == Vehicle.CurrencyId);
                if (currencyDetails != null)
                    obj.Currency = currencyDetails.Name;
                else
                    obj.Currency = "USD";



                if (ReinsuranceTransaction.Count > 0 && ReinsuranceTransaction != null)
                {
                    ListReinsurancelist.Add(new ReinsuranceCommissionReportModel()
                    {
                        FirstName = Customer.FirstName,
                        LastName = Customer.LastName,
                        PolicyNumber = Policy.PolicyNumber,
                        StartDate = Vehicle.CoverStartDate == null ? null : Vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy"),
                        EndDate = Vehicle.CoverEndDate == null ? null : Vehicle.CoverEndDate.Value.ToString("dd/MM/yyyy"),
                        TransactionDate = Vehicle.TransactionDate == null ? null : Vehicle.TransactionDate.Value.ToString("dd/MM/yyyy"),
                        AutoFacultativeReinsurance = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 0 ? Convert.ToDecimal(ReinsuranceTransaction[0].ReinsuranceCommission) : 0.00m),
                        FacultativeReinsurance = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 1 ? Convert.ToDecimal(ReinsuranceTransaction[1].ReinsuranceCommission) : 0.00m),//FacultativeReinsurance = "";
                        Currency = _summaryDetailService.GetCurrencyName(currenyList, Vehicle.CurrencyId)
                    });
                }
            }
            model.Reinsurance = ListReinsurancelist.OrderBy(x => x.FirstName).ToList();
            return View(model);
        }


        public ActionResult ReinsuranceCommissionSearchReport(ReinsuranceCommissionSearchReportModel Model)
        {

            var ListReinsurancelist = new List<ReinsuranceCommissionReportModel>();

            ReinsuranceCommissionReportModel obj = new ReinsuranceCommissionReportModel();
            ReinsuranceCommissionSearchReportModel model = new ReinsuranceCommissionSearchReportModel();
            ListReinsurance _Reinsurance = new ListReinsurance();
            _Reinsurance.Reinsurance = new List<ReinsuranceCommissionReportModel>();
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();
            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;

            var currencyList = _summaryDetailService.GetAllCurrency();


            if (!string.IsNullOrEmpty(Model.FromDate) && !string.IsNullOrEmpty(Model.EndDate))
            {
                fromDate = Convert.ToDateTime(Model.FromDate);
                endDate = Convert.ToDateTime(Model.EndDate);
                ViewBag.fromdate = fromDate.ToString("dd/MM/yyyy");
                ViewBag.enddate = endDate.ToString("dd/MM/yyyy");
            }
            VehicleDetails = VehicleDetails.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();
            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                var ReinsuranceTransaction = InsuranceContext.ReinsuranceTransactions.All(where: $"VehicleId={Vehicle.Id}").ToList();
                if (ReinsuranceTransaction.Count > 0 && ReinsuranceTransaction != null)
                {
                    ListReinsurancelist.Add(new ReinsuranceCommissionReportModel()
                    {
                        FirstName = Customer.FirstName,
                        LastName = Customer.LastName,
                        PolicyNumber = Policy.PolicyNumber,
                        StartDate = Vehicle.CoverStartDate == null ? null : Vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy"),
                        EndDate = Vehicle.CoverEndDate == null ? null : Vehicle.CoverEndDate.Value.ToString("dd/MM/yyyy"),
                        TransactionDate = Vehicle.TransactionDate == null ? null : Vehicle.TransactionDate.Value.ToString("dd/MM/yyyy"),
                        AutoFacultativeReinsurance = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 0 ? Convert.ToDecimal(ReinsuranceTransaction[0].ReinsuranceCommission) : 0.00m),
                        FacultativeReinsurance = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 1 ? Convert.ToDecimal(ReinsuranceTransaction[1].ReinsuranceCommission) : 0.00m),
                        Currency = _summaryDetailService.GetCurrencyName(currencyList, Vehicle.CurrencyId)
                    });

                }
            }
            model.Reinsurance = ListReinsurancelist.OrderBy(x => x.FirstName).ToList();

            return View("ReinsuranceCommissionReport", model);
        }
        public ActionResult BasicCommissionReport()
        {
            //ReinsuranceCommissionReportModel ReinsurancReportListmodel = new ReinsuranceCommissionReportModel();
            var ListBasicCommissionReport = new List<BasicCommissionReportModel>();
            ListBasicCommissionReport _BasicCommissionReport = new ListBasicCommissionReport();
            _BasicCommissionReport.BasicCommissionReport = new List<BasicCommissionReportModel>();
            BasicCommissionReportSearchModel model = new BasicCommissionReportSearchModel();

            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();

            var currenyList = _summaryDetailService.GetAllCurrency();


            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                //var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                var ReinsuranceTransaction = InsuranceContext.ReinsuranceTransactions.All(where: $"VehicleId={item.Id}").ToList();
                var commision = InsuranceContext.AgentCommissions.Single(item.AgentCommissionId);
                if (ReinsuranceTransaction.Count > 0 && ReinsuranceTransaction != null)
                {
                    ListBasicCommissionReport.Add(new BasicCommissionReportModel()
                    {
                        FirstName = Customer.FirstName,
                        LastName = Customer.LastName,
                        PolicyNumber = Policy.PolicyNumber,
                        TransactionDate = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("dd/MM/yyyy"),
                        SumInsured = item.SumInsured,
                        Premium = item.Premium,
                        Commission = (item.Premium - item.RoadsideAssistanceAmount - item.PassengerAccidentCoverAmount - item.ExcessBuyBackAmount - item.ExcessAmount - item.MedicalExpensesAmount) * Convert.ToDecimal(commision.CommissionAmount) / 100,
                        ManagementCommission = (item.Premium - item.RoadsideAssistanceAmount - item.PassengerAccidentCoverAmount - item.ExcessBuyBackAmount - item.ExcessAmount - item.MedicalExpensesAmount) * Convert.ToDecimal(commision.ManagementCommission) / 100,
                        Currency = _summaryDetailService.GetCurrencyName(currenyList, item.CurrencyId)
                    });
                }
            }
            model.BasicCommissionReport = ListBasicCommissionReport.OrderBy(x => x.FirstName).ToList();
            return View(model);
        }
        public ActionResult BasicCommissionSearchReport(BasicCommissionReportSearchModel Model)
        {

            //ReinsuranceCommissionReportModel ReinsurancReportListmodel = new ReinsuranceCommissionReportModel();
            var ListBasicCommissionReport = new List<BasicCommissionReportModel>();
            ListBasicCommissionReport _BasicCommissionReport = new ListBasicCommissionReport();
            _BasicCommissionReport.BasicCommissionReport = new List<BasicCommissionReportModel>();
            BasicCommissionReportSearchModel model = new BasicCommissionReportSearchModel();

            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();

            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrEmpty(Model.FromDate) && !string.IsNullOrEmpty(Model.EndDate))
            {
                fromDate = Convert.ToDateTime(Model.FromDate);
                endDate = Convert.ToDateTime(Model.EndDate);
                ViewBag.fromdate = fromDate.ToString("dd/MM/yyyy");
                ViewBag.enddate = endDate.ToString("dd/MM/yyyy");
            }


            VehicleDetails = VehicleDetails.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();


            var currenyList = _summaryDetailService.GetAllCurrency();


            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                //var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                var ReinsuranceTransaction = InsuranceContext.ReinsuranceTransactions.All(where: $"VehicleId={item.Id}").ToList();
                var commision = InsuranceContext.AgentCommissions.Single(item.AgentCommissionId);
                if (ReinsuranceTransaction.Count > 0 && ReinsuranceTransaction != null)
                {
                    ListBasicCommissionReport.Add(new BasicCommissionReportModel()
                    {
                        FirstName = Customer.FirstName,
                        LastName = Customer.LastName,
                        PolicyNumber = Policy.PolicyNumber,
                        TransactionDate = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("dd/MM/yyyy"),
                        SumInsured = item.SumInsured,
                        Premium = item.Premium,
                        Commission = (item.Premium - item.RoadsideAssistanceAmount - item.PassengerAccidentCoverAmount - item.ExcessBuyBackAmount - item.ExcessAmount - item.MedicalExpensesAmount) * Convert.ToDecimal(commision.CommissionAmount) / 100,
                        ManagementCommission = (item.Premium - item.RoadsideAssistanceAmount - item.PassengerAccidentCoverAmount - item.ExcessBuyBackAmount - item.ExcessAmount - item.MedicalExpensesAmount) * Convert.ToDecimal(commision.ManagementCommission) / 100,
                        Currency = _summaryDetailService.GetCurrencyName(currenyList, item.CurrencyId)
                    });
                }
            }
            model.BasicCommissionReport = ListBasicCommissionReport.OrderBy(x => x.FirstName).ToList();
            return View("BasicCommissionReport", model);
        }

        public ActionResult ReinsuranceReport()
        {
            var ListReinsuranceReport = new List<ReinsuranceReport>();
            ListReinsuranceReport _ReinsuranceReport = new ListReinsuranceReport();
            _ReinsuranceReport.ReinsuranceReport = new List<Models.ReinsuranceReport>();
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();
            ReinsuranceSearchReport model = new ReinsuranceSearchReport();

            // var currenyList = InsuranceContext.Currencies.All();

            var currenyList = _summaryDetailService.GetAllCurrency();


            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                var ReinsuranceTransaction = InsuranceContext.ReinsuranceTransactions.All(where: $"VehicleId={Vehicle.Id}").ToList();

                if (ReinsuranceTransaction.Count > 0 && ReinsuranceTransaction != null)
                {
                    ListReinsuranceReport.Add(new ReinsuranceReport()
                    {
                        FirstName = Customer.FirstName,
                        LastName = Customer.LastName,
                        PhoneNumber = Customer.PhoneNumber,
                        PolicyNumber = Policy.PolicyNumber,
                        StartDate = Vehicle.CoverStartDate == null ? null : Vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy"),
                        EndDate = Vehicle.CoverEndDate == null ? null : Vehicle.CoverEndDate.Value.ToString("dd/MM/yyyy"),
                        TransactionDate = Vehicle.TransactionDate == null ? null : Vehicle.TransactionDate.Value.ToString("dd/MM/yyyy"),
                        SumInsured = Vehicle.SumInsured,
                        Premium = Vehicle.Premium,
                        AutoFacSumInsured = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 0 ? Convert.ToDecimal(ReinsuranceTransaction[0].ReinsuranceAmount) : 0.00m),
                        AutoFacPremium = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 0 ? Convert.ToDecimal(ReinsuranceTransaction[0].ReinsurancePremium) : 0.00m),
                        AutoFacCommission = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 0 ? Convert.ToDecimal(ReinsuranceTransaction[0].ReinsuranceCommission) : 0.00m),
                        FacSumInsured = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 1 ? Convert.ToDecimal(ReinsuranceTransaction[1].ReinsuranceAmount) : 0.00m),
                        FacPremium = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 1 ? Convert.ToDecimal(ReinsuranceTransaction[1].ReinsurancePremium) : 0.00m),
                        FacCommission = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 1 ? Convert.ToDecimal(ReinsuranceTransaction[1].ReinsuranceCommission) : 0.00m),
                        Currency = _summaryDetailService.GetCurrencyName(currenyList, item.CurrencyId)
                    });
                }
            }
            model.ReinsuranceReport = ListReinsuranceReport.OrderBy(x => x.FirstName).ToList();
            return View(model);
        }
        public ActionResult ReinsuranceSearchReport(ReinsuranceSearchReport Model)
        {

            var ListReinsuranceReport = new List<ReinsuranceReport>();
            ListReinsuranceReport _ReinsuranceReport = new ListReinsuranceReport();
            _ReinsuranceReport.ReinsuranceReport = new List<Models.ReinsuranceReport>();
            ReinsuranceSearchReport model = new ReinsuranceSearchReport();
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();


            // var VehicleDetails = InsuranceContext.VehicleDetails.All().ToList();

            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrEmpty(Model.FromDate) && !string.IsNullOrEmpty(Model.EndDate))
            {
                fromDate = Convert.ToDateTime(Model.FromDate);
                endDate = Convert.ToDateTime(Model.EndDate);
                ViewBag.fromdate = fromDate.ToString("dd/MM/yyyy");
                ViewBag.enddate = endDate.ToString("dd/MM/yyyy");
            }


            VehicleDetails = VehicleDetails.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();


            var currencyList = _summaryDetailService.GetAllCurrency();

            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                var ReinsuranceTransaction = InsuranceContext.ReinsuranceTransactions.All(where: $"VehicleId={Vehicle.Id}").ToList();

                if (ReinsuranceTransaction.Count > 0 && ReinsuranceTransaction != null)
                {
                    ListReinsuranceReport.Add(new ReinsuranceReport()
                    {
                        FirstName = Customer.FirstName,
                        LastName = Customer.LastName,
                        PhoneNumber = Customer.PhoneNumber,
                        PolicyNumber = Policy.PolicyNumber,
                        StartDate = Vehicle.CoverStartDate == null ? null : Vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy"),
                        EndDate = Vehicle.CoverEndDate == null ? null : Vehicle.CoverEndDate.Value.ToString("dd/MM/yyyy"),
                        TransactionDate = Vehicle.TransactionDate == null ? null : Vehicle.TransactionDate.Value.ToString("dd/MM/yyyy"),
                        SumInsured = Vehicle.SumInsured,
                        Premium = Vehicle.Premium,
                        AutoFacSumInsured = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 0 ? Convert.ToDecimal(ReinsuranceTransaction[0].ReinsuranceAmount) : 0.00m),
                        AutoFacPremium = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 0 ? Convert.ToDecimal(ReinsuranceTransaction[0].ReinsurancePremium) : 0.00m),
                        AutoFacCommission = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 0 ? Convert.ToDecimal(ReinsuranceTransaction[0].ReinsuranceCommission) : 0.00m),
                        FacSumInsured = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 1 ? Convert.ToDecimal(ReinsuranceTransaction[1].ReinsuranceAmount) : 0.00m),
                        FacPremium = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 1 ? Convert.ToDecimal(ReinsuranceTransaction[1].ReinsurancePremium) : 0.00m),
                        FacCommission = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 1 ? Convert.ToDecimal(ReinsuranceTransaction[1].ReinsuranceCommission) : 0.00m),
                        Currency = _summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId)
                    });
                }
            }
            model.ReinsuranceReport = ListReinsuranceReport.OrderBy(x => x.FirstName).ToList();


            return View("ReinsuranceReport", model);
        }
        public ActionResult InsuranceAndLicenceDeliveryReport()

        {

            List<InsuranceAndLicenceDeliveryReportModel> ListLicenceDeliveryReport = new List<InsuranceAndLicenceDeliveryReportModel>();
            ListInsuranceAndLicenceDeliveryReport _LicenceDeliveryReport = new ListInsuranceAndLicenceDeliveryReport();
            _LicenceDeliveryReport.InsuranceAndLicense = new List<InsuranceAndLicenceDeliveryReportModel>();
            InsuraceAndLicenseSearchReportModel model = new InsuraceAndLicenseSearchReportModel();
            var Receipthistory = InsuranceContext.ReceiptHistorys.All().ToList().OrderByDescending(c => c.Id);
            var policydetail = InsuranceContext.PolicyDetails.All().ToList();
            var customers = InsuranceContext.Customers.All().ToList();
            var paymentmethod = InsuranceContext.PaymentMethods.All();
            var userList = UserManager.Users.ToList();

            foreach (var item in Receipthistory)
            {

                var policy = policydetail.FirstOrDefault(x => x.Id == item.PolicyId);
                var payment = paymentmethod.FirstOrDefault(x => x.Id == item.PaymentMethodId) == null ? "" : paymentmethod.FirstOrDefault(x => x.Id == item.PaymentMethodId).Name;
                var customerdetail = customers.FirstOrDefault(x => x.CustomerId == item.CreatedBy);
                if (customerdetail != null)
                {
                    var customerfirstname = customers.FirstOrDefault(x => x.CustomerId == item.CreatedBy) == null ? "" : customers.FirstOrDefault(x => x.CustomerId == item.CreatedBy).FirstName;
                    var customerlastname = customers.FirstOrDefault(x => x.CustomerId == item.CreatedBy) == null ? "" : customers.FirstOrDefault(x => x.CustomerId == item.CreatedBy).LastName;
                    var user = UserManager.FindById(customerdetail.UserID);
                    InsuranceAndLicenceDeliveryReportModel obj = new InsuranceAndLicenceDeliveryReportModel();
                    obj.CustomerName = item.CustomerName;
                    obj.Courier = customerfirstname + " " + customerlastname;
                    obj.PolicyNo = item.PolicyNumber;
                    obj.PaymentMethod = payment;
                    obj.TransactionReference = item.TransactionReference;
                    obj.Receiptno = item.Id;
                    obj.ReceiptAmount = item.AmountPaid;
                    obj.DateDeliverd = item.CreatedOn.Date;
                    obj.TimeofDelivery = item.CreatedOn.ToString("hh:mm");
                    obj.ContactDetails = user.PhoneNumber + ", " + user.Email;
                    obj.AddressOfCustomer = customerdetail.AddressLine1 + "," + customerdetail.AddressLine2;
                    obj.SignaturePath = ConfigurationManager.AppSettings["SignaturePath"] + item.SignaturePath;

                    ListLicenceDeliveryReport.Add(obj);
                }
                else { }
            }

            model.InsuranceAndLicense = ListLicenceDeliveryReport.OrderBy(x => x.CustomerName).ToList();
            return View(model);
        }


        public ActionResult InsuranceAndLicenceDeliverySearchReports(InsuraceAndLicenseSearchReportModel _model)
        {
            List<InsuranceAndLicenceDeliveryReportModel> ListLicenceDeliveryReport = new List<InsuranceAndLicenceDeliveryReportModel>();
            ListInsuranceAndLicenceDeliveryReport _LicenceDeliveryReport = new ListInsuranceAndLicenceDeliveryReport();
            _LicenceDeliveryReport.InsuranceAndLicense = new List<InsuranceAndLicenceDeliveryReportModel>();
            InsuraceAndLicenseSearchReportModel model = new InsuraceAndLicenseSearchReportModel();

            var receipthistory = InsuranceContext.ReceiptHistorys.All().ToList();
            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrEmpty(_model.FromDate) && !string.IsNullOrEmpty(_model.EndDate))
            {
                fromDate = Convert.ToDateTime(_model.FromDate);
                endDate = Convert.ToDateTime(_model.EndDate);


                ViewBag.fromdate = fromDate.ToString("dd/MM/yyyy");
                ViewBag.enddate = endDate.ToString("dd/MM/yyyy");

            }

            receipthistory = receipthistory.Where(c => c.DatePosted >= fromDate && c.DatePosted <= endDate).ToList();
            var policydetail = InsuranceContext.PolicyDetails.All().ToList();
            var customers = InsuranceContext.Customers.All().ToList();
            var paymentmethod = InsuranceContext.PaymentMethods.All();
            var userList = UserManager.Users.ToList();
            foreach (var item in receipthistory)
            {

                var policy = policydetail.FirstOrDefault(x => x.Id == item.PolicyId);
                var payment = paymentmethod.FirstOrDefault(x => x.Id == item.PaymentMethodId) == null ? "" : paymentmethod.FirstOrDefault(x => x.Id == item.PaymentMethodId).Name;
                var customerdetail = customers.FirstOrDefault(x => x.CustomerId == item.CreatedBy);
                if (customerdetail != null)
                {
                    var customerfirstname = customers.FirstOrDefault(x => x.CustomerId == item.CreatedBy) == null ? "" : customers.FirstOrDefault(x => x.CustomerId == item.CreatedBy).FirstName;
                    var customerlastname = customers.FirstOrDefault(x => x.CustomerId == item.CreatedBy) == null ? "" : customers.FirstOrDefault(x => x.CustomerId == item.CreatedBy).LastName;
                    var user = UserManager.FindById(customerdetail.UserID);
                    InsuranceAndLicenceDeliveryReportModel obj = new InsuranceAndLicenceDeliveryReportModel();
                    obj.CustomerName = item.CustomerName;
                    obj.Courier = customerfirstname + " " + customerlastname;
                    obj.PolicyNo = item.PolicyNumber;
                    obj.PaymentMethod = payment;
                    obj.TransactionReference = item.TransactionReference;
                    obj.Receiptno = item.Id;
                    obj.ReceiptAmount = item.AmountPaid;
                    obj.DateDeliverd = item.CreatedOn.Date;
                    obj.TimeofDelivery = item.CreatedOn.ToString("hh:mm");
                    obj.ContactDetails = user.PhoneNumber + ", " + user.Email;
                    obj.AddressOfCustomer = customerdetail.AddressLine1 + "," + customerdetail.AddressLine2;
                    ListLicenceDeliveryReport.Add(obj);
                }
                else { }
            }
            model.InsuranceAndLicense = ListLicenceDeliveryReport.OrderBy(x => x.CustomerName).ToList();
            return View("InsuranceandLicenceDeliveryReport", model);
        }


        public ActionResult RadioLicenceReport()
        {
            var ListRadioLicenceReport = new List<RadioLicenceReportModel>();
            ListRadioLicenceReport _RadioLicence = new ListRadioLicenceReport();
            _RadioLicence.RadioLicence = new List<RadioLicenceReportModel>();
            RadioLicenceSearchReportModel model = new RadioLicenceSearchReportModel();
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();

            var currenyList = _summaryDetailService.GetAllCurrency();


            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                // var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

                ListRadioLicenceReport.Add(new RadioLicenceReportModel()
                {

                    FirstName = Customer.FirstName,
                    LastName = Customer.LastName,
                    Transaction_date = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("dd/MM/yyyy"),
                    RadioLicenseCost = item.RadioLicenseCost,
                    Policy_Number = Policy.PolicyNumber,
                    Currency = _summaryDetailService.GetCurrencyName(currenyList, item.CurrencyId)
                });
            }
            model.RadioLicence = ListRadioLicenceReport.OrderBy(x => x.FirstName).ToList();
            return View(model);
        }
        public ActionResult RadioLicenceSearchReports(RadioLicenceSearchReportModel Model)
        {


            var ListRadioLicenceReport = new List<RadioLicenceReportModel>();
            ListRadioLicenceReport _RadioLicence = new ListRadioLicenceReport();
            _RadioLicence.RadioLicence = new List<RadioLicenceReportModel>();
            RadioLicenceSearchReportModel model = new RadioLicenceSearchReportModel();
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();
            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrEmpty(Model.FromDate) && !string.IsNullOrEmpty(Model.EndDate))
            {
                fromDate = Convert.ToDateTime(Model.FromDate);
                endDate = Convert.ToDateTime(Model.EndDate);
                ViewBag.fromdate = fromDate.ToString("dd/MM/yyyy");
                ViewBag.enddate = endDate.ToString("dd/MM/yyyy");
            }


            VehicleDetails = VehicleDetails.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();


            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                // var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

                ListRadioLicenceReport.Add(new RadioLicenceReportModel()
                {

                    FirstName = Customer.FirstName,
                    LastName = Customer.LastName,
                    Transaction_date = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("dd/MM/yyyy"),
                    RadioLicenseCost = item.RadioLicenseCost,
                    Policy_Number = Policy.PolicyNumber
                });
            }
            model.RadioLicence = ListRadioLicenceReport.OrderBy(x => x.FirstName).ToList();


            return View("RadioLicenceReport", model);
        }
        public ActionResult CustomerListingReport()
        {
            List<CustomerListingReportModel> ListCustomerListingReport = new List<CustomerListingReportModel>();
            ListCustomerListingReport _CustomerListingReport = new ListCustomerListingReport();

            _CustomerListingReport.CustomerListingReport = new List<CustomerListingReportModel>();

            CustomerListingSearchReportModel model = new CustomerListingSearchReportModel();
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();

            var userList = UserManager.Users.ToList();


            foreach (var item in VehicleDetails.Take(100))
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                // var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

                var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");

                if (vehicleSummarydetail != null)
                {

                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);

                    //var _User = UserManager.FindById(Customer.UserID.ToString());

                    var _User = userList.FirstOrDefault(c => c.Id == Customer.UserID.ToString());


                    ListCustomerListingReport.Add(new CustomerListingReportModel()
                    {

                        FirstName = Customer.FirstName == null ? "" : Customer.FirstName,
                        LastName = Customer.LastName == null ? "" : Customer.LastName,
                        Gender = Customer.Gender == null ? "" : Customer.Gender,
                        EmailAddress = _User.Email == null ? "" : _User.Email,
                        ContactNumber = Customer.Countrycode == null ? "" : Customer.Countrycode + "-" + Customer.PhoneNumber == null ? "" : Customer.PhoneNumber,
                        Dateofbirth = Convert.ToDateTime(Customer.DateOfBirth),
                        NationalIdentificationNumber = Customer.NationalIdentificationNumber == null ? "" : Customer.NationalIdentificationNumber,
                        City = Customer.City == null ? "" : Customer.City,
                        Product = InsuranceContext.Products.Single(item.ProductId) == null ? "" : InsuranceContext.Products.Single(item.ProductId).ProductName,
                        VehicleMake = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'") == null ? "" : InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'").MakeDescription,
                        VehicleModel = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'") == null ? "" : InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ModelDescription,
                        VehicleUsage = InsuranceContext.VehicleUsages.Single(item.VehicleUsage == null ? 0 : item.VehicleUsage) == null ? "" : InsuranceContext.VehicleUsages.Single(item.VehicleUsage).VehUsage,
                        PaymentTerm = InsuranceContext.PaymentTerms.Single(item.PaymentTermId) == null ? "" : InsuranceContext.PaymentTerms.Single(item.PaymentTermId).Name,
                        PaymentType = InsuranceContext.PaymentMethods.Single(summary == null ? 0 : summary.PaymentMethodId) == null ? "Cash" : InsuranceContext.PaymentMethods.Single(summary.PaymentMethodId).Name,



                    });

                }
            }
            model.CustomerListingReport = ListCustomerListingReport.OrderBy(x => x.FirstName).ToList();

            return View(model);
        }

        public ActionResult CustomerListingSearchReport(CustomerListingSearchReportModel Model)
        {

            var ListCustomerListingReport = new List<CustomerListingReportModel>();
            ListCustomerListingReport _CustomerListingReport = new ListCustomerListingReport();

            _CustomerListingReport.CustomerListingReport = new List<CustomerListingReportModel>();

            CustomerListingSearchReportModel model = new CustomerListingSearchReportModel();
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();

            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrEmpty(Model.FromDate) && !string.IsNullOrEmpty(Model.EndDate))
            {
                fromDate = Convert.ToDateTime(Model.FromDate);
                endDate = Convert.ToDateTime(Model.EndDate);
                ViewBag.fromdate = fromDate.ToString("dd/MM/yyyy");
                ViewBag.enddate = endDate.ToString("dd/MM/yyyy");
            }

            VehicleDetails = VehicleDetails.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();

            var userList = UserManager.Users.ToList();


            //foreach (var item in VehicleDetails)
            //{
            //    var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
            //    var Customer = InsuranceContext.Customers.Single(item.CustomerId);
            //    // var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

            //    var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");

            //    if (vehicleSummarydetail != null)
            //    {

            //        var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);
            //        var _User = UserManager.FindById(Customer.UserID.ToString());

            //        ListCustomerListingReport.Add(new CustomerListingReportModel()
            //        {
            //            FirstName = Customer.FirstName,
            //            LastName = Customer.LastName,
            //            Gender = Customer.Gender,
            //            EmailAddress = _User.Email,
            //            ContactNumber = Customer.Countrycode + "-" + Customer.PhoneNumber,
            //            Dateofbirth = Convert.ToDateTime(Customer.DateOfBirth),
            //            NationalIdentificationNumber = Customer.NationalIdentificationNumber,
            //            City = Customer.City,
            //            Product = InsuranceContext.Products.Single(item.ProductId).ProductName,
            //            VehicleMake = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'").MakeDescription,
            //            VehicleModel = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ModelDescription,
            //            VehicleUsage = InsuranceContext.VehicleUsages.Single(item.VehicleUsage).VehUsage,
            //            PaymentTerm = InsuranceContext.PaymentTerms.Single(item.PaymentTermId).Name,
            //            PaymentType = InsuranceContext.PaymentMethods.Single(summary.PaymentMethodId).Name,
            //        });
            //    }
            //}




            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                // var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

                var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");

                if (vehicleSummarydetail != null)
                {

                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);

                    //var _User = UserManager.FindById(Customer.UserID.ToString());

                    var _User = userList.FirstOrDefault(c => c.Id == Customer.UserID.ToString());


                    ListCustomerListingReport.Add(new CustomerListingReportModel()
                    {

                        FirstName = Customer.FirstName == null ? "" : Customer.FirstName,
                        LastName = Customer.LastName == null ? "" : Customer.LastName,
                        Gender = Customer.Gender == null ? "" : Customer.Gender,
                        EmailAddress = _User.Email == null ? "" : _User.Email,
                        ContactNumber = Customer.Countrycode == null ? "" : Customer.Countrycode + "-" + Customer.PhoneNumber == null ? "" : Customer.PhoneNumber,
                        Dateofbirth = Convert.ToDateTime(Customer.DateOfBirth),
                        NationalIdentificationNumber = Customer.NationalIdentificationNumber == null ? "" : Customer.NationalIdentificationNumber,
                        City = Customer.City == null ? "" : Customer.City,
                        Product = InsuranceContext.Products.Single(item.ProductId) == null ? "" : InsuranceContext.Products.Single(item.ProductId).ProductName,
                        VehicleMake = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'") == null ? "" : InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'").MakeDescription,
                        VehicleModel = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'") == null ? "" : InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ModelDescription,
                        VehicleUsage = InsuranceContext.VehicleUsages.Single(item.VehicleUsage == null ? 0 : item.VehicleUsage) == null ? "" : InsuranceContext.VehicleUsages.Single(item.VehicleUsage).VehUsage,
                        PaymentTerm = InsuranceContext.PaymentTerms.Single(item.PaymentTermId) == null ? "" : InsuranceContext.PaymentTerms.Single(item.PaymentTermId).Name,
                        PaymentType = InsuranceContext.PaymentMethods.Single(summary == null ? 0 : summary.PaymentMethodId) == null ? "Cash" : InsuranceContext.PaymentMethods.Single(summary.PaymentMethodId).Name,



                    });

                }
            }







            model.CustomerListingReport = ListCustomerListingReport.OrderBy(x => x.FirstName).ToList();


            return View("CustomerListingReport", model);
        }
        public ActionResult DailyReceiptsReport()
        {
            var ListDailyReceiptsReport = new List<DailyReceiptsReportModel>();
            DailyReceiptsSearchReportModel model = new DailyReceiptsSearchReportModel();
            SummaryDetailService _summaryDetailService = new SummaryDetailService();
            var currenyList = _summaryDetailService.GetAllCurrency();

            //var query = "select ReceiptModuleHistory.*, Customer.FirstName +' ' + Customer.LastName as PolicyCreatedBy from ReceiptModuleHistory ";
            //query += " join SummaryDetail on ReceiptModuleHistory.SummaryDetailId = SummaryDetail.id ";
            //query += "Left join Customer  on ReceiptModuleHistory.CreatedBy = Customer.Id  ";


            var query = "select VehicleDetail.[CurrencyId], ReceiptModuleHistory.*, Customer.FirstName +' ' + Customer.LastName as PolicyCreatedBy from ReceiptModuleHistory ";
            query += " join SummaryDetail on ReceiptModuleHistory.SummaryDetailId = SummaryDetail.id ";
            //query +=  " join Customer on SummaryDetail.CreatedBy = Customer.Id";
            query += "Left join Customer  on ReceiptModuleHistory.CreatedBy = Customer.Id  ";
            query += "join VehicleDetail  on ReceiptModuleHistory.PolicyId=VehicleDetail.PolicyId";



            var list = InsuranceContext.Query(query)
               .Select(res => new PreviewReceiptListModel()
               {
                   Id = res.Id,
                   CustomerName = res.CustomerName,

                   PolicyNumber = res.PolicyNumber,
                   DatePosted = res.DatePosted,
                   AmountDue = res.AmountDue,
                   AmountPaid = res.AmountPaid,
                   Balance = res.Balance,
                   paymentMethodType = (res.PaymentMethodId == 1 ? "Cash" : (res.PaymentMethodId == 2 ? "Ecocash" : (res.PaymentMethodId == 3 ? "Swipe" : "MasterVisa Card"))),
                   InvoiceNumber = res.InvoiceNumber,
                   TransactionReference = res.TransactionReference,
                   PolicyCreatedBy = res.PolicyCreatedBy,
                   Currency = _summaryDetailService.GetCurrencyName(currenyList, res.CurrencyId)
               }).ToList();

            //var list = (from res in InsuranceContext.ReceiptHistorys.All().ToList()
            //            select new PreviewReceiptListModel
            //            {
            //                CustomerName = res.CustomerName,                        
            //                PolicyNumber = res.PolicyNumber,
            //                DatePosted = res.DatePosted,
            //                AmountDue = res.AmountDue,
            //                AmountPaid = res.AmountPaid,
            //                Balance = res.Balance,                      
            //                paymentMethodType = (res.PaymentMethodId == 1 ? "Cash" : (res.PaymentMethodId == 2 ? "Ecocash" : (res.PaymentMethodId == 3 ? "Swipe" : "MasterVisa Card"))),
            //                InvoiceNumber = res.InvoiceNumber,
            //                TransactionReference = res.TransactionReference,
            //            }).ToList();


            model.DailyReceiptsReport = list.OrderByDescending(c => c.Id).ToList();

            return View(model);
        }

        public ActionResult ReconciliationReport()
        {
            var ListDailyReceiptsReport = new List<PreviewReceiptListModel>();
            DailyReceiptsSearchReportModel model = new DailyReceiptsSearchReportModel();


            var currenyList = _summaryDetailService.GetAllCurrency();
            //var query1 = "select PolicyDetail.PolicyNumber, Customer.FirstName + ' ' + Customer.LastName as CustomerName, SummaryDetail.CreatedOn as TransactionDate, ";
            //query1 += "SummaryDetail.TotalPremium, PolicyDetail.PolicyNumber as InvoiceNumber, ReceiptModuleHistory.AmountDue, ";
            //query1 += "ReceiptModuleHistory.Id as ReceiptNo, ReceiptModuleHistory.AmountPaid, ";
            //query1 += " case  ReceiptModuleHistory.Id when 0 then 'Yes' else 'No' end as Paid, ReceiptModuleHistory.DatePosted, ";
            //query1 += " ReceiptModuleHistory.Balance  from Customer join PolicyDetail on Customer.Id = PolicyDetail.CustomerId ";
            //query1 += " join VehicleDetail on PolicyDetail.id= VehicleDetail.PolicyId ";
            //query1 += " join SummaryVehicleDetail on VehicleDetail.Id = SummaryVehicleDetail.VehicleDetailsId";
            //query1 += " join SummaryDetail on SummaryDetail.Id= SummaryVehicleDetail.SummaryDetailId";
            //query1 += " left join ReceiptModuleHistory on ReceiptModuleHistory.SummaryDetailId= SummaryDetail.Id";

            var query1 = "select top 100 PolicyDetail.PolicyNumber,createcust.FirstName + '' + createcust.LastName as Created, prcustomer.FirstName + ' ' + prcustomer.LastName as CustomerName, SummaryDetail.CreatedOn as TransactionDate,";
            query1 += "Summarydetail.createdby , ReceiptModuleHistory.PaymentMethodId, SummaryDetail.TotalPremium, PolicyDetail.PolicyNumber as InvoiceNumber, ReceiptModuleHistory.AmountDue,";
            query1 += "ReceiptModuleHistory.Id as ReceiptNo, ReceiptModuleHistory.AmountPaid, ";
            query1 += " case  ReceiptModuleHistory.Id when 0 then 'Yes' else 'No' end as Paid, ReceiptModuleHistory.DatePosted, ";
            query1 += " ReceiptModuleHistory.Balance, VehicleDetail.CurrencyId from Customer as prcustomer join PolicyDetail on prcustomer.Id = PolicyDetail.CustomerId";
            query1 += " join VehicleDetail on PolicyDetail.id = VehicleDetail.PolicyId";
            query1 += " join SummaryVehicleDetail on VehicleDetail.Id = SummaryVehicleDetail.VehicleDetailsId";
            query1 += " join SummaryDetail on SummaryDetail.Id = SummaryVehicleDetail.SummaryDetailId";
            query1 += " left join ReceiptModuleHistory on ReceiptModuleHistory.SummaryDetailId = SummaryDetail.Id";
            query1 += " left join customer as createcust on createcust.id = summarydetail.createdby where VehicleDetail.IsActive=1 and SummaryDetail.isQuotation=0";

            var list = InsuranceContext.Query(query1)
               .Select(res => new PreviewReceiptListModel()
               {
                   Id = Convert.ToInt32(res.ReceiptNo),
                   CustomerName = res.CustomerName,
                   PolicyCreatedBy = res.Created,
                   PolicyNumber = res.PolicyNumber,
                   DatePosted = Convert.ToDateTime(res.DatePosted),
                   TransactionDate = res.TransactionDate,
                   AmountDue = res.AmountDue,
                   AmountPaid = res.AmountPaid,
                   Balance = res.Balance,
                   TotalPremium = Convert.ToInt32(res.TotalPremium),
                   paymentMethodType = (res.PaymentMethodId == 1 ? "Cash" : (res.PaymentMethodId == 2 ? "Ecocash" : (res.PaymentMethodId == 3 ? "Swipe" : "MasterVisa Card"))),
                   // paymentMethodType = (res.PaymentMethodId == 1 ? "Cash" : (res.PaymentMethodId == 2 ? "Ecocash" : (res.PaymentMethodId == 3 ? "Swipe" : "MasterVisa Card"))),
                   InvoiceNumber = res.InvoiceNumber,
                   Currency = _summaryDetailService.GetCurrencyName(currenyList, res.CurrencyId)
                   //TransactionReference = res.TransactionReference,
                   //PolicyCreatedBy = res.PolicyCreatedBy
               }).ToList();




            model.DailyReceiptsReport = list.OrderByDescending(c => c.Id).ToList();

            return View(model);
        }

        public ActionResult ReconciliationSearchReport(DailyReceiptsSearchReportModel Model)
        {
            var ListDailyReceiptsReport = new List<PreviewReceiptListModel>();
            DailyReceiptsSearchReportModel model = new DailyReceiptsSearchReportModel();


            var currenyList = _summaryDetailService.GetAllCurrency();

            var query1 = "select PolicyDetail.PolicyNumber,createcust.FirstName + '' + createcust.LastName as Created, prcustomer.FirstName + ' ' + prcustomer.LastName as CustomerName,   SummaryDetail.CreatedOn TransactionDate,";
            query1 += "Summarydetail.createdby , SummaryDetail.TotalPremium, PolicyDetail.PolicyNumber as InvoiceNumber, ReceiptModuleHistory.AmountDue,";
            query1 += "ReceiptModuleHistory.Id as ReceiptNo, ReceiptModuleHistory.AmountPaid, VehicleDetail.CurrencyId , ";
            query1 += " case  ReceiptModuleHistory.Id when 0 then 'Yes' else 'No' end as Paid, ReceiptModuleHistory.DatePosted, ";
            query1 += " ReceiptModuleHistory.Balance from Customer as prcustomer join PolicyDetail on prcustomer.Id = PolicyDetail.CustomerId";
            query1 += " join VehicleDetail on PolicyDetail.id = VehicleDetail.PolicyId";
            query1 += " join SummaryVehicleDetail on VehicleDetail.Id = SummaryVehicleDetail.VehicleDetailsId";
            query1 += " join SummaryDetail on SummaryDetail.Id = SummaryVehicleDetail.SummaryDetailId";
            query1 += " left join ReceiptModuleHistory on ReceiptModuleHistory.SummaryDetailId = SummaryDetail.Id";
            query1 += " left join customer as createcust on createcust.id = summarydetail.createdby where VehicleDetail.IsActive=1 and SummaryDetail.isQuotation=0";


            //var query1 = "select Distinct PolicyDetail.PolicyNumber, Customer.FirstName + ' ' + Customer.LastName as CustomerName, CONVERT(datetime, CONVERT(varchar,SummaryDetail.CreatedOn, 101)) as TransactionDate, ";
            //query1 += "SummaryDetail.TotalPremium, PolicyDetail.PolicyNumber as InvoiceNumber, ReceiptModuleHistory.AmountDue, ";
            //query1 += "ReceiptModuleHistory.Id as ReceiptNo, ReceiptModuleHistory.AmountPaid, ";
            //query1 += " case  ReceiptModuleHistory.Id when 0 then 'Yes' else 'No' end as Paid, ReceiptModuleHistory.DatePosted, ";
            //query1 += " ReceiptModuleHistory.Balance  from Customer join PolicyDetail on Customer.Id = PolicyDetail.CustomerId ";
            //query1 += " join VehicleDetail on PolicyDetail.id= VehicleDetail.PolicyId ";
            //query1 += " join SummaryVehicleDetail on VehicleDetail.Id = SummaryVehicleDetail.VehicleDetailsId";
            //query1 += " join SummaryDetail on SummaryDetail.Id= SummaryVehicleDetail.SummaryDetailId";
            //query1 += " left join ReceiptModuleHistory on ReceiptModuleHistory.SummaryDetailId= SummaryDetail.Id";




            var list = InsuranceContext.Query(query1)
               .Select(res => new PreviewReceiptListModel()
               {
                   Id = Convert.ToInt32(res.ReceiptNo),
                   CustomerName = res.CustomerName,
                   PolicyCreatedBy = res.Created,
                   PolicyNumber = res.PolicyNumber,
                   DatePosted = Convert.ToDateTime(res.DatePosted),
                   TransactionDate = res.TransactionDate,
                   AmountDue = res.AmountDue,
                   AmountPaid = res.AmountPaid,
                   Balance = res.Balance,
                   TotalPremium = Convert.ToInt32(res.TotalPremium),
                   // paymentMethodType = (res.PaymentMethodId == 1 ? "Cash" : (res.PaymentMethodId == 2 ? "Ecocash" : (res.PaymentMethodId == 3 ? "Swipe" : "MasterVisa Card"))),
                   InvoiceNumber = res.InvoiceNumber,
                   Currency = _summaryDetailService.GetCurrencyName(currenyList, res.CurrencyId)
                   //TransactionReference = res.TransactionReference,
                   //PolicyCreatedBy = res.PolicyCreatedBy
               }).ToList();

            ViewBag.fromdate = Model.FromDate;
            ViewBag.enddate = Model.EndDate;

            model.DailyReceiptsReport = list.Where(c => Convert.ToDateTime(c.TransactionDate.ToShortDateString()) >= Convert.ToDateTime(Model.FromDate) && Convert.ToDateTime(c.TransactionDate.ToShortDateString()) <= Convert.ToDateTime(Model.EndDate)).OrderByDescending(c => c.TransactionDate).ToList();


            return View("ReconciliationReport", model);
        }
        public ActionResult DailyReceiptsSearchReport(DailyReceiptsSearchReportModel Model)
        {
            var ListDailyReceiptsReport = new List<DailyReceiptsReportModel>();
            DailyReceiptsSearchReportModel model = new DailyReceiptsSearchReportModel();


            var query = "select ReceiptModuleHistory.*, Customer.FirstName +' ' + Customer.LastName as PolicyCreatedBy from ReceiptModuleHistory ";
            query += " join SummaryDetail on ReceiptModuleHistory.SummaryDetailId = SummaryDetail.id ";
            //query += " join Customer on SummaryDetail.CreatedBy = Customer.Id";
            query += "Left join Customer  on ReceiptModuleHistory.CreatedBy = Customer.Id ";


            var list = InsuranceContext.Query(query)
               .Select(res => new PreviewReceiptListModel()
               {
                   CustomerName = res.CustomerName,
                   PolicyNumber = res.PolicyNumber,
                   DatePosted = res.DatePosted,
                   AmountDue = res.AmountDue,
                   AmountPaid = res.AmountPaid,
                   Balance = res.Balance,
                   paymentMethodType = (res.PaymentMethodId == 1 ? "Cash" : (res.PaymentMethodId == 2 ? "Ecocash" : (res.PaymentMethodId == 3 ? "Swipe" : "MasterVisa Card"))),
                   InvoiceNumber = res.InvoiceNumber,
                   TransactionReference = res.TransactionReference,
                   PolicyCreatedBy = res.PolicyCreatedBy
               }).ToList();

            ViewBag.fromdate = Model.FromDate;
            ViewBag.enddate = Model.EndDate;

            model.DailyReceiptsReport = list.Where(c => c.DatePosted >= Convert.ToDateTime(Model.FromDate) && c.DatePosted <= Convert.ToDateTime(Model.EndDate)).OrderByDescending(c => c.DatePosted).ToList();


            return View("ReconciliationReport", model);
        }
        public ActionResult LapsedPoliciesReport()
        {
            var ListLapsedPoliciesReport = new List<LapsedPoliciesReportModels>();
            ListLapsedPoliciesReport _LapsedPoliciesReport = new ListLapsedPoliciesReport();
            _LapsedPoliciesReport.LapsedPoliciesReport = new List<LapsedPoliciesReportModels>();
            LapsedPoliciesSearchReportModels _model = new LapsedPoliciesSearchReportModels();
            var whereClause = "isLapsed = 'True' or " + $"CAST(RenewalDate as date) < '{DateTime.Now.ToString("yyyy-MM-dd")}'";
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: whereClause).ToList();

            var currenyList = _summaryDetailService.GetAllCurrency();

            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                var make = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'");
                var model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                var ReinsuranceTransaction = InsuranceContext.ReinsuranceTransactions.All(where: $"VehicleId={Vehicle.Id}").ToList();
                if (ReinsuranceTransaction.Count > 0 && ReinsuranceTransaction != null)
                {
                    ListLapsedPoliciesReport.Add(new LapsedPoliciesReportModels()
                    {
                        customerName = Customer.FirstName + " " + Customer.LastName,
                        contactDetails = Customer.Countrycode + "-" + Customer.PhoneNumber,
                        Premium = Vehicle.Premium,
                        sumInsured = Vehicle.SumInsured,
                        vehicleMake = make.MakeDescription,
                        vehicleModel = model.ModelDescription,
                        startDate = Vehicle.CoverStartDate == null ? null : Vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy"),
                        endDate = Vehicle.CoverEndDate == null ? null : Vehicle.CoverEndDate.Value.ToString("dd/MM/yyyy"),
                        Currency = _summaryDetailService.GetCurrencyName(currenyList, item.CurrencyId)
                    });
                }
            }
            _model.LapsedPoliciesReport = ListLapsedPoliciesReport.OrderBy(x => x.customerName).ToList();
            return View(_model);
        }
        public ActionResult LapsedPoliciesSearchReport(LapsedPoliciesSearchReportModels Model)
        {
            var ListLapsedPoliciesReport = new List<LapsedPoliciesReportModels>();
            ListLapsedPoliciesReport _LapsedPoliciesReport = new ListLapsedPoliciesReport();
            _LapsedPoliciesReport.LapsedPoliciesReport = new List<LapsedPoliciesReportModels>();
            LapsedPoliciesSearchReportModels _model = new LapsedPoliciesSearchReportModels();
            var whereClause = "isLapsed = 'True' or " + $"CAST(RenewalDate as date) < '{DateTime.Now.ToString("yyyy-MM-dd")}'";
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: whereClause).ToList();

            #region
            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;
            if (!string.IsNullOrEmpty(Model.FromDate) && !string.IsNullOrEmpty(Model.EndDate))
            {
                fromDate = Convert.ToDateTime(Model.FromDate);
                endDate = Convert.ToDateTime(Model.EndDate);
            }
            VehicleDetails = VehicleDetails.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();

            #endregion


            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                var make = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'");
                var model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                var ReinsuranceTransaction = InsuranceContext.ReinsuranceTransactions.All(where: $"VehicleId={Vehicle.Id}").ToList();
                if (ReinsuranceTransaction.Count > 0 && ReinsuranceTransaction != null)
                {
                    ListLapsedPoliciesReport.Add(new LapsedPoliciesReportModels()
                    {
                        customerName = Customer.FirstName + " " + Customer.LastName,
                        contactDetails = Customer.Countrycode + "-" + Customer.PhoneNumber,
                        Premium = Vehicle.Premium,
                        sumInsured = Vehicle.SumInsured,
                        vehicleMake = make.MakeDescription,
                        vehicleModel = model.ModelDescription,
                        startDate = Vehicle.CoverStartDate == null ? null : Vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy"),
                        endDate = Vehicle.CoverEndDate == null ? null : Vehicle.CoverEndDate.Value.ToString("dd/MM/yyyy")
                    });
                }
            }

            ViewBag.fromdate = Model.FromDate;

            ViewBag.enddate = Model.EndDate;
            _model.LapsedPoliciesReport = ListLapsedPoliciesReport.OrderBy(x => x.customerName).ToList();


            return View("LapsedPoliciesReport", _model);
        }


        public ActionResult ProductivityReport()
        {

            //var ListProductiviyReport = new List<ProductiviyReportModel>();
            List<ProductiviyReportModel> listProductiviyReport = new List<ProductiviyReportModel>();
            List<ProductiviyReportModel> listProductiviyReport_Endorsed = new List<ProductiviyReportModel>();
            ListProductiviyReportModel _listListProductiviyReport = new ListProductiviyReportModel();
            _listListProductiviyReport.ListProductiviyReport = new List<ProductiviyReportModel>();
            ProductiviySearchReportModel model = new ProductiviySearchReportModel();
            //   var vehicledetail = InsuranceContext.VehicleDetails.All(where: $"IsActive = 'True'or IsActive is null").OrderByDescending(c=>c.Id).ToList();

            try
            {

         
            var vehicledetail = InsuranceContext.VehicleDetails.All().OrderByDescending(c => c.Id).ToList();
            var endorsedVehicle = InsuranceContext.EndorsementVehicleDetails.All().OrderByDescending(x => x.PrimaryVehicleId).ToList();
            var currencyList = _summaryDetailService.GetAllCurrency();
            var endorsePayment = InsuranceContext.EndorsementPaymentInformations.All().ToList();
            var endorsedPolicy = InsuranceContext.EndorsementPolicyDetails.All();
            var productlist = InsuranceContext.Products.All().ToList();
            var endorsed_customer = InsuranceContext.EndorsementCustomers.All();
            foreach (var item in vehicledetail.Take(100))
            {
                var item_endorsed = endorsedVehicle.FirstOrDefault(x => x.PrimaryVehicleId == item.Id);
                var endorsepay = endorsePayment.FirstOrDefault(x => x.PrimaryPolicyId == item.PolicyId);
                //if (item.IsActive == false && !item.isLapsed) // for disable vehicle
                //    continue;


                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var enfpol = endorsedPolicy.Where(x => x.PrimaryPolicyId == policy.Id);


                var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
                if (vehicleSummarydetail != null)
                {
                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);

                    if (summary != null)
                    {
                        var customer = InsuranceContext.Customers.Single(item.CustomerId);
                        var customer_endorsed = endorsed_customer.FirstOrDefault(x => x.PrimeryCustomerId == item.CustomerId);
                        var customerForRole = InsuranceContext.Customers.Single(summary.CreatedBy);

                        if (customerForRole != null)
                        {

                            var userDetials = UserManager.FindById(customerForRole.UserID);

                            if (userDetials != null)
                            {

                                var roles = InsuranceContext.Query("select Name from AspNetUserRoles join AspNetRoles on AspNetUserRoles.RoleId = AspNetRoles.Id where UserId='" + userDetials.Id + "'  ")
                 .Select(x => new CustomerModel()
                 {
                     UserRoleName = x.Name,
                 }).FirstOrDefault();


                                //if (roles != null && roles.UserRoleName.ToString() == "Staff")
                                //{
                                ProductiviyReportModel obj = new ProductiviyReportModel();
                                obj.CustomerName = customer.FirstName + " " + customer.LastName;
                                obj.VehicleId = item.Id;
                                obj.PolicyNumber = policy.PolicyNumber;
                                obj.TransactionDate = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("dd/MM/yyyy");

                                obj.PremiumDue = Convert.ToDecimal(item.Premium);

                                obj.SumInsured = Convert.ToDecimal(item.SumInsured);
                                obj.UserName = userDetials.Email;
                                obj.Product = productlist.FirstOrDefault(x => x.Id == item.ProductId).ProductName;
                                //InsuranceContext.Products.Single(item.ProductId).ProductName;
                                obj.Currency = _summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId);

                                obj.RenewPolicyNumber = item.RenewPolicyNumber;
                                if (summary.isQuotation)
                                    obj.PolicyStatus = "Quotation";
                                else
                                    obj.PolicyStatus = "Policy";

                                obj.isLapsed = item.isLapsed;
                                obj.IsActive = item.IsActive == null ? false : item.IsActive.Value;
                                obj.IsEndorsed = false;



                                listProductiviyReport.Add(obj);
                                if (item_endorsed != null)
                                {
                                    if (endorsepay != null)
                                    {


                                        ProductiviyReportModel obj_endorsed = new ProductiviyReportModel();
                                        if (customer_endorsed != null)
                                        {
                                            obj_endorsed.CustomerName = customer_endorsed.FirstName + "" + customer_endorsed.LastName;
                                        }
                                        obj_endorsed.VehicleId = item.Id;
                                        obj_endorsed.PolicyNumber = policy.PolicyNumber;
                                        obj_endorsed.TransactionDate = item_endorsed.TransactionDate.Value.ToString("dd/MM/yyyy");
                                        obj_endorsed.PremiumDue = Convert.ToDecimal(item_endorsed.Premium);
                                        obj_endorsed.SumInsured = Convert.ToDecimal(item_endorsed.SumInsured);
                                        obj_endorsed.isLapsed = item_endorsed.isLapsed;
                                        obj_endorsed.IsActive = item_endorsed.IsActive == null ? false : item_endorsed.IsActive.Value;
                                        obj_endorsed.RenewPolicyNumber = item.RenewPolicyNumber;
                                        obj_endorsed.UserName = userDetials.Email;
                                        obj_endorsed.Product = productlist.FirstOrDefault(x => x.Id == item_endorsed.ProductId).ProductName;
                                        obj_endorsed.Currency = _summaryDetailService.GetCurrencyName(currencyList, item_endorsed.CurrencyId);
                                        if (summary.isQuotation)
                                            obj_endorsed.PolicyStatus = "Quotation";
                                        else
                                            obj_endorsed.PolicyStatus = "Policy";
                                        obj_endorsed.IsEndorsed = true;
                                        listProductiviyReport.Add(obj_endorsed);
                                    }
                                }
                                //}
                            }
                        }




                    }
                }
            }
            model.ListProductiviyReport = listProductiviyReport.OrderBy(x => x.VehicleId).ToList();



            }
            catch (Exception ex)
            {

            }


            return View(model);
        }














        public ActionResult ProductivitySearchReport(ProductiviySearchReportModel Model)
        {

            List<ProductiviyReportModel> listProductiviyReport = new List<ProductiviyReportModel>();
            ListProductiviyReportModel _listListProductiviyReport = new ListProductiviyReportModel();
            _listListProductiviyReport.ListProductiviyReport = new List<ProductiviyReportModel>();
            ProductiviySearchReportModel model = new ProductiviySearchReportModel();
            ViewBag.fromdate = Model.FromDate;
            ViewBag.enddate = Model.EndDate;
            var vehicledetail = InsuranceContext.VehicleDetails.All().OrderByDescending(c => c.Id).ToList();

            var currencyList = _summaryDetailService.GetAllCurrency();

            #region
            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;
            if (!string.IsNullOrEmpty(Model.FromDate) && !string.IsNullOrEmpty(Model.EndDate))
            {
                fromDate = Convert.ToDateTime(Model.FromDate);
                endDate = Convert.ToDateTime(Model.EndDate);
            }
            var Vehicledetail = vehicledetail.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();


            var endorsedVehicle = InsuranceContext.EndorsementVehicleDetails.All().OrderByDescending(x => x.PrimaryVehicleId).ToList();

            var endorsePayment = InsuranceContext.EndorsementPaymentInformations.All().ToList();
            var endorsedPolicy = InsuranceContext.EndorsementPolicyDetails.All();
            var productlist = InsuranceContext.Products.All().ToList();
            var endorsed_customer = InsuranceContext.EndorsementCustomers.All();

            #endregion
            foreach (var item in Vehicledetail)
            {
                var item_endorsed = endorsedVehicle.FirstOrDefault(x => x.PrimaryVehicleId == item.Id);
                var endorsepay = endorsePayment.FirstOrDefault(x => x.PrimaryPolicyId == item.PolicyId);
                //if (item.IsActive == false && !item.isLapsed) // for disable vehicle
                //    continue;


                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);


                var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
                if (vehicleSummarydetail != null)
                {
                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);

                    if (summary != null)
                    {
                        var customer = InsuranceContext.Customers.Single(item.CustomerId);
                        var customer_endorsed = endorsed_customer.FirstOrDefault(x => x.PrimeryCustomerId == item.CustomerId);
                        var customerForRole = InsuranceContext.Customers.Single(summary.CreatedBy);

                        if (customerForRole != null)
                        {

                            var userDetials = UserManager.FindById(customerForRole.UserID);

                            if (userDetials != null)
                            {

                                var roles = InsuranceContext.Query("select Name from AspNetUserRoles join AspNetRoles on AspNetUserRoles.RoleId = AspNetRoles.Id where UserId='" + userDetials.Id + "'  ")
                 .Select(x => new CustomerModel()
                 {
                     UserRoleName = x.Name,
                 }).FirstOrDefault();


                                if (roles != null && roles.UserRoleName.ToString() == "Staff")
                                {
                                    ProductiviyReportModel obj = new ProductiviyReportModel();
                                    obj.CustomerName = customer.FirstName + " " + customer.LastName;
                                    obj.PolicyNumber = policy.PolicyNumber;

                                    obj.VehicleId = item.Id;
                                    //obj.TransactionDate = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("dd/MM/yyyy");
                                    obj.TransactionDate = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("MM/dd/yyyy");
                                    // obj.PremiumDue = Convert.ToDecimal(item.Premium + item.StampDuty + item.ZTSCLevy + item.RadioLicenseCost); // 28_may_2019
                                    obj.PremiumDue = Convert.ToDecimal(item.Premium);
                                    obj.SumInsured = Convert.ToDecimal(item.SumInsured);
                                    obj.UserName = userDetials.Email;
                                    obj.Product = InsuranceContext.Products.Single(item.ProductId).ProductName;
                                    obj.Currency = _summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId);

                                    obj.RenewPolicyNumber = item.RenewPolicyNumber;

                                    if (summary.isQuotation)
                                        obj.PolicyStatus = "Quotation";
                                    else
                                        obj.PolicyStatus = "Policy";

                                    obj.isLapsed = item.isLapsed;
                                    obj.IsActive = item.IsActive.Value;
                                    obj.IsEndorsed = false;

                                    listProductiviyReport.Add(obj);

                                    if (item_endorsed != null)
                                    {
                                        if (endorsepay != null)
                                        {

                                            ProductiviyReportModel obj_endorsed = new ProductiviyReportModel();
                                            if (customer_endorsed != null)
                                            {
                                                obj_endorsed.CustomerName = customer_endorsed.FirstName + "" + customer_endorsed.LastName;
                                            }
                                            obj_endorsed.PolicyNumber = policy.PolicyNumber;
                                            obj_endorsed.VehicleId = item.Id;
                                            obj_endorsed.TransactionDate = item_endorsed.TransactionDate.Value.ToString("dd/MM/yyyy");
                                            obj_endorsed.PremiumDue = Convert.ToDecimal(item_endorsed.Premium);
                                            obj_endorsed.SumInsured = Convert.ToDecimal(item_endorsed.SumInsured);
                                            obj_endorsed.isLapsed = item_endorsed.isLapsed;
                                            obj_endorsed.IsActive = item_endorsed.IsActive == null ? false : item_endorsed.IsActive.Value;
                                            obj_endorsed.RenewPolicyNumber = item.RenewPolicyNumber;
                                            obj_endorsed.UserName = userDetials.Email;
                                            obj_endorsed.Product = productlist.FirstOrDefault(x => x.Id == item_endorsed.ProductId).ProductName;
                                            obj_endorsed.Currency = _summaryDetailService.GetCurrencyName(currencyList, item_endorsed.CurrencyId);
                                            if (summary.isQuotation)
                                                obj_endorsed.PolicyStatus = "Quotation";
                                            else
                                                obj_endorsed.PolicyStatus = "Policy";
                                            obj_endorsed.IsEndorsed = true;
                                            listProductiviyReport.Add(obj_endorsed);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            model.ListProductiviyReport = listProductiviyReport.OrderByDescending(x => x.VehicleId).ToList();
            return View("ProductivityReport", model);
        }







        //public ActionResult ProductivityReport()
        //{

        //    //var ListProductiviyReport = new List<ProductiviyReportModel>();
        //    List<ProductiviyReportModel> listProductiviyReport = new List<ProductiviyReportModel>();
        //    ListProductiviyReportModel _listListProductiviyReport = new ListProductiviyReportModel();
        //    _listListProductiviyReport.ListProductiviyReport = new List<ProductiviyReportModel>();
        //    ProductiviySearchReportModel model = new ProductiviySearchReportModel();
        //    //   var vehicledetail = InsuranceContext.VehicleDetails.All(where: $"IsActive = 'True'or IsActive is null").OrderByDescending(c=>c.Id).ToList();

        //    var vehicledetail = InsuranceContext.VehicleDetails.All().OrderByDescending(c => c.Id).ToList();

        //    var currencyList = _summaryDetailService.GetAllCurrency();


        //    foreach (var item in vehicledetail)
        //    {

        //        //if (item.IsActive == false && !item.isLapsed) // for disable vehicle
        //        //    continue;


        //        var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);


        //        var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
        //        if (vehicleSummarydetail != null)
        //        {
        //            var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);

        //            if (summary != null)
        //            {
        //                var customer = InsuranceContext.Customers.Single(item.CustomerId);
        //                var customerForRole = InsuranceContext.Customers.Single(summary.CreatedBy);

        //                if (customerForRole != null)
        //                {

        //                    var userDetials = UserManager.FindById(customerForRole.UserID);

        //                    if (userDetials != null)
        //                    {

        //                        var roles = InsuranceContext.Query("select Name from AspNetUserRoles join AspNetRoles on AspNetUserRoles.RoleId = AspNetRoles.Id where UserId='" + userDetials.Id + "'  ")
        //         .Select(x => new CustomerModel()
        //         {
        //             UserRoleName = x.Name,
        //         }).FirstOrDefault();


        //                        //if (roles != null && roles.UserRoleName.ToString() == "Staff")
        //                        //{
        //                        ProductiviyReportModel obj = new ProductiviyReportModel();
        //                        obj.CustomerName = customer.FirstName + " " + customer.LastName;
        //                        obj.PolicyNumber = policy.PolicyNumber;
        //                        obj.TransactionDate = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("dd/MM/yyyy");
        //                        //obj.PremiumDue = Convert.ToDecimal(item.Premium + item.StampDuty + item.ZTSCLevy + item.RadioLicenseCost); // 28_may_2019
        //                        obj.PremiumDue = Convert.ToDecimal(item.Premium);
        //                        obj.SumInsured = Convert.ToDecimal(item.SumInsured);
        //                        obj.UserName = userDetials.Email;
        //                        obj.Product = InsuranceContext.Products.Single(item.ProductId).ProductName;
        //                        obj.Currency = _summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId);

        //                        obj.RenewPolicyNumber = item.RenewPolicyNumber;
        //                        if (summary.isQuotation)
        //                            obj.PolicyStatus = "Quotation";
        //                        else
        //                            obj.PolicyStatus = "Policy";

        //                        obj.isLapsed = item.isLapsed;
        //                        obj.IsActive = item.IsActive == null ? false : item.IsActive.Value;



        //                        listProductiviyReport.Add(obj);
        //                        //}
        //                    }
        //                }




        //            }
        //        }
        //    }
        //    model.ListProductiviyReport = listProductiviyReport.OrderBy(x => x.UserName).ToList();

        //    return View(model);
        //}
        //public ActionResult ProductivitySearchReport(ProductiviySearchReportModel Model)
        //{

        //    List<ProductiviyReportModel> listProductiviyReport = new List<ProductiviyReportModel>();
        //    ListProductiviyReportModel _listListProductiviyReport = new ListProductiviyReportModel();
        //    _listListProductiviyReport.ListProductiviyReport = new List<ProductiviyReportModel>();
        //    ProductiviySearchReportModel model = new ProductiviySearchReportModel();
        //    ViewBag.fromdate = Model.FromDate;
        //    ViewBag.enddate = Model.EndDate;
        //    var vehicledetail = InsuranceContext.VehicleDetails.All().OrderByDescending(c => c.Id).ToList();

        //    var currencyList = _summaryDetailService.GetAllCurrency();

        //    #region
        //    DateTime fromDate = DateTime.Now.AddDays(-1);
        //    DateTime endDate = DateTime.Now;
        //    if (!string.IsNullOrEmpty(Model.FromDate) && !string.IsNullOrEmpty(Model.EndDate))
        //    {
        //        fromDate = Convert.ToDateTime(Model.FromDate);
        //        endDate = Convert.ToDateTime(Model.EndDate);
        //    }
        //    var Vehicledetail = vehicledetail.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();

        //    #endregion
        //    foreach (var item in Vehicledetail)
        //    {

        //        //if (item.IsActive == false && !item.isLapsed) // for disable vehicle
        //        //    continue;


        //        var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);


        //        var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
        //        if (vehicleSummarydetail != null)
        //        {
        //            var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);

        //            if (summary != null)
        //            {
        //                var customer = InsuranceContext.Customers.Single(item.CustomerId);
        //                var customerForRole = InsuranceContext.Customers.Single(summary.CreatedBy);

        //                if (customerForRole != null)
        //                {

        //                    var userDetials = UserManager.FindById(customerForRole.UserID);

        //                    if (userDetials != null)
        //                    {

        //                        var roles = InsuranceContext.Query("select Name from AspNetUserRoles join AspNetRoles on AspNetUserRoles.RoleId = AspNetRoles.Id where UserId='" + userDetials.Id + "'  ")
        //         .Select(x => new CustomerModel()
        //         {
        //             UserRoleName = x.Name,
        //         }).FirstOrDefault();


        //                        if (roles != null && roles.UserRoleName.ToString() == "Staff")
        //                        {
        //                            ProductiviyReportModel obj = new ProductiviyReportModel();
        //                            obj.CustomerName = customer.FirstName + " " + customer.LastName;
        //                            obj.PolicyNumber = policy.PolicyNumber;
        //                            //obj.TransactionDate = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("dd/MM/yyyy");
        //                            obj.TransactionDate = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("MM/dd/yyyy");
        //                            // obj.PremiumDue = Convert.ToDecimal(item.Premium + item.StampDuty + item.ZTSCLevy + item.RadioLicenseCost); // 28_may_2019
        //                            obj.PremiumDue = Convert.ToDecimal(item.Premium);
        //                            obj.SumInsured = Convert.ToDecimal(item.SumInsured);
        //                            obj.UserName = userDetials.Email;
        //                            obj.Product = InsuranceContext.Products.Single(item.ProductId).ProductName;
        //                            obj.Currency = _summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId);

        //                            obj.RenewPolicyNumber = item.RenewPolicyNumber;

        //                            if (summary.isQuotation)
        //                                obj.PolicyStatus = "Quotation";
        //                            else
        //                                obj.PolicyStatus = "Policy";

        //                            obj.isLapsed = item.isLapsed;
        //                            obj.IsActive = item.IsActive.Value;


        //                            listProductiviyReport.Add(obj);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    model.ListProductiviyReport = listProductiviyReport.OrderByDescending(x => x.UserName).ToList();
        //    return View("ProductivityReport", model);
        //}
        public ActionResult ClaimsPaymentReport()
        {
            var results = new List<ClaimRegistrationProviderModel>();
            try
            {
                //results = (from vehicalDetials in InsuranceContext.VehicleDetails.All()
                //           join customer in InsuranceContext.Customers.All()
                //           on vehicalDetials.CustomerId equals customer.Id
                //           join make in InsuranceContext.VehicleMakes.All()
                //           on vehicalDetials.MakeId equals make.MakeCode
                //           join vehicalModel in InsuranceContext.VehicleModels.All()
                //           on vehicalDetials.ModelId equals vehicalModel.ModelCode
                //           join coverType in InsuranceContext.CoverTypes.All().ToList()
                //           on vehicalDetials.CoverTypeId equals coverType.Id
                //           join user in UserManager.Users
                //           on customer.UserID equals user.Id

                //           select new RiskDetailModel
                //           {
                //               PolicyExpireDate = vehicalDetials.CoverEndDate.Value.ToShortDateString(),
                //               CoverTypeName = coverType.Name,
                //               SumInsured = vehicalDetials.SumInsured,
                //               VechicalMake = make.MakeDescription,
                //               VechicalModel = vehicalModel.ModelDescription,
                //               VehicleYear = vehicalDetials.VehicleYear,
                //               CustomerDetails = new CustomerModel { FirstName = customer.FirstName, LastName = customer.LastName, PhoneNumber = customer.PhoneNumber, EmailAddress = user.Email }


                //           }).ToList();



                var query = "select ClaimRegistrationProviderDetial.Id,ClaimRegistrationId,RegistrationNo,PolicyNumber,ClaimantName, ProviderType, ServiceProviderName,ClaimRegistrationProviderDetial.CreatedOn, ServiceProviderFee from ClaimRegistrationProviderDetial";
                query += " join ServiceProvider on ClaimRegistrationProviderDetial.ServiceProviderId = ServiceProvider.Id ";
                query += " left join ClaimRegistration on ClaimRegistration.Id=ClaimRegistrationProviderDetial.ClaimRegistrationId";


                query += " join ServiceProviderType on ClaimRegistrationProviderDetial.ServiceProviderTypeId = ServiceProviderType.id  where ClaimRegistrationProviderDetial.IsActive=1 ";


                results = InsuranceContext.Query(query)
.Select(x => new ClaimRegistrationProviderModel()
{
    Id = x.Id,
    ClaimRegistrationId = x.ClaimRegistrationId,
    ServiceProviderType = x.ProviderType,
    ServiceProviderName = x.ServiceProviderName,
    CreatedOn = x.CreatedOn,
    ServiceProviderFee = x.ServiceProviderFee,
    RegistrationNo = x.RegistrationNo,
    PolicyNumber = x.PolicyNumber,
    ClaimantName = x.ClaimantName



}).ToList();





            }
            catch (Exception ex)
            {

            }


            return View(results);
        }
        public ActionResult LoyaltyPointsReport()
        {
            var ListDailyReceiptsReport = new List<LoyaltyPointsModel>();
            LoyaltyPointsReport _LoyaltyPt = new LoyaltyPointsReport();
            _LoyaltyPt.LoyaltyPoints = new List<LoyaltyPointsModel>();
            LoyaltyPointsReportSeachModels model = new LoyaltyPointsReportSeachModels();


            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList().Take(500);
            var currencyList = _summaryDetailService.GetAllCurrency();

            if (VehicleDetails != null)
            {
                foreach (var item in VehicleDetails)
                {
                    var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                    var User = UserManager.FindById(Customer.UserID.ToString());
                    //var loyalityDetail = InsuranceContext.LoyaltyDetails.All(where: $"CustomerId={item.CustomerId}").Sum(x => x.PointsEarned);


                    var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
                    if (vehicleSummarydetail != null)
                    {
                        var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);
                        if (summary != null)
                        {

                            if (summary.isQuotation != true)
                            {

                                var loyalityDetail = InsuranceContext.LoyaltyDetails.All(where: $"CustomerId={item.CustomerId}").Sum(x => x.PointsEarned);

                                if (loyalityDetail != null)
                                {

                                    var ListDailyReceiptsDetails = ListDailyReceiptsReport.FirstOrDefault(c => c.PolicyId == item.PolicyId);

                                    if (ListDailyReceiptsDetails == null)
                                    {
                                        ListDailyReceiptsReport.Add(new LoyaltyPointsModel()
                                        {
                                            CustomerName = Customer.FirstName + " " + Customer.LastName,
                                            CellPhoneNumber = Customer.Countrycode + "-" + Customer.PhoneNumber,
                                            Address = Customer.AddressLine1 + " " + Customer.AddressLine2,
                                            SumInsured = Convert.ToDecimal(summary.TotalSumInsured),
                                            PremiumPaid = Convert.ToDecimal(summary.TotalPremium),
                                            EmailAddress = User.Email,
                                            LoyaltyPoints = Convert.ToString(loyalityDetail),
                                            PolicyId = item.PolicyId,
                                            Currency = _summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId),
                                            TransactionDate = item.TransactionDate == null ? DateTime.MinValue : item.TransactionDate.Value

                                        });
                                    }


                                }
                                else
                                {

                                }

                            }
                        }
                    }
                }
            }
            model.LoyaltyPoints = ListDailyReceiptsReport.OrderBy(x => x.CustomerName).ToList();
            return View(model);
        }

        public ActionResult LoyaltyPointsSearchReport(LoyaltyPointsReportSeachModels Model)
        {
            var ListDailyReceiptsReport = new List<LoyaltyPointsModel>();
            LoyaltyPointsReport _LoyaltyPointsReport = new LoyaltyPointsReport();
            _LoyaltyPointsReport.LoyaltyPoints = new List<LoyaltyPointsModel>();
            LoyaltyPointsReportSeachModels _model = new LoyaltyPointsReportSeachModels();
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();
            ViewBag.fromdate = Model.FromDate;
            ViewBag.enddate = Model.EndDate;

            var Vehicledetail = VehicleDetails.Where(c => c.TransactionDate >= Convert.ToDateTime(Model.FromDate) && c.TransactionDate <= Convert.ToDateTime(Model.EndDate)).ToList();



            #region
            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;
            if (!string.IsNullOrEmpty(Model.FromDate) && !string.IsNullOrEmpty(Model.EndDate))
            {
                fromDate = Convert.ToDateTime(Model.FromDate);
                endDate = Convert.ToDateTime(Model.EndDate);
            }
            VehicleDetails = VehicleDetails.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();

            var currencyList = _summaryDetailService.GetAllCurrency();


            #endregion
            if (VehicleDetails != null)
            {
                foreach (var item in VehicleDetails)
                {

                    var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
                    if (vehicleSummarydetail != null)
                    {
                        var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);
                        if (summary != null)
                        {

                            if (summary.isQuotation != true)
                            {

                                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                                var User = UserManager.FindById(Customer.UserID.ToString());
                                var loyalityDetail = InsuranceContext.LoyaltyDetails.All(where: $"CustomerId={item.CustomerId}").Sum(x => x.PointsEarned);
                                if (loyalityDetail != null)
                                {

                                    var ListDailyReceiptsDetails = ListDailyReceiptsReport.FirstOrDefault(c => c.PolicyId == item.PolicyId);

                                    if (ListDailyReceiptsDetails == null)
                                    {
                                        ListDailyReceiptsReport.Add(new LoyaltyPointsModel()
                                        {
                                            CustomerName = Customer.FirstName + " " + Customer.LastName,
                                            CellPhoneNumber = Customer.Countrycode + "-" + Customer.PhoneNumber,
                                            Address = Customer.AddressLine1 + " " + Customer.AddressLine2,
                                            SumInsured = Convert.ToDecimal(summary.TotalSumInsured),
                                            PremiumPaid = Convert.ToDecimal(summary.TotalPremium),
                                            EmailAddress = User.Email,
                                            LoyaltyPoints = Convert.ToString(loyalityDetail),
                                            PolicyId = item.PolicyId,
                                            Currency = _summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId),
                                            TransactionDate = item.TransactionDate == null ? DateTime.MinValue : item.TransactionDate.Value
                                        });
                                    }
                                }
                                else
                                {

                                }

                            }
                        }
                    }



                }
            }
            _model.LoyaltyPoints = ListDailyReceiptsReport.OrderBy(x => x.CustomerName).ToList();
            return View("LoyaltyPointsReport", _model);
        }



        public ActionResult ClaimsRegistrationReport()
        {
            try
            {
                ClaimSearchReport model = new ClaimSearchReport();
                List<ClaimReportModel> ListClaimReport = new List<ClaimReportModel>();

                // Claim Register report
                // var query = "Select a.PolicyNumber as Policyno,a.ClaimNumber as ClaimNo,a.ClaimStatus as Claimstatus,a.DateOfLoss as LossOfDate,
                //b.ClaimantName as INsuredName,a.DateOfNotifications as DateNotifications,a.DescriptionOfLoss as LossofDescriptiom,a.RegistrationNo as registrationno ,
                //a.MakeId,a.ModelId,a.EstimatedValueOfLoss as EstimatedLoss,b.ClaimantName as Name,b.CoverStartDate as PStartDate,b.CoverEndDate as PEndDate,v.SumInsured as suminsured ,
                //c.Name as CoverType,make.MakeDescription as CoverMake,model.ModelDescription as covermodel from ClaimRegistration as a inner join ClaimNotification as b on a.ClaimNotificationId = b.Id 
                //left join VehicleDetail as v on b.VehicleId = v.Id inner join CoverType as c on v.CoverTypeId = c.Id inner join VehicleMake as make on a.MakeId = make.MakeCode inner join VehicleModel as 
                //model on a.ModelId = model.ModelCode";


                var query = "select PolicyDetail.Id, Customer.FirstName + ' ' + Customer.LastName as InsuredName, PolicyDetail.PolicyNumber, ";
                query += "VehicleDetail.CoverStartDate, VehicleDetail.CoverEndDate, ClaimRegistration.ClaimNumber, ClaimRegistration.ClaimantName, ClaimStatus.Status, ";
                query += "ClaimRegistration.DateOfLoss, ClaimRegistration.CreatedOn, ClaimRegistration.DescriptionOfLoss, Product.ProductName, ";
                query += "CoverType.Name,VehicleDetail.SumInsured, VehicleMake.MakeDescription + ' ' + VehicleModel.ModelDescription as VehicleDesc, ";
                query += " VehicleDetail.RegistrationNo , ClaimRegistration.EstimatedValueOfLoss from ClaimRegistration ";
                query += " join PolicyDetail on ClaimRegistration.PolicyNumber= PolicyDetail.PolicyNumber ";
                query += " join VehicleDetail on PolicyDetail.Id= VehicleDetail.PolicyId ";
                query += " join Customer on PolicyDetail.CustomerId = Customer.Id ";
                query += " join ClaimNotification on ClaimNotification.id= ClaimRegistration.ClaimNotificationId ";
                query += " join ClaimStatus on ClaimRegistration.ClaimStatus= ClaimStatus.Id ";
                query += " join Product on VehicleDetail.ProductId = Product.Id ";
                query += " join CoverType on VehicleDetail.CoverTypeId = CoverType.Id ";
                query += " join VehicleMake on VehicleMake.MakeCode =VehicleDetail.MakeId ";
                query += " join VehicleModel on VehicleModel.ModelCode = VehicleDetail.ModelId order by ClaimRegistration.Id desc";

                var Claimdetail = InsuranceContext.Query(query)
                .Select(x => new ClaimReportModel
                {
                    Id = x.Id,
                    InsuredName = x.InsuredName,
                    PolicyNumber = x.PolicyNumber,
                    PolicyStartDate = x.CoverStartDate.ToShortDateString(),
                    PolicyEndDate = x.CoverEndDate.ToShortDateString(),
                    ClaimNumber = x.ClaimNumber,
                    ClaimantName = x.ClaimantName,
                    ProductName = x.ProductName,
                    ClaimStatus = x.Status,
                    DateOfLoss = x.DateOfLoss.ToShortDateString(),
                    DateOfNotification = x.CreatedOn.ToShortDateString(),
                    DescripationOfLoss = x.DescriptionOfLoss,
                    CoverType = x.Name,
                    SumInsured = x.SumInsured,
                    VehicleDescription = x.VehicleDesc,
                    VRN = x.RegistrationNo,
                    EstimatedLoss = x.EstimatedValueOfLoss
                }).ToList();


                var distinctClaimList = new List<ClaimReportModel>();


                foreach (var item in Claimdetail)
                {

                    if (distinctClaimList.FirstOrDefault(c => c.Id == item.Id) == null)
                    {
                        distinctClaimList.Add(item);
                    }


                }




                ListClaimReport = distinctClaimList;
                model.ClaimReportModelData = ListClaimReport;
                return View(model);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public ActionResult SearchClaimRegisterReport(ClaimSearchReport model)
        {
            try
            {

                ClaimSearchReport _model = new ClaimSearchReport();
                List<ClaimReportModel> ListClaimReport = new List<ClaimReportModel>();
                DateTime fromDate = DateTime.Now.AddDays(-1);
                DateTime endDate = DateTime.Now;

                if (!string.IsNullOrEmpty(model.FromDate) && !string.IsNullOrEmpty(model.EndDate))
                {
                    fromDate = Convert.ToDateTime(model.FromDate);
                    endDate = Convert.ToDateTime(model.EndDate);


                    ViewBag.fromdate = fromDate.ToString("dd/MM/yyyy");
                    ViewBag.enddate = endDate.ToString("dd/MM/yyyy");

                }



                var query = "select PolicyDetail.Id, Customer.FirstName + ' ' + Customer.LastName as InsuredName, PolicyDetail.PolicyNumber, ";
                query += "VehicleDetail.CoverStartDate, VehicleDetail.CoverEndDate, ClaimRegistration.ClaimNumber, ClaimRegistration.ClaimantName, ClaimStatus.Status, ";
                query += "ClaimRegistration.DateOfLoss, ClaimRegistration.CreatedOn, ClaimRegistration.DescriptionOfLoss, Product.ProductName, ";
                query += "CoverType.Name,VehicleDetail.SumInsured, VehicleMake.MakeDescription + ' ' + VehicleModel.ModelDescription as VehicleDesc, ";
                query += " VehicleDetail.RegistrationNo , ClaimRegistration.EstimatedValueOfLoss from ClaimRegistration ";
                query += " join PolicyDetail on ClaimRegistration.PolicyNumber= PolicyDetail.PolicyNumber ";
                query += " join VehicleDetail on PolicyDetail.Id= VehicleDetail.PolicyId ";
                query += " join Customer on PolicyDetail.CustomerId = Customer.Id ";
                query += " join ClaimNotification on ClaimNotification.id= ClaimRegistration.ClaimNotificationId ";
                query += " join ClaimStatus on ClaimRegistration.ClaimStatus= ClaimStatus.Id ";
                query += " join Product on VehicleDetail.ProductId = Product.Id ";
                query += " join CoverType on VehicleDetail.CoverTypeId = CoverType.Id ";
                query += " join VehicleMake on VehicleMake.MakeCode =VehicleDetail.MakeId ";
                query += " join VehicleModel on VehicleModel.ModelCode = VehicleDetail.ModelId Where ClaimRegistration.CreatedOn>= '" + fromDate + "' And ClaimRegistration.CreatedOn<='" + endDate + "'" + "order by ClaimRegistration.Id desc";


                var Claimdetail = InsuranceContext.Query(query)
                .Select(x => new ClaimReportModel
                {
                    Id = x.Id,
                    InsuredName = x.InsuredName,
                    PolicyNumber = x.PolicyNumber,
                    PolicyStartDate = x.CoverStartDate.ToShortDateString(),
                    PolicyEndDate = x.CoverEndDate.ToShortDateString(),
                    ClaimNumber = x.ClaimNumber,
                    ClaimantName = x.ClaimantName,
                    ProductName = x.ProductName,
                    ClaimStatus = x.Status,
                    DateOfLoss = x.DateOfLoss.ToShortDateString(),
                    DateOfNotification = x.CreatedOn.ToShortDateString(),
                    DescripationOfLoss = x.DescriptionOfLoss,
                    CoverType = x.Name,
                    SumInsured = x.SumInsured,
                    VehicleDescription = x.VehicleDesc,
                    VRN = x.RegistrationNo,
                    EstimatedLoss = x.EstimatedValueOfLoss

                }).Distinct().ToList();


                var distinctClaimList = new List<ClaimReportModel>();


                foreach (var item in Claimdetail)
                {

                    if (distinctClaimList.FirstOrDefault(c => c.Id == item.Id) == null)
                    {
                        distinctClaimList.Add(item);
                    }


                }





                ListClaimReport = distinctClaimList;
                model.ClaimReportModelData = ListClaimReport.OrderBy(x => x.DateOfNotification).ToList();



                return View("ClaimsRegistrationReport", model);

            }
            catch (Exception ex)
            {
                return View("ClaimsRegistrationReport");
            }


        }


        public ActionResult ClaimsNotificationReport()
        {
            try
            {
                ClaimSearchReport model = new ClaimSearchReport();
                List<ClaimReportModel> ListClaimReport = new List<ClaimReportModel>();

                //  Claim Register report
                // var query = "Select a.PolicyNumber as Policyno,a.ClaimNumber as ClaimNo,a.ClaimStatus as Claimstatus,a.DateOfLoss as LossOfDate,
                //b.ClaimantName as INsuredName,a.DateOfNotifications as DateNotifications,a.DescriptionOfLoss as LossofDescriptiom,a.RegistrationNo as registrationno ,
                //a.MakeId,a.ModelId,a.EstimatedValueOfLoss as EstimatedLoss,b.ClaimantName as Name,b.CoverStartDate as PStartDate,b.CoverEndDate as PEndDate,v.SumInsured as suminsured ,
                //c.Name as CoverType,make.MakeDescription as CoverMake,model.ModelDescription as covermodel from ClaimRegistration as a inner join ClaimNotification as b on a.ClaimNotificationId = b.Id 
                //left join VehicleDetail as v on b.VehicleId  = v.Id inner join CoverType as c on v.CoverTypeId = c.Id  inner join VehicleMake as make on a.MakeId = make.MakeCode inner join VehicleModel as 
                //model on a.ModelId = model.ModelCode";


                var query = "select PolicyDetail.Id, Customer.FirstName + ' ' + Customer.LastName as InsuredName, PolicyDetail.PolicyNumber, ";
                query += "VehicleDetail.CoverStartDate, VehicleDetail.CoverEndDate, ClaimRegistration.ClaimNumber, ClaimNotification.ClaimantName, ClaimStatus.Status, ";
                query += "ClaimNotification.DateOfLoss, ClaimNotification.CreatedOn, ClaimNotification.DescriptionOfLoss, Product.ProductName, ";
                query += "CoverType.Name,VehicleDetail.SumInsured, VehicleMake.MakeDescription + ' ' + VehicleModel.ModelDescription as VehicleDesc, ";
                query += " VehicleDetail.RegistrationNo , ClaimNotification.EstimatedValueOfLoss  from ClaimNotification ";
                query += " join PolicyDetail on ClaimNotification.PolicyNumber= PolicyDetail.PolicyNumber ";
                query += " join VehicleDetail on PolicyDetail.Id= VehicleDetail.PolicyId ";
                query += " join Customer on PolicyDetail.CustomerId = Customer.Id ";
                query += " left join ClaimRegistration on ClaimNotification.id= ClaimRegistration.ClaimNotificationId ";
                query += " left join ClaimStatus on ClaimRegistration.ClaimStatus= ClaimStatus.Id ";
                query += " join Product on VehicleDetail.ProductId = Product.Id ";
                query += " join CoverType on VehicleDetail.CoverTypeId = CoverType.Id ";
                query += " join VehicleMake on VehicleMake.MakeCode =VehicleDetail.MakeId  ";
                query += " join VehicleModel on VehicleModel.ModelCode = VehicleDetail.ModelId order  by ClaimNotification.Id desc";

                var Claimdetail = InsuranceContext.Query(query)
                          .Select(x => new ClaimReportModel
                          {
                              Id = x.Id,
                              InsuredName = x.InsuredName,
                              PolicyNumber = x.PolicyNumber,
                              PolicyStartDate = x.CoverStartDate.ToShortDateString(),
                              PolicyEndDate = x.CoverEndDate.ToShortDateString(),
                              ClaimNumber = x.ClaimNumber,
                              ClaimantName = x.ClaimantName,
                              ProductName = x.ProductName,
                              ClaimStatus = x.Status,
                              DateOfLoss = x.DateOfLoss.ToShortDateString(),
                              DateOfNotification = x.CreatedOn.ToShortDateString(),
                              DescripationOfLoss = x.DescriptionOfLoss,
                              CoverType = x.Name,
                              SumInsured = x.SumInsured,
                              VehicleDescription = x.VehicleDesc,
                              VRN = x.RegistrationNo,
                              EstimatedLoss = x.EstimatedValueOfLoss
                          }).ToList();


                var distinctClaimList = new List<ClaimReportModel>();


                foreach (var item in Claimdetail)
                {

                    if (distinctClaimList.FirstOrDefault(c => c.Id == item.Id) == null)
                    {
                        distinctClaimList.Add(item);
                    }


                }




                ListClaimReport = distinctClaimList;
                model.ClaimReportModelData = ListClaimReport;
                return View(model);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public ActionResult SearchClaimReport(ClaimSearchReport model)
        {
            try
            {

                ClaimSearchReport _model = new ClaimSearchReport();
                List<ClaimReportModel> ListClaimReport = new List<ClaimReportModel>();
                DateTime fromDate = DateTime.Now.AddDays(-1);
                DateTime endDate = DateTime.Now;

                if (!string.IsNullOrEmpty(model.FromDate) && !string.IsNullOrEmpty(model.EndDate))
                {
                    fromDate = Convert.ToDateTime(model.FromDate);
                    endDate = Convert.ToDateTime(model.EndDate);


                    ViewBag.fromdate = fromDate.ToString("dd/MM/yyyy");
                    ViewBag.enddate = endDate.ToString("dd/MM/yyyy");

                }



                var query = "select PolicyDetail.Id, Customer.FirstName + ' ' + Customer.LastName as InsuredName, PolicyDetail.PolicyNumber, ";
                query += "VehicleDetail.CoverStartDate, VehicleDetail.CoverEndDate, ClaimRegistration.ClaimNumber, ClaimNotification.ClaimantName, ClaimStatus.Status, ";
                query += "ClaimNotification.DateOfLoss, ClaimNotification.CreatedOn, ClaimNotification.DescriptionOfLoss, Product.ProductName, ";
                query += "CoverType.Name,VehicleDetail.SumInsured, VehicleMake.MakeDescription + ' ' + VehicleModel.ModelDescription as VehicleDesc, ";
                query += " VehicleDetail.RegistrationNo , ClaimNotification.EstimatedValueOfLoss  from ClaimNotification ";
                query += " join PolicyDetail on ClaimNotification.PolicyNumber= PolicyDetail.PolicyNumber ";
                query += " join VehicleDetail on PolicyDetail.Id= VehicleDetail.PolicyId ";
                query += " join Customer on PolicyDetail.CustomerId = Customer.Id ";
                query += " left join ClaimRegistration on ClaimNotification.id= ClaimRegistration.ClaimNotificationId ";
                query += " left join ClaimStatus on ClaimRegistration.ClaimStatus= ClaimStatus.Id ";
                query += " join Product on VehicleDetail.ProductId = Product.Id ";
                query += " join CoverType on VehicleDetail.CoverTypeId = CoverType.Id ";
                query += " join VehicleMake on VehicleMake.MakeCode =VehicleDetail.MakeId  ";
                query += " join VehicleModel on VehicleModel.ModelCode = VehicleDetail.ModelId Where ClaimNotification.CreatedOn>= '" + fromDate + "' And ClaimNotification.CreatedOn<='" + endDate + "'" + "order  by ClaimNotification.Id desc";



                var Claimdetail = InsuranceContext.Query(query)
                           .Select(x => new ClaimReportModel
                           {
                               Id = x.Id,
                               InsuredName = x.InsuredName,
                               PolicyNumber = x.PolicyNumber,
                               PolicyStartDate = x.CoverStartDate.ToShortDateString(),
                               PolicyEndDate = x.CoverEndDate.ToShortDateString(),
                               ClaimNumber = x.ClaimNumber,
                               ClaimantName = x.ClaimantName,
                               ProductName = x.ProductName,
                               ClaimStatus = x.Status,
                               DateOfLoss = x.DateOfLoss.ToShortDateString(),
                               DateOfNotification = x.CreatedOn.ToShortDateString(),
                               DescripationOfLoss = x.DescriptionOfLoss,
                               CoverType = x.Name,
                               SumInsured = x.SumInsured,
                               VehicleDescription = x.VehicleDesc,
                               VRN = x.RegistrationNo,
                               EstimatedLoss = x.EstimatedValueOfLoss

                           }).Distinct().ToList();


                var distinctClaimList = new List<ClaimReportModel>();


                foreach (var item in Claimdetail)
                {

                    if (distinctClaimList.FirstOrDefault(c => c.Id == item.Id) == null)
                    {
                        distinctClaimList.Add(item);
                    }


                }





                ListClaimReport = distinctClaimList;
                model.ClaimReportModelData = ListClaimReport.OrderBy(x => x.DateOfNotification).ToList();



                return View("ClaimsNotificationReport", model);

            }
            catch (Exception ex)
            {
                return View("ClaimsNotificationReport");
            }


        }
    }
}