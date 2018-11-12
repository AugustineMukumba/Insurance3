using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InsuranceClaim.Models;
using Insurance.Domain;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace InsuranceClaim.Controllers
{
    public class ReportController : Controller
    {

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
            List<ZTSCLevyReportModels> listZTSCLevyreport = new List<ZTSCLevyReportModels>();
            ListZTSCLevyReportModels _listZTSCLevyreport = new ListZTSCLevyReportModels();
            _listZTSCLevyreport.ListZTSCreportdata = new List<ZTSCLevyReportModels>();

            ZTSCLevyReportSeachModels model = new ZTSCLevyReportSeachModels();

            var vehicledetail = InsuranceContext.VehicleDetails.All().ToList();
            foreach (var item in vehicledetail)
            {
                ZTSCLevyReportModels obj = new ZTSCLevyReportModels();
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

                obj.Customer_Name = customer.FirstName + " " + customer.LastName;
                obj.Policy_Number = policy.PolicyNumber;
                obj.Premium_due = Convert.ToDecimal(item.Premium);
                obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("dd/MM/yyy");
                obj.ZTSCLevy = Convert.ToDecimal(item.ZTSCLevy);

                listZTSCLevyreport.Add(obj);
            }
            // _listZTSCLevyreport.ListZTSCreportdata = listZTSCLevyreport.OrderByDescending(x => x.Transaction_date).ToList();


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

            var vehicledetail = InsuranceContext.VehicleDetails.All().ToList();

            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrEmpty(Model.FromDate) && !string.IsNullOrEmpty(Model.EndDate))
            {
                fromDate = Convert.ToDateTime(Model.FromDate);
                endDate = Convert.ToDateTime(Model.EndDate);
            }




            vehicledetail = vehicledetail.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();



            foreach (var item in vehicledetail)
            {
                // if (Model.FromDate ==Convert.ToString(item.TransactionDate) && Model.EndDate > Convert.ToString(item.TransactionDate))

                ZTSCLevyReportModels obj = new ZTSCLevyReportModels();
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

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

            var vehicledetail = InsuranceContext.VehicleDetails.All().ToList();
            foreach (var item in vehicledetail)
            {
                StampDutyReportModels obj = new StampDutyReportModels();
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

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

            var vehicledetail = InsuranceContext.VehicleDetails.All().ToList();


            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrEmpty(Model.FromDate) && !string.IsNullOrEmpty(Model.EndDate))
            {
                fromDate = Convert.ToDateTime(Model.FromDate);
                endDate = Convert.ToDateTime(Model.EndDate);
            }




            vehicledetail = vehicledetail.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();



            foreach (var item in vehicledetail)
            {
                StampDutyReportModels obj = new StampDutyReportModels();
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

                obj.Customer_Name = customer.FirstName + " " + customer.LastName;
                obj.Policy_Number = policy.PolicyNumber;
                obj.Premium_due = Convert.ToDecimal(item.Premium);
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
            VehicleRiskAboutExpireModels obj = new VehicleRiskAboutExpireModels();

            //if (Date == null)
            vehicledetail = InsuranceContext.VehicleDetails.All().ToList();
            //else
            //vehicledetail = InsuranceContext.VehicleDetails.All().Where(p => p.CoverEndDate.Value.ToShortDateString() == (Date == null ? DateTime.Now.ToShortDateString() : Date.Value.ToShortDateString())).ToList();
            foreach (var item in vehicledetail)
            {

                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var customer = InsuranceContext.Customers.Single(item.CustomerId);
                var make = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'");
                var model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                obj.Customer_Name = customer.FirstName + " " + customer.LastName;
                obj.Policy_Number = policy.PolicyNumber;
                obj.phone_number = customer.PhoneNumber;
                obj.Vehicle_makeandmodel = make.MakeDescription + "/" + model.ModelDescription;
                obj.Vehicle_startdate = Convert.ToDateTime(item.CoverStartDate).ToString("dd/MM/yyy");
                obj.Vehicle_enddate = Convert.ToDateTime(item.CoverEndDate).ToString("dd/MM/yyy");
                obj.Premium_due = Convert.ToDecimal(item.Premium);
                obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("dd/MM/yyy");
                obj.Sum_Insured = Convert.ToDecimal(item.SumInsured);
                ListVehicleRiskAboutExpire.Add(obj);
            }
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
            VehicleRiskAboutExpireModels obj = new VehicleRiskAboutExpireModels();

            //if (Date == null)
            vehicledetail = InsuranceContext.VehicleDetails.All().ToList();

            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrEmpty(_Model.FromDate) && !string.IsNullOrEmpty(_Model.EndDate))
            {
                fromDate = Convert.ToDateTime(_Model.FromDate);
                endDate = Convert.ToDateTime(_Model.EndDate);
            }




            vehicledetail = vehicledetail.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();

            //else
            //vehicledetail = InsuranceContext.VehicleDetails.All().Where(p => p.CoverEndDate.Value.ToShortDateString() == (Date == null ? DateTime.Now.ToShortDateString() : Date.Value.ToShortDateString())).ToList();
            foreach (var item in vehicledetail)
            {

                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var customer = InsuranceContext.Customers.Single(item.CustomerId);
                var make = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'");
                var model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                obj.Customer_Name = customer.FirstName + " " + customer.LastName;
                obj.Policy_Number = policy.PolicyNumber;
                obj.phone_number = customer.PhoneNumber;
                obj.Vehicle_makeandmodel = make.MakeDescription + "/" + model.ModelDescription;
                obj.Vehicle_startdate = Convert.ToDateTime(item.CoverStartDate).ToString("dd/MM/yyy");
                obj.Vehicle_enddate = Convert.ToDateTime(item.CoverEndDate).ToString("dd/MM/yyy");
                obj.Premium_due = Convert.ToDecimal(item.Premium);
                obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("dd/MM/yyy");
                obj.Sum_Insured = Convert.ToDecimal(item.SumInsured);
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

            GrossWrittenPremiumReportSearchModels Model = new GrossWrittenPremiumReportSearchModels();
            var vehicledetail = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();
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
                    if (summary.isQuotation != true)
                    {
                        obj.Payment_Term = InsuranceContext.PaymentTerms.Single(item.PaymentTermId).Name;
                        obj.Payment_Mode = InsuranceContext.PaymentMethods.Single(summary.PaymentMethodId).Name;
                        obj.Customer_Name = customer.FirstName + " " + customer.LastName;
                        obj.Policy_Number = policy.PolicyNumber;
                        obj.Policy_startdate = Convert.ToDateTime(item.CoverStartDate).ToString("dd/MM/yyy");
                        obj.Policy_endate = Convert.ToDateTime(item.CoverEndDate).ToString("dd/MM/yyy");
                        obj.Vehicle_makeandmodel = make.MakeDescription + "/" + model.ModelDescription;
                        obj.Stamp_duty = Convert.ToDecimal(item.StampDuty);
                        obj.ZTSC_Levy = Convert.ToDecimal(item.ZTSCLevy);
                        obj.Sum_Insured = Convert.ToDecimal(item.SumInsured);
                        obj.Zinara_License_Fee = Vehicle.VehicleLicenceFee;

                        obj.Comission_percentage = 30;

                        if (Vehicle != null)
                        {
                            obj.Comission_Amount = Convert.ToDecimal(Vehicle.Premium * 30 / 100);
                        }


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

            var vehicledetail = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();


            DateTime fromDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;

            if (!string.IsNullOrEmpty(_model.FormDate) && !string.IsNullOrEmpty(_model.EndDate))
            {
                fromDate = Convert.ToDateTime(_model.FormDate);
                endDate = Convert.ToDateTime(_model.EndDate);
            }


            vehicledetail = vehicledetail.Where(c => Convert.ToDateTime(c.TransactionDate.Value.ToShortDateString()) >= fromDate && Convert.ToDateTime(c.TransactionDate.Value.ToShortDateString()) <= endDate).ToList();


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
                    if (summary.isQuotation != true)
                    {
                        obj.Payment_Term = InsuranceContext.PaymentTerms.Single(item.PaymentTermId).Name;
                        obj.Payment_Mode = InsuranceContext.PaymentMethods.Single(summary.PaymentMethodId).Name;
                        obj.Customer_Name = customer.FirstName + " " + customer.LastName;
                        obj.Policy_Number = policy.PolicyNumber;
                        obj.Policy_startdate = Convert.ToDateTime(item.CoverStartDate).ToString("dd/MM/yyy");
                        obj.Policy_endate = Convert.ToDateTime(item.CoverEndDate).ToString("dd/MM/yyy");
                        obj.Vehicle_makeandmodel = make.MakeDescription + "/" + model.ModelDescription;
                        obj.Stamp_duty = Convert.ToDecimal(item.StampDuty);
                        obj.ZTSC_Levy = Convert.ToDecimal(item.ZTSCLevy);
                        obj.Sum_Insured = Convert.ToDecimal(item.SumInsured);

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
            //_ListGrossWrittenPremiumReport.ListGrossWrittenPremiumReportdata = ListGrossWrittenPremiumReport.OrderBy(p => p.Customer_Name).ThenBy(p => p.Payment_Term).ThenBy(p => p.Payment_Mode).ToList();
            Model.ListGrossWrittenPremiumReportdata = ListGrossWrittenPremiumReport.OrderBy(p => p.Customer_Name).ThenBy(p => p.Payment_Term).ThenBy(p => p.Payment_Mode).ToList();
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
                        FacultativeReinsurance = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 1 ? Convert.ToDecimal(ReinsuranceTransaction[1].ReinsuranceCommission) : 0.00m)//FacultativeReinsurance = "";
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
                        FacultativeReinsurance = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 1 ? Convert.ToDecimal(ReinsuranceTransaction[1].ReinsuranceCommission) : 0.00m)//FacultativeReinsurance = "";
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
            var ListCustomerListingReport = new List<CustomerListingReportModel>();
            ListCustomerListingReport _CustomerListingReport = new ListCustomerListingReport();

            _CustomerListingReport.CustomerListingReport = new List<CustomerListingReportModel>();

            CustomerListingSearchReportModel model = new CustomerListingSearchReportModel();
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();

            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                // var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

                var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");

                if (vehicleSummarydetail != null)
                {

                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);
                    var _User = UserManager.FindById(Customer.UserID.ToString());


                    ListCustomerListingReport.Add(new CustomerListingReportModel()
                    {


                        FirstName = Customer.FirstName,
                        LastName = Customer.LastName,
                        Gender = Customer.Gender,
                        EmailAddress = _User.Email,
                        ContactNumber = Customer.Countrycode + "-" + Customer.PhoneNumber,
                        Dateofbirth = Convert.ToDateTime(Customer.DateOfBirth),
                        NationalIdentificationNumber = Customer.NationalIdentificationNumber,
                        City = Customer.City,
                        Product = InsuranceContext.Products.Single(item.ProductId).ProductName,
                        VehicleMake = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'").MakeDescription,
                        VehicleModel = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ModelDescription,
                        VehicleUsage = InsuranceContext.VehicleUsages.Single(item.VehicleUsage).VehUsage,
                        PaymentTerm = InsuranceContext.PaymentTerms.Single(item.PaymentTermId).Name,
                        PaymentType = InsuranceContext.PaymentMethods.Single(summary.PaymentMethodId).Name,
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



            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                // var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

                var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");

                if (vehicleSummarydetail != null)
                {

                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);
                    var _User = UserManager.FindById(Customer.UserID.ToString());


                    ListCustomerListingReport.Add(new CustomerListingReportModel()
                    {


                        FirstName = Customer.FirstName,
                        LastName = Customer.LastName,
                        Gender = Customer.Gender,
                        EmailAddress = _User.Email,
                        ContactNumber = Customer.Countrycode + "-" + Customer.PhoneNumber,
                        Dateofbirth = Convert.ToDateTime(Customer.DateOfBirth),
                        NationalIdentificationNumber = Customer.NationalIdentificationNumber,
                        City = Customer.City,
                        Product = InsuranceContext.Products.Single(item.ProductId).ProductName,
                        VehicleMake = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'").MakeDescription,
                        VehicleModel = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ModelDescription,
                        VehicleUsage = InsuranceContext.VehicleUsages.Single(item.VehicleUsage).VehUsage,
                        PaymentTerm = InsuranceContext.PaymentTerms.Single(item.PaymentTermId).Name,
                        PaymentType = InsuranceContext.PaymentMethods.Single(summary.PaymentMethodId).Name,
                    });
                }
            }
            model.CustomerListingReport = ListCustomerListingReport.OrderBy(x => x.FirstName).ToList();


            return View("CustomerListingReport", model);
        }
        public ActionResult DailyReceiptsReport()
        {
            var ListDailyReceiptsReport = new List<DailyReceiptsReportModel>();
            ListDailyReceiptsReport _DailyReceiptsReport = new ListDailyReceiptsReport();
            _DailyReceiptsReport.DailyReceiptsReport = new List<DailyReceiptsReportModel>();
            DailyReceiptsSearchReportModel model = new DailyReceiptsSearchReportModel();

            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();

            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
                if (vehicleSummarydetail != null)
                {
                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);
                    if (summary != null)
                    {
                        ListDailyReceiptsReport.Add(new DailyReceiptsReportModel()
                        {

                            FirstName = Customer.FirstName,
                            LastName = Customer.LastName,
                            Contact = Customer.Countrycode + "-" + Customer.PhoneNumber,
                            Product = InsuranceContext.Products.Single(item.ProductId).ProductName,
                            PolicyNumber = Policy.PolicyNumber,
                            TransactionDate = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("dd/MM/yyyy"),
                            PremiumDue = item.Premium + item.StampDuty + item.ZTSCLevy + item.RadioLicenseCost,
                            AmountPaid = Convert.ToString(item.SumInsured),
                            Balance = Convert.ToString(item.BalanceAmount),
                            PaymentType = InsuranceContext.PaymentMethods.Single(summary.PaymentMethodId).Name,
                            ReceiptNumber = summary.DebitNote,
                        });
                    }
                }
            }
            model.DailyReceiptsReport = ListDailyReceiptsReport.OrderBy(x => x.FirstName).ToList();
            return View(model);
        }
        public ActionResult DailyReceiptsSearchReport(DailyReceiptsSearchReportModel Model)
        {
            var ListDailyReceiptsReport = new List<DailyReceiptsReportModel>();
            ListDailyReceiptsReport _DailyReceiptsReport = new ListDailyReceiptsReport();
            _DailyReceiptsReport.DailyReceiptsReport = new List<DailyReceiptsReportModel>();
            DailyReceiptsSearchReportModel model = new DailyReceiptsSearchReportModel();
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();
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


                var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");

                if (vehicleSummarydetail != null)
                {
                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);

                    if (summary != null)
                    {
                        ListDailyReceiptsReport.Add(new DailyReceiptsReportModel()
                        {
                            FirstName = Customer.FirstName,
                            LastName = Customer.LastName,
                            Contact = Customer.Countrycode + "-" + Customer.PhoneNumber,
                            Product = InsuranceContext.Products.Single(item.ProductId).ProductName,
                            PolicyNumber = Policy.PolicyNumber,
                            TransactionDate = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("dd/MM/yyyy"),
                            PremiumDue = item.Premium + item.StampDuty + item.ZTSCLevy + item.RadioLicenseCost,
                            AmountPaid = Convert.ToString(item.SumInsured),
                            Balance = Convert.ToString(item.BalanceAmount),
                            PaymentType = InsuranceContext.PaymentMethods.Single(summary.PaymentMethodId).Name,
                            ReceiptNumber = summary.DebitNote,
                        });
                    }
                }
            }
            model.DailyReceiptsReport = ListDailyReceiptsReport.OrderBy(x => x.FirstName).ToList();
            return View("DailyReceiptsReport", model);
        }
        public ActionResult LapsedPoliciesReport()
        {
            var ListLapsedPoliciesReport = new List<LapsedPoliciesReportModels>();
            ListLapsedPoliciesReport _LapsedPoliciesReport = new ListLapsedPoliciesReport();
            _LapsedPoliciesReport.LapsedPoliciesReport = new List<LapsedPoliciesReportModels>();
            LapsedPoliciesSearchReportModels _model = new LapsedPoliciesSearchReportModels();
            var whereClause = "isLapsed = 'True' or " + $"CAST(RenewalDate as date) < '{DateTime.Now.ToString("yyyy-MM-dd")}'";
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: whereClause).ToList();
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

            foreach (var item in vehicledetail)
            {
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var customer = InsuranceContext.Customers.Single(item.CustomerId);
                var User = UserManager.FindById(customer.UserID.ToString());
                var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
                if (vehicleSummarydetail != null)
                {
                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);
                    if (summary != null)
                    {
                        ProductiviyReportModel obj = new ProductiviyReportModel();
                        obj.CustomerName = customer.FirstName + " " + customer.LastName;
                        obj.PolicyNumber = policy.PolicyNumber;
                        obj.TransactionDate = item.TransactionDate == null ? null : item.TransactionDate.Value.ToString("dd/MM/yyyy");
                        obj.PremiumDue = Convert.ToDecimal(item.Premium + item.StampDuty + item.ZTSCLevy + item.RadioLicenseCost);
                        obj.SumInsured = Convert.ToDecimal(item.SumInsured);
                        obj.UserName = User.Email;
                        obj.Product = InsuranceContext.Products.Single(item.ProductId).ProductName;
                        listProductiviyReport.Add(obj);
                    }
                }
            }
            model.ListProductiviyReport = listProductiviyReport.OrderByDescending(x => x.TransactionDate).ToList();

            return View(model);
        }


        public ActionResult LoyaltyPointsReport()
        {
            var ListDailyReceiptsReport = new List<LoyaltyPointsModel>();
            LoyaltyPointsReport _LoyaltyPt = new LoyaltyPointsReport();
            _LoyaltyPt.LoyaltyPoints = new List<LoyaltyPointsModel>();
            LoyaltyPointsReportSeachModels model = new LoyaltyPointsReportSeachModels();

            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();

            if (VehicleDetails != null)
            {
                foreach (var item in VehicleDetails)
                {
                    var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                    var User = UserManager.FindById(Customer.UserID.ToString());
                    //var loyalityDetail = InsuranceContext.LoyaltyDetails.All(where: $"CustomerId={item.CustomerId}").Sum(x => x.PointsEarned);

                    var loyalityDetail = InsuranceContext.LoyaltyDetails.All(where: $"CustomerId={item.CustomerId}").Sum(x => x.PointsEarned);

                    if (loyalityDetail != null)
                    {
                        ListDailyReceiptsReport.Add(new LoyaltyPointsModel()
                        {
                            CustomerName = Customer.FirstName + " " + Customer.LastName,
                            CellPhoneNumber = Customer.Countrycode + "-" + Customer.PhoneNumber,
                            Address = Customer.AddressLine1 + " " + Customer.AddressLine2,
                            SumInsured = Convert.ToDecimal(item.SumInsured),
                            PremiumPaid = Convert.ToDecimal(item.Premium),
                            EmailAddress = User.Email,
                            LoyaltyPoints = Convert.ToString(loyalityDetail),

                        });
                    }
                    else
                    {

                    }
                }
            }
            model.LoyaltyPoints = ListDailyReceiptsReport.OrderBy(x => x.CustomerName).ToList();
            return View(model);
        }

        //public ActionResult LoyaltyPointsSearchReport(LoyaltyPointsReportSeachModels Model)
        //{
        //    var ListDailyReceiptsReport = new List<LoyaltyPointsModel>();
        //    LoyaltyPointsReport _LoyaltyPointsReport = new LoyaltyPointsReport();
        //    _LoyaltyPointsReport.LoyaltyPoints = new List<LoyaltyPointsModel>();
        //    LoyaltyPointsReportSeachModels _model = new LoyaltyPointsReportSeachModels();
        //    var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "IsActive ='True'").ToList();
        //    #region
        //    DateTime fromDate = DateTime.Now.AddDays(-1);
        //    DateTime endDate = DateTime.Now;
        //    if (!string.IsNullOrEmpty(Model.FromDate) && !string.IsNullOrEmpty(Model.EndDate))
        //    {
        //        fromDate = Convert.ToDateTime(Model.FromDate);
        //        endDate = Convert.ToDateTime(Model.EndDate);
        //    }
        //    VehicleDetails = VehicleDetails.Where(c => c.TransactionDate >= fromDate && c.TransactionDate <= endDate).ToList();
        //    #endregion
        //    if (VehicleDetails != null)
        //    {
        //        foreach (var item in VehicleDetails)
        //        {
        //            var Customer = InsuranceContext.Customers.Single(item.CustomerId);
        //            var User = UserManager.FindById(Customer.UserID.ToString());
        //            var loyalityDetail = InsuranceContext.LoyaltyDetails.All(where: $"CustomerId={item.CustomerId}").Sum(x => x.PointsEarned);
        //            if (loyalityDetail != null)
        //            {
        //                ListDailyReceiptsReport.Add(new LoyaltyPointsModel()
        //                {
        //                    CustomerName = Customer.FirstName + " " + Customer.LastName,
        //                    CellPhoneNumber = Customer.Countrycode + "-" + Customer.PhoneNumber,
        //                    Address = Customer.AddressLine1 + " " + Customer.AddressLine2,
        //                    SumInsured = Convert.ToDecimal(item.SumInsured),
        //                    PremiumPaid = Convert.ToDecimal(item.Premium),
        //                    EmailAddress = User.Email,
        //                    LoyaltyPoints = Convert.ToString(loyalityDetail),
        //                });
        //            }
        //            else
        //            {
        //            }
        //        }
        //    }

        //    return View("LoyaltyPointsReport", _model);
        //}
    }
}