﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InsuranceClaim.Models;
using AutoMapper;
using Insurance.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Insurance.Service;
using System.IO;

namespace InsuranceClaim.Controllers
{
    public class ClaimantController : Controller
    {
        public ActionResult SaveClaimant()
        {

            var service = new VehicleService();
            var makers = service.GetMakers();
            ViewBag.Makers = makers;
            if (makers.Count > 0)
            {
                var model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = model;
            }
            return View();
        }

        [HttpPost]
        public ActionResult SaveClaimant(ClaimNotificationModel model)
        {
            if (ModelState.IsValid)
            {


                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                string userid = "";
                if (userLoggedin)
                {
                    userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    var customer = InsuranceContext.Customers.Single(where: $"UserId ='{userid}'");
                    var dbModel = Mapper.Map<ClaimNotificationModel, ClaimNotification>(model);
                    var policy = model.PolicyNumber;
                    var Detailofpolicy = InsuranceContext.PolicyDetails.Single(where: $"PolicyNumber='{policy}'");

                    var vehicalDetails = InsuranceContext.VehicleDetails.Single(where: $"PolicyId='{Detailofpolicy.Id}' and RegistrationNo='"+ model.RegistrationNo + "'");

                    if(vehicalDetails!=null)
                    {
                        dbModel.VehicleId = vehicalDetails.Id;
                    }

                    var startdate = Detailofpolicy.StartDate;
                    var enddate = Detailofpolicy.EndDate;
                    //if (model.DateOfLoss >= startdate && model.DateOfLoss <= enddate)
                    //{
                    dbModel.PolicyId = Detailofpolicy.Id;
                    dbModel.CreatedBy = customer.Id;
                    dbModel.CreatedOn = DateTime.Now;
                    dbModel.IsDeleted = true;
                    dbModel.IsRegistered = false;
                    InsuranceContext.ClaimNotifications.Insert(dbModel);
                    return RedirectToAction("ClaimantList");
                    //}
                    //TempData["errorMsg"] = "Date of Loss Not Exist in Policy Period";
                }
            }

            return View();
        }


        // GET: Claimant
        //[Authorize(Roles = "Staff")]
        [HttpGet]
        public ActionResult ClaimantList()
        {

            List<ClaimNotificationModel> objList = new List<ClaimNotificationModel>();

            objList = (from j in InsuranceContext.ClaimNotifications.All().ToList()
                       join jt in InsuranceContext.PolicyDetails.All().ToList() 
                       on j.PolicyNumber equals jt.PolicyNumber
                       into rd
                       from rt in rd.DefaultIfEmpty()
                       join make in InsuranceContext.VehicleMakes.All() on j.ThirdPartyMakeId equals make.MakeCode into makes
                       from m in makes.DefaultIfEmpty()
                       join model in InsuranceContext.VehicleModels.All() on j.ThirdPartyModelId equals model.ModelCode into models 
                       from mod in models.DefaultIfEmpty()
                       where j.IsDeleted == true && j.IsRegistered == false
                       select new ClaimNotificationModel
                       {

                           PolicyNumber = j.PolicyNumber,
                           DateOfLoss = j.DateOfLoss,
                           PlaceOfLoss = j.PlaceOfLoss,
                           DescriptionOfLoss = j.DescriptionOfLoss,
                           EstimatedValueOfLoss = j.EstimatedValueOfLoss,
                           ThirdPartyInvolvement = j.ThirdPartyInvolvement,
                           ClaimantName = j.ClaimantName,
                           RegistrationNo = j.RegistrationNo,
                           ThirdPartyName = j.ThirdPartyName,
                           ThirdPartyContactDetails = j.ThirdPartyContactDetails,
                           ThirdPartyMakeId =m==null?"" : m.MakeDescription,
                           ThirdPartyModelId = mod==null ? "" :  mod.ModelDescription,
                           ThirdPartyEstimatedValueOfLoss = j.ThirdPartyEstimatedValueOfLoss,
                           CustomerName = j.CustomerName,
                           Id = j.Id,
                           IsExists = rt == null ? false : true,
                       }
                     ).ToList();

            return View(objList);
        }

        [HttpGet]
        public ActionResult EditClaimant(int Id)
        {
            var record = InsuranceContext.ClaimNotifications.All(where: $"Id ={Id}").FirstOrDefault();
            var model = Mapper.Map<ClaimNotification, ClaimNotificationModel>(record);
            var service = new VehicleService();
            var makers = service.GetMakers();
            ViewBag.Makers = makers;
            if (makers.Count > 0)
            {
                var _model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = _model;
            }
            model.ThirdPartyModelId = record.ThirdPartyModelId;
            return View(model);
        }

        [HttpPost]
        public ActionResult EditClaimant(ClaimNotificationModel model)
        {
            if (ModelState.IsValid)
            {
                string userid = "";
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                if (userLoggedin)
                {
                    userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    var customer = InsuranceContext.Customers.Single(where: $"UserId ='{userid}'");
                    var data = Mapper.Map<ClaimNotificationModel, ClaimNotification>(model);

                    var record = InsuranceContext.ClaimNotifications.Single(where: $"Id = '{model.Id}'");
                    data.ModifiedOn = DateTime.Now;
                    data.IsDeleted = true;
                    data.IsRegistered = false;
                    data.ModifiedBy = Convert.ToInt32(customer.Id);
                    data.CreatedOn = Convert.ToDateTime(record.CreatedOn);
                    InsuranceContext.ClaimNotifications.Update(data);
                    return RedirectToAction("ClaimantList");
                }
            }
            return View("EditClaimant");
        }
        public ActionResult DeleteClaimant(int Id)
        {
            string query = $"update ClaimNotification set IsDeleted = 0 where Id = {Id}";
            InsuranceContext.ClaimNotifications.Execute(query);
            return RedirectToAction("ClaimantList");
        }

        public ActionResult SaveUpdatedata(string PolicyNumber, int id)
        {

            var ClaimDetail = InsuranceContext.ClaimNotifications.All().SingleOrDefault(p => p.Id == id);
            //var VehicleDetail = InsuranceContext.VehicleDetails.All().Where(p => p.RegistrationNo == ClaimDetail.RegistrationNo).ToList();
            //var vehicleId = InsuranceContext.VehicleDetails.All().SingleOrDefault(p => p.RegistrationNo == ClaimDetail.RegistrationNo);

            var query = "select VehicleDetail.* from VehicleDetail join PolicyDetail on VehicleDetail.PolicyId = PolicyDetail.Id";
            query += " where VehicleDetail.IsActive=1 and PolicyDetail.PolicyNumber = '" + PolicyNumber +"' and VehicleDetail.RegistrationNo = '" + ClaimDetail.RegistrationNo + "'";


            var VehicleDetail = InsuranceContext.Query(query).Select(x => new VehicleDetail()
            {
                Id = x.Id
            }).FirstOrDefault();


            //save in ClaimRegistration
            ClaimRegistrationModel model = new ClaimRegistrationModel();
            var claimNumber = GenerateClaimNumber();
            model.PolicyNumber = PolicyNumber;
            model.ClaimNumber = claimNumber;
            model.DateOfLoss = Convert.ToDateTime(ClaimDetail.DateOfLoss.ToShortDateString());
            model.DateOfNotifications = Convert.ToDateTime(ClaimDetail.CreatedOn.ToShortDateString());
            model.PlaceOfLoss = ClaimDetail == null ? null : ClaimDetail.PlaceOfLoss;
            model.DescriptionOfLoss = ClaimDetail == null ? null : ClaimDetail.DescriptionOfLoss;
            model.EstimatedValueOfLoss = ClaimDetail.EstimatedValueOfLoss;
            model.VehicleDetailId = VehicleDetail.Id;
            model.RegistrationNo = ClaimDetail.RegistrationNo;
            //model.ThirdPartyDamageValue = ClaimDetail.ThirdPartyInvolvement;
            model.Claimsatisfaction = true;
            model.ClaimStatus = "1";
            model.CreatedOn = DateTime.Now;
            var dbModel = Mapper.Map<ClaimRegistrationModel, ClaimRegistration>(model);
            InsuranceContext.ClaimRegistrations.Insert(dbModel);

            // save in claimNotification

            var NotificationId = ClaimDetail.Id;
            var updateNotificationRecord = InsuranceContext.ClaimNotifications.Single(NotificationId);
            updateNotificationRecord.IsRegistered = true;
            InsuranceContext.ClaimNotifications.Update(updateNotificationRecord);

            return RedirectToAction("RegisterClaim", "Claimant", new { @id = id, @PolicyNumber = PolicyNumber, @dbmodel = dbModel.Id });
        }
        public ActionResult RegisterClaim(string PolicyNumber, int id, int dbmodel)
        {
            try
            {

                ViewBag.ClaimStatus = InsuranceContext.ClaimStatuss.All().ToList();
                var service = new VehicleService();
                if (PolicyNumber != "")
                {
                    var PolicyDetail = InsuranceContext.PolicyDetails.All().FirstOrDefault(p => p.PolicyNumber == PolicyNumber);
                    var ClaimDetail = InsuranceContext.ClaimNotifications.All().SingleOrDefault(p => p.Id == id);
                    var Policyid = PolicyDetail.Id;
                    var CustomerId = PolicyDetail.CustomerId;
                    //var VehicleDetail = InsuranceContext.VehicleDetails.All().Where(p => p.PolicyId == Policyid).ToList();
                    var VehicleDetail = InsuranceContext.VehicleDetails.All().Where(p => p.RegistrationNo == ClaimDetail.RegistrationNo && p.PolicyId== Policyid).ToList();

                
                    RegisterClaimViewModel VehicleDetailVM = new RegisterClaimViewModel();

                    List<RiskViewModel> VehicleData = new List<RiskViewModel>();
                    List<ChecklistModel> ChecklistModel = new List<ChecklistModel>();

                    var Checklist = InsuranceContext.Checklists.All().ToList();

                    ChecklistModel = Checklist.Select(p => new ChecklistModel()
                    {

                        Id = p.Id,
                        ChecklistDetail = p.ChecklistDetail,
                        IsChecked = false

                    }).ToList();

                    //get only Vehicle Detail
                    VehicleData = VehicleDetail.Select(p => new RiskViewModel()
                    {
                        Make = InsuranceContext.VehicleMakes.All().Where(q => q.MakeCode == p.MakeId).FirstOrDefault().MakeDescription,
                        Model = InsuranceContext.VehicleModels.All().Where(q => q.ModelCode == p.ModelId).FirstOrDefault().ModelDescription,
                        paymentTerm = InsuranceContext.PaymentTerms.All().Where(q => q.Id == p.PaymentTermId).FirstOrDefault().Name,
                        Product = InsuranceContext.Products.All().Where(q => q.Id == p.ProductId).FirstOrDefault().ProductName,
                        VehUsage = InsuranceContext.VehicleUsages.All().Where(q => q.Id == p.VehicleUsage).FirstOrDefault().VehUsage,
                        CoverType = InsuranceContext.CoverTypes.All().Where(q => q.Id == p.CoverTypeId).FirstOrDefault().Name,
                        VehicleYear = p.VehicleYear,
                        CubicCapacity = p.CubicCapacity,
                        EngineNumber = p.EngineNumber,
                        ChasisNumber = p.ChasisNumber,
                        AddThirdPartyAmount = p.AddThirdPartyAmount,
                        Excess = p.Excess,
                        ExcessType = Convert.ToString(p.ExcessType),
                        Premium = p.Premium,
                        StampDuty = p.StampDuty,
                        ZTSCLevy = p.ZTSCLevy,
                        Discount = p.Discount,

                        VehicleLicenceFee = p.VehicleLicenceFee,
                        Addthirdparty = p.Addthirdparty == true ? "yes" : "No",
                        IncludeRadioLicenseCost = p.IncludeRadioLicenseCost == true ? "yes" : "No",
                        IsLicenseDiskNeeded = p.IsLicenseDiskNeeded == true ? "yes" : "No",
                        FirstName = InsuranceContext.Customers.All().Where(q => q.Id == p.CustomerId).FirstOrDefault().FirstName,
                        LastName = InsuranceContext.Customers.All().Where(q => q.Id == p.CustomerId).FirstOrDefault().LastName,
                        RegisterNumber = p.RegistrationNo,
                        CoverStartDate = Convert.ToString(p.CoverStartDate.Value.ToShortDateString()),
                        CoverEndDate = Convert.ToString(p.CoverEndDate.Value.ToShortDateString()),
                        SumInsured = p.SumInsured,
                        RadioLicenseCost = p.RadioLicenseCost,
                        PassengerAccidentCover = p.PassengerAccidentCover == true ? "yes" : "No",
                        ExcessBuyBack = p.ExcessBuyBack == true ? "yes" : "No",
                        RoadsideAssistance = p.RoadsideAssistance == true ? "yes" : "No",
                        MedicalExpenses = p.MedicalExpenses == true ? "yes" : "No"
                    }).ToList();


                    VehicleDetailVM = new RegisterClaimViewModel()
                    {
                        PolicyNumber = PolicyDetail.PolicyNumber,
                        DateOfLoss = ClaimDetail.DateOfLoss.ToShortDateString(),
                        PlaceOfLoss = ClaimDetail == null ? null : ClaimDetail.PlaceOfLoss,
                        DescriptionOfLoss = ClaimDetail == null ? null : ClaimDetail.DescriptionOfLoss,
                        EstimatedValueOfLoss = ClaimDetail.EstimatedValueOfLoss,
                        //ThirdPartyDamageValue = ClaimDetail.ThirdPartyInvolvement,
                        DateOfNotifications = ClaimDetail.CreatedOn.ToShortDateString(),
                        RiskViewModel = VehicleData,
                        chklist = ChecklistModel,
                        FirstName = InsuranceContext.Customers.All().Where(q => q.Id == CustomerId).FirstOrDefault().FirstName,
                        LastName = InsuranceContext.Customers.All().Where(q => q.Id == CustomerId).FirstOrDefault().LastName,

                    };
                    var claimregistrationdetail = InsuranceContext.ClaimRegistrations.Single(where: $"Id = '{dbmodel}'");

                    VehicleDetailVM.ClaimId = dbmodel;
                    VehicleDetailVM.Claimnumber = claimregistrationdetail.ClaimNumber;
                    VehicleDetailVM.Claimsatisfaction = claimregistrationdetail.Claimsatisfaction;
                    return View(VehicleDetailVM);
                }
                return View();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult ClaimDetailServiceProvider(int id, string PolicyNumber, int claimNumber)
        {
            ClaimDetailsProviderModel model = new ClaimDetailsProviderModel();
            var claimRegisterdata = InsuranceContext.ClaimRegistrations.Single(id);
            var ServiceProvidersList = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True' or IsDeleted is null ").ToList();
            var claimdetail = InsuranceContext.ClaimDetailsProviders.Single(where: $"ClaimNumber = '{claimNumber}'");
            if (claimdetail != null && claimdetail.Count() > 0)
            {
                ViewBag.AssessorsType = ServiceProvidersList.Where(w => w.ServiceProviderType == 1).ToList();
                ViewBag.ValuersType = ServiceProvidersList.Where(w => w.ServiceProviderType == 2).ToList();
                ViewBag.LawyersType = ServiceProvidersList.Where(w => w.ServiceProviderType == 3).ToList();
                ViewBag.RepairersType = ServiceProvidersList.Where(w => w.ServiceProviderType == 4).ToList();
                model.AssessorsProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == claimdetail.AssessorsProviderType).Id;
                model.ValuersProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == claimdetail.ValuersProviderType).Id;
                model.LawyersProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == claimdetail.LawyersProviderType).Id;
                model.RepairersProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == claimdetail.RepairersProviderType).Id;
                model.PolicyNumber = claimdetail.PolicyNumber;
                model.ClaimNumber = claimdetail.ClaimNumber;
                return View(model);
            }
            else
            {
                var Providertype = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True' or IsDeleted is null ").ToList();
                ViewBag.AssessorsType = Providertype.Where(w => w.ServiceProviderType == 1).ToList();
                ViewBag.ValuersType = Providertype.Where(w => w.ServiceProviderType == 2).ToList();
                ViewBag.LawyersType = Providertype.Where(w => w.ServiceProviderType == 3).ToList();
                ViewBag.RepairersType = Providertype.Where(w => w.ServiceProviderType == 4).ToList();
                model.PolicyNumber = claimRegisterdata.PolicyNumber;
                model.ClaimNumber = Convert.ToInt32(claimRegisterdata.ClaimNumber);
            }
            return View(model);
        }
        public ActionResult SaveClaimDetails(ClaimDetailsProviderModel model)
        {


            if (ModelState.IsValid)
            {
                string customId = "";
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                var claimdata = InsuranceContext.ClaimDetailsProviders.Single(where: $"PolicyNumber = '{model.PolicyNumber}' and ClaimNumber = '{model.ClaimNumber}'");
                if (claimdata == null || claimdata.Count() == 0)
                {
                    if (userLoggedin)
                    {
                        var _userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        var customer = InsuranceContext.Customers.Single(where: $"UserId = '{_userid}'");
                        customId = Convert.ToString(customer.Id);

                        var data = Mapper.Map<ClaimDetailsProviderModel, ClaimDetailsProvider>(model);
                        data.CreatedBy = Convert.ToInt32(customId);
                        data.CreatedOn = DateTime.Now;
                        data.IsActive = true;
                        InsuranceContext.ClaimDetailsProviders.Insert(data);
                    }
                    return RedirectToAction("ClaimRegistrationList");
                }

                else
                {
                    if (userLoggedin)
                    {
                        var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");
                        customId = Convert.ToString(customer.Id);

                        //var claimDetailsdata = Mapper.Map<ClaimDetailsProviderModel, ClaimDetailsProvider>(model);
                        claimdata.AssessorsProviderType = model.AssessorsProviderType;
                        claimdata.ValuersProviderType = model.ValuersProviderType;
                        claimdata.LawyersProviderType = model.LawyersProviderType;
                        claimdata.RepairersProviderType = model.RepairersProviderType;
                        claimdata.PolicyNumber = model.PolicyNumber;
                        claimdata.ClaimNumber = model.ClaimNumber;
                        claimdata.ModifiedBy = Convert.ToInt32(customId);
                        claimdata.ModifiedOn = DateTime.Now;
                        claimdata.IsActive = true;
                        InsuranceContext.ClaimDetailsProviders.Update(claimdata);
                    }
                    return RedirectToAction("ClaimRegistrationList");
                }
            }
            TempData["Errormsg"] = "Exception Occur. Please Try Again";
            return RedirectToAction("ClaimDetailServiceProvider");
        }
        public ActionResult ClaimDetailsList()
        {

            var claimList = InsuranceContext.ClaimDetailsProviders.All(where: $"IsActive = 'True' or IsActive is null").ToList();
            var ServiceProvidersList = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True' or IsDeleted is null ").ToList();
            var ClaimDetailsProviderList = new List<ClaimDetailsProviderModel>();
            if (claimList != null && claimList.Count > 0)
            {
                foreach (var item in claimList)
                {
                    ClaimDetailsProviderModel model = new ClaimDetailsProviderModel();

                    model.Assessors_Type = ServiceProvidersList.FirstOrDefault(c => c.Id == item.AssessorsProviderType).ServiceProviderName;
                    model.Valuers_Type = ServiceProvidersList.FirstOrDefault(c => c.Id == item.ValuersProviderType).ServiceProviderName;
                    model.Lawyers_Type = ServiceProvidersList.FirstOrDefault(c => c.Id == item.LawyersProviderType).ServiceProviderName;
                    model.Repairers_Type = ServiceProvidersList.FirstOrDefault(c => c.Id == item.RepairersProviderType).ServiceProviderName;
                    model.PolicyNumber = item.PolicyNumber;
                    model.ClaimNumber = item.ClaimNumber;
                    model.CreatedOn = Convert.ToDateTime(item.CreatedOn).ToShortDateString();
                    model.Id = item.Id;
                    model.CreatedBy = item.CreatedBy;

                    ClaimDetailsProviderList.Add(model);
                }
            }
            return View(ClaimDetailsProviderList.OrderByDescending(x => x.Id));
        }

        public ActionResult EditClaimDetailsProvider(int Id)
        {
            var record = InsuranceContext.ClaimDetailsProviders.Single(where: $"Id ={Id}");
            var ServiceProvidersList = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True' or IsDeleted is null ").ToList();
            var model = Mapper.Map<ClaimDetailsProvider, ClaimDetailsProviderModel>(record);
            if (model != null)
            {
                ViewBag.AssessorsType = ServiceProvidersList.Where(w => w.ServiceProviderType == 1).ToList();
                ViewBag.ValuersType = ServiceProvidersList.Where(w => w.ServiceProviderType == 2).ToList();
                ViewBag.LawyersType = ServiceProvidersList.Where(w => w.ServiceProviderType == 3).ToList();
                ViewBag.RepairersType = ServiceProvidersList.Where(w => w.ServiceProviderType == 4).ToList();
                model.AssessorsProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == model.AssessorsProviderType).Id;
                model.ValuersProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == model.ValuersProviderType).Id;
                model.LawyersProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == model.LawyersProviderType).Id;
                model.RepairersProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == model.RepairersProviderType).Id;

            }
            return View(model);
        }
        [HttpPost]
        public ActionResult EditClaimDetailsProvider(ClaimDetailsProviderModel model)
        {

            if (ModelState.IsValid)
            {
                string customId = "";
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                if (userLoggedin)
                {
                    var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");
                    customId = Convert.ToString(customer.Id);

                    var data = Mapper.Map<ClaimDetailsProviderModel, ClaimDetailsProvider>(model);
                    var record = InsuranceContext.ClaimDetailsProviders.Single(where: $"Id ={model.Id}");
                    data.ModifiedOn = DateTime.Now;
                    data.IsActive = true;
                    data.CreatedBy = record.CreatedBy;
                    data.CreatedOn = record.CreatedOn;
                    data.ModifiedBy = Convert.ToInt32(customId);
                    InsuranceContext.ClaimDetailsProviders.Update(data);
                    return RedirectToAction("ClaimDetailsList");
                }
            }
            return View("EditClaimDetailsProvider");
        }
        public ActionResult DeteteClaimDetailsProvider(int id)
        {

            string query = $"update ClaimDetailsProvider set IsActive = 0 where Id={id}";
            InsuranceContext.ClaimDetailsProviders.Execute(query);

            return RedirectToAction("ClaimDetailsList");
        }

        public long GenerateClaimNumber()
        {
            int number = 0;
            long cNumber = 0;
            var objList = InsuranceContext.ClaimRegistrations.All(orderBy: "Id desc").FirstOrDefault();
            if (objList != null)
            {
                number = Convert.ToInt32(objList.ClaimNumber);
                cNumber = Convert.ToInt64(number) + 1;
            }
            else
            {
                cNumber = 1001;
            }


            return cNumber;
        }

        [HttpPost]
        public ActionResult SaveRegisterClaim(RegisterClaimViewModel model)
        {

            if (ModelState.IsValid)
            {
                var caimNumber = model.Claimnumber;
                var claimStatis = model.Claimsatisfaction;
                var status = model.Status;
                var names = String.Join(",", model.chklist.Where(p => p.IsChecked).Select(p => p.Id));
                var updateRecord = InsuranceContext.ClaimRegistrations.Single(model.ClaimId);
                updateRecord.Checklist = names;
                if (model.PlaceOfLoss != "")
                {
                    updateRecord.PlaceOfLoss = model.PlaceOfLoss;
                }

                if (model.DescriptionOfLoss != "")
                {
                    updateRecord.DescriptionOfLoss = model.DescriptionOfLoss;
                }
                if (model.EstimatedValueOfLoss > 0)
                {
                    updateRecord.EstimatedValueOfLoss = model.EstimatedValueOfLoss;
                }

                if (model.ThirdPartyDamageValue != "")
                {
                    updateRecord.ThirdPartyDamageValue = model.ThirdPartyDamageValue;
                }
                if (model.RejectionStatus != "")
                {
                    updateRecord.RejectionStatus = model.RejectionStatus;
                }
                if (model.Status != "")
                {
                    updateRecord.ClaimStatus = Convert.ToInt32(model.Status);
                }
                updateRecord.Claimsatisfaction = claimStatis;
                InsuranceContext.ClaimRegistrations.Update(updateRecord);
                return RedirectToAction("ClaimRegistrationList");
            }
            return RedirectToAction("RegisterClaim");
        }




        public ActionResult ClaimRegistrationList()
        {
            ViewBag.servicename = InsuranceContext.ServiceProviderTypes.All();
            ViewBag.providername = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True'").ToList();
            var service = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True'").ToList();
            var claimList = InsuranceContext.ClaimDetailsProviders.All(where: $"IsActive = 'True' or IsActive is null").ToList();

            var Adjustments = InsuranceContext.ClaimAdjustments.All(where: $"IsActive = 'True'").ToList();

            var list = (from _claimRegistration in InsuranceContext.ClaimRegistrations.All().ToList()
                        join _clamdetail in InsuranceContext.ClaimDetailsProviders.All().ToList()
                        on _claimRegistration.ClaimNumber equals _clamdetail.ClaimNumber into data
                        from date in data.DefaultIfEmpty()
                        join Claimstatusdata in InsuranceContext.ClaimStatuss.All().ToList()
                        on _claimRegistration.ClaimStatus equals Claimstatusdata.Id


                        select new ClaimRegistrationModel
                        {
                            PolicyNumber = _claimRegistration.PolicyNumber,
                            PaymentDetails = _claimRegistration.PaymentDetails,
                            ClaimNumber = _claimRegistration.ClaimNumber,
                            PlaceOfLoss = _claimRegistration.PlaceOfLoss,
                            DescriptionOfLoss = _claimRegistration.DescriptionOfLoss,
                            EstimatedValueOfLoss = _claimRegistration.EstimatedValueOfLoss,
                            ThirdPartyDamageValue = _claimRegistration.ThirdPartyDamageValue,
                            AssessProviderName = GetProvider(date == null ? 0 : date.AssessorsProviderType, service),
                            LawyeProviderName = GetProvider(date == null ? 0 : date.LawyersProviderType, service),
                            ValueProviderName = GetProvider(date == null ? 0 : date.ValuersProviderType, service),
                            RepairProviderName = GetProvider(date == null ? 0 : date.RepairersProviderType, service),
                            ClaimStatus = Convert.ToString(Claimstatusdata.Status),
                            Id = _claimRegistration.Id,
                            VehicleDetailId= _claimRegistration.VehicleDetailId,
                            ClaimValue = GetClaimValue(_claimRegistration.ClaimNumber, Adjustments),


                        }).ToList();

            return View(list.OrderByDescending(x => x.Id));
        }



        public string GetClaimValue(long RClaimNumber, List<ClaimAdjustment> AdjustmentList)
        {
            string ClaimNumber = "";
            if (RClaimNumber != null && RClaimNumber != 0)
            {
                var details = AdjustmentList.FirstOrDefault(c => c.ClaimNumber == Convert.ToInt16(RClaimNumber));

                if (details != null)
                {
                    ClaimNumber = Convert.ToString(details.ClaimNumber);

                }
            }
            return ClaimNumber;
        }




        ////public ActionResult ClaimRegistrationList()
        ////{
        ////    ViewBag.servicename = InsuranceContext.ServiceProviderTypes.All();
        ////    ViewBag.providername = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True'").ToList();
        ////    var service = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True'").ToList();
        ////    var claimList = InsuranceContext.ClaimDetailsProviders.All(where: $"IsActive = 'True' or IsActive is null").ToList();

        ////    var list = (from _claimRegistration in InsuranceContext.ClaimRegistrations.All().ToList()
        ////                join _clamdetail in InsuranceContext.ClaimDetailsProviders.All().ToList()
        ////                on _claimRegistration.ClaimNumber equals _clamdetail.ClaimNumber into data
        ////                from date in data.DefaultIfEmpty()
        ////                join Claimstatusdata in InsuranceContext.ClaimStatuss.All().ToList()
        ////                on _claimRegistration.ClaimStatus equals Claimstatusdata.Id
        ////                //join claimadjust in InsuranceContext.ClaimAdjustments.All().ToList()


        ////                select new ClaimRegistrationModel
        ////                {
        ////                    PolicyNumber = _claimRegistration.PolicyNumber,
        ////                    PaymentDetails = _claimRegistration.PaymentDetails,
        ////                    ClaimNumber = _claimRegistration.ClaimNumber,
        ////                    PlaceOfLoss = _claimRegistration.PlaceOfLoss,
        ////                    DescriptionOfLoss = _claimRegistration.DescriptionOfLoss,
        ////                    EstimatedValueOfLoss = _claimRegistration.EstimatedValueOfLoss,
        ////                    ThirdPartyDamageValue = _claimRegistration.ThirdPartyDamageValue,
        ////                    AssessProviderName = GetProvider(date == null ? 0 : date.AssessorsProviderType, service),
        ////                    LawyeProviderName = GetProvider(date == null ? 0 : date.LawyersProviderType, service),
        ////                    ValueProviderName = GetProvider(date == null ? 0 : date.ValuersProviderType, service),
        ////                    RepairProviderName = GetProvider(date == null ? 0 : date.RepairersProviderType, service),
        ////                    ClaimStatus = Convert.ToString(Claimstatusdata.Status),
        ////                    Id = _claimRegistration.Id


        ////                }).ToList();

        ////    return View(list.OrderByDescending(x => x.Id));
        //}
        public string GetProvider(int providerId, List<ServiceProvider> serviceList)
        {
            string provideName = "";

            if (providerId != null && providerId != 0)
            {
                var details = serviceList.FirstOrDefault(c => c.Id == Convert.ToInt16(providerId));

                if (details != null)
                {
                    provideName = details.ServiceProviderName;
                }
            }
            return provideName;
        }
        public ActionResult UploadFile()
        {
            if (Request.Files.Count > 0)
            {
                try
                {

                    var PolicyNumber = System.Web.HttpContext.Current.Request.Params["PolicyNumber"];
                    var ClaimNumber = System.Web.HttpContext.Current.Request.Params["ClaimNumber"];
                    var servicename = System.Web.HttpContext.Current.Request.Params["ServiceProvide"];
                    var serviceprovidername = System.Web.HttpContext.Current.Request.Params["ServiceProviderName"];

                    var Title = System.Web.HttpContext.Current.Request.Params["Title"];
                    var Description = System.Web.HttpContext.Current.Request.Params["Description"];

                    //Get Guid 

                    Guid id = Guid.NewGuid();

                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {

                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = id + "." + testfiles[testfiles.Length - 1].Split('.')[1];
                        }
                        else
                        {
                            fname = id + "." + file.FileName.Split('.')[1];
                        }


                        //if folder exist : folder name : customer id eg 1,2,3 etc
                        string policyfolderpath = @Server.MapPath("~/ClaimDocument/" + PolicyNumber + "/");
                        string Claimfolderpath = @Server.MapPath("~/ClaimDocument/" + PolicyNumber + "/" + ClaimNumber + "/");

                        if (!Directory.Exists(policyfolderpath))
                        {
                            Directory.CreateDirectory(policyfolderpath);
                            Directory.CreateDirectory(Claimfolderpath);
                        }
                        else
                        {
                            if (!Directory.Exists(Claimfolderpath))
                            {
                                Directory.CreateDirectory(policyfolderpath);
                                Directory.CreateDirectory(Claimfolderpath);
                            }
                        }

                        fname = "/ClaimDocument/" + PolicyNumber + "/" + ClaimNumber + "/" + fname;
                        file.SaveAs(Server.MapPath(fname));

                        ClaimDocument doc = new ClaimDocument();
                        doc.PolicyNumber = PolicyNumber;
                        doc.Title = Title;
                        doc.Description = Description;
                        doc.CreatedBy = Convert.ToInt32(ClaimNumber);
                        doc.CreatedOn = DateTime.Now;
                        doc.FilePath = fname;
                        doc.ClaimNumber = Convert.ToInt32(ClaimNumber);
                        doc.ServiceProvider = Convert.ToInt32(servicename);
                        doc.ServiceProviderName = Convert.ToInt32(serviceprovidername);
                        InsuranceContext.ClaimDocuments.Insert(doc);
                    }
                    // Returns message that successfully uploaded  
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }

        }

        [HttpPost]
        public JsonResult GetUplodedFiles()
        {
            var PolicyNumber = System.Web.HttpContext.Current.Request.Params["PolicyNumber"];
            var ClaimNumber = System.Web.HttpContext.Current.Request.Params["ClaimNumber"];

            //string[] filePaths = Directory.GetFiles(Server.MapPath("~/Documents/" + CustomerId + "/" + PolicyNumber + "/" + vehicleId + "/"));

            var FileList = InsuranceContext.ClaimDocuments.All(where: $" PolicyNumber='{PolicyNumber}' and ClaimNumber={ClaimNumber}");
            var list = new List<InsuranceClaim.Models.ClaimDocumentModel>();
            foreach (var item in FileList)
            {
                var service = InsuranceContext.ServiceProviders.Single(where: $"Id = {item.ServiceProviderName}");
                var obj = new InsuranceClaim.Models.ClaimDocumentModel();
                obj.Title = item.Title;
                obj.Description = item.Description;
                obj.FilePath = item.FilePath;
                obj.Id = item.Id;
                obj.ServiceProviderName = service.ServiceProviderName;

                list.Add(obj);
            }

            return Json(list.OrderByDescending(x => x.Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteDocument()
        {
            try
            {
                var docid = System.Web.HttpContext.Current.Request.Params["docid"];

                var document = InsuranceContext.ClaimDocuments.Single(Convert.ToInt32(docid));

                string filelocation = document.FilePath;

                if (System.IO.File.Exists(Server.MapPath(filelocation)))
                {
                    System.IO.File.Delete(Server.MapPath(filelocation));
                }

                InsuranceContext.ClaimDocuments.Delete(document);

                return Json(true, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult getServiceProviderName(int? id)
        {
            JsonResult jsonResult = new JsonResult();

            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            var Providertype = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True' or IsDeleted is null ").ToList();
            if (id == 1)
            {
                jsonResult.Data = Providertype.Where(w => w.ServiceProviderType == 1).Select(y => new { y.Id, y.ServiceProviderName }).ToList();
            }
            if (id == 2)
            {
                jsonResult.Data = Providertype.Where(w => w.ServiceProviderType == 2).Select(y => new { y.Id, y.ServiceProviderName }).ToList();
            }
            if (id == 3)
            {
                jsonResult.Data = Providertype.Where(w => w.ServiceProviderType == 3).Select(y => new { y.Id, y.ServiceProviderName }).ToList();
            }
            if (id == 4)
            {
                jsonResult.Data = Providertype.Where(w => w.ServiceProviderType == 4).Select(y => new { y.Id, y.ServiceProviderName }).ToList();
            }
            return jsonResult;
        }
        public ActionResult ClaimSetting()
        {

            var eSettingValueTypedata = from eSettingValueType e in Enum.GetValues(typeof(eSettingValueType))
                                        select new
                                        {
                                            ID = (int)e,
                                            Name = e.ToString()
                                        };

            ViewBag.eSettingValueTypes = new SelectList(eSettingValueTypedata, "ID", "Name");

            var eSettingValuedata = from eVehicleType e in Enum.GetValues(typeof(eVehicleType))
                                    select new
                                    {
                                        ID = (int)e,
                                        Name = e.ToString()
                                    };

            ViewBag.eSettingValueType = new SelectList(eSettingValuedata, "ID", "Name");


            return View();
        }
        public ActionResult SaveSetting(ClaimSettingModel model)
        {

            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {

                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                string userid = "";
                if (userLoggedin)
                {
                    userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");
                    var dbModel = Mapper.Map<ClaimSettingModel, ClaimSetting>(model);
                    dbModel.CreatedBy = customer.Id;
                    dbModel.CreatedOn = DateTime.Now;
                    dbModel.IsActive = true;
                    InsuranceContext.ClaimSettings.Insert(dbModel);
                    return RedirectToAction("ClaimSettingList");
                }
            }
            return View("ClaimSetting");
        }
        public ActionResult ClaimSettingList()
        {
            var ClaimList = InsuranceContext.ClaimSettings.All(where: $"IsActive = 'True' or IsActive is null").OrderByDescending(x => x.Id).ToList();


            return View(ClaimList);
        }


        [HttpPost]
        public JsonResult GetAutoSuggestions()
        {

            List<ClaimNotificationModel> objList = new List<ClaimNotificationModel>();
            /*var Policylist = InsuranceContext.PolicyDetails.All().ToList();*/   ////  get data from database 
            var NotificationList = new List<ClaimNotificationModel>(); /// create list 

            var result = (from Vehicle in InsuranceContext.VehicleDetails.All().ToList()
                          join Policylist in InsuranceContext.PolicyDetails.All().ToList()
                          on Vehicle.PolicyId equals Policylist.Id
                          join customer in InsuranceContext.Customers.All()
                          on Vehicle.CustomerId equals customer.Id
                          where Vehicle.IsActive == true
                          select new ClaimNotificationModel
                          {
                              PolicyNumber = Policylist.PolicyNumber,
                              VRNNumber = Vehicle.RegistrationNo,
                              VehicleId = Vehicle.Id,
                              PolicyId = Vehicle.PolicyId,
                              CustomerName = customer.FirstName + " " + customer.LastName

                          }).ToList().OrderBy(x => x.PolicyId).Take(30);
            //NotificationList = Policylist.Select(p => new ClaimNotificationModel()  //// get data from table using loop and added in model.
            //{
            //    PolicyNumber = p.PolicyNumber

            //}).ToList();
            //var result = (from j in NotificationList
            //              select new
            //              {
            //                  j.PolicyNumber
            //              }).ToList().Take(10);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCustomername(string txtvalue)
        {
            var customerName = "";
            var policyNumber = "";
            var vrn = "";
            var policyAndRegistrationNumber = txtvalue; //Policy Number,VRN Number,Customer Name 
            var policyAndRegistrationNumberArray = policyAndRegistrationNumber.Split(',');
            if (policyAndRegistrationNumberArray.Length > 1)
            {
                policyNumber = policyAndRegistrationNumberArray[0]; //Policy Number
                vrn = policyAndRegistrationNumberArray[1];//VRN Number
            }
            else
            {
                policyNumber = policyAndRegistrationNumberArray[0];
            }

            ClaimNotificationModel model = new ClaimNotificationModel();

            var detail = InsuranceContext.PolicyDetails.Single(where: $"PolicyNumber='{policyNumber}'");
            if (detail != null)
            {
                var vehicle = InsuranceContext.VehicleDetails.Single(where: $"RegistrationNo = '{vrn}'");
                var customerdetail = InsuranceContext.Customers.Single(where: $"Id='{detail.CustomerId}'");
                customerName = customerdetail.FirstName + " " + customerdetail.LastName;

                model.CustomerName = customerName;
                model.RegistrationNo = vrn;
                model.PolicyNumber = policyNumber;

                model.CoverStartDate = vehicle.CoverStartDate;
                model.CoverEndDate = vehicle.CoverEndDate;
                //model.CoverStartDate =Convert.ToDateTime(vehicle.CoverStartDate).ToShortDateString();
                //model.CoverEndDate =Convert.ToDateTime(vehicle.CoverEndDate).ToShortDateString();

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            return Json(model, JsonRequestBehavior.AllowGet);

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
        public JsonResult GetPolicydate(string Datelose, string StartDate, string EndDate)
        {

            bool result = false;

            if(string.IsNullOrEmpty(Datelose) && string.IsNullOrEmpty(StartDate) && string.IsNullOrEmpty(StartDate))
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            if (Convert.ToDateTime(Datelose) >= (Convert.ToDateTime(StartDate)))
            {
                if (Convert.ToDateTime(EndDate) >= Convert.ToDateTime(Datelose))
                {
                    result = true;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                    result = false;
            }
            else
            {
                return Json(result, JsonRequestBehavior.AllowGet);

            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }




        public ActionResult EditRegisterClaim(string PolicyNumber, int ClaimRegisterid, int VehicleDetailId)
        {
            try
            {
                ViewBag.ClaimStatus = InsuranceContext.ClaimStatuss.All().ToList();
                var service = new VehicleService();
                if (PolicyNumber != "")
                {
                    var PolicyDetail = InsuranceContext.PolicyDetails.Single(where: $"PolicyNumber = '{PolicyNumber}'");

                    var ClaimDetail = InsuranceContext.ClaimNotifications.Single(where: $"VehicleId = '{VehicleDetailId}' and PolicyId=" + PolicyDetail.Id);


                   // var RegisterDetail = InsuranceContext.ClaimRegistrations.All().SingleOrDefault(p => p.Id == ClaimRegisterid);

                    var claimregistrationdetail = InsuranceContext.ClaimRegistrations.Single(where: $"Id = '{ClaimRegisterid}'");



                    var CustomerId = PolicyDetail.CustomerId;

                    var VehicleDetail = InsuranceContext.VehicleDetails.All(where: $"Id = '{VehicleDetailId}' and PolicyId=" + PolicyDetail.Id).ToList();


                    RegisterClaimViewModel VehicleDetailVM = new RegisterClaimViewModel();

                    List<RiskViewModel> VehicleData = new List<RiskViewModel>();
                    List<ChecklistModel> ChecklistModel = new List<ChecklistModel>();

                    var Checklist = InsuranceContext.Checklists.All().ToList();

                   
                    ChecklistModel = Checklist.Select(p => new ChecklistModel()
                    {
                        Id = p.Id,
                        ChecklistDetail = p.ChecklistDetail,
                        IsChecked = false,
                    }).ToList();


                    List<RegisterClaimViewModel> VehicleDetailList = new List<RegisterClaimViewModel>();


                    string[] getCheckList = null;

                    if (claimregistrationdetail.Checklist!=null)
                    {
                        getCheckList = claimregistrationdetail.Checklist.Split(',');
                    }

               



                    for (var i = 0; i < ChecklistModel.Count(); i++)
                    {
                        RegisterClaimViewModel VehicleVM = new RegisterClaimViewModel();

                        var chekExist = false;

                        if(getCheckList!=null)
                        {
                            chekExist = getCheckList.Contains(ChecklistModel[i].Id.ToString());
                        }
                        

                        if (chekExist)
                        {
                            VehicleVM.isChecked = true;
                            VehicleVM.CheckBoxName = ChecklistModel[i].ChecklistDetail;
                            VehicleVM.checkId = ChecklistModel[i].Id;

                        }
                        else
                        {
                            VehicleVM.isChecked = false;
                            VehicleVM.CheckBoxName = ChecklistModel[i].ChecklistDetail;
                            VehicleVM.checkId = ChecklistModel[i].Id;

                        }

                        VehicleDetailList.Add(VehicleVM);
                    }


                    //get only Vehicle Detail
                    VehicleData = VehicleDetail.Select(p => new RiskViewModel()
                    {
                        Make = InsuranceContext.VehicleMakes.All().Where(q => q.MakeCode == p.MakeId).FirstOrDefault().MakeDescription,
                        Model = InsuranceContext.VehicleModels.All().Where(q => q.ModelCode == p.ModelId).FirstOrDefault().ModelDescription,
                        paymentTerm = InsuranceContext.PaymentTerms.All().Where(q => q.Id == p.PaymentTermId).FirstOrDefault().Name,
                        Product = InsuranceContext.Products.All().Where(q => q.Id == p.ProductId).FirstOrDefault().ProductName,
                        VehUsage = InsuranceContext.VehicleUsages.All().Where(q => q.Id == p.VehicleUsage).FirstOrDefault().VehUsage,
                        CoverType = InsuranceContext.CoverTypes.All().Where(q => q.Id == p.CoverTypeId).FirstOrDefault().Name,
                        VehicleYear = p.VehicleYear,
                        CubicCapacity = p.CubicCapacity,
                        EngineNumber = p.EngineNumber,
                        ChasisNumber = p.ChasisNumber,
                        AddThirdPartyAmount = p.AddThirdPartyAmount,
                        Excess = p.Excess,
                        ExcessType = Convert.ToString(p.ExcessType),
                        Premium = p.Premium,
                        StampDuty = p.StampDuty,
                        ZTSCLevy = p.ZTSCLevy,
                        Discount = p.Discount,

                        VehicleLicenceFee = p.VehicleLicenceFee,
                        Addthirdparty = p.Addthirdparty == true ? "yes" : "No",
                        IncludeRadioLicenseCost = p.IncludeRadioLicenseCost == true ? "yes" : "No",
                        IsLicenseDiskNeeded = p.IsLicenseDiskNeeded == true ? "yes" : "No",
                        FirstName = InsuranceContext.Customers.All().Where(q => q.Id == p.CustomerId).FirstOrDefault().FirstName,
                        LastName = InsuranceContext.Customers.All().Where(q => q.Id == p.CustomerId).FirstOrDefault().LastName,
                        RegisterNumber = p.RegistrationNo,
                        CoverStartDate = Convert.ToString(p.CoverStartDate.Value.ToShortDateString()),
                        CoverEndDate = Convert.ToString(p.CoverEndDate.Value.ToShortDateString()),
                        SumInsured = p.SumInsured,
                        RadioLicenseCost = p.RadioLicenseCost,
                        PassengerAccidentCover = p.PassengerAccidentCover == true ? "yes" : "No",
                        ExcessBuyBack = p.ExcessBuyBack == true ? "yes" : "No",
                        RoadsideAssistance = p.RoadsideAssistance == true ? "yes" : "No",
                        MedicalExpenses = p.MedicalExpenses == true ? "yes" : "No"
                    }).ToList();

                    VehicleDetailVM = new RegisterClaimViewModel()
                    {
                        PolicyNumber = PolicyDetail.PolicyNumber,
                        DateOfLoss = claimregistrationdetail.DateOfLoss.ToString(),// dispaly only date
                        PlaceOfLoss = claimregistrationdetail == null ? null : claimregistrationdetail.PlaceOfLoss,
                        DescriptionOfLoss = claimregistrationdetail == null ? null : claimregistrationdetail.DescriptionOfLoss,
                        EstimatedValueOfLoss = claimregistrationdetail.EstimatedValueOfLoss,
                        DateOfNotifications = claimregistrationdetail.CreatedOn.ToString(),// dispaly only date
                        RiskViewModel = VehicleData,
                        chklistDetail = VehicleDetailList,
                        checklistvalue = claimregistrationdetail.Checklist,

                        FirstName = InsuranceContext.Customers.All().Where(q => q.Id == CustomerId).FirstOrDefault().FirstName,
                        LastName = InsuranceContext.Customers.All().Where(q => q.Id == CustomerId).FirstOrDefault().LastName,

                    };
                    

                    VehicleDetailVM.ClaimId = ClaimRegisterid;
                    VehicleDetailVM.Claimnumber = claimregistrationdetail.ClaimNumber;
                    VehicleDetailVM.Claimsatisfaction = claimregistrationdetail.Claimsatisfaction;
                    VehicleDetailVM.Id = claimregistrationdetail.Id;
                    return View(VehicleDetailVM);
                }
                return View();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool GetcheckdValue(string ChecklistDetail)
        {
            bool IsChecked = false;
            if (ChecklistDetail == "Driverslicence")
            {
                IsChecked = true;
            }
            else if (ChecklistDetail == "ClaimantCopyOfID")
            {
                IsChecked = true;
            }
            else if (ChecklistDetail == "PoliceReport")
            {
                IsChecked = true;
            }

            else if (ChecklistDetail == "ClaimForm")
            {
                IsChecked = true;
            }

            return IsChecked;
        }

        public ActionResult updateRegisterDetail(RegisterClaimViewModel model)
        {

            RemoveValidation();

            //if (ModelState.IsValid)
            //{
                var caimNumber = model.Claimnumber;
                var claimStatis = model.Claimsatisfaction;
                var status = model.Status;
                var names = String.Join(",", model.chklist.Where(p => p.IsChecked).Select(p => p.Id));
                var updateRecord = InsuranceContext.ClaimRegistrations.Single(model.ClaimId);
                updateRecord.Checklist = names;
                if (model.PlaceOfLoss != "")
                {
                    updateRecord.PlaceOfLoss = model.PlaceOfLoss;
                }

                if (model.DescriptionOfLoss != "")
                {
                    updateRecord.DescriptionOfLoss = model.DescriptionOfLoss;
                }
                if (model.EstimatedValueOfLoss > 0)
                {
                    updateRecord.EstimatedValueOfLoss = model.EstimatedValueOfLoss;
                }

                if (model.ThirdPartyDamageValue != "")
                {
                    updateRecord.ThirdPartyDamageValue = model.ThirdPartyDamageValue;
                }
                if (model.RejectionStatus != "")
                {
                    updateRecord.RejectionStatus = model.RejectionStatus;
                }
                if (model.Status != "")
                {
                    updateRecord.ClaimStatus = Convert.ToInt32(model.Status);
                }
                updateRecord.Claimsatisfaction = claimStatis;
                InsuranceContext.ClaimRegistrations.Update(updateRecord);
                return RedirectToAction("ClaimRegistrationList");
            //}
            
            return RedirectToAction("RegisterClaim");
        }

        public void RemoveValidation()
        {
            ModelState.Remove("PlaceOfLoss");

            ModelState.Remove("PlaceOfLoss");
            ModelState.Remove("DescriptionOfLoss");
            ModelState.Remove("EstimatedValueOfLoss");
            ModelState.Remove("ThirdPartyDamageValue");
        }


    }
}