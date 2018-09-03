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
            _listZTSCLevyreport.ListZTSCreportdata = listZTSCLevyreport.OrderByDescending(x => x.Transaction_date).ToList();
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
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

                obj.Customer_Name = customer.FirstName + " " + customer.LastName;
                obj.Policy_Number = policy.PolicyNumber;
                obj.Premium_due = Convert.ToDecimal(item.Premium);
                obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("dd/MM/yyy");
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

                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
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
                obj.Transaction_date = Convert.ToDateTime(Vehicle.TransactionDate).ToString("dd/MM/yyy");
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
            var vehicledetail = InsuranceContext.VehicleDetails.All(where: "isActive=1").ToList();
            foreach (var item in vehicledetail)
            {
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                GrossWrittenPremiumReportModels obj = new GrossWrittenPremiumReportModels();
                var policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var customer = InsuranceContext.Customers.Single(item.CustomerId);
                var make = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'");
                var model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                var vehicleSUmmarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");
                if (vehicleSUmmarydetail !=null)
                {
                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSUmmarydetail.SummaryDetailId);
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
                }
               

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
                    StartDate = Vehicle.CoverStartDate == null ? null : Vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy"),
                    EndDate = Vehicle.CoverEndDate == null ? null : Vehicle.CoverEndDate.Value.ToString("dd/MM/yyyy"),
                    TransactionDate = Vehicle.TransactionDate == null ? null : Vehicle.TransactionDate.Value.ToString("dd/MM/yyyy"),
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
                //var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                var ReinsuranceTransaction = InsuranceContext.ReinsuranceTransactions.All(where: $"VehicleId={item.Id}").ToList();
                var commision = InsuranceContext.AgentCommissions.Single(item.AgentCommissionId);

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
            return View(ListReinsuranceReport);
        }
        public ActionResult RadioLicenceReport()
        {
            var ListRadioLicenceReport = new List<RadioLicenceReportModel>();

            var VehicleDetails = InsuranceContext.VehicleDetails.All().ToList();

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
            return View(ListRadioLicenceReport);
        }
        public ActionResult CustomerListingReport()
        {
            var ListCustomerListingReport = new List<CustomerListingReportModel>();
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "isActive=1").ToList();

            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                // var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);

                var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");

                if (vehicleSummarydetail != null) {

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
               

            return View(ListCustomerListingReport);
        }
        public ActionResult DailyReceiptsReport()
        {
            var ListDailyReceiptsReport = new List<DailyReceiptsReportModel>();
            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "isActive=1").ToList();

            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                //var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All().ToList();
                //var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId); 

                var vehicleSummarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId='{item.Id}'");

                if (vehicleSummarydetail != null)
                {
                    var summary = InsuranceContext.SummaryDetails.Single(vehicleSummarydetail.SummaryDetailId);

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

            return View(ListDailyReceiptsReport);
        }

        public ActionResult LapsedPoliciesReport()
        {
            var ListLapsedPoliciesReport = new List<LapsedPoliciesReportModels>();

            var VehicleDetails = InsuranceContext.VehicleDetails.All(where: "isLapsed=1").ToList();

            foreach (var item in VehicleDetails)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(item.PolicyId);
                var Customer = InsuranceContext.Customers.Single(item.CustomerId);
                var Vehicle = InsuranceContext.VehicleDetails.Single(item.Id);
                var make = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{item.MakeId}'");
                var model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                var ReinsuranceTransaction = InsuranceContext.ReinsuranceTransactions.All(where: $"VehicleId={Vehicle.Id}").ToList();

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
            return View(ListLapsedPoliciesReport);
        }



    }
}