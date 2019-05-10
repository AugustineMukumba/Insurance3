using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InsuranceClaim.Models;
using Insurance.Domain;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Insurance.Service;

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

            ZTSCLevyReportSeachModels model = new ZTSCLevyReportSeachModels();
            List<ZTSCLevyReportModels> listZTSCLevyreport = new List<ZTSCLevyReportModels>();
            var vehicledetails = InsuranceContext.Query("SELECT VEHICLEDETAIL.TransactionDate,Premium AS PREMIUMDUE,ZTSCLevy, FirstName AS CUSTOMERNAME,PolicyNumber,Name AS CURRENCY FROM VEHICLEDETAIL left JOIN CUSTOMER ON CUSTOMER.ID = VehicleDetail.CUSTOMERID left  JOIN POLICYDETAIL ON POLICYDETAIL.ID = VehicleDetail.POLICYID left JOIN CURRENCY ON CURRENCY.ID = VehicleDetail.CurrencyId WHERE VEHICLEDETAIL.IsActive = 1")
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


            foreach (var item in vehicledetail)
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


            var query = "select  VehicleDetail.RegistrationNo, Customer.FirstName + ' ' + Customer.LastName as FullName, Customer.PhoneNumber , PolicyDetail.PolicyNumber,  VehicleDetail.MakeId, VehicleDetail.ModelId, ";
            query += "  VehicleDetail.CoverStartDate, VehicleDetail.CoverEndDate, VehicleDetail.SumInsured, VehicleDetail.TransactionDate, VehicleDetail.Premium, VehicleDetail.CurrencyId from VehicleDetail ";
            query += " join PolicyDetail on VehicleDetail.PolicyId = PolicyDetail.Id ";
            query += " join Customer on VehicleDetail.CustomerId = Customer.Id ";



            ListVehicleRiskAboutExpire = InsuranceContext.Query(query)
        .Select(x => new VehicleRiskAboutExpireModels()
        {
            Customer_Name = x.FullName,
            Policy_Number = x.PhoneNumber,
            phone_number = x.PolicyNumber,
            Vehicle_makeandmodel = makeList.FirstOrDefault(c => c.MakeCode == x.MakeId).MakeDescription + " " + modelList.FirstOrDefault(c => c.ModelCode == x.ModelId).ModelDescription,
            Vehicle_startdate = x.CoverStartDate.ToShortDateString(),
            Vehicle_enddate = x.CoverEndDate.ToShortDateString(),
            Premium_due = x.Premium,
            Transaction_date = x.TransactionDate.ToShortDateString(),
            Sum_Insured = x.SumInsured == null ? 0 : x.SumInsured,
            Currency = currenyList.FirstOrDefault(c => c.Id == x.CurrencyId) == null ? "USD" : currenyList.FirstOrDefault(c => c.Id == x.CurrencyId).Name,
            RegistrationNumber= x.RegistrationNo
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

        public ActionResult GrossWrittenPremiumReport()
        {
            List<GrossWrittenPremiumReportModels> ListGrossWrittenPremiumReport = new List<GrossWrittenPremiumReportModels>();
            ListGrossWrittenPremiumReportModels _ListGrossWrittenPremiumReport = new ListGrossWrittenPremiumReportModels();
            _ListGrossWrittenPremiumReport.ListGrossWrittenPremiumReportdata = new List<GrossWrittenPremiumReportModels>();


            //var customerList = InsuranceContext.Customers.All().ToList();
            //var makeList = InsuranceContext.VehicleMakes.All().ToList();
            //var modelList = InsuranceContext.VehicleModels.All().ToList();


            GrossWrittenPremiumReportSearchModels Model = new GrossWrittenPremiumReportSearchModels();
            //   var vehicledetail = InsuranceContext.VehicleDetails.All(where: $"IsActive='1'").ToList().Take(200);

            var vehicledetail = InsuranceContext.VehicleDetails.All().OrderByDescending(c=>c.Id).ToList().Take(200);

            var currenyList = _summaryDetailService.GetAllCurrency();


            foreach (var item in vehicledetail)
            {
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                GrossWrittenPremiumReportModels obj = new GrossWrittenPremiumReportModels();
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);

                //var customer = customerList.Single(c=>c.CustomerId==item.CustomerId);
                //var make = makeList.Single(c=>c.MakeCode==item.MakeId);
                //var model = modelList.Single(c => c.ModelCode == item.ModelId);




                var customer = InsuranceContext.Customers.Single(item.CustomerId);
                var make = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'");
                var model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");


                var vehicleSUmmarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
                if (vehicleSUmmarydetail != null)
                {
                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSUmmarydetail.SummaryDetailId);
                    if (summary != null)
                    {
                        if (summary.isQuotation != true)
                        {

                            obj.Payment_Term = InsuranceContext.PaymentTerms.Single(item.PaymentTermId).Name;
                            var paymentMethod = InsuranceContext.PaymentMethods.Single(summary.PaymentMethodId);

                            obj.Payment_Mode = paymentMethod == null ? "" : paymentMethod.Name;

                            if (customer != null)
                                obj.Customer_Name = customer.FirstName + " " + customer.LastName;

                            obj.Policy_Number = policy.PolicyNumber;
                            obj.Policy_startdate = Convert.ToDateTime(item.CoverStartDate).ToString("dd/MM/yyy");
                            obj.Policy_endate = Convert.ToDateTime(item.CoverEndDate).ToString("dd/MM/yyy");

                            //8 Feb
                            obj.PolicyRenewalDate = Convert.ToDateTime(item.RenewalDate);
                            obj.IsActive = item.IsActive;
                            obj.IsLapsed = item.isLapsed;


                            var modelDescription = "";

                            if (model != null && model.ModelDescription != null)
                                modelDescription = model.ModelDescription;


                            obj.Vehicle_makeandmodel = make == null ? "" : make.MakeDescription + "/" + modelDescription;
                            obj.Stamp_duty = Convert.ToDecimal(item.StampDuty);
                            obj.ZTSC_Levy = Convert.ToDecimal(item.ZTSCLevy);
                            obj.Sum_Insured = Convert.ToDecimal(item.SumInsured);
                            obj.Zinara_License_Fee = Vehicle.VehicleLicenceFee;

                            var customerDetails = InsuranceContext.Customers.Single(summary.CreatedBy);

                            // var customerDetails = customerList.Single(c => c.CustomerId == summary.CreatedBy);

                            if (customerDetails != null)
                                obj.PolicyCreatedBy = customerDetails.FirstName + " " + customerDetails.LastName;



                            obj.Comission_percentage = 30;

                            if (Vehicle != null)
                            {
                                obj.Comission_Amount = Convert.ToDecimal(Vehicle.Premium * 30 / 100);
                            }


                            obj.Currency = _summaryDetailService.GetCurrencyName(currenyList, Vehicle.CurrencyId);


                            string converType = "";

                            if (item.CoverTypeId == (int)eCoverType.ThirdParty)
                                converType = eCoverType.ThirdParty.ToString();

                            if (item.CoverTypeId == (int)eCoverType.FullThirdParty)
                                converType = eCoverType.FullThirdParty.ToString();

                            if (item.CoverTypeId == (int)eCoverType.Comprehensive)
                                converType = eCoverType.Comprehensive.ToString();

                            obj.CoverType = converType;

                            obj.Net_Premium = item.Premium;
                            obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("dd/MM/yyy");

                            if (item.PaymentTermId == 1)
                            {
                                obj.Annual_Premium = Convert.ToDecimal(item.Premium);

                                obj.Premium_due = Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.ZTSCLevy) + Convert.ToDecimal(item.RadioLicenseCost);
                            }
                            if (item.PaymentTermId == 3)
                            {
                                obj.Premium_due = Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.ZTSCLevy) + Convert.ToDecimal(item.RadioLicenseCost);
                                obj.Annual_Premium = obj.Premium_due * 4;

                            }
                            if (item.PaymentTermId == 4)
                            {
                                obj.Premium_due = Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.ZTSCLevy) + Convert.ToDecimal(item.RadioLicenseCost);
                                obj.Annual_Premium = obj.Premium_due * 3;

                            }

                            obj.RadioLicenseCost = item.RadioLicenseCost;
                            ListGrossWrittenPremiumReport.Add(obj);

                        }
                    }
                }

            }
            //_ListGrossWrittenPremiumReport.ListGrossWrittenPremiumReportdata = ListGrossWrittenPremiumReport.OrderBy(p => p.Customer_Name).ThenBy(p => p.Payment_Term).ThenBy(p => p.Payment_Mode).ToList();
            Model.ListGrossWrittenPremiumReportdata = ListGrossWrittenPremiumReport.OrderBy(p => p.Customer_Name).ThenBy(p => p.Payment_Term).ThenBy(p => p.Payment_Mode).ToList();
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

            var vehicledetail = InsuranceContext.VehicleDetails.All().OrderByDescending(c=>c.Id).ToList();



            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrEmpty(_model.FormDate) && !string.IsNullOrEmpty(_model.EndDate))
            {
                fromDate = Convert.ToDateTime(_model.FormDate);
                endDate = Convert.ToDateTime(_model.EndDate);
            }


            vehicledetail = vehicledetail.Where(c => Convert.ToDateTime(c.TransactionDate.Value.ToShortDateString()) >= fromDate && Convert.ToDateTime(c.TransactionDate.Value.ToShortDateString()) <= endDate).ToList();




            var currencyList = _summaryDetailService.GetAllCurrency();



            //var customerList = InsuranceContext.Customers.All().ToList();
            //var makeList = InsuranceContext.VehicleMakes.All().ToList();
            //var modelList = InsuranceContext.VehicleModels.All().ToList();



            foreach (var item in vehicledetail)
            {
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                GrossWrittenPremiumReportModels obj = new GrossWrittenPremiumReportModels();
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);

                var customer = InsuranceContext.Customers.Single(item.CustomerId);
                var make = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'");
                var model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");



                var vehicleSUmmarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
                if (vehicleSUmmarydetail != null)
                {
                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSUmmarydetail.SummaryDetailId);
                    if (summary != null)
                    {

                        if (summary.isQuotation != true)
                        {
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

                            if (item.PaymentTermId == 1)
                            {
                                obj.Annual_Premium = Convert.ToDecimal(item.Premium);

                                obj.Premium_due = Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.ZTSCLevy) + Convert.ToDecimal(item.RadioLicenseCost);
                            }
                            if (item.PaymentTermId == 3)
                            {
                                obj.Premium_due = Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.ZTSCLevy) + Convert.ToDecimal(item.RadioLicenseCost);
                                obj.Annual_Premium = obj.Premium_due * 4;

                            }
                            if (item.PaymentTermId == 4)
                            {
                                obj.Premium_due = Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.ZTSCLevy) + Convert.ToDecimal(item.RadioLicenseCost);
                                obj.Annual_Premium = obj.Premium_due * 3;

                            }

                            obj.RadioLicenseCost = item.RadioLicenseCost;
                            ListGrossWrittenPremiumReport.Add(obj);
                        }
                    }
                }

            }
            //_ListGrossWrittenPremiumReport.ListGrossWrittenPremiumReportdata = ListGrossWrittenPremiumReport.OrderBy(p => p.Customer_Name).ThenBy(p => p.Payment_Term).ThenBy(p => p.Payment_Mode).ToList();
            Model.ListGrossWrittenPremiumReportdata = ListGrossWrittenPremiumReport.OrderBy(p => p.Customer_Name).ThenBy(c => c.Policy_Number).ThenBy(c => c.Policy_Number).ThenBy(p => p.Payment_Term).ThenBy(p => p.Payment_Mode).ToList();
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
           List <CustomerListingReportModel> ListCustomerListingReport = new List<CustomerListingReportModel>();
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

                    var _User = userList.FirstOrDefault(c=>c.Id== Customer.UserID.ToString());


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
                        VehicleUsage = InsuranceContext.VehicleUsages.Single(item.VehicleUsage==null? 0 : item.VehicleUsage) == null ? "" : InsuranceContext.VehicleUsages.Single(item.VehicleUsage).VehUsage,
                        PaymentTerm = InsuranceContext.PaymentTerms.Single(item.PaymentTermId) == null ? "" : InsuranceContext.PaymentTerms.Single(item.PaymentTermId).Name,
                        PaymentType = InsuranceContext.PaymentMethods.Single(summary==null?0: summary.PaymentMethodId) ==null ? "Cash" : InsuranceContext.PaymentMethods.Single(summary.PaymentMethodId).Name,



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

            var query1 = "select PolicyDetail.PolicyNumber,createcust.FirstName + '' + createcust.LastName as Created, prcustomer.FirstName + ' ' + prcustomer.LastName as CustomerName, SummaryDetail.CreatedOn as TransactionDate,";
            query1 += "Summarydetail.createdby , SummaryDetail.TotalPremium, PolicyDetail.PolicyNumber as InvoiceNumber, ReceiptModuleHistory.AmountDue,";
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
            _model.LapsedPoliciesReport = ListLapsedPoliciesReport.OrderBy(x => x.customerName).ToList();


            return View("LapsedPoliciesReport", _model);
        }

        public ActionResult ProductivityReport()
        {

            //var ListProductiviyReport = new List<ProductiviyReportModel>();
            List<ProductiviyReportModel> listProductiviyReport = new List<ProductiviyReportModel>();
            ListProductiviyReportModel _listListProductiviyReport = new ListProductiviyReportModel();
            _listListProductiviyReport.ListProductiviyReport = new List<ProductiviyReportModel>();
            ProductiviySearchReportModel model = new ProductiviySearchReportModel();
            var vehicledetail = InsuranceContext.VehicleDetails.All(where: $"IsActive = 'True'or IsActive is null").ToList();

            var currencyList = _summaryDetailService.GetAllCurrency();


            foreach (var item in vehicledetail)
            {
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);


                var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
                if (vehicleSummarydetail != null)
                {
                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);

                    if (summary != null)
                    {
                        var customer = InsuranceContext.Customers.Single(item.CustomerId);
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
                                    obj.TransactionDate = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("dd/MM/yyyy");
                                    obj.PremiumDue = Convert.ToDecimal(item.Premium + item.StampDuty + item.ZTSCLevy + item.RadioLicenseCost);
                                    obj.SumInsured = Convert.ToDecimal(item.SumInsured);
                                    obj.UserName = userDetials.Email;
                                    obj.Product = InsuranceContext.Products.Single(item.ProductId).ProductName;
                                    obj.Currency = _summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId);

                                    //if (summary.isQuotation)
                                    //    obj.PolicyStatus = "Quotation";
                                    //else
                                    //    obj.PolicyStatus = "Policy";



                                    listProductiviyReport.Add(obj);
                                }
                            }
                        }




                    }
                }
            }
            model.ListProductiviyReport = listProductiviyReport.OrderBy(x => x.UserName).ToList();

            return View(model);
        }
        public ActionResult ProductivitySearchReport(ProductiviySearchReportModel Model)
        {

            List<ProductiviyReportModel> listProductiviyReport = new List<ProductiviyReportModel>();
            ListProductiviyReportModel _listListProductiviyReport = new ListProductiviyReportModel();
            _listListProductiviyReport.ListProductiviyReport = new List<ProductiviyReportModel>();
            ProductiviySearchReportModel model = new ProductiviySearchReportModel();

            var vehicledetail = InsuranceContext.VehicleDetails.All(where: $"IsActive = 'True'or IsActive is null").ToList();

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

            #endregion
            foreach (var item in Vehicledetail)
            {
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);


                var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
                if (vehicleSummarydetail != null)
                {
                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);

                    if (summary != null)
                    {
                        var customer = InsuranceContext.Customers.Single(item.CustomerId);
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
                                    //obj.TransactionDate = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("dd/MM/yyyy");
                                    obj.TransactionDate = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("MM/dd/yyyy");
                                    obj.PremiumDue = Convert.ToDecimal(item.Premium + item.StampDuty + item.ZTSCLevy + item.RadioLicenseCost);
                                    obj.SumInsured = Convert.ToDecimal(item.SumInsured);
                                    obj.UserName = userDetials.Email;
                                    obj.Product = InsuranceContext.Products.Single(item.ProductId).ProductName;
                                    obj.Currency = _summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId);

                                    if (summary.isQuotation)
                                        obj.PolicyStatus = "Quotation";
                                    else
                                        obj.PolicyStatus = "Policy";

                                    listProductiviyReport.Add(obj);
                                }
                            }
                        }
                    }
                }
            }
            model.ListProductiviyReport = listProductiviyReport.OrderByDescending(x => x.UserName).ToList();
            return View("ProductivityReport", model);
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
    }
}