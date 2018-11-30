using AutoMapper;
using Insurance.Domain;
using Insurance.Service;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static InsuranceClaim.Controllers.CustomerRegistrationController;

namespace InsuranceClaim.Controllers
{
    public class EndorsementController : Controller
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

        // GET: Endorsement
        public ActionResult EndorsementDetials(int sumaryid = 0)
        {
            EndorsementCustomerModel endorcustom = new EndorsementCustomerModel();
            if (sumaryid != 0)
            {
                //var endorseSummaryDetail = InsuranceContext.EndorsementSummaryDetails.All(where: $"SummaryId={sumaryid}").FirstOrDefault();

                var summaryDetail = InsuranceContext.SummaryDetails.Single(sumaryid);
                var Cusotmer = InsuranceContext.Customers.Single(summaryDetail.CustomerId);
                endorcustom = Mapper.Map<Customer, EndorsementCustomerModel>(Cusotmer);

                if (Cusotmer != null)
                {
                    var dbUser = UserManager.Users.FirstOrDefault(c => c.Id == Cusotmer.UserID);
                    if (dbUser != null)
                    {
                        endorcustom.EmailAddress = dbUser.Email; ;
                        endorcustom.PrimeryCustomerId = Cusotmer.Id;
                        endorcustom.SummaryId = sumaryid;

                    }
                }
                Session["SummaryDetailIdView"] = sumaryid;
                string path = Server.MapPath("~/Content/Countries.txt");
                var countries = System.IO.File.ReadAllText(path);
                var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(countries);
                ViewBag.Countries = resultt.countries.OrderBy(x => x.code.Replace("+", ""));
                ViewBag.Cities = InsuranceContext.Cities.All();
            }

            return View(endorcustom);
        }
        public async Task<JsonResult> UpdateCustomerData(EndorsementCustomerModel model, string buttonUpdate)
        {
            if (ModelState.IsValid)
            {
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                if (userLoggedin)
                {
                    var summaryDetails = InsuranceContext.SummaryDetails.Single(model.SummaryId);

                    if (summaryDetails != null)
                    {
                        if (summaryDetails.CustomerId != null)
                        {
                            //var customerDetails = InsuranceContext.EndorsementCustomers.Single(where: $"CustomerId= '{model.CustomerId}'");
                            //if (customerDetails== null)
                            //{
                            var customerdata = Mapper.Map<EndorsementCustomerModel, EndorsementCustomer>(model);

                            customerdata.CustomerId = summaryDetails.CustomerId.Value;
                            customerdata.PrimeryCustomerId = model.PrimeryCustomerId;
                            customerdata.UserID = model.UserID;
                            customerdata.ZipCode = model.ZipCode;
                            customerdata.CreatedOn = DateTime.Now;
                            customerdata.IsCompleted = false;
                            InsuranceContext.EndorsementCustomers.Insert(customerdata);
                            var user = UserManager.FindById(model.UserID);
                            Session["CustomerDetail"] = customerdata.Id;
                        }
                    }

                    return Json(new { IsError = false, error = "Sucessfully update" }, JsonRequestBehavior.AllowGet);

                }
            }
            return Json(new { IsError = false, error = TempData["ErrorMessage"].ToString() }, JsonRequestBehavior.AllowGet);
        }




        public ActionResult EndorsementInsertRiskDetails(int? id = 1)
        {
            var viewModel = new EndorsementRiskDetailModel();

            if (Session["SummaryDetailIdView"] != null)
            {

                InsertEndersoment(Convert.ToInt32(Session["SummaryDetailIdView"]));

                var Endorsesummartid = (EndorsementSummaryDetail)Session["EnsummaryId"];


                SetEndorsementValueIntoSession(Endorsesummartid.Id);

            }

            return RedirectToAction("EndorsementRiskDetails");
        }

        public void InsertEndersoment(int summaryId)
        {

            var summaryDetail = InsuranceContext.SummaryDetails.Single(summaryId);
            var paymeninfo = InsuranceContext.PaymentInformations.Single(where: $"SummaryDetailId = '{summaryId}'");
            var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summaryId}").ToList();
            var Endorsmentcutom = Convert.ToInt32(Session["CustomerDetail"]);

            int vehicalId = 0;
            if (SummaryVehicleDetails.Count > 0)
            {
                vehicalId = SummaryVehicleDetails[0].VehicleDetailsId;
            }

            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            var _customerData = new Customer();

            ///Insertt into PolicyDetail//
            if (paymeninfo != null)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(where: $"Id= '{paymeninfo.PolicyId}'");
                if (Policy != null)
                {
                    //EndorsementPolicyDetailModel obj = new EndorsementPolicyDetailModel();
                  //  EndorsementPolicyDetailModel obj = new EndorsementPolicyDetailModel();
                    var obj = Mapper.Map<PolicyDetail, EndorsementPolicyDetailModel>(Policy);
                    var data = Mapper.Map<EndorsementPolicyDetailModel, EndorsementPolicyDetail>(obj);

                    data.EndorsementCustomerId = Endorsmentcutom;
                    data.CustomerId = obj.CustomerId;
                    data.PrimaryPolicyId = obj.Id;
                    InsuranceContext.EndorsementPolicyDetails.Insert(data);
                    Session["PolicyId"] = data;
                }
            }


            ///Insert in EndorsementSummaryDetail////
            var _Endorsmentpolicy = (EndorsementPolicyDetail)Session["PolicyId"];
            //var dbModel = Mapper.Map<SummaryDetail, EndorsementSummaryDetail>(summaryDetail);
            EndorsementSummaryDetail endorsementSummaryDetail = new EndorsementSummaryDetail();
            endorsementSummaryDetail.PrimarySummaryId = summaryDetail.Id;
            endorsementSummaryDetail.EndorsementPolicyId = _Endorsmentpolicy.Id;
            endorsementSummaryDetail.VehicleDetailId = summaryDetail.VehicleDetailId;
            endorsementSummaryDetail.EndorsementCustomerId = Endorsmentcutom;
            //
            endorsementSummaryDetail.CustomerId = summaryDetail.CustomerId;
            endorsementSummaryDetail.PaymentTermId = summaryDetail.PaymentTermId;
            endorsementSummaryDetail.PaymentMethodId = summaryDetail.PaymentMethodId;
            endorsementSummaryDetail.TotalSumInsured = summaryDetail.TotalSumInsured;
            endorsementSummaryDetail.TotalPremium = summaryDetail.TotalPremium;
            endorsementSummaryDetail.TotalStampDuty = summaryDetail.TotalStampDuty;
            endorsementSummaryDetail.TotalZTSCLevies = summaryDetail.TotalZTSCLevies;
            endorsementSummaryDetail.TotalRadioLicenseCost = summaryDetail.TotalRadioLicenseCost;
            endorsementSummaryDetail.AmountPaid = summaryDetail.AmountPaid;
            endorsementSummaryDetail.DebitNote = summaryDetail.DebitNote;
            endorsementSummaryDetail.ReceiptNumber = summaryDetail.ReceiptNumber;
            endorsementSummaryDetail.SMSConfirmation = summaryDetail.SMSConfirmation;
            endorsementSummaryDetail.CreatedOn = summaryDetail.CreatedOn;
            if (_userLoggedin)
            {
                var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();
                endorsementSummaryDetail.CreatedBy = _customerData.Id;
            }
            endorsementSummaryDetail.ModifiedOn = summaryDetail.ModifiedOn;
            endorsementSummaryDetail.ModifiedBy = summaryDetail.ModifiedBy;
            endorsementSummaryDetail.IsActive = summaryDetail.IsActive;
            endorsementSummaryDetail.BalancePaidDate = summaryDetail.BalancePaidDate;

            endorsementSummaryDetail.Notes = summaryDetail.Notes;
            endorsementSummaryDetail.isQuotation = summaryDetail.isQuotation;
            endorsementSummaryDetail.IsCompleted = false;
            InsuranceContext.EndorsementSummaryDetails.Insert(endorsementSummaryDetail);
            Session["EnsummaryId"] = endorsementSummaryDetail;


            /// Insert into EndorsementVehicleDetail///
            List<EndorsementVehicleDetail> listriskdetailmodel = new List<EndorsementVehicleDetail>();
            foreach (var item in SummaryVehicleDetails)
            {
                var vehicle = InsuranceContext.VehicleDetails.Single(where: $"Id={item.VehicleDetailsId}");
                var Endorsmentpolicy = (EndorsementPolicyDetail)Session["PolicyId"];
                var vehicleInsert = new EndorsementVehicleDetail();

                vehicleInsert.NoOfCarsCovered = vehicle.NoOfCarsCovered;
                vehicleInsert.PolicyId = vehicle.PolicyId;
                vehicleInsert.RegistrationNo = vehicle.RegistrationNo;
                //Endor//
                vehicleInsert.PrimaryVehicleId = vehicle.Id;
                vehicleInsert.EndorsementPolicyId = Endorsmentpolicy.Id;
                vehicleInsert.EndorsementCustomerId = Endorsmentcutom;
                //
                vehicleInsert.MakeId = vehicle.MakeId;
                vehicleInsert.ModelId = vehicle.ModelId;
                vehicleInsert.ModelId = vehicle.ModelId;
                vehicleInsert.CubicCapacity = vehicle.CubicCapacity;
                vehicleInsert.VehicleYear = vehicle.VehicleYear;
                vehicleInsert.EngineNumber = vehicle.EngineNumber;
                vehicleInsert.ChasisNumber = vehicle.ChasisNumber;
                vehicleInsert.VehicleColor = vehicle.VehicleColor;
                vehicleInsert.VehicleUsage = vehicle.VehicleUsage;
                vehicleInsert.CoverTypeId = vehicle.CoverTypeId;
                vehicleInsert.CoverTypeId = vehicle.CoverTypeId;
                vehicleInsert.CoverStartDate = vehicle.CoverStartDate;
                vehicleInsert.CoverEndDate = vehicle.CoverEndDate;
                vehicleInsert.SumInsured = vehicle.SumInsured;
                vehicleInsert.Premium = vehicle.Premium;
                vehicleInsert.AgentCommissionId = vehicle.AgentCommissionId;
                vehicleInsert.Rate = vehicle.Rate;
                vehicleInsert.StampDuty = vehicle.StampDuty;
                vehicleInsert.ZTSCLevy = vehicle.ZTSCLevy;
                vehicleInsert.RadioLicenseCost = vehicle.RadioLicenseCost;
                vehicleInsert.OptionalCovers = vehicle.OptionalCovers;
                vehicleInsert.Excess = vehicle.Excess;
                vehicleInsert.CoverNoteNo = vehicle.CoverNoteNo;
                vehicleInsert.ExcessType = vehicle.ExcessType;
                vehicleInsert.ExcessType = vehicle.ExcessType;
                vehicleInsert.CreatedOn = DateTime.Now;
                if (_userLoggedin)
                {
                    var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                    _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();
                    vehicleInsert.CreatedBy = _customerData.Id;
                }

                vehicleInsert.IsActive = vehicle.IsActive;
                vehicleInsert.Addthirdparty = vehicle.Addthirdparty;
                vehicleInsert.Addthirdparty = vehicle.Addthirdparty;
                vehicleInsert.AddThirdPartyAmount = vehicle.AddThirdPartyAmount;

                vehicleInsert.PassengerAccidentCoverAmount = vehicle.PassengerAccidentCoverAmount == null ? 0 : vehicle.PassengerAccidentCoverAmount;
                vehicleInsert.ExcessBuyBackAmount = vehicle.ExcessBuyBackAmount == null ? 0 : vehicle.ExcessBuyBackAmount;

                vehicleInsert.NumberofPersons = vehicle.NumberofPersons;
                vehicleInsert.IsLicenseDiskNeeded = vehicle.IsLicenseDiskNeeded;


                vehicleInsert.PassengerAccidentCoverAmountPerPerson = vehicle.PassengerAccidentCoverAmountPerPerson == null ? 0 : vehicle.PassengerAccidentCoverAmountPerPerson;
                vehicleInsert.ExcessBuyBackPercentage = vehicle.ExcessBuyBackPercentage == null ? 0 : vehicle.ExcessBuyBackPercentage;

                vehicleInsert.PaymentTermId = vehicle.PaymentTermId;
                vehicleInsert.ProductId = vehicle.ProductId;
                vehicleInsert.RoadsideAssistanceAmount = vehicle.RoadsideAssistanceAmount == null ? 0 : vehicle.RoadsideAssistanceAmount;
                vehicleInsert.MedicalExpensesAmount = vehicle.MedicalExpensesAmount == null ? 0 : vehicle.MedicalExpensesAmount;

                vehicleInsert.ExcessAmount = vehicle.ExcessAmount;

                vehicleInsert.TransactionDate = DateTime.Now;
                vehicleInsert.RenewalDate = vehicle.RenewalDate;
                vehicleInsert.IncludeRadioLicenseCost = vehicle.IncludeRadioLicenseCost;
                vehicleInsert.InsuranceId = vehicle.InsuranceId;
                vehicleInsert.InsuranceId = vehicle.InsuranceId;
                vehicleInsert.AnnualRiskPremium = vehicle.AnnualRiskPremium == null ? 0 : vehicle.AnnualRiskPremium;
                vehicleInsert.TermlyRiskPremium = vehicle.TermlyRiskPremium == null ? 0 : vehicle.TermlyRiskPremium;
                vehicleInsert.QuaterlyRiskPremium = vehicle.QuaterlyRiskPremium == null ? 0 : vehicle.QuaterlyRiskPremium;
                vehicleInsert.Discount = vehicle.Discount;

                vehicleInsert.isLapsed = vehicle.isLapsed;
                vehicleInsert.BalanceAmount = vehicle.BalanceAmount;
                vehicleInsert.VehicleLicenceFee = vehicle.VehicleLicenceFee;
                vehicleInsert.BusinessSourceId = vehicle.BusinessSourceDetailId;
                vehicleInsert.CurrencyId = vehicle.CurrencyId;

                vehicleInsert.RoadsideAssistancePercentage = vehicle.RoadsideAssistancePercentage == null ? 0 : vehicle.RoadsideAssistancePercentage;
                vehicleInsert.MedicalExpensesPercentage = vehicle.MedicalExpensesPercentage == null ? 0 : vehicle.MedicalExpensesPercentage;
                vehicleInsert.IsCompleted = false;
                InsuranceContext.EndorsementVehicleDetails.Insert(vehicleInsert);
                //var vehicleId = vehicleInsert;
                //Session["vehicleId"] = vehicleInsert;
                listriskdetailmodel.Add(vehicleInsert);
                Session["vehicleId"] = listriskdetailmodel;
            }

            //// insert into EndorsementSummaryVehicleDetails///
            var Endorsmentvehicle = (List<EndorsementVehicleDetail>)Session["vehicleId"];
            if (Endorsmentvehicle != null)
            {
                foreach (var item in Endorsmentvehicle)
                {

                    EndorsementSummaryVehicleDetail summaryVehicalDetials = new EndorsementSummaryVehicleDetail();
                    var endorsesummayid = (EndorsementSummaryDetail)Session["EnsummaryId"];
                    summaryVehicalDetials.SummaryDetailId = summaryId;
                    summaryVehicalDetials.VehicleDetailsId = Convert.ToInt32(item.PrimaryVehicleId);
                    summaryVehicalDetials.CreatedOn = DateTime.Now;
                    summaryVehicalDetials.CreatedBy = _customerData.Id;
                    summaryVehicalDetials.ModifiedOn = DateTime.Now;
                    summaryVehicalDetials.ModifiedBy = _customerData.Id;
                    summaryVehicalDetials.IsCompleted = false;
                    //endorse//
                    summaryVehicalDetials.EndorsementVehicleId = item.Id;
                    summaryVehicalDetials.EndorsementSummaryId = endorsesummayid.Id;
                    ////
                    InsuranceContext.EndorsementSummaryVehicleDetails.Insert(summaryVehicalDetials);
                }
            }

        }

        public void SetEndorsementValueIntoSession(int Endorsementsummaryid)
        {
            //Session["SummaryDetailIdView"] = Endorsementsummaryid;

            var endorsesummaryDetail = InsuranceContext.EndorsementSummaryDetails.Single(where: $"Id={Endorsementsummaryid}");
            var endorseSummaryVehicleDetails = InsuranceContext.EndorsementSummaryVehicleDetails.All(where: $"EndorsementSummaryId={Endorsementsummaryid}").ToList();


            var endorsevehicle = InsuranceContext.EndorsementVehicleDetails.All(where: $"Id={endorseSummaryVehicleDetails[0].EndorsementVehicleId}").FirstOrDefault();
            var endorsepolicy = InsuranceContext.EndorsementPolicyDetails.Single(endorsevehicle.EndorsementPolicyId);
            //var endorseproduct = InsuranceContext.Products.Single(Convert.ToInt32(endorsepolicy.PolicyName));
            Session["PolicyDataView"] = endorsepolicy;

            List<EndorsementRiskDetailModel> listRiskDetail = new List<EndorsementRiskDetailModel>();
            foreach (var item in endorseSummaryVehicleDetails)
            {
                var _vehicle = InsuranceContext.EndorsementVehicleDetails.Single(where: $"Id={item.EndorsementVehicleId}");
                EndorsementRiskDetailModel riskDetail = Mapper.Map<EndorsementVehicleDetail, EndorsementRiskDetailModel>(_vehicle);
                riskDetail.PrimaryVehicleId = Convert.ToInt32(_vehicle.PrimaryVehicleId);
                listRiskDetail.Add(riskDetail);
            }
            Session["ViewlistVehicles"] = listRiskDetail;

            EndorsementSummaryDetailModel summarymodel = Mapper.Map<EndorsementSummaryDetail, EndorsementSummaryDetailModel>(endorsesummaryDetail);
            summarymodel.Id = endorsesummaryDetail.Id;
            Session["ENViewSummaryDetail"] = endorsesummaryDetail;


        }
        public ActionResult EndorsementRiskDetails(int? id = 1)
        {
            var viewModel = new EndorsementRiskDetailModel();
            var ensummertdetail = (EndorsementSummaryDetail)Session["EnsummaryId"];
            viewModel.SummaryId = ensummertdetail.Id;
            var eExcessTypeData = from eExcessType e in Enum.GetValues(typeof(eExcessType))
                                  select new
                                  {
                                      ID = (int)e,
                                      Name = e.ToString()
                                  };

            ViewBag.eExcessTypeData = new SelectList(eExcessTypeData, "ID", "Name");

            ViewBag.Products = InsuranceContext.Products.All().ToList();
            var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm))
                                   select new
                                   {
                                       ID = (int)e,
                                       Name = e.ToString()
                                   };

            ViewBag.ePaymentTermData = new SelectList(ePaymentTermData, "ID", "Name");
            int RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
            var PolicyData = (EndorsementPolicyDetail)Session["PolicyId"];
            // Id is policyid from Policy detail table

            var service = new VehicleService();

            ViewBag.VehicleUsage = service.GetAllVehicleUsage();
            viewModel.NumberofPersons = 0;
            viewModel.AddThirdPartyAmount = 0.00m;
            viewModel.RadioLicenseCost = Convert.ToDecimal(RadioLicenseCosts);
            var makers = service.GetMakers();
            ViewBag.CoverType = service.GetCoverType();
            ViewBag.AgentCommission = service.GetAgentCommission();
            ViewBag.Sources = InsuranceContext.BusinessSources.All();
            ViewBag.Makers = makers;
            viewModel.isUpdate = false;
            viewModel.VehicleUsage = 0;
            var _list = "";
            //  TempData["Policy"] = service.GetPolicy(id);
            TempData["Policy"] = PolicyData;
            if (makers.Count > 0)
            {
                var model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = model;
            }

            viewModel.NoOfCarsCovered = 1;

            if (Session["ViewlistVehicles"] != null)
            {
                var list = (List<EndorsementRiskDetailModel>)Session["ViewlistVehicles"];
                if (list.Count > 0)
                {
                    viewModel.NoOfCarsCovered = list.Count + 1;
                }
            }
            if (id > 0)
            {
                var list = (List<EndorsementRiskDetailModel>)Session["ViewlistVehicles"];
                if (list != null && list.Count > 0 && (list.Count >= id))
                {
                    var data = (EndorsementRiskDetailModel)list[Convert.ToInt32(id - 1)];
                    if (data != null)
                    {
                        viewModel.AgentCommissionId = data.AgentCommissionId;
                        viewModel.ChasisNumber = data.ChasisNumber;
                        viewModel.CoverEndDate = data.CoverEndDate;
                        viewModel.CoverNoteNo = data.CoverNoteNo;
                        viewModel.CoverStartDate = data.CoverStartDate;
                        viewModel.CoverTypeId = data.CoverTypeId;
                        viewModel.CubicCapacity = data.CubicCapacity == null ? 0 : (int)Math.Round(data.CubicCapacity.Value, 0);
                        viewModel.CustomerId = data.CustomerId;
                        viewModel.EngineNumber = data.EngineNumber;
                        // viewModel.Equals = data.Equals;
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
                        viewModel.StampDuty = data.StampDuty;
                        viewModel.SumInsured = (int)Math.Round(data.SumInsured == null ? 0 : data.SumInsured.Value, 0);
                        viewModel.VehicleColor = data.VehicleColor;
                        viewModel.VehicleUsage = data.VehicleUsage;
                        viewModel.VehicleYear = data.VehicleYear;
                        viewModel.Id = data.Id;
                        viewModel.ZTSCLevy = data.ZTSCLevy;
                        viewModel.NumberofPersons = data.NumberofPersons;
                        viewModel.PassengerAccidentCover = data.PassengerAccidentCover;
                        viewModel.IsLicenseDiskNeeded = data.IsLicenseDiskNeeded;
                        viewModel.ExcessBuyBack = data.ExcessBuyBack;
                        viewModel.RoadsideAssistance = data.RoadsideAssistance;
                        viewModel.MedicalExpenses = data.MedicalExpenses;
                        viewModel.Addthirdparty = data.Addthirdparty;
                        viewModel.AddThirdPartyAmount = data.AddThirdPartyAmount;
                        viewModel.ExcessAmount = data.ExcessAmount;
                        viewModel.ProductId = data.ProductId;
                        viewModel.PaymentTermId = data.PaymentTermId;
                        viewModel.IncludeRadioLicenseCost = data.IncludeRadioLicenseCost;
                        viewModel.Discount = data.Discount;
                        viewModel.VehicleLicenceFee = Convert.ToDecimal(data.VehicleLicenceFee);
                        viewModel.BusinessSourceId = data.BusinessSourceId;
                        viewModel.VehicleUsage = data.VehicleUsage;
                        viewModel.isUpdate = true;
                        viewModel.vehicleindex = Convert.ToInt32(id);
                        viewModel.VehicleId = data.VehicleId;
                        viewModel.EndorsementCustomerId = data.EndorsementCustomerId;
                        viewModel.EndorsementPolicyId = data.EndorsementPolicyId;
                        viewModel.PrimaryVehicleId = data.PrimaryVehicleId;

                        viewModel.Id = data.Id;
                        var ser = new VehicleService();
                        var model = ser.GetModel(data.MakeId);
                        ViewBag.Model = model;
                    }
                }
            }
            return View(viewModel);
        }

        [HttpGet]
        public JsonResult GetEndorsementLicenseAddress()
        {
            var customerData = (CustomerModel)Session["CustomerDataModal"];
            //LicenseAddress licenseAddress = new LicenseAddress();
            EndorsementRiskDetailModel riskDetailModel = new EndorsementRiskDetailModel();
            riskDetailModel.LicenseAddress1 = customerData.AddressLine1;
            riskDetailModel.LicenseAddress2 = customerData.AddressLine2;
            riskDetailModel.LicenseCity = customerData.City;
            return Json(riskDetailModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getEndorsementVehicleList(int EndorsementsummaryDetailId = 0)
        {
            try
            {
                if (EndorsementsummaryDetailId != 0)
                {
                    if (Session["ViewlistVehicles"] != null)
                    {
                        var list = (List<EndorsementRiskDetailModel>)Session["ViewlistVehicles"];
                        List<VehicleListModel> vehiclelist = new List<VehicleListModel>();

                        foreach (var item in list)
                        {
                            VehicleListModel obj = new VehicleListModel();
                            obj.make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'").ShortDescription;
                            obj.model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ShortDescription;
                            obj.covertype = InsuranceContext.CoverTypes.Single(item.CoverTypeId).Name;
                            obj.premium = item.Premium.ToString();
                            obj.suminsured = item.SumInsured.ToString();
                            obj.ZTSCLevy = item.ZTSCLevy == null ? "0" : item.ZTSCLevy.ToString();
                            vehiclelist.Add(obj);
                        }

                        return Json(vehiclelist, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (Session["ViewlistVehicles"] != null)
                {
                    var _list = (List<EndorsementRiskDetailModel>)Session["ViewlistVehicles"];
                    List<VehicleListModel> _vehiclelist = new List<VehicleListModel>();
                    foreach (var item in _list)
                    {
                        VehicleListModel obj = new VehicleListModel();
                        obj.make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'").ShortDescription;
                        obj.model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ShortDescription;
                        obj.covertype = InsuranceContext.CoverTypes.Single(item.CoverTypeId).Name;
                        obj.premium = item.Premium.ToString();
                        obj.suminsured = item.SumInsured.ToString();
                        obj.ZTSCLevy = item.ZTSCLevy == null ? "0" : item.ZTSCLevy.ToString();
                        obj.RegistrationNo = item.RegistrationNo;
                        obj.excess = item.ExcessAmount == null ? "0" : item.ExcessAmount.ToString();
                        obj.vehicle_license_fee = item.VehicleLicenceFee == 0 ? "0" : item.VehicleLicenceFee.ToString();
                        obj.stampDuty = item.StampDuty == null ? "0" : item.StampDuty.ToString();


                        if (item.IncludeRadioLicenseCost == true)
                        {
                            obj.radio_license_fee = item.RadioLicenseCost == null ? "0" : item.RadioLicenseCost.ToString();
                        }
                        else
                        {
                            obj.radio_license_fee = "0";
                        }
                        decimal? radioLicenseCost = 0;
                        if (item.IncludeRadioLicenseCost)
                        {
                            radioLicenseCost = item.RadioLicenseCost;
                        }

                        // var calculationAmount = item.Premium + radioLicenseCost + item.Excess + item.VehicleLicenceFee + item.StampDuty + item.ZTSCLevy;

                        var calculationAmount = item.Premium + radioLicenseCost + item.VehicleLicenceFee + item.StampDuty + item.ZTSCLevy;


                        obj.total = calculationAmount.ToString();
                        obj.ZTSCLevy = item.ZTSCLevy == null ? "0" : Convert.ToString(item.ZTSCLevy);

                        _vehiclelist.Add(obj);
                    }
                    return Json(_vehiclelist, JsonRequestBehavior.AllowGet);
                }
                {

                }
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult SaveEndorsementRiskDetails(EndorsementRiskDetailModel model)
        {

            var dbVehicle = InsuranceContext.VehicleDetails.Single(where: $"Id={model.VehicleId}");

            //var endorsementvehicle = InsuranceContext.EndorsementVehicleDetails.Single(where: $"Id = '{model.Id}'");
            var EnderSomentVehical = InsuranceContext.EndorsementVehicleDetails.Single(where: $"Id={model.Id}");
            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;


            var vehicleUpdate = Mapper.Map<EndorsementRiskDetailModel, EndorsementVehicleDetail>(model);

            EnderSomentVehical.PrimaryVehicleId = model.PrimaryVehicleId;
            EnderSomentVehical.Id = model.Id;
            EnderSomentVehical.EndorsementCustomerId = model.EndorsementCustomerId;
            EnderSomentVehical.EndorsementPolicyId = model.EndorsementPolicyId;
            EnderSomentVehical.NoOfCarsCovered = vehicleUpdate.NoOfCarsCovered;
            EnderSomentVehical.PolicyId = vehicleUpdate.PolicyId;
            EnderSomentVehical.RegistrationNo = vehicleUpdate.RegistrationNo;
            EnderSomentVehical.CustomerId = model.CustomerId;
            EnderSomentVehical.MakeId = vehicleUpdate.MakeId;
            EnderSomentVehical.ModelId = vehicleUpdate.ModelId;
            EnderSomentVehical.CubicCapacity = vehicleUpdate.CubicCapacity;
            EnderSomentVehical.VehicleYear = vehicleUpdate.VehicleYear;
            EnderSomentVehical.EngineNumber = vehicleUpdate.EngineNumber;
            EnderSomentVehical.ChasisNumber = vehicleUpdate.ChasisNumber;
            EnderSomentVehical.VehicleColor = vehicleUpdate.VehicleColor;
            EnderSomentVehical.VehicleUsage = vehicleUpdate.VehicleUsage;
            EnderSomentVehical.CoverTypeId = vehicleUpdate.CoverTypeId;
            EnderSomentVehical.CoverStartDate = vehicleUpdate.CoverStartDate;
            EnderSomentVehical.CoverEndDate = vehicleUpdate.CoverEndDate;
            EnderSomentVehical.SumInsured = vehicleUpdate.SumInsured;
            EnderSomentVehical.Premium = vehicleUpdate.Premium;
            EnderSomentVehical.AgentCommissionId = vehicleUpdate.AgentCommissionId;
            EnderSomentVehical.Rate = vehicleUpdate.Rate;
            EnderSomentVehical.StampDuty = vehicleUpdate.StampDuty;
            EnderSomentVehical.ZTSCLevy = vehicleUpdate.ZTSCLevy;
            EnderSomentVehical.RadioLicenseCost = vehicleUpdate.RadioLicenseCost;
            EnderSomentVehical.OptionalCovers = vehicleUpdate.OptionalCovers;
            EnderSomentVehical.Excess = vehicleUpdate.Excess;
            EnderSomentVehical.CoverNoteNo = vehicleUpdate.CoverNoteNo;
            EnderSomentVehical.ExcessType = vehicleUpdate.ExcessType;
            EnderSomentVehical.CreatedOn = vehicleUpdate.CreatedOn;
            EnderSomentVehical.CreatedBy = vehicleUpdate.CreatedBy;
            EnderSomentVehical.ModifiedOn = DateTime.Now;
            EnderSomentVehical.ModifiedBy = vehicleUpdate.ModifiedBy;
            if (_userLoggedin)
            {
                var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();

                EnderSomentVehical.ModifiedBy = _customerData.Id;
            }
            EnderSomentVehical.IsActive = model.IsActive;
            EnderSomentVehical.Addthirdparty = vehicleUpdate.Addthirdparty;
            EnderSomentVehical.AddThirdPartyAmount = vehicleUpdate.AddThirdPartyAmount;
            EnderSomentVehical.PassengerAccidentCover = vehicleUpdate.PassengerAccidentCover;
            EnderSomentVehical.ExcessBuyBack = vehicleUpdate.ExcessBuyBack;
            EnderSomentVehical.RoadsideAssistance = vehicleUpdate.RoadsideAssistance;
            EnderSomentVehical.MedicalExpenses = vehicleUpdate.MedicalExpenses;
            EnderSomentVehical.NumberofPersons = vehicleUpdate.NumberofPersons;
            EnderSomentVehical.IsLicenseDiskNeeded = vehicleUpdate.IsLicenseDiskNeeded;
            EnderSomentVehical.PassengerAccidentCoverAmount = vehicleUpdate.PassengerAccidentCoverAmount == null ? 0 : vehicleUpdate.PassengerAccidentCoverAmount;
            EnderSomentVehical.ExcessBuyBackAmount = vehicleUpdate.ExcessBuyBackAmount == null ? 0 : vehicleUpdate.ExcessBuyBackAmount;
            EnderSomentVehical.PaymentTermId = vehicleUpdate.PaymentTermId;
            EnderSomentVehical.ProductId = vehicleUpdate.ProductId;
            EnderSomentVehical.RoadsideAssistanceAmount = vehicleUpdate.RoadsideAssistanceAmount == null ? 0 : vehicleUpdate.RoadsideAssistanceAmount;
            EnderSomentVehical.MedicalExpensesAmount = vehicleUpdate.MedicalExpensesAmount == null ? 0 : vehicleUpdate.MedicalExpensesAmount;
            EnderSomentVehical.PassengerAccidentCoverAmountPerPerson = vehicleUpdate.PassengerAccidentCoverAmountPerPerson == null ? 0 : vehicleUpdate.PassengerAccidentCoverAmountPerPerson;
            EnderSomentVehical.ExcessBuyBackPercentage = vehicleUpdate.ExcessBuyBackPercentage == null ? 0 : vehicleUpdate.ExcessBuyBackPercentage;
            EnderSomentVehical.RoadsideAssistancePercentage = vehicleUpdate.RoadsideAssistancePercentage == null ? 0 : vehicleUpdate.RoadsideAssistancePercentage;
            EnderSomentVehical.MedicalExpensesPercentage = vehicleUpdate.MedicalExpensesPercentage == null ? 0 : vehicleUpdate.MedicalExpensesPercentage;
            EnderSomentVehical.ExcessAmount = vehicleUpdate.ExcessAmount;
            EnderSomentVehical.RenewalDate = vehicleUpdate.RenewalDate;
            EnderSomentVehical.TransactionDate = DateTime.Now;
            EnderSomentVehical.IncludeRadioLicenseCost = vehicleUpdate.IncludeRadioLicenseCost;
            EnderSomentVehical.InsuranceId = vehicleUpdate.InsuranceId;
            EnderSomentVehical.AnnualRiskPremium = vehicleUpdate.AnnualRiskPremium == null ? 0 : vehicleUpdate.AnnualRiskPremium;
            EnderSomentVehical.TermlyRiskPremium = vehicleUpdate.TermlyRiskPremium == null ? 0 : vehicleUpdate.TermlyRiskPremium;
            EnderSomentVehical.QuaterlyRiskPremium = vehicleUpdate.QuaterlyRiskPremium == null ? 0 : vehicleUpdate.QuaterlyRiskPremium;
            EnderSomentVehical.Discount = vehicleUpdate.Discount;
            EnderSomentVehical.isLapsed = vehicleUpdate.isLapsed;
            EnderSomentVehical.BalanceAmount = model.BalanceAmount;
            EnderSomentVehical.VehicleLicenceFee = vehicleUpdate.VehicleLicenceFee;
            EnderSomentVehical.BusinessSourceId = vehicleUpdate.BusinessSourceId;
            EnderSomentVehical.IsCompleted = true;
            EnderSomentVehical.CreatedOn = model.CreatedOn;
            EnderSomentVehical.CreatedBy = model.CreatedBy;
            InsuranceContext.EndorsementVehicleDetails.Update(EnderSomentVehical);
            Session.Remove("ViewlistVehicles");
            return RedirectToAction("EndorsementSummaryDetail", "Endorsement");

        }


        public ActionResult EndorsementSummaryDetail(int? Id = 0)
        {
            ViewBag.SummaryDetailId = Id;
            var _model = new EndorsementSummaryDetailModel();
            var EnorsesummaryDetail = (EndorsementSummaryDetail)Session["ENViewSummaryDetail"];
            List<EndorsementVehicleDetail> _listriskdetailmodel = new List<EndorsementVehicleDetail>();
            List<EndorsementVehicleDetail> listriskdetailmodels = new List<EndorsementVehicleDetail>();
            List<EndorsementRiskDetailModel> RiskDetalModel = new List<EndorsementRiskDetailModel>();
            //var listVehicles = Session["ViewlistVehicles"];
            //var vehicle = (List<EndorsementRiskDetailModel>)Session["ViewlistVehicles"];// summary.GetVehicleInformation(id);
            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;


            var endorsesummaryDetail = InsuranceContext.EndorsementSummaryDetails.All(where: $"Id={EnorsesummaryDetail.Id}").FirstOrDefault();
            var endorseSummaryVehicleDetails = InsuranceContext.EndorsementSummaryVehicleDetails.All(where: $"EndorsementSummaryId={EnorsesummaryDetail.Id}").ToList();
            if (endorseSummaryVehicleDetails != null)
            {
                foreach (var item in endorseSummaryVehicleDetails)
                {
                    var envehicle = InsuranceContext.EndorsementVehicleDetails.Single(where: $"Id={item.EndorsementVehicleId}");
                    var model = Mapper.Map<EndorsementVehicleDetail, EndorsementRiskDetailModel>(envehicle);
                    EndorsementRiskDetailModel obj = new EndorsementRiskDetailModel();


                    // var vehicleInsert = new EndorsementVehicleDetail();
                    obj.NoOfCarsCovered = envehicle.NoOfCarsCovered;
                    obj.PolicyId = envehicle.PolicyId;
                    obj.RegistrationNo = envehicle.RegistrationNo;
                    //Endor//
                    obj.PrimaryVehicleId = envehicle.PrimaryVehicleId;
                    obj.EndorsementPolicyId = envehicle.Id;
                    obj.EndorsementCustomerId = envehicle.EndorsementCustomerId;
                    //
                    obj.MakeId = envehicle.MakeId;
                    obj.ModelId = envehicle.ModelId;
                    obj.ModelId = envehicle.ModelId;
                    obj.CubicCapacity = envehicle.CubicCapacity;
                    obj.VehicleYear = envehicle.VehicleYear;
                    obj.EngineNumber = envehicle.EngineNumber;
                    obj.ChasisNumber = envehicle.ChasisNumber;
                    obj.VehicleColor = envehicle.VehicleColor;
                    obj.VehicleUsage = envehicle.VehicleUsage;
                    obj.CoverTypeId = envehicle.CoverTypeId;
                    obj.CoverTypeId = envehicle.CoverTypeId;
                    obj.CoverStartDate = envehicle.CoverStartDate;
                    obj.CoverEndDate = envehicle.CoverEndDate;
                    obj.SumInsured = envehicle.SumInsured;
                    obj.Premium = envehicle.Premium;
                    obj.AgentCommissionId = envehicle.AgentCommissionId;
                    obj.Rate = envehicle.Rate;
                    obj.StampDuty = envehicle.StampDuty;
                    obj.ZTSCLevy = envehicle.ZTSCLevy;
                    obj.RadioLicenseCost = envehicle.RadioLicenseCost;
                    obj.OptionalCovers = envehicle.OptionalCovers;
                    obj.Excess = envehicle.Excess;
                    obj.CoverNoteNo = envehicle.CoverNoteNo;
                    obj.ExcessType = envehicle.ExcessType;
                    obj.ExcessType = envehicle.ExcessType;
                    obj.CreatedOn = DateTime.Now;
                    obj.CreatedBy = envehicle.CreatedBy;


                    obj.IsActive = envehicle.IsActive;
                    obj.Addthirdparty = envehicle.Addthirdparty;
                    obj.Addthirdparty = envehicle.Addthirdparty;
                    obj.AddThirdPartyAmount = envehicle.AddThirdPartyAmount;

                    obj.PassengerAccidentCoverAmount = envehicle.PassengerAccidentCoverAmount == null ? 0 : envehicle.PassengerAccidentCoverAmount;
                    obj.ExcessBuyBackAmount = envehicle.ExcessBuyBackAmount == null ? 0 : envehicle.ExcessBuyBackAmount;

                    obj.NumberofPersons = envehicle.NumberofPersons;
                    obj.IsLicenseDiskNeeded = Convert.ToBoolean(envehicle.IsLicenseDiskNeeded);


                    obj.PassengerAccidentCoverAmountPerPerson = envehicle.PassengerAccidentCoverAmountPerPerson == null ? 0 : envehicle.PassengerAccidentCoverAmountPerPerson;
                    obj.ExcessBuyBackPercentage = envehicle.ExcessBuyBackPercentage == null ? 0 : envehicle.ExcessBuyBackPercentage;

                    obj.PaymentTermId = envehicle.PaymentTermId;
                    obj.ProductId = envehicle.ProductId;
                    obj.RoadsideAssistanceAmount = envehicle.RoadsideAssistanceAmount == null ? 0 : envehicle.RoadsideAssistanceAmount;
                    obj.MedicalExpensesAmount = envehicle.MedicalExpensesAmount == null ? 0 : envehicle.MedicalExpensesAmount;

                    obj.ExcessAmount = envehicle.ExcessAmount;

                    obj.TransactionDate = DateTime.Now;
                    obj.RenewalDate = Convert.ToDateTime(envehicle.RenewalDate);
                    obj.IncludeRadioLicenseCost = Convert.ToBoolean(envehicle.IncludeRadioLicenseCost);
                    obj.InsuranceId = envehicle.InsuranceId;
                    obj.InsuranceId = envehicle.InsuranceId;
                    obj.AnnualRiskPremium = envehicle.AnnualRiskPremium == null ? 0 : envehicle.AnnualRiskPremium;
                    obj.TermlyRiskPremium = envehicle.TermlyRiskPremium == null ? 0 : envehicle.TermlyRiskPremium;
                    obj.QuaterlyRiskPremium = envehicle.QuaterlyRiskPremium == null ? 0 : envehicle.QuaterlyRiskPremium;
                    obj.Discount = envehicle.Discount;

                    obj.isLapsed = envehicle.isLapsed;
                    obj.BalanceAmount = envehicle.BalanceAmount;
                    obj.VehicleLicenceFee = Convert.ToDecimal(envehicle.VehicleLicenceFee);
                    obj.BusinessSourceId = envehicle.BusinessSourceId;
                    obj.CurrencyId = envehicle.CurrencyId;

                    obj.RoadsideAssistancePercentage = envehicle.RoadsideAssistancePercentage == null ? 0 : envehicle.RoadsideAssistancePercentage;
                    obj.MedicalExpensesPercentage = envehicle.MedicalExpensesPercentage == null ? 0 : envehicle.MedicalExpensesPercentage;
                    obj.IsCompleted = envehicle.IsCompleted;
                    //InsuranceContext.EndorsementVehicleDetails.Insert(vehicleInsert);
                    //var vehicleId = vehicleInsert;
                    //Session["vehicleId"] = vehicleInsert;
                    RiskDetalModel.Add(obj);
                    Session["ViewlistVehicles"] = RiskDetalModel;
                }

            }

            var smrydetail = (List<EndorsementRiskDetailModel>)Session["ViewlistVehicles"];

            EndorsementSummaryDetailService endorsementService = new EndorsementSummaryDetailService();

            if (smrydetail != null)
            {
                //var model = Mapper.Map<EndorsementSummaryDetail, EndorsementSummaryDetailModel>(smrydetail);
                _model.CarInsuredCount = smrydetail.Count;
                _model.DebitNote = "INV" + Convert.ToString(endorsementService.getNewDebitNote());
                _model.PaymentMethodId = 1;
                _model.PaymentTermId = 1;
                _model.ReceiptNumber = "";
                _model.SMSConfirmation = false;
                _model.AmountPaid = 0.00m;
                _model.TotalPremium = 0.00m;
                _model.TotalRadioLicenseCost = 0.00m;
                _model.Discount = 0.00m;
                _model.Id = EnorsesummaryDetail.Id;
                foreach (var item in smrydetail)
                {
                    _model.TotalPremium += item.Premium + item.ZTSCLevy + item.StampDuty + item.VehicleLicenceFee;
                    if (Convert.ToBoolean(item.IncludeRadioLicenseCost))
                    {
                        _model.TotalPremium += item.RadioLicenseCost;
                        _model.TotalRadioLicenseCost += item.RadioLicenseCost;
                    }
                }

                _model.AmountPaid =Convert.ToDecimal(_model.TotalPremium);
                _model.TotalStampDuty = smrydetail.Sum(item => item.StampDuty);
                _model.TotalSumInsured = smrydetail.Sum(item => item.SumInsured);
                _model.TotalZTSCLevies = smrydetail.Sum(item => item.ZTSCLevy);
                _model.ExcessBuyBackAmount = smrydetail.Sum(item => item.ExcessBuyBackAmount);
                _model.MedicalExpensesAmount = smrydetail.Sum(item => item.MedicalExpensesAmount);
                _model.PassengerAccidentCoverAmount = smrydetail.Sum(item => item.PassengerAccidentCoverAmount);
                _model.RoadsideAssistanceAmount = smrydetail.Sum(item => item.RoadsideAssistanceAmount);
                _model.ExcessAmount = smrydetail.Sum(item => item.ExcessAmount);
                _model.Discount = smrydetail.Sum(item => item.Discount);
                _model.isQuotation = false;
                _model.IsCompleted = false;
                _model.CreatedOn = DateTime.Now;
                if (_userLoggedin)
                {
                    var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                    var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();
                    _model.CreatedBy = _customerData.Id;
                }
                _model.PrimarySummaryId = EnorsesummaryDetail.PrimarySummaryId;
                _model.EndorsementCustomerId = EnorsesummaryDetail.EndorsementCustomerId;
                _model.EndorsementPolicyId = EnorsesummaryDetail.EndorsementPolicyId;
                _model.CustomerId = EnorsesummaryDetail.CustomerId;
                if (Session["PolicyId"] != null)
                {
                    var PolicyData = (EndorsementPolicyDetail)Session["PolicyId"];
                    _model.InvoiceNumber = PolicyData.PolicyNumber;
                }


            }

            return View(_model);
        }
        public ActionResult SaveEndorsementSummaryDetails(EndorsementSummaryDetailModel model)
        {

            var ensummerydetail = InsuranceContext.EndorsementSummaryDetails.Single(where: $"Id = '{model.Id}'");
            if (ensummerydetail != null)
            {


                var EndorseSummery = Mapper.Map<EndorsementSummaryDetailModel, EndorsementSummaryDetail>(model);
                //EndorsementSummaryDetailModel endorsemodel = new EndorsementSummaryDetailModel();
                ensummerydetail.EndorsementPolicyId = model.EndorsementPolicyId;
                ensummerydetail.EndorsementCustomerId = model.EndorsementCustomerId;
                ensummerydetail.PrimarySummaryId = model.PrimarySummaryId;
                ensummerydetail.SummaryId = model.SummaryId;
                ensummerydetail.CustomerId = EndorseSummery.CustomerId;
                ensummerydetail.PaymentTermId = EndorseSummery.PaymentTermId;
                ensummerydetail.PaymentMethodId = EndorseSummery.PaymentMethodId;
                ensummerydetail.TotalSumInsured = EndorseSummery.TotalSumInsured;
                ensummerydetail.TotalPremium = EndorseSummery.TotalPremium;
                ensummerydetail.TotalStampDuty = EndorseSummery.TotalStampDuty;
                ensummerydetail.TotalZTSCLevies = EndorseSummery.TotalZTSCLevies;
                ensummerydetail.TotalRadioLicenseCost = EndorseSummery.TotalRadioLicenseCost;
                ensummerydetail.DebitNote = EndorseSummery.DebitNote;
                ensummerydetail.ReceiptNumber = EndorseSummery.ReceiptNumber;
                ensummerydetail.SMSConfirmation = Convert.ToBoolean(EndorseSummery.SMSConfirmation);
                ensummerydetail.CreatedBy = EndorseSummery.CreatedBy;
                ensummerydetail.CreatedOn = model.CreatedOn;
                ensummerydetail.PaymentMethodId = model.PaymentMethodId;
                ensummerydetail.IsCompleted = true;
                InsuranceContext.EndorsementSummaryDetails.Update(ensummerydetail);


                //if (model.PaymentMethodId == 1)
                //    return RedirectToAction("SaveEndorsementDetailList", "Endorsement", new { id = model.Id, invoiceNumer = model.InvoiceNumber });
            }

            return RedirectToAction("MyPolicies", "Account");
        }


        public async Task<ActionResult> SaveEndorsementDetailList(Int32 id ,string invoiceNumer,string Paymentid = "")
        {
            string PaymentMethod = "";
            if (Paymentid == "1")
            {
                PaymentMethod = "CASH";
            }
            else if (Paymentid == "2")
            {
                PaymentMethod = "MasterCard";
            }
            else if (Paymentid == "3")
            {
                PaymentMethod = "paynow";
            }
            else if (Paymentid == "")
            {
                PaymentMethod = "CASH";
            }

            var endorsementsummay = InsuranceContext.EndorsementSummaryDetails.Single(id);

            if (endorsementsummay != null && endorsementsummay.isQuotation)
            {
                endorsementsummay.isQuotation = false;
            }
            var EndorsementSummaryVehicleDetails = InsuranceContext.EndorsementSummaryDetails.All(where: $"EndorsementSummaryId={id}").ToList();
            var endorsevehicle = InsuranceContext.EndorsementVehicleDetails.Single(where: $"Id={EndorsementSummaryVehicleDetails[0].EndorsementVehicleId}");
            var endorsepolicy = InsuranceContext.EndorsementPolicyDetails.Single(endorsevehicle.EndorsementPolicyId);
            var endorsementCustomer = InsuranceContext.EndorsementCustomers.Single(endorsementsummay.EndorsementCustomerId);
            var product = InsuranceContext.Products.Single(Convert.ToInt32(endorsevehicle.ProductId));
            var currency = InsuranceContext.Currencies.Single(endorsepolicy.CurrencyId);
            var paymentInformations = InsuranceContext.PaymentInformations.SingleCustome(id);

            var user = UserManager.FindById(endorsementCustomer.UserID);
            var DebitNote = endorsementsummay.DebitNote;
            EndorsementPaymentInformation objSaveDetailListModel = new EndorsementPaymentInformation();
            objSaveDetailListModel.CurrencyId = endorsepolicy.CurrencyId;
            objSaveDetailListModel.PrimaryPolicyId = endorsepolicy.PrimaryPolicyId;
            objSaveDetailListModel.PrimaryCustomerId = endorsementsummay.CustomerId.Value;

            objSaveDetailListModel.PrimarySummaryDetailId = endorsementsummay.PrimarySummaryId;
            objSaveDetailListModel.DebitNote = endorsementsummay.DebitNote;
            objSaveDetailListModel.ProductId = product.Id;

            objSaveDetailListModel.PaymentId = PaymentMethod;
            objSaveDetailListModel.InvoiceId = invoiceNumer;
            objSaveDetailListModel.CreatedBy = endorsementCustomer.Id;
            objSaveDetailListModel.CreatedOn = DateTime.Now;
            objSaveDetailListModel.InvoiceNumber = endorsepolicy.PolicyNumber;
            List<EndorsementVehicleDetail> ListOfVehicles = new List<EndorsementVehicleDetail>();



            string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
            Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();

            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            var dbEndorsemenetPaymentInformation = InsuranceContext.EndorsementPaymentInformations.Single(where: $"EndorsementSummaryId='{id}'");
            if (dbEndorsemenetPaymentInformation == null)
            {
                InsuranceContext.EndorsementPaymentInformations.Insert(objSaveDetailListModel);
            }
            else
            {
                objSaveDetailListModel.Id = dbEndorsemenetPaymentInformation.Id;
                InsuranceContext.EndorsementPaymentInformations.Update(objSaveDetailListModel);
            }

            //ApproveVRNToIceCash(id);



            return View();
        }


        //get SummeryList 
        public ActionResult GetEndorsementSummeryList()
        {
            return View();
        }


        //[HttpPost]
        //public JsonResult DeleteVehicle(int? index)
        //{

        //    try
        //    {
        //        if (Session["VehicleDetails"] != null)
        //        {
        //            var list = (List<RiskDetailModel>)Session["VehicleDetails"];

        //            list.RemoveAt(Convert.ToInt32(index) - 1);

        //            return Json(true, JsonRequestBehavior.AllowGet);
        //        }
        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {

        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }


        //}
        //Get Customer Id And Detail
        public string GetCustomerEmailbyCustomerID(int? customerId)
        {
            var customerDetial = InsuranceContext.Customers.Single(customerId);

            string email = "";

            if (customerDetial != null)
            {
                var user = UserManager.FindById(customerDetial.UserID);

                email = user.Email;
            }
            return email;

        }

        public ActionResult EndorsementDetail()
        {
            ListEndorsementPolicy Endorpolicylist = new ListEndorsementPolicy();
            Endorpolicylist.listendorsementpolicy = new List<EndorsementPolicyListViewModel>();

            var endorsementsummary = new List<EndorsementSummaryDetail>();
            endorsementsummary = InsuranceContext.EndorsementSummaryDetails.All().OrderByDescending(c=>c.CreatedOn).ToList();

            foreach (var item in endorsementsummary)
            {

                EndorsementPolicyListViewModel ListEndorsmentDetail = new EndorsementPolicyListViewModel();

                ListEndorsmentDetail.TotalPremium = Convert.ToDecimal(item.TotalPremium);
                ListEndorsmentDetail.TotalSumInsured = Convert.ToDecimal(item.TotalSumInsured);
                ListEndorsmentDetail.PaymentMethodId = Convert.ToInt32(item.PaymentMethodId);
                ListEndorsmentDetail.CustomerId = Convert.ToInt32(item.CustomerId);
                ListEndorsmentDetail.CustomerEmail = GetCustomerEmailbyCustomerID(item.CustomerId);
                ListEndorsmentDetail.SummaryId = item.SummaryId;
                ListEndorsmentDetail.createdOn = Convert.ToDateTime(item.CreatedOn);
                ListEndorsmentDetail.EndorsementSummaryId = item.Id;

                var Endorsementsummaryvehicledetail = InsuranceContext.EndorsementSummaryVehicleDetails.All(where: $"EndorsementSummaryId = '{item.Id}'").ToList();
                var Endorsementvehicle = InsuranceContext.EndorsementVehicleDetails.Single(Endorsementsummaryvehicledetail[0].EndorsementVehicleId);

                if (Endorsementvehicle != null)
                {
                    var Endorsepolicy = InsuranceContext.EndorsementPolicyDetails.Single(Endorsementvehicle.EndorsementPolicyId);
                    var product = InsuranceContext.Products.Single(Convert.ToInt32(Endorsementvehicle.ProductId));

                    ListEndorsmentDetail.PolicyNumber = Endorsepolicy.PolicyNumber;

                    int i = 0;

                }
                EndorsementVehicleReinsurance obj = new EndorsementVehicleReinsurance();
                var _Endorsementvehicle = InsuranceContext.EndorsementVehicleDetails.Single(Endorsementsummaryvehicledetail[0].EndorsementVehicleId);
                if (_Endorsementvehicle != null)
                {

                    //var _reinsurenaceTrans = InsuranceContext.ReinsuranceTransactions.All(where: $"SummaryDetailId={item.Id} and VehicleId={SummaryVehicleDetails[0].VehicleDetailsId}").ToList();



                    obj.CoverType = Convert.ToInt32(_Endorsementvehicle.CoverTypeId);
                    obj.isReinsurance = (_Endorsementvehicle.SumInsured > 100000 ? true : false);
                    obj.MakeId = _Endorsementvehicle.MakeId;
                    obj.ModelId = _Endorsementvehicle.ModelId;
                    obj.RegisterationNumber = _Endorsementvehicle.RegistrationNo;
                    obj.SumInsured = Convert.ToDecimal(_Endorsementvehicle.SumInsured);
                    obj.VehicleId = _Endorsementvehicle.Id;
                    obj.startdate = Convert.ToDateTime(_Endorsementvehicle.CoverStartDate);
                    obj.enddate = Convert.ToDateTime(_Endorsementvehicle.CoverEndDate);
                    obj.RenewalDate = Convert.ToDateTime(_Endorsementvehicle.RenewalDate);
                    obj.isLapsed = _Endorsementvehicle.isLapsed;
                    obj.BalanceAmount = Convert.ToDecimal(_Endorsementvehicle.BalanceAmount);
                    obj.isActive = Convert.ToBoolean(_Endorsementvehicle.IsActive);
                    obj.Premium = Convert.ToDecimal(_Endorsementvehicle.Premium + _Endorsementvehicle.StampDuty + _Endorsementvehicle.ZTSCLevy + (Convert.ToBoolean(_Endorsementvehicle.IncludeRadioLicenseCost) ? Convert.ToDecimal(_Endorsementvehicle.RadioLicenseCost) : 0.00m));

                    //if (_reinsurenaceTrans != null && _reinsurenaceTrans.Count > 0)
                    //{
                    //    obj.BrokerCommission = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceCommission);
                    //    obj.AutoFacPremium = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsurancePremium);
                    //    obj.AutoFacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceAmount);

                    //    if (_reinsurenaceTrans.Count > 1)
                    //    {
                    //        obj.FacultativeCommission = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsuranceCommission);
                    //        obj.FacPremium = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsurancePremium);
                    //        obj.FacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsuranceAmount);
                    //    }



                    //    policylistviewmodel.Vehicles.Add(obj);
                    //}
                    Endorpolicylist.listendorsementpolicy.Add(ListEndorsmentDetail);
                }
            }

            return View(Endorpolicylist);
        }

        public ActionResult ViewEndorsementCustomer(int id = 0)
        {
           
            EndorsementCustomerModel endorcustom = new EndorsementCustomerModel();
            if (id != 0)
            {
                var EndorsementSummery = InsuranceContext.EndorsementSummaryDetails.All(where: $"Id ='{id}'").FirstOrDefault();
                var Endorsenecustomer = InsuranceContext.EndorsementCustomers.Single(where: $"Id = '{EndorsementSummery.EndorsementCustomerId}'");
                endorcustom = Mapper.Map<EndorsementCustomer, EndorsementCustomerModel>(Endorsenecustomer);

                if (endorcustom != null)
                {
                    var dbUser = UserManager.Users.FirstOrDefault(c => c.Id == endorcustom.UserID);
                    if (dbUser != null)
                    {
                        endorcustom.EmailAddress = dbUser.Email; ;
                        endorcustom.PrimeryCustomerId = endorcustom.Id;
                        endorcustom.SummaryId = id;
                    }
                }
               
                string path = Server.MapPath("~/Content/Countries.txt");
                var countries = System.IO.File.ReadAllText(path);
                var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(countries);
                ViewBag.Countries = resultt.countries.OrderBy(x => x.code.Replace("+", ""));
                ViewBag.Cities = InsuranceContext.Cities.All();
                Session["EndorsummeryDetail"] = id;
            }
            else
            {
                return RedirectToAction("EndorsementDetail", "Endorsement");
            }
            return View(endorcustom);
        }

        [HttpPost]
        public async Task<JsonResult> SaveEnCustomerData(EndorsementCustomerModel model, string buttonUpdate)
        {
            ModelState.Remove("City");
            ModelState.Remove("CountryCode");

            if (ModelState.IsValid)
            {
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                if (userLoggedin)
                {

                    Session["EndorseCustomerDataModal"] = model;
                    return Json(new { IsError = true, error = "" }, JsonRequestBehavior.AllowGet);
                }


            }
            return Json(new { IsError = false, error = TempData["ErrorMessage"].ToString() }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EndorseProductDetail()
        {

            return RedirectToAction("EndorsementRiskDetail");
        }
        public ActionResult EndorsementRiskDetail(int ? id = 1)
        {
            var endorsementRisk = new EndorsementRiskDetailModel();
            SetEndorseValueIntoSession(Convert.ToInt32(Session["EndorsummeryDetail"]));
           endorsementRisk.EndorsementSummaryId =Convert.ToInt32(Session["EndorsummeryDetail"]);


            ViewBag.Products = InsuranceContext.Products.All().ToList();
            var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm))
                                   select new
                                   {
                                       ID = (int)e,
                                       Name = e.ToString()
                                   };

            ViewBag.ePaymentTermData = new SelectList(ePaymentTermData, "ID", "Name");
            int RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
            var _PolicyData = (EndorsementPolicyDetail)Session["EnPolicyDataView"];
            // Id is policyid from Policy detail table

            var service = new VehicleService();

            ViewBag.VehicleUsage = service.GetAllVehicleUsage();


            endorsementRisk.NumberofPersons = 0;
            endorsementRisk.AddThirdPartyAmount = 0.00m;
            endorsementRisk.RadioLicenseCost = Convert.ToDecimal(RadioLicenseCosts);
            var makers = service.GetMakers();
            ViewBag.CoverType = service.GetCoverType();
            ViewBag.AgentCommission = service.GetAgentCommission();
            ViewBag.Makers = makers;
            endorsementRisk.isUpdate = false;
            endorsementRisk.VehicleUsage = 0;
            TempData["Policy"] = _PolicyData;
            if (makers.Count > 0)
            {
                var model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = model;
            }

            endorsementRisk.NoOfCarsCovered = 1;
            if (Session["EndorselistVehicles"] != null)
            {
                var list = (List<EndorsementRiskDetailModel>)Session["EndorselistVehicles"];
                if (list.Count > 0)
                {
                    endorsementRisk.NoOfCarsCovered = list.Count + 1;
                }
            }
            if (id > 0)
            {
                var list = (List<EndorsementRiskDetailModel>)Session["EndorselistVehicles"];
                if (list != null && list.Count > 0 && (list.Count >= id))
                {
                    var data = (EndorsementRiskDetailModel)list[Convert.ToInt32(id - 1)];
                    if (data != null)
                    {
                        endorsementRisk.AgentCommissionId = data.AgentCommissionId;
                        endorsementRisk.ChasisNumber = data.ChasisNumber;
                        endorsementRisk.CoverEndDate = data.CoverEndDate;
                        endorsementRisk.CoverNoteNo = data.CoverNoteNo;
                        endorsementRisk.CoverStartDate = data.CoverStartDate;
                        endorsementRisk.CoverTypeId = data.CoverTypeId;
                        endorsementRisk.CubicCapacity = (int)Math.Round(data.CubicCapacity.Value, 0);
                        endorsementRisk.CustomerId = data.CustomerId;
                        endorsementRisk.EngineNumber = data.EngineNumber;
                        // viewModel.Equals = data.Equals;
                        endorsementRisk.Excess = (int)Math.Round(data.Excess, 0);
                        endorsementRisk.ExcessType = data.ExcessType;
                        endorsementRisk.MakeId = data.MakeId;
                        endorsementRisk.ModelId = data.ModelId;
                        endorsementRisk.NoOfCarsCovered = id;
                        endorsementRisk.OptionalCovers = data.OptionalCovers;
                        endorsementRisk.PolicyId = data.PolicyId;
                        endorsementRisk.Premium = data.Premium;
                        endorsementRisk.RadioLicenseCost = (int)Math.Round(data.RadioLicenseCost == null ? 0 : data.RadioLicenseCost.Value, 0);
                        endorsementRisk.Rate = data.Rate;
                        endorsementRisk.RegistrationNo = data.RegistrationNo;
                        endorsementRisk.StampDuty = data.StampDuty;
                        endorsementRisk.SumInsured = (int)Math.Round(data.SumInsured == null ? 0 : data.SumInsured.Value, 0);
                        endorsementRisk.VehicleColor = data.VehicleColor;
                        endorsementRisk.VehicleUsage = data.VehicleUsage;
                        endorsementRisk.VehicleYear = data.VehicleYear;
                        endorsementRisk.Id = data.Id;
                        endorsementRisk.ZTSCLevy = data.ZTSCLevy;
                        endorsementRisk.NumberofPersons = data.NumberofPersons;
                        endorsementRisk.PassengerAccidentCover = data.PassengerAccidentCover;
                        endorsementRisk.IsLicenseDiskNeeded = data.IsLicenseDiskNeeded;
                        endorsementRisk.ExcessBuyBack = data.ExcessBuyBack;
                        endorsementRisk.RoadsideAssistance = data.RoadsideAssistance;
                        endorsementRisk.MedicalExpenses = data.MedicalExpenses;
                        endorsementRisk.Addthirdparty = data.Addthirdparty;
                        endorsementRisk.AddThirdPartyAmount = data.AddThirdPartyAmount;
                        endorsementRisk.ExcessAmount = data.ExcessAmount;
                        endorsementRisk.ProductId = data.ProductId;
                        endorsementRisk.PaymentTermId = data.PaymentTermId;
                        endorsementRisk.IncludeRadioLicenseCost = data.IncludeRadioLicenseCost;
                        endorsementRisk.Discount = data.Discount;
                        endorsementRisk.VehicleLicenceFee = Convert.ToDecimal(data.VehicleLicenceFee);

                        endorsementRisk.VehicleUsage = data.VehicleUsage;


                        endorsementRisk.isUpdate = true;
                        endorsementRisk.vehicleindex = Convert.ToInt32(id);

                        var ser = new VehicleService();
                        var model = ser.GetModel(data.MakeId);
                        ViewBag.Model = model;
                    }
                }
            }
            return View(endorsementRisk);
        }
        public void SetEndorseValueIntoSession(int summaryId)
        {
            //Session["ICEcashToken"] = null;

            Session["EndorsummeryDetail"] = summaryId;

            var EnsummaryDetail = InsuranceContext.EndorsementSummaryDetails.Single(summaryId);
            var EnSummaryVehicleDetails = InsuranceContext.EndorsementSummaryVehicleDetails.All(where: $"EndorsementSummaryId={summaryId}").ToList();
            var Envehicle = InsuranceContext.EndorsementVehicleDetails.Single(EnSummaryVehicleDetails[0].EndorsementVehicleId);
            var Enpolicy = InsuranceContext.EndorsementPolicyDetails.Single(Envehicle.EndorsementPolicyId);
            var product = InsuranceContext.Products.Single(Convert.ToInt32(Enpolicy.PolicyName));


            Session["EnPolicyDataView"] = Enpolicy;

            List<EndorsementRiskDetailModel> listRiskDetail = new List<EndorsementRiskDetailModel>();
            foreach (var item in EnSummaryVehicleDetails)
            {
                

                var _vehicle = InsuranceContext.EndorsementVehicleDetails.Single(where:$"Id ='{item.EndorsementVehicleId}'" );
                EndorsementRiskDetailModel _riskDetail = Mapper.Map<EndorsementVehicleDetail, EndorsementRiskDetailModel>(_vehicle);
                listRiskDetail.Add(_riskDetail);
            }
            // Session["VehicleDetails"] = listRiskDetail;
            Session["EndorselistVehicles"] = listRiskDetail;

            EndorsementSummaryDetailModel summarymodel = Mapper.Map<EndorsementSummaryDetail, EndorsementSummaryDetailModel>(EnsummaryDetail);
            summarymodel.Id = EnsummaryDetail.Id;

            Session["EndorsementSummaryDetail"] = EnsummaryDetail;

        }


        public JsonResult getEdorsementVehicle()
        {
            try
            {
                if (Session["EndorselistVehicles"] != null)
                {
                    var list = (List<EndorsementRiskDetailModel>)Session["EndorselistVehicles"];
                    List<VehicleListModel> vehiclelist = new List<VehicleListModel>();

                    foreach (var item in list)
                    {
                        VehicleListModel obj = new VehicleListModel();
                        obj.make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'").ShortDescription;
                        obj.model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ShortDescription;
                        obj.covertype = InsuranceContext.CoverTypes.Single(item.CoverTypeId).Name;
                        obj.premium = item.Premium.ToString();
                        obj.suminsured = item.SumInsured.ToString();
                        obj.ZTSCLevy = item.ZTSCLevy == null ? "0" : item.ZTSCLevy.ToString();
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

        public ActionResult EndorsementSummaryDetails(int? Id = 0)
        {

            var _model = new SummaryDetailModel();
            var summaryDetail = Session["EndorsementSummaryDetail"];
            var Endorsementvehicle = (List<EndorsementRiskDetailModel>)Session["EndorselistVehicles"];
            
           /* var Endorsevehicle = (List<EndorsementRiskDetailModel>)Session["ViewlistVehicles"];*/// summary.GetVehicleInformation(id);
            var summarydetail = (EndorsementSummaryDetail)Session["EndorsementSummaryDetail"];
            SummaryDetailService SummaryDetailServiceObj = new SummaryDetailService();

            if (summarydetail != null)
            {
                var model = Mapper.Map<EndorsementSummaryDetail, EndorsementSummaryDetailModel>(summarydetail);
                model.CarInsuredCount = Endorsementvehicle.Count;
                model.DebitNote = "INV" + Convert.ToString(SummaryDetailServiceObj.getNewDebitNote());
                model.PaymentMethodId = summarydetail.PaymentMethodId;
                model.PaymentTermId = 1;
                model.ReceiptNumber = "";
                model.SMSConfirmation = false;

                model.TotalPremium = Endorsementvehicle.Sum(item => item.Premium + item.ZTSCLevy + item.StampDuty + item.VehicleLicenceFee + (item.IncludeRadioLicenseCost ? item.RadioLicenseCost : 0.00m));// + vehicle.StampDuty + vehicle.ZTSCLevy;
                                                                                                                                                                                                  //model.TotalRadioLicenseCost = vehicle.Sum(item => item.RadioLicenseCost);
                model.TotalStampDuty = Endorsementvehicle.Sum(item => item.StampDuty);
                model.TotalSumInsured = Endorsementvehicle.Sum(item => item.SumInsured);
                model.TotalZTSCLevies = Endorsementvehicle.Sum(item => item.ZTSCLevy);
                model.ExcessBuyBackAmount = Endorsementvehicle.Sum(item => item.ExcessBuyBackAmount);
                model.MedicalExpensesAmount = Endorsementvehicle.Sum(item => item.MedicalExpensesAmount);
                model.PassengerAccidentCoverAmount = Endorsementvehicle.Sum(item => item.PassengerAccidentCoverAmount);
                model.RoadsideAssistanceAmount = Endorsementvehicle.Sum(item => item.RoadsideAssistanceAmount);
                model.ExcessAmount = Endorsementvehicle.Sum(item => item.ExcessAmount);
                model.Discount = Endorsementvehicle.Sum(item => item.Discount);
                decimal radio = 0.00m;
                foreach (var item in Endorsementvehicle)
                {
                    if (item.IncludeRadioLicenseCost)
                    {
                        radio += Convert.ToDecimal(item.RadioLicenseCost);
                    }
                }
                model.TotalRadioLicenseCost = radio;

                //var Model = Mapper.Map<SummaryDetailModel, SummaryDetail>(summarydetail);
                //InsuranceContext.SummaryDetails.Insert(Model);

                return View(model);
            }

                return View();
        }
    }
}