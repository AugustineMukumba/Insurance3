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
    }
}