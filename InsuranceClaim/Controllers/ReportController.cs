using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InsuranceClaim.Models;
using Insurance.Domain;

namespace InsuranceClaim.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        public ActionResult ZTSCLevyReport()
        {
            List<ZTSCLevyReportModels> listZTSCLevyreport = new List<ZTSCLevyReportModels>();
            ListZTSCLevyReportModels _listZTSCLevyreport = new ListZTSCLevyReportModels();
            _listZTSCLevyreport.ListZTSCreportdata = new List<ZTSCLevyReportModels>();

            var vehicledetail = InsuranceContext.VehicleDetails.All().ToList();
            foreach (var item in vehicledetail)
            {
                ZTSCLevyReportModels obj = new ZTSCLevyReportModels();
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var customer = InsuranceContext.Customers.Single(item.CustomerId);

                obj.Customer_Name = customer.FirstName + " " + customer.LastName;
                obj.Policy_Number = policy.PolicyNumber;
                obj.Premium_due = Convert.ToDecimal(item.Premium);
                obj.Transaction_date = Convert.ToDateTime(policy.TransactionDate).ToString("dd/MM/yyy");
                obj.ZTSCLevy = Convert.ToDecimal(item.ZTSCLevy);

                listZTSCLevyreport.Add(obj);
            }
            _listZTSCLevyreport.ListZTSCreportdata = listZTSCLevyreport.OrderBy(x => x.Customer_Name).ToList();
            return View(_listZTSCLevyreport);
        }

        public ActionResult StampDutyReport()
        {
            List<StampDutyReportModels> ListStampDutyReport = new List<StampDutyReportModels>();
            ListStampDutyReportModels _ListStampDutyReport = new ListStampDutyReportModels();
            _ListStampDutyReport.ListStampDutyReportdata = new List<StampDutyReportModels>();
            var vehicledetail = InsuranceContext.VehicleDetails.All().ToList();
            foreach (var item in vehicledetail)
            {
                StampDutyReportModels obj = new StampDutyReportModels();
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var customer = InsuranceContext.Customers.Single(item.CustomerId);

                obj.Customer_Name = customer.FirstName + " " + customer.LastName;
                obj.Policy_Number = policy.PolicyNumber;
                obj.Premium_due = Convert.ToDecimal(item.Premium);
                obj.Transaction_date = Convert.ToDateTime(policy.TransactionDate).ToString("dd/MM/yyy");
                obj.Stamp_duty = Convert.ToDecimal(item.StampDuty);

                ListStampDutyReport.Add(obj);
            }
            _ListStampDutyReport.ListStampDutyReportdata = ListStampDutyReport.OrderBy(x => x.Customer_Name).ToList();
            return View(_ListStampDutyReport);
        }

        public ActionResult VehicleRiskAboutExpire(DateTime? Date)
        {
            List<VehicleRiskAboutExpireModels> ListVehicleRiskAboutExpire = new List<VehicleRiskAboutExpireModels>();
            ListVehicleRiskAboutExpireModels _ListVehicleRiskAboutExpire = new ListVehicleRiskAboutExpireModels();
            _ListVehicleRiskAboutExpire.ListVehicleRiskAboutExpiredata = new List<VehicleRiskAboutExpireModels>();
            List<VehicleDetail> vehicledetail = new List<VehicleDetail>();
            if (Date == null)
                vehicledetail = InsuranceContext.VehicleDetails.All().ToList();
            else
                vehicledetail = InsuranceContext.VehicleDetails.All().Where(p => p.CoverEndDate.Value.ToShortDateString() == (Date == null ? DateTime.Now.ToShortDateString() : Date.Value.ToShortDateString())).ToList();
            foreach (var item in vehicledetail)
            {
                VehicleRiskAboutExpireModels obj = new VehicleRiskAboutExpireModels();
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
                obj.Transaction_date = Convert.ToDateTime(policy.TransactionDate).ToString("dd/MM/yyy");
                obj.Sum_Insured = Convert.ToDecimal(item.SumInsured);
                ListVehicleRiskAboutExpire.Add(obj);
            }
            _ListVehicleRiskAboutExpire.ListVehicleRiskAboutExpiredata = ListVehicleRiskAboutExpire;

            return View(_ListVehicleRiskAboutExpire);
        }

        public ActionResult GrossWrittenPremiumReport()
        {
            List<GrossWrittenPremiumReportModels> ListGrossWrittenPremiumReport = new List<GrossWrittenPremiumReportModels>();
            ListGrossWrittenPremiumReportModels _ListGrossWrittenPremiumReport = new ListGrossWrittenPremiumReportModels();
            _ListGrossWrittenPremiumReport.ListGrossWrittenPremiumReportdata = new List<GrossWrittenPremiumReportModels>();
            var vehicledetail = InsuranceContext.VehicleDetails.All().ToList();
            foreach (var item in vehicledetail)
            {
                GrossWrittenPremiumReportModels obj = new GrossWrittenPremiumReportModels();
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var customer = InsuranceContext.Customers.Single(item.CustomerId);
                var make = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'");
                var model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                var vehicleSUmmarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
                var summary = InsuranceContext.SummaryDetails.Single(vehicleSUmmarydetail.SummaryDetailId);
                obj.Payment_Term = InsuranceContext.PaymentTerms.Single(summary.PaymentTermId).Name;
                obj.Payment_Mode = InsuranceContext.PaymentMethods.Single(summary.PaymentMethodId).Name;
                obj.Customer_Name = customer.FirstName + " " + customer.LastName;
                obj.Policy_Number = policy.PolicyNumber;
                obj.Policy_startdate = Convert.ToDateTime(policy.StartDate).ToString("dd/MM/yyy");
                obj.Policy_endate = Convert.ToDateTime(policy.EndDate).ToString("dd/MM/yyy");
                obj.Vehicle_makeandmodel = make.MakeDescription + "/" + model.ModelDescription;
                obj.Stamp_duty = Convert.ToDecimal(item.StampDuty);
                obj.ZTSC_Levy = Convert.ToDecimal(item.ZTSCLevy);
                obj.Premium_due = Convert.ToDecimal(item.Premium);
                obj.Transaction_date = Convert.ToDateTime(policy.TransactionDate).ToString("dd/MM/yyy");
                obj.Sum_Insured = Convert.ToDecimal(item.SumInsured);
                obj.Annual_Premium = Convert.ToDecimal(item.Premium);
                ListGrossWrittenPremiumReport.Add(obj);
            }
            _ListGrossWrittenPremiumReport.ListGrossWrittenPremiumReportdata = ListGrossWrittenPremiumReport.OrderBy(p => p.Customer_Name).ThenBy(p => p.Payment_Term).ThenBy(p => p.Payment_Mode).ToList();
            return View(_ListGrossWrittenPremiumReport);
        }

        public ActionResult ReinsuranceCommissionReport()
        {
            //ReinsuranceCommissionReportModel ReinsurancReportListmodel = new ReinsuranceCommissionReportModel();
            var ListReinsurancelist = new List<ReinsuranceCommissionReportModel>();

            ReinsuranceCommissionReportModel obj = new ReinsuranceCommissionReportModel();
            var VehicleDetails = InsuranceContext.VehicleDetails.All().ToList();


            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);


                var ReinsuranceTransaction = InsuranceContext.ReinsuranceTransactions.All(where: $"VehicleId={Vehicle.Id}").ToList();

                ListReinsurancelist.Add(new ReinsuranceCommissionReportModel()
                {
                    FirstName = Customer.FirstName,
                    LastName = Customer.LastName,
                    PolicyNumber = Policy.PolicyNumber,
                    StartDate = Policy.StartDate == null ? null : Policy.StartDate.Value.ToString("dd/MM/yyyy"),
                    EndDate = Policy.EndDate == null ? null : Policy.EndDate.Value.ToString("dd/MM/yyyy"),
                    TransactionDate = Policy.TransactionDate == null ? null : Policy.TransactionDate.Value.ToString("dd/MM/yyyy"),
                    AutoFacultativeReinsurance = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 0 ? Convert.ToDecimal(ReinsuranceTransaction[0].ReinsuranceCommission) : 0.00m),
                    FacultativeReinsurance = (ReinsuranceTransaction != null && ReinsuranceTransaction.Count > 1 ? Convert.ToDecimal(ReinsuranceTransaction[1].ReinsuranceCommission) : 0.00m)//FacultativeReinsurance = "";
                });
            }



            return View(ListReinsurancelist.OrderBy(x => x.FirstName).ToList());
        }

        public ActionResult BasicCommissionReport()
        {
            //ReinsuranceCommissionReportModel ReinsurancReportListmodel = new ReinsuranceCommissionReportModel();
            var ListBasicCommissionReport = new List<BasicCommissionReportModel>();

            var VehicleDetails = InsuranceContext.VehicleDetails.All().ToList();

            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                var ReinsuranceTransaction = InsuranceContext.ReinsuranceTransactions.All(where: $"VehicleId={Vehicle.Id}").ToList();

                ListBasicCommissionReport.Add(new BasicCommissionReportModel()
                {
                    FirstName = Customer.FirstName,
                    LastName = Customer.LastName,
                    PolicyNumber = Policy.PolicyNumber,
                    TransactionDate = Policy.TransactionDate == null ? null : Policy.TransactionDate.Value.ToString("dd/mm/yyyy"),
                    SumInsured = Vehicle.SumInsured,
                    Premium = Vehicle.Premium
                });
            }
            return View(ListBasicCommissionReport);
        }

        public ActionResult ReinsuranceReport()
        {
            var ListReinsuranceReport = new List<ReinsuranceReport>();

            var VehicleDetails = InsuranceContext.VehicleDetails.All().ToList();

            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                var ReinsuranceTransaction = InsuranceContext.ReinsuranceTransactions.All(where: $"VehicleId={Vehicle.Id}").ToList();

                ListReinsuranceReport.Add(new ReinsuranceReport()
                {
                    FirstName = Customer.FirstName,
                    LastName = Customer.LastName,
                    PhoneNumber = Customer.PhoneNumber,
                    PolicyNumber = Policy.PolicyNumber,
                    StartDate = Policy.StartDate == null ? null : Policy.StartDate.Value.ToString("dd/mm/yyyy"),
                    EndDate = Policy.EndDate == null ? null : Policy.EndDate.Value.ToString("dd/mm/yyyy"),
                    TransactionDate = Policy.TransactionDate == null ? null : Policy.TransactionDate.Value.ToString("dd/mm/yyyy"),
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
            return View(ListReinsuranceReport);
        }
    }
}