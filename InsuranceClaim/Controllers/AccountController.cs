﻿using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using InsuranceClaim.Models;
using Insurance.Domain;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using AutoMapper;
using System.Configuration;
using static InsuranceClaim.Controllers.CustomerRegistrationController;
using InsuranceClaim.Controllers;
using System.IO;
using Insurance.Service;
using System.Web.Configuration;

namespace InsuranceClaim.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        string AdminEmail = WebConfigurationManager.AppSettings["AdminEmail"];
        RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
        Insurance.Service.smsService objsmsService = new Insurance.Service.smsService();

        SummaryDetailService _summaryDetailService = new SummaryDetailService();

        string _superUser = "fe19c887-f8a9-4353-939f-65e19afe0D5L";
        static string _staff = "bbbeffe0-94fa-41b7-bd8b-72d9ddc7f8f0";
        static string _Agentstaff = "bbbeffe0-94fa-41b7-bd8b-72d9ddc7HGTR";

        public AccountController()
        {

        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
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

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            Session.Abandon();
            Session.Clear();

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    var _user = UserManager.FindByEmail(model.Email);
                    var role = UserManager.GetRoles(_user.Id.ToString()).FirstOrDefault();
                    Session["LoggedInUserRole"] = role;

                    var customer = InsuranceContext.Customers.All(where: $"UserId ='{_user.Id.ToString()}'").OrderByDescending(c => c.Id).FirstOrDefault();
                    Session["firstname"] = customer.FirstName;
                    Session["lastname"] = customer.LastName;


                    if (role == "Administrator" || role == "SuperUser" || role == "Agent" || role == "AgentStaff")
                    {
                        return RedirectToAction("Dashboard", "Account");
                    }
                    else
                    {
                        return Redirect("/CustomerRegistration/index");
                    }

                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            Session.Clear();
            var AuthenticationManager = HttpContext.GetOwinContext().Authentication;
            AuthenticationManager.SignOut();

            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // UserManager.AddToRole(user.Id, "SuperAdmin");
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            ViewBag.UserDoesnotExists = 0;
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            //if (ModelState.IsValid)
            //{
            //    var user = await UserManager.FindByNameAsync(model.Email);
            //    if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
            //    {
            //        // Don't reveal that the user does not exist or is not confirmed
            //        return View("ForgotPasswordConfirmation");
            //    }

            //    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
            //    // Send an email with this link
            //    // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            //    // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
            //    // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
            //    // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            //}

            //// If we got this far, something failed, redisplay form
            //return View(model);



            if (ModelState.IsValid)
            {

                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null)
                {


                    // If user does not exist or is not confirmed.  
                    ViewBag.UserDoesnotExists = 1;
                    return View();

                }
                else
                {
                    ViewBag.UserDoesnotExists = 0;
                    //Create URL with above token
                    Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
                    string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    //var lnkHref = "<a href='" + Url.Action("ResetPassword", "Account", new { email = UserName, code = token }, "http") + "'>Reset Password</a>";


                    //HTML Template for Send email  

                    //string subject = "Your changed password";
                    //string emailTemplatePath = "/Views/Shared/EmaiTemplates/UserRegisteration.cshtml";
                    //string EmailBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(emailTemplatePath));
                    string body = "<a>Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>";
                    //var Body = EmailBody.Replace(" #PolicyNumber#", policy.PolicyNumber).Replace("#TodayDate#", DateTime.Now.ToShortDateString()).Replace("#FirstName#", customer.FirstName).Replace("#LastName#", customer.LastName).Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2).Replace("#Email#", user.Email).Replace("#change#", callbackUrl);
                    objEmailService.SendEmail(user.Email, "", "", "Account Creation", body, null);

                    //string body = "Hello " + customer.FirstName + "\nWelcome to the GENE-INSURE family, we would like to simplify your life." + "\nYour policy number is : " + policy.PolicyNumber + "\nUsername is : " + user.Email + "\nYour Password : Geneinsure@123" + "\nPlease reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>" + "\nThank you once again.";


                }

                // Don't reveal that the user does not exist or is not confirmed

            }
            return View("ForgotPasswordConfirmation");
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        public JsonResult ResendPolicy(string summaryId)
        {
            SummaryDetailService detailService = new SummaryDetailService();
            string CurrencyName = "";
            List<VehicleDetail> ListOfVehicles = new List<VehicleDetail>();
            var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summaryId}").ToList();
            foreach (var itemSummaryVehicleDetails in SummaryVehicleDetails)
            {
                var itemVehicle = InsuranceContext.VehicleDetails.Single(itemSummaryVehicleDetails.VehicleDetailsId);
                ListOfVehicles.Add(itemVehicle);
            }

            var vehicleDetials = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
            var policyDetials = InsuranceContext.PolicyDetails.Single(vehicleDetials.PolicyId);
            var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
            var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == vehicleDetials.PaymentTermId);

            var summaryDetail = InsuranceContext.SummaryDetails.Single(where: $"id='{summaryId}'");

            var customerDetails = InsuranceContext.Customers.Single(summaryDetail.CustomerId);
            var user = UserManager.FindById(customerDetails.UserID);


            string ScheduleEmailPath = "/Views/Shared/EmaiTemplates/Schedule.cshtml";
            string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];

            // string urlPath = WebConfigurationManager.AppSettings["urlPath"];


            //List<VehicleDetail> ListOfVehicles = new List<VehicleDetail>();
            string Summeryofcover = "";
            var RoadsideAssistanceAmount = 0.00m;
            var MedicalExpensesAmount = 0.00m;
            var ExcessBuyBackAmount = 0.00m;
            var PassengerAccidentCoverAmount = 0.00m;
            var ExcessAmount = 0.00m;



            foreach (var item in ListOfVehicles)
            {
                Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                VehicleModel modell = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'");

                string vehicledescription = modell.ModelDescription + " / " + make.MakeDescription;

                RoadsideAssistanceAmount = RoadsideAssistanceAmount + Convert.ToDecimal(item.RoadsideAssistanceAmount);
                MedicalExpensesAmount = MedicalExpensesAmount + Convert.ToDecimal(item.MedicalExpensesAmount);
                ExcessBuyBackAmount = ExcessBuyBackAmount + Convert.ToDecimal(item.ExcessBuyBackAmount);
                PassengerAccidentCoverAmount = PassengerAccidentCoverAmount + Convert.ToDecimal(item.PassengerAccidentCoverAmount);
                ExcessAmount = ExcessAmount + Convert.ToDecimal(item.ExcessAmount);


                var currencylist = detailService.GetAllCurrency();
                CurrencyName = detailService.GetCurrencyName(currencylist, item.CurrencyId);


                string converType = "";

                if (item.CoverTypeId == 1)
                {
                    converType = eCoverType.ThirdParty.ToString();
                }
                if (item.CoverTypeId == 2)
                {
                    converType = eCoverType.FullThirdParty.ToString();
                }

                if (item.CoverTypeId == 4)
                {
                    converType = eCoverType.Comprehensive.ToString();
                }


                var paymentTermVehicel = ePaymentTermData.FirstOrDefault(p => p.ID == item.PaymentTermId);
                string paymentTermsName = "";
                if (item.PaymentTermId == 1)
                    paymentTermsName = "Annual";
                else if (item.PaymentTermId == 4)
                    paymentTermsName = "Termly";
                else
                    paymentTermsName = paymentTermVehicel.Name + " Months";




                string policyPeriod = item.CoverStartDate.Value.ToString("dd/MM/yyyy") + " - " + item.CoverEndDate.Value.ToString("dd/MM/yyyy");
                Summeryofcover += "<tr> <td style='padding: 7px 10px; font - size:15px;'>" + item.RegistrationNo + " </td>  <td style='padding: 7px 10px; font - size:15px;'>" + vehicledescription + "</td><td style='padding: 7px 10px; font - size:15px;'>" + CurrencyName + item.SumInsured + "</td><td style='padding: 7px 10px; font - size:15px;'>" + converType + "</td><td style='padding: 7px 10px; font - size:15px;'>" + InsuranceContext.VehicleUsages.All(Convert.ToString(item.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</td><td style='padding: 7px 10px; font - size:15px;'>" + policyPeriod + "</td><td style='padding: 7px 10px; font - size:15px;'>" + paymentTermsName + "</td><td style='padding: 7px 10px; font - size:15px;'>" + CurrencyName + Convert.ToString(item.Premium + item.Discount) + "</td></tr>";
            }

            Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();

            string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(ScheduleEmailPath));
            var Bodyy = MotorBody.Replace("##PolicyNo##", policyDetials.PolicyNumber).Replace("##path##", filepath).Replace("##Cellnumber##", user.PhoneNumber).
                Replace("##FirstName##", customerDetails.FirstName).Replace("##LastName##", customerDetails.LastName).Replace("##Email##", user.Email).
                Replace("##BirthDate##", customerDetails.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customerDetails.AddressLine1).
                Replace("##Address2##", customerDetails.AddressLine2).Replace("##Renewal##", vehicleDetials.RenewalDate.Value.ToString("dd/MM/yyyy")).
                Replace("##InceptionDate##", vehicleDetials.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).
                Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (vehicleDetials.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + vehicleDetials.PaymentTermId.ToString() + "Months)")).
                Replace("##TotalPremiumDue##", Convert.ToString(summaryDetail.TotalPremium)).Replace("##StampDuty##", Convert.ToString(summaryDetail.TotalStampDuty)).
                Replace("##MotorLevy##", Convert.ToString(summaryDetail.TotalZTSCLevies)).
                Replace("##PremiumDue##", Convert.ToString(summaryDetail.TotalPremium - summaryDetail.TotalStampDuty - summaryDetail.TotalZTSCLevies - summaryDetail.TotalRadioLicenseCost + ListOfVehicles.Sum(x => x.Discount) - ListOfVehicles.Sum(x => x.VehicleLicenceFee))).
                Replace("##PostalAddress##", customerDetails.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(ExcessBuyBackAmount)).
                Replace("##MedicalExpenses##", Convert.ToString(MedicalExpensesAmount)).Replace("##PassengerAccidentCover##", Convert.ToString(PassengerAccidentCoverAmount)).
                Replace("##RoadsideAssistance##", Convert.ToString(RoadsideAssistanceAmount)).Replace("##RadioLicence##", Convert.ToString(summaryDetail.TotalRadioLicenseCost)).
                Replace("##Discount##", Convert.ToString(ListOfVehicles.Sum(x => x.Discount))).Replace("##ExcessAmount##", Convert.ToString(ExcessAmount)).
                 Replace("##currencyName##", CurrencyName)
                .Replace("##NINumber##", customerDetails.NationalIdentificationNumber).Replace("##VehicleLicenceFee##", Convert.ToString(ListOfVehicles.Sum(x => x.VehicleLicenceFee)));

            #region Invoice PDF
            var attacehmetn_File = MiscellaneousService.EmailPdf(Bodyy, policyDetials.CustomerId, policyDetials.PolicyNumber, "Policy schedule");
            #endregion

            #region Invoice EMail
            //var _yAtter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
            List<string> _attachementss = new List<string>();
            _attachementss.Add(attacehmetn_File);
            //_attachementss.Add(_yAtter);
            #endregion

            if (GetValidEmailAddress(user.Email))
            {
                objEmailService.SendEmail(LoggedUserEmail(), "", "", "Policy schedule", Bodyy, _attachementss);
            }
            else
            {
                objEmailService.SendEmail(user.Email, "", "", "Policy schedule", Bodyy, _attachementss);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }


        public async Task<JsonResult> RenewResendPolicy(string vehicleId)
        {
            try
            {
                SummaryDetailService detailService = new SummaryDetailService();
                var currencyList = detailService.GetAllCurrency();
                var vehicledetail = InsuranceContext.VehicleDetails.Single(where: $"Id = '{vehicleId}'");
                Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
                if (vehicledetail != null)
                {
                    var currencyName = detailService.GetCurrencyName(currencyList, vehicledetail.CurrencyId);
                    var customerinfo = InsuranceContext.Customers.Single(where: $"Id = '{vehicledetail.CustomerId}'");
                    var userinfo = UserManager.FindById(customerinfo.UserID);

                    var policyinfo = InsuranceContext.PolicyDetails.Single(where: $"Id = '{vehicledetail.PolicyId}'");
                    var vehiclesummry = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId = '{vehicledetail.Id}'");
                    var summaydetails = InsuranceContext.SummaryDetails.Single(where: $"Id = '{vehiclesummry.SummaryDetailId}'");
                    var paymentinfmation = InsuranceContext.PaymentInformations.Single(where: $"SummaryDetailId = '{summaydetails.Id}'");

                    string code = await UserManager.GeneratePasswordResetTokenAsync(userinfo.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = userinfo.Id, code = code }, protocol: Request.Url.Scheme);
                    string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];

                    #region 
                    //WelCome Letter
                    string emailTemplatePath = "/Views/Shared/EmaiTemplates/UserRegisteration.cshtml";
                    string EmailBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(emailTemplatePath));
                    var Body = EmailBody.Replace(" #PolicyNumber#", policyinfo.PolicyNumber).Replace("##path##", filepath).Replace("#TodayDate#", DateTime.Now.ToShortDateString()).Replace("#FirstName#", customerinfo.FirstName).Replace("#LastName#", customerinfo.LastName).Replace("#Address1#", customerinfo.AddressLine1).Replace("#Address2#", customerinfo.AddressLine2).Replace("#Email#", userinfo.Email).Replace("#change#", callbackUrl);
                    List<string> _attachements = new List<string>();
                    var attachementFile1 = MiscellaneousService.EmailPdf(Body, policyinfo.CustomerId, policyinfo.PolicyNumber, "WelCome Letter ");
                    _attachements.Add(attachementFile1);

                    if (GetValidEmailAddress(userinfo.Email)) // if customer has custom email
                    {
                        objEmailService.SendEmail(LoggedUserEmail(), "", "", "Account Creation", Body, _attachements);
                        //objEmailService.SendEmail("deepak.s@kindlebit.com", "", "", "Account Creation", Body, _attachements);
                    }
                    else
                    {
                        objEmailService.SendEmail(userinfo.Email, "", "", "Account Creation", Body, _attachements);
                        //objEmailService.SendEmail("deepak.s@kindlebit.com", "", "", "Account Creation", Body, _attachements);
                    }
                    #endregion

                    #region
                    //Reciept Letter

                    string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/Reciept.cshtml";
                    string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(userRegisterationEmailPath));
                    var Body2 = EmailBody2.Replace("#DATE#", DateTime.Now.ToShortDateString()).Replace("##path##", filepath).Replace("#currencyName#", currencyName).Replace("#FirstName#", customerinfo.FirstName).Replace("#LastName#", customerinfo.LastName).Replace("#AccountName#", customerinfo.FirstName + ", " + customerinfo.LastName).Replace("#Address1#", customerinfo.AddressLine1).Replace("#Address2#", customerinfo.AddressLine2).Replace("#Amount#", Convert.ToString(summaydetails.AmountPaid)).Replace("#PaymentDetails#", "New Premium").Replace("#ReceiptNumber#", policyinfo.PolicyNumber).Replace("#PaymentType#", (summaydetails.PaymentMethodId == 1 ? "Cash" : (summaydetails.PaymentMethodId == 2 ? "PayPal" : "PayNow")));
                    var attachementFile = MiscellaneousService.EmailPdf(Body2, policyinfo.CustomerId, policyinfo.PolicyNumber, "Invoice");
                    List<string> attachements = new List<string>();
                    attachements.Add(attachementFile);

                    if (customerinfo.IsCustomEmail) // if customer has custom email
                    {
                        objEmailService.SendEmail(LoggedUserEmail(), "", "", "Invoice", Body2, attachements);
                    }
                    else
                    {
                        objEmailService.SendEmail(userinfo.Email, "", "", "Invoice", Body2, attachements);
                        //objEmailService.SendEmail("deepak.s@kindlebit.com", "", "", "Invoice", Body2, attachements);
                    }
                    #endregion


                    #region 
                    string Summeryofcover = "";
                    var RoadsideAssistanceAmount = 0.00m;
                    var MedicalExpensesAmount = 0.00m;
                    var ExcessBuyBackAmount = 0.00m;
                    var PassengerAccidentCoverAmount = 0.00m;
                    var ExcessAmount = 0.00m;
                    var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };




                    Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                    VehicleModel model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{vehicledetail.ModelId}'");
                    VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{vehicledetail.MakeId}'");
                    string vehicledescription = model.ModelDescription + " / " + make.MakeDescription;

                    RoadsideAssistanceAmount = RoadsideAssistanceAmount + Convert.ToDecimal(vehicledetail.RoadsideAssistanceAmount);
                    MedicalExpensesAmount = MedicalExpensesAmount + Convert.ToDecimal(vehicledetail.MedicalExpensesAmount);
                    ExcessBuyBackAmount = ExcessBuyBackAmount + Convert.ToDecimal(vehicledetail.ExcessBuyBackAmount);
                    PassengerAccidentCoverAmount = PassengerAccidentCoverAmount + Convert.ToDecimal(vehicledetail.PassengerAccidentCoverAmount);
                    ExcessAmount = ExcessAmount + Convert.ToDecimal(vehicledetail.ExcessAmount);

                    var paymentTermVehicel = ePaymentTermData.FirstOrDefault(p => p.ID == vehicledetail.PaymentTermId);
                    string paymentTermsName = "";
                    if (vehicledetail.PaymentTermId == 1)
                        paymentTermsName = "Annual";
                    else if (vehicledetail.PaymentTermId == 4)
                        paymentTermsName = "Termly";
                    else
                        paymentTermsName = paymentTermVehicel.Name + " Months";





                    string policyPeriod = vehicledetail.CoverStartDate.Value.ToString("dd/MM/yyyy") + " - " + vehicledetail.CoverEndDate.Value.ToString("dd/MM/yyyy");
                    Summeryofcover += "<tr><td style='padding: 7px 10px; font - size:15px;'>" + vehicledetail.RegistrationNo + " </td> <td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + vehicledescription + "</font></td>  <td style='padding: 7px 10px; font - size:15px;'>" + vehicledetail.CoverNote + " </td> <td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + currencyName + vehicledetail.SumInsured + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + (vehicledetail.CoverTypeId == 4 ? eCoverType.Comprehensive.ToString() : eCoverType.ThirdParty.ToString()) + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + InsuranceContext.VehicleUsages.All(Convert.ToString(vehicledetail.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + policyPeriod + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>$" + paymentTermsName + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + currencyName + Convert.ToString(vehicledetail.Premium) + "</font></td></tr>";
                    var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == vehicledetail.PaymentTermId);



                    string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/SeheduleMotor.cshtml";
                    string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));

                    //var Bodyy = MotorBody.Replace("##PolicyNo##", policyinfo.PolicyNumber).Replace("##paht##", filepath).Replace("##Cellnumber##", userinfo.PhoneNumber).Replace("##FirstName##", customerinfo.FirstName).Replace("##LastName##", customerinfo.LastName).Replace("##Email##", userinfo.Email).Replace("##BirthDate##", customerinfo.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customerinfo.AddressLine1).Replace("##Address2##", customerinfo.AddressLine2).Replace("##Renewal##", vehicledetail.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", vehicledetail.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (vehicledetail.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + vehicledetail.PaymentTermId.ToString() + "Months)")).Replace("##TotalPremiumDue##", Convert.ToString(summaydetails.TotalPremium)).Replace("##StampDuty##", Convert.ToString(summaydetails.TotalStampDuty)).Replace("##MotorLevy##", Convert.ToString(summaydetails.TotalZTSCLevies)).Replace("##PremiumDue##", Convert.ToString(summaydetails.TotalPremium - summaydetails.TotalStampDuty - summaydetails.TotalZTSCLevies - summaydetails.TotalRadioLicenseCost - ListOfVehicles.Sum(x => x.VehicleLicenceFee) + ListOfVehicles.Sum(x => x.Discount))).Replace("##PostalAddress##", customerinfo.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(ExcessBuyBackAmount)).Replace("##MedicalExpenses##", Convert.ToString(MedicalExpensesAmount)).Replace("##PassengerAccidentCover##", Convert.ToString(PassengerAccidentCoverAmount)).Replace("##RoadsideAssistance##", Convert.ToString(RoadsideAssistanceAmount)).Replace("##RadioLicence##", Convert.ToString(summaydetails.TotalRadioLicenseCost)).Replace("##Discount##", Convert.ToString(ListOfVehicles.Sum(x => x.Discount))).Replace("##ExcessAmount##", Convert.ToString(ExcessAmount)).Replace("##NINumber##", customerinfo.NationalIdentificationNumber).Replace("##VehicleLicenceFee##", Convert.ToString(ListOfVehicles.Sum(x => x.VehicleLicenceFee)));




                    var Bodyy = MotorBody.Replace("##PolicyNo##", policyinfo.PolicyNumber).Replace("##currencyName##", currencyName)
                        .Replace("##paht##", filepath).Replace("##Cellnumber##", userinfo.PhoneNumber).Replace("##FirstName##", customerinfo.FirstName)
                        .Replace("##LastName##", customerinfo.LastName).Replace("##Email##", userinfo.Email).Replace("##BirthDate##", customerinfo.DateOfBirth.Value.ToString("dd/MM/yyyy"))
                        .Replace("##Address1##", customerinfo.AddressLine1).Replace("##Address2##", customerinfo.AddressLine2).Replace("##Renewal##", vehicledetail.RenewalDate.Value.ToString("dd/MM/yyyy"))
                        .Replace("##InceptionDate##", vehicledetail.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name)
                        .Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (vehicledetail.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + vehicledetail.PaymentTermId.ToString() + "Months)"))
                        .Replace("##TotalPremiumDue##", Convert.ToString(summaydetails.TotalPremium)).Replace("##StampDuty##", Convert.ToString(summaydetails.TotalStampDuty))
                        .Replace("##MotorLevy##", Convert.ToString(summaydetails.TotalZTSCLevies))
                        .Replace("##PremiumDue##", Convert.ToString(summaydetails.TotalPremium - summaydetails.TotalStampDuty - summaydetails.TotalZTSCLevies - summaydetails.TotalRadioLicenseCost - vehicledetail.VehicleLicenceFee + vehicledetail.Discount))
                        .Replace("##PostalAddress##", customerinfo.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(ExcessBuyBackAmount))
                        .Replace("##MedicalExpenses##", Convert.ToString(MedicalExpensesAmount)).Replace("##PassengerAccidentCover##", Convert.ToString(PassengerAccidentCoverAmount))
                        .Replace("##RoadsideAssistance##", Convert.ToString(RoadsideAssistanceAmount)).Replace("##RadioLicence##", Convert.ToString(summaydetails.TotalRadioLicenseCost))
                        .Replace("##Discount##", Convert.ToString(vehicledetail.Discount)).Replace("##ExcessAmount##", Convert.ToString(ExcessAmount))
                         .Replace("##currencyName##", currencyName)
                        .Replace("##NINumber##", customerinfo.NationalIdentificationNumber).Replace("##VehicleLicenceFee##", Convert.ToString(vehicledetail.VehicleLicenceFee));
                    #region Invoice PDF
                    var attacehmetnFile = MiscellaneousService.EmailPdf(Bodyy, policyinfo.CustomerId, policyinfo.PolicyNumber, "Schedule-motor");
                    var Atter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";

                    List<string> __attachements = new List<string>();
                    __attachements.Add(attacehmetnFile);

                    __attachements.Add(Atter);

                    #endregion
                    if (customerinfo.IsCustomEmail) // if customer has custom email
                    {
                        objEmailService.SendEmail(LoggedUserEmail(), "", "", "Schedule-motor", Bodyy, __attachements);
                        //objEmailService.SendEmail("deepak.s@kindlebit.com", "", "", "Account Creation", Body, _attachements);
                    }
                    else
                    {

                        objEmailService.SendEmail(userinfo.Email, "", "", "Schedule-motor", Bodyy, __attachements);
                        //objEmailService.SendEmail("deepak.s@kindlebit.com", "", "", "Schedule-motor", Bodyy, __attachements);
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }


        public bool GetValidEmailAddress(string customerEmail)
        {
            bool result = false;

            if (customerEmail.Contains("Guest-"))
            {
                result = true;
            }
            return result;
        }


        public string LoggedUserEmail()
        {
            string email = "";
            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            if (_userLoggedin)
            {
                var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                email = _User.Email;
                //email = "deepak.s@kindlebit.com";
            }
            return email;

        }


        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion


        public ActionResult RoleManagement(string id = "0")
        {

            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            if (userLoggedin)
            {
                var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                var roles = UserManager.GetRoles(userid).FirstOrDefault();
                //if (roles != "SuperAdmin")
                //{
                // return RedirectToAction("Index", "CustomerRegistration");
                //}
            }
            else
            {
                return RedirectToAction("Index", "CustomerRegistration");
            }


            if (id != "0")
            {
                var test = roleManager.Roles.Where(x => x.Id == id).FirstOrDefault();

                RoleViewModel rolemodel = new RoleViewModel();
                rolemodel.Id = test.Id;
                rolemodel.RoleName = test.Name;

                return View(rolemodel);
            }
            else
            {
                return View(new RoleViewModel());
            }
        }
        public ActionResult Summary()
        {
            ViewBag.data = InsuranceContext.SummaryDetails.All().ToList().OrderByDescending(x => x.Id).Take(50);
            return View();
        }

        public JsonResult UpdateSummary(int? SummartId, int? Amount)
        {
            var output = 0;
            var summarydetail = InsuranceContext.SummaryDetails.All(where: $"Id = {SummartId}").FirstOrDefault();
            if (summarydetail != null)
            {
                summarydetail.TotalPremium = Amount;
                InsuranceContext.SummaryDetails.Update(summarydetail);
                output = 1;
                return Json(output, JsonRequestBehavior.AllowGet);
            }

            return Json(output, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddRole(RoleViewModel model)
        {
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            if (userLoggedin)
            {
                var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                var roles = UserManager.GetRoles(userid).FirstOrDefault();
                //if (roles != "SuperAdmin")
                //{
                //    return RedirectToAction("Index", "CustomerRegistration");
                //}
            }
            else
            {
                return RedirectToAction("Index", "CustomerRegistration");
            }


            if (model.Id == "0")
            {
                if (roleManager.RoleExists(model.RoleName))
                {
                    //alert user that role already exist
                }
                else
                {
                    var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                    role.Name = model.RoleName;
                    roleManager.Create(role);
                }
            }
            else
            {
                var test = roleManager.Roles.Where(x => x.Id == model.Id).FirstOrDefault();

                test.Name = model.RoleName;
                roleManager.Update(test);
            }



            return RedirectToAction("RoleManagementList");
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult RoleManagementList()
        {
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            if (userLoggedin)
            {
                var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                var role = UserManager.GetRoles(userid).FirstOrDefault();
                //if (role != "SuperAdmin")
                //{
                //    return RedirectToAction("Index", "CustomerRegistration");
                //}
            }
            else
            {
                return RedirectToAction("Index", "CustomerRegistration");
            }


            List<IdentityRole> roles = roleManager.Roles.ToList();

            InsuranceClaim.Models.RoleManagementListViewModel _roles = new RoleManagementListViewModel();

            _roles.RoleList = roles;

            return View(_roles);
        }


        public ActionResult DeleteRoleManagement(RoleViewModel model)
        {
            //Task<IdentityResult> RemoveFromRolesAsync(Id, params string[] roles);
            var test = roleManager.Roles.Where(x => x.Id == model.Id).FirstOrDefault();
            test.Name = model.RoleName;
            roleManager.Delete(test);
            return RedirectToAction("RoleManagementList");
        }


        public ActionResult UserManagement(int id = 0, string Claim = "")
        {
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            string path = Server.MapPath("~/Content/Countries.txt");
            var _countries = System.IO.File.ReadAllText(path);
            var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(_countries);
            ViewBag.Countries = resultt.countries;


            //string paths = Server.MapPath("~/Content/Cities.txt");
            //var _cities = System.IO.File.ReadAllText(paths);
            //var resultts = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObjects>(_cities);
            //ViewBag.Cities = resultts.cities;

            ViewBag.Cities = InsuranceContext.Cities.All();
            ViewBag.Branches = InsuranceContext.Branches.All();



            if (userLoggedin)
            {
                var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                // var role = UserManager.GetRoles(userid).FirstOrDefault();
                //if (role != "SuperAdmin")
                //{
                //    return RedirectToAction("Index", "CustomerRegistration");
                //}
            }
            else
            {
                return RedirectToAction("Index", "CustomerRegistration");
            }

            CustomerModel obj = new CustomerModel();
            List<IdentityRole> roles = roleManager.Roles.Where(c => c.Id != _superUser).ToList();

            if (Claim != "")
            {
                roles = roles.Where(c => c.Id == "4e19c887-f8a9-4353-939f-65e19afe0D2h").ToList();
            }

            InsuranceClaim.Models.RoleManagementListViewModel _roles = new RoleManagementListViewModel();

            _roles.RoleList = roles;
            ViewBag.Adduser = _roles.RoleList;



            if (id != 0)
            {
                var data = InsuranceContext.Customers.Single(id);
                var branchs = InsuranceContext.Branches.Single(data.BranchId) == null ? "" : InsuranceContext.Branches.Single(data.BranchId).BranchName;
                var user = UserManager.FindById(data.UserID);
                var email = user.Email;
                var phone = user.PhoneNumber;
                var role = UserManager.GetRoles(data.UserID).FirstOrDefault();




                obj.FirstName = data.FirstName;
                obj.LastName = data.LastName;
                obj.AddressLine1 = data.AddressLine1;
                obj.AddressLine2 = data.AddressLine2;
                obj.City = data.City;
                obj.Branch = Convert.ToString(data.BranchId);
                obj.CountryCode = data.Countrycode;
                obj.Gender = data.Gender;
                obj.Id = data.Id;
                obj.DateOfBirth = data.DateOfBirth;
                obj.NationalIdentificationNumber = data.NationalIdentificationNumber;
                obj.Zipcode = data.Zipcode;
                obj.role = role;
                obj.PhoneNumber = Convert.ToString(phone);
                obj.EmailAddress = Convert.ToString(email);

            }
            return View(obj);


        }

        [HttpPost]
        public async Task<ActionResult> AddUserManagement(UserManagementViewModel model)
        {

            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            if (userLoggedin)
            {
                var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                // var userid = model.UserID;
                var roles = UserManager.GetRoles(userid).FirstOrDefault();
                //if (roles != "SuperAdmin")
                //{
                //    return RedirectToAction("Index", "CustomerRegistration");
                //}
            }
            else
            {
                return RedirectToAction("Index", "CustomerRegistration");
            }



            if (model.Id == 0)
            {

                try
                {
                    decimal custId = 0.00m;
                    var user = new ApplicationUser { UserName = model.EmailAddress, Email = model.EmailAddress, PhoneNumber = model.PhoneNumber };
                    var result = await UserManager.CreateAsync(user, "Geninsure@123");
                    if (result.Succeeded)
                    {
                        var currentUser = UserManager.FindByName(user.UserName);
                        var roleresult = UserManager.AddToRole(currentUser.Id, model.role);

                        var objCustomer = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).FirstOrDefault();
                        if (objCustomer != null)
                        {
                            custId = objCustomer.CustomerId + 1;
                        }
                        else
                        {
                            custId = Convert.ToDecimal(ConfigurationManager.AppSettings["CustomerId"]);
                        }


                        model.UserID = user.Id;
                        model.CustomerId = custId;

                        Customer cstmr = new Customer();
                        cstmr.Id = model.Id;
                        cstmr.CustomerId = model.CustomerId;
                        cstmr.AddressLine1 = model.AddressLine1;
                        cstmr.AddressLine2 = model.AddressLine2;
                        cstmr.City = model.City;
                        cstmr.BranchId = Convert.ToInt32(model.Branch);
                        cstmr.Countrycode = model.CountryCode;
                        cstmr.DateOfBirth = model.DateOfBirth;
                        cstmr.FirstName = model.FirstName;
                        cstmr.LastName = model.LastName;
                        cstmr.NationalIdentificationNumber = model.NationalIdentificationNumber;
                        cstmr.Zipcode = model.Zipcode;
                        cstmr.Gender = model.Gender;
                        cstmr.Country = model.Country;
                        cstmr.IsActive = model.IsActive;
                        cstmr.IsLicenseDiskNeeded = model.IsLicenseDiskNeeded;
                        cstmr.IsOTPConfirmed = model.IsOTPConfirmed;
                        cstmr.IsPolicyDocSent = model.IsPolicyDocSent;
                        cstmr.IsWelcomeNoteSent = model.IsWelcomeNoteSent;
                        cstmr.UserID = user.Id;
                        cstmr.PhoneNumber = model.PhoneNumber;
                        InsuranceContext.Customers.Insert(cstmr);

                    }
                }
                catch (Exception ex)
                {
                }

            }
            else
            {


                Customer ctems = InsuranceContext.Customers.Single(model.Id);
                var user = UserManager.FindById(ctems.UserID);
                var role = UserManager.GetRoles(ctems.UserID).FirstOrDefault();
                user.PhoneNumber = model.PhoneNumber;

                if (role == null)
                {
                    UserManager.AddToRole(user.Id, model.role);
                }
                else if (role != model.role)
                {
                    UserManager.RemoveFromRole(user.Id, role);
                    UserManager.AddToRole(user.Id, model.role);
                    //update role                    
                }


                ctems.CustomerId = model.CustomerId;
                ctems.Id = ctems.Id;
                ctems.AddressLine1 = model.AddressLine1;
                ctems.AddressLine2 = model.AddressLine2;
                ctems.City = model.City;
                ctems.BranchId = Convert.ToInt32(model.Branch);
                ctems.Countrycode = model.CountryCode;
                ctems.DateOfBirth = model.DateOfBirth;
                ctems.FirstName = model.FirstName;
                ctems.LastName = model.LastName;
                ctems.NationalIdentificationNumber = model.NationalIdentificationNumber;
                ctems.Zipcode = model.Zipcode;
                ctems.Gender = model.Gender;
                ctems.Country = model.Country;
                ctems.IsActive = model.IsActive;
                ctems.IsLicenseDiskNeeded = model.IsLicenseDiskNeeded;
                ctems.IsOTPConfirmed = model.IsOTPConfirmed;
                ctems.IsPolicyDocSent = model.IsPolicyDocSent;
                ctems.IsWelcomeNoteSent = model.IsWelcomeNoteSent;
                ctems.PhoneNumber = model.PhoneNumber;

                InsuranceContext.Customers.Update(ctems);
                UserManager.Update(user);

            }


            return RedirectToAction("UserManagementList");
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult UserManagementList()
        {

            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            if (userLoggedin)
            {
                var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                var roles = UserManager.GetRoles(userid).FirstOrDefault();
                //if (roles != "SuperAdmin")
                //{
                //    return RedirectToAction("Index", "CustomerRegistration");
                //}
            }
            else
            {
                return RedirectToAction("Index", "CustomerRegistration");
            }



            List<CustomerModel> ListUserViewModel = new List<CustomerModel>();
            var user = InsuranceContext.Customers.All(where: "IsActive = 'True' or IsActive is null").OrderByDescending(x => x.Id).ToList().Take(50);

            var branchList = InsuranceContext.Branches.All();


            foreach (var item in user)
            {
                CustomerModel cstmrModel = new CustomerModel();
                cstmrModel.Id = item.Id;
                cstmrModel.UserID = item.UserID;
                cstmrModel.CustomerId = item.CustomerId;
                cstmrModel.FirstName = item.FirstName;
                cstmrModel.LastName = item.LastName;
                cstmrModel.Gender = item.Gender;
                cstmrModel.DateOfBirth = item.DateOfBirth;
                cstmrModel.CountryCode = item.Countrycode;
                cstmrModel.City = item.City;
                cstmrModel.Country = item.Country;
                cstmrModel.IsActive = item.IsActive;
                cstmrModel.IsLicenseDiskNeeded = item.IsLicenseDiskNeeded;
                cstmrModel.IsPolicyDocSent = item.IsPolicyDocSent;
                cstmrModel.AddressLine1 = item.AddressLine1;
                cstmrModel.AddressLine1 = item.AddressLine2;
                cstmrModel.NationalIdentificationNumber = item.NationalIdentificationNumber;
                cstmrModel.IsOTPConfirmed = item.IsOTPConfirmed;
                cstmrModel.IsWelcomeNoteSent = item.IsWelcomeNoteSent;

                cstmrModel.PhoneNumber = UserManager.GetPhoneNumber(item.UserID);
                cstmrModel.EmailAddress = UserManager.GetEmail(item.UserID);
                cstmrModel.role = Convert.ToString(UserManager.GetRoles(item.UserID).Count > 0 ? Convert.ToString(UserManager.GetRoles(item.UserID)[0]) : "");

                cstmrModel.Branch = branchList.FirstOrDefault(c => c.Id == item.BranchId) == null ? "" : branchList.FirstOrDefault(c => c.Id == item.BranchId).BranchName;

                ListUserViewModel.Add(cstmrModel);
            }

            ListUserViewModel lstUserModel = new ListUserViewModel();
            lstUserModel.ListUsers = ListUserViewModel;

            return View(lstUserModel);


        }

        public ActionResult SearchUserManagementList(string searchText)
        {
            ListUserViewModel lstUserModel = new ListUserViewModel();
            lstUserModel.ListUsers = new List<CustomerModel>();
            List<CustomerModel> ListUserViewModel = new List<CustomerModel>();
            //ListPolicy policylist = new ListPolicy();
            //policylist.listpolicy = new List<PolicyListViewModel>();

            var branchList = InsuranceContext.Branches.All();
            if (searchText != null && searchText != "")
            {
                var custom = searchText.Split(' ');
                var customers = new List<Customer>();

                if (custom.Length == 2)
                {
                    var searchtext1 = Convert.ToString(custom[0]);
                    var searchtext2 = Convert.ToString(custom[1]);

                    customers = InsuranceContext.Customers.All(where: $"FirstName like '%{searchtext1}%' and LastName like '%{searchtext2}%' ").ToList();
                }
                if (custom.Length == 1)
                {
                    customers = InsuranceContext.Customers.All(where: $"FirstName like '%{searchText}%' or LastName like '%{searchText}%' ").ToList();
                }

                if (customers != null && customers.Count > 0)
                {

                    foreach (var item in customers)
                    {
                        CustomerModel cstmrModel = new CustomerModel();
                        cstmrModel.Id = item.Id;
                        cstmrModel.UserID = item.UserID;
                        cstmrModel.CustomerId = item.CustomerId;
                        cstmrModel.FirstName = item.FirstName;
                        cstmrModel.LastName = item.LastName;
                        cstmrModel.Gender = item.Gender;
                        cstmrModel.DateOfBirth = item.DateOfBirth;
                        cstmrModel.CountryCode = item.Countrycode;
                        cstmrModel.City = item.City;
                        cstmrModel.Country = item.Country;
                        cstmrModel.IsActive = item.IsActive;
                        cstmrModel.IsLicenseDiskNeeded = item.IsLicenseDiskNeeded;
                        cstmrModel.IsPolicyDocSent = item.IsPolicyDocSent;
                        cstmrModel.AddressLine1 = item.AddressLine1;
                        cstmrModel.AddressLine1 = item.AddressLine2;
                        cstmrModel.NationalIdentificationNumber = item.NationalIdentificationNumber;
                        cstmrModel.IsOTPConfirmed = item.IsOTPConfirmed;
                        cstmrModel.IsWelcomeNoteSent = item.IsWelcomeNoteSent;

                        cstmrModel.PhoneNumber = UserManager.GetPhoneNumber(item.UserID);
                        cstmrModel.EmailAddress = UserManager.GetEmail(item.UserID);
                        cstmrModel.role = Convert.ToString(UserManager.GetRoles(item.UserID).Count > 0 ? Convert.ToString(UserManager.GetRoles(item.UserID)[0]) : "");


                        cstmrModel.Branch = branchList.FirstOrDefault(c => c.Id == item.BranchId) == null ? "" : branchList.FirstOrDefault(c => c.Id == item.BranchId).BranchName;

                        ListUserViewModel.Add(cstmrModel);

                    }
                    lstUserModel.ListUsers = ListUserViewModel;
                    return View("UserManagementList", lstUserModel);
                }
                else
                {
                    var user = UserManager.Users.Where(m => m.Email.Contains(searchText)).ToList();
                    if (user != null)
                    {
                        foreach (var item in user)
                        {
                            var customer = InsuranceContext.Customers.Single(where: $"UserId = '{item.Id}'");
                            if (customer != null)
                            {

                                CustomerModel cstmrModel = new CustomerModel();
                                cstmrModel.Id = customer.Id;
                                cstmrModel.UserID = customer.UserID;
                                cstmrModel.CustomerId = customer.CustomerId;
                                cstmrModel.FirstName = customer.FirstName;
                                cstmrModel.LastName = customer.LastName;
                                cstmrModel.Gender = customer.Gender;
                                cstmrModel.DateOfBirth = customer.DateOfBirth;
                                cstmrModel.CountryCode = customer.Countrycode;
                                cstmrModel.City = customer.City;
                                cstmrModel.Country = customer.Country;
                                cstmrModel.IsActive = customer.IsActive;
                                cstmrModel.IsLicenseDiskNeeded = customer.IsLicenseDiskNeeded;
                                cstmrModel.IsPolicyDocSent = customer.IsPolicyDocSent;
                                cstmrModel.AddressLine1 = customer.AddressLine1;
                                cstmrModel.AddressLine1 = customer.AddressLine2;
                                cstmrModel.NationalIdentificationNumber = customer.NationalIdentificationNumber;
                                cstmrModel.IsOTPConfirmed = customer.IsOTPConfirmed;
                                cstmrModel.IsWelcomeNoteSent = customer.IsWelcomeNoteSent;

                                cstmrModel.PhoneNumber = (item.PhoneNumber);
                                cstmrModel.EmailAddress = item.Email;
                                cstmrModel.role = Convert.ToString(UserManager.GetRoles(item.Id).Count > 0 ? Convert.ToString(UserManager.GetRoles(item.Id)[0]) : "");


                                cstmrModel.Branch = branchList.FirstOrDefault(c => c.Id == customer.BranchId) == null ? "" : branchList.FirstOrDefault(c => c.Id == customer.BranchId).BranchName;

                                ListUserViewModel.Add(cstmrModel);
                            }
                        }
                        lstUserModel.ListUsers = ListUserViewModel;
                        return View("UserManagementList", lstUserModel);
                    }
                }
            }
            return View("UserManagementList", lstUserModel);
        }

        public ActionResult DeleteUserManagement(int id)
        {
            var data = InsuranceContext.Customers.Single(id);

            var userid = data.UserID;
            data.IsActive = false;
            InsuranceContext.Customers.Update(data);
            // InsuranceContext.Customers.Delete(data);

            //  var currentUser = UserManager.FindById(userid);
            //  UserManager.Delete(currentUser);

            return RedirectToAction("UserManagementList");
        }

        [HttpPost]
        public ActionResult ActiveDeactive(string id, string flag)
        {
            var data = InsuranceContext.Customers.Single(id);
            var userid = data.UserID;
            data.IsActive = Convert.ToBoolean(flag);
            InsuranceContext.Customers.Update(data);
            // InsuranceContext.Customers.Delete(data);

            var currentUser = UserManager.FindById(userid);
            //  UserManager.Delete(currentUser);

            return RedirectToAction("UserManagementList");
        }


        [Authorize(Roles = "Staff,Administrator,Renewals, Agent, AgentStaff")]
        public ActionResult PolicyList()
        {

            ClearRenewSession();

            TempData["RedirectedFrom"] = "PolicyList";
            Session["ViewlistVehicles"] = null;
            ListPolicy policylist = new ListPolicy();
            policylist.listpolicy = new List<PolicyListViewModel>();
            //   var SummaryList = InsuranceContext.SummaryDetails.All().OrderByDescending(x => x.Id).ToList(); // commented 

            //      var SummaryList = InsuranceContext.SummaryDetails.All().OrderByDescending(x => x.Id).ToList();

            var SummaryList = new List<SummaryDetail>();
            var currenyList = _summaryDetailService.GetAllCurrency();
            var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
            var role = UserManager.GetRoles(_User.Id.ToString()).FirstOrDefault();

            var customerID = InsuranceContext.Customers.Single(where: $"userid='{User.Identity.GetUserId().ToString()}'").Id;
            if (role == "Staff")
            {
                //  SummaryList = InsuranceContext.SummaryDetails.All(where: $"CreatedBy={customerID} and isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();
                var query = " select SummaryDetail.* from SummaryDetail  join Customer on SummaryDetail.CreatedBy = Customer.Id ";
                query += "    join AspNetUsers on Customer.UserID = AspNetUsers.Id ";
                query += " join AspNetUserRoles on Customer.UserID = AspNetUserRoles.UserId where RoleId = '" + _staff + "'  order by Id desc ";

                SummaryList = InsuranceContext.Query(query).Select(x => new SummaryDetail()
                {
                    TotalPremium = x.TotalPremium,
                    TotalSumInsured = x.TotalSumInsured,
                    PaymentMethodId = x.PaymentMethodId,
                    CustomerId = x.CustomerId,
                    Id = x.Id,
                    CreatedOn = x.CreatedOn
                }).ToList();
            }
            else if (role == "AgentStaff")
            {
                var query = " select SummaryDetail.* from SummaryDetail  join Customer on SummaryDetail.CreatedBy = Customer.Id ";
                query += "    join AspNetUsers on Customer.UserID = AspNetUsers.Id ";
                query += " join AspNetUserRoles on Customer.UserID = AspNetUserRoles.UserId where RoleId = '" + _Agentstaff + "'  order by Id desc ";

                SummaryList = InsuranceContext.Query(query).Select(x => new SummaryDetail()
                {
                    TotalPremium = x.TotalPremium,
                    TotalSumInsured = x.TotalSumInsured,
                    PaymentMethodId = x.PaymentMethodId,
                    CustomerId = x.CustomerId,
                    Id = x.Id,
                    CreatedOn = x.CreatedOn
                }).ToList();
            }

            else if (role == "Agent")
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"AgentId=" + customerID + " and isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();
            }
            else if (role == "Renewals")
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();
            }
            else if (role == "Administrator")
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();
            }
            else
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"customerid={customerID} and isQuotation = '0'").OrderByDescending(x => x.Id).ToList();
            }

            var vehicelList = InsuranceContext.VehicleDetails.All();
            var policyList = InsuranceContext.PolicyDetails.All();
            var summaryVehicelList = InsuranceContext.SummaryVehicleDetails.All();
            var paymentlList = InsuranceContext.PaymentInformations.All();
            var reInsuranceList = InsuranceContext.ReinsuranceTransactions.All();
            var customerList = InsuranceContext.Customers.All();
            var paymentMethodList = InsuranceContext.PaymentMethods.All();
            var makeList = InsuranceContext.VehicleMakes.All();
            var modelList = InsuranceContext.VehicleModels.All();


            foreach (var item in SummaryList.Take(50))
            {
                PolicyListViewModel policylistviewmodel = new PolicyListViewModel();

                var paymentDetails = paymentlList.FirstOrDefault(c => c.SummaryDetailId == item.Id);

                if (paymentDetails == null)
                {
                    continue;
                }

                policylistviewmodel.Vehicles = new List<VehicleReinsuranceViewModel>();
                policylistviewmodel.TotalPremium = Convert.ToDecimal(item.TotalPremium);
                policylistviewmodel.TotalSumInsured = Convert.ToDecimal(item.TotalSumInsured);
                policylistviewmodel.PaymentMethodId = Convert.ToInt32(item.PaymentMethodId);
                policylistviewmodel.CustomerId = Convert.ToInt32(item.CustomerId);
                policylistviewmodel.SummaryId = item.Id;
                policylistviewmodel.createdOn = Convert.ToDateTime(item.CreatedOn);

                var paymentMethodDetails = paymentMethodList.FirstOrDefault(c => c.Id == item.PaymentMethodId);
                policylistviewmodel.PaymentMethod = paymentMethodDetails == null ? "" : paymentMethodDetails.Name;

                // var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={item.Id}").ToList();

                var SummaryVehicleDetails = summaryVehicelList.Where(c => c.SummaryDetailId == item.Id).ToList();


                if (SummaryVehicleDetails != null && SummaryVehicleDetails.Count > 0)
                {
                    //  var vehicle = InsuranceContext.VehicleDetails.Single(where: $" Id='{SummaryVehicleDetails[0].VehicleDetailsId}'");

                    var vehicle = vehicelList.FirstOrDefault(c => c.Id == SummaryVehicleDetails[0].VehicleDetailsId);

                    if (vehicle != null)
                    {

                        var customerDetails = customerList.FirstOrDefault(c => c.Id == vehicle.CustomerId);
                        policylistviewmodel.CustomerName = customerDetails == null ? "" : customerDetails.FirstName + " " + customerDetails.LastName;

                        // var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
                        // var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));

                        var policy = policyList.FirstOrDefault(c => c.Id == vehicle.PolicyId);

                        policylistviewmodel.PolicyNumber = policy.PolicyNumber;

                        foreach (var _item in SummaryVehicleDetails)
                        {
                            VehicleReinsuranceViewModel obj = new VehicleReinsuranceViewModel();
                            //  var _vehicle = InsuranceContext.VehicleDetails.Single(where: $" Id='{_item.VehicleDetailsId}'");

                            var _vehicle = vehicelList.FirstOrDefault(c => c.Id == _item.VehicleDetailsId);

                            if (_vehicle != null)
                            {
                                //   var _reinsurenaceTrans = InsuranceContext.ReinsuranceTransactions.All(where: $"SummaryDetailId={item.Id} and VehicleId={_item.VehicleDetailsId}").ToList();

                                var _reinsurenaceTrans = reInsuranceList.Where(c => c.SummaryDetailId == item.Id && c.VehicleId == item.VehicleDetailId).ToList();

                                obj.RegistrationNo = _vehicle.RegistrationNo;



                                obj.CoverType = Convert.ToInt32(_vehicle.CoverTypeId);
                                obj.isReinsurance = (_vehicle.SumInsured > 100000 ? true : false);
                                obj.MakeId = _vehicle.MakeId;
                                obj.ModelId = _vehicle.ModelId;

                                var makeDetails = makeList.FirstOrDefault(c => c.MakeCode == _vehicle.MakeId);
                                var modelDetails = modelList.FirstOrDefault(c => c.ModelCode == _vehicle.ModelId);

                                obj.Make = makeDetails == null ? "" : makeDetails.MakeDescription;
                                obj.Model = modelDetails == null ? "" : modelDetails.ModelDescription;
                                //obj.Premium = Convert.ToDecimal(_vehicle.Premium);
                                obj.RegisterationNumber = _vehicle.RegistrationNo;
                                obj.SumInsured = Convert.ToDecimal(_vehicle.SumInsured);
                                obj.VehicleId = _vehicle.Id;
                                obj.startdate = Convert.ToDateTime(_vehicle.CoverStartDate);
                                obj.enddate = Convert.ToDateTime(_vehicle.CoverEndDate);
                                obj.RenewalDate = Convert.ToDateTime(_vehicle.RenewalDate);
                                obj.isLapsed = _vehicle.isLapsed;
                                obj.isActive = Convert.ToBoolean(_vehicle.IsActive);
                                obj.currency = _summaryDetailService.GetCurrencyName(currenyList, _vehicle.CurrencyId);
                                if (_reinsurenaceTrans != null && _reinsurenaceTrans.Count > 0)
                                {
                                    obj.BrokerCommission = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceCommission);
                                    obj.AutoFacPremium = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsurancePremium);
                                    obj.AutoFacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceAmount);

                                    if (_reinsurenaceTrans.Count > 1)
                                    {
                                        obj.FacultativeCommission = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsuranceCommission);
                                        obj.FacPremium = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsurancePremium);
                                        obj.FacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceAmount);
                                    }
                                }
                                policylistviewmodel.Vehicles.Add(obj);
                            }
                        }
                    }
                }
                policylist.listpolicy.Add(policylistviewmodel);
            }
            return View(policylist);
        }



        //[Authorize(Roles = "Staff,Administrator,Renewals")]
        //public ActionResult PolicyList()
        //{

        //    ClearRenewSession();


        //    TempData["RedirectedFrom"] = "PolicyList";
        //    Session["ViewlistVehicles"] = null;
        //    ListPolicy policylist = new ListPolicy();
        //    policylist.listpolicy = new List<PolicyListViewModel>();

        //    PolicyListViewModel policylistviewmodel = new PolicyListViewModel();

        //    //   var SummaryList = InsuranceContext.SummaryDetails.All().OrderByDescending(x => x.Id).ToList(); // commented 

        //    //      var SummaryList = InsuranceContext.SummaryDetails.All().OrderByDescending(x => x.Id).ToList();

        //    //  var SummaryList = new List<SummaryDetail>();
        //    //  var currenyList = _summaryDetailService.GetAllCurrency();
        //    //   var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
        //    //  var role = UserManager.GetRoles(_User.Id.ToString()).FirstOrDefault();

        //    //  var customerID = InsuranceContext.Customers.Single(where: $"userid='{User.Identity.GetUserId().ToString()}'").Id;

        //    var SummaryList = new List<PolicyListViewModel>();
        //    var reInsuranceList = InsuranceContext.ReinsuranceTransactions.All();

        //    policylistviewmodel.Vehicles = new List<VehicleReinsuranceViewModel>();


        //    var query = "select PolicyDetail.PolicyNumber, Customer.FirstName + ' ' +Customer.LastName as CustomerName, PaymentMethod.Name as PaymentMethod, ";
        //    query += " SummaryDetail.TotalSumInsured, SummaryDetail.TotalPremium, SummaryDetail.CreatedOn, ";
        //    query += " VehicleDetail.RegistrationNo, VehicleMake.MakeDescription + '/'+ VehicleModel.ModelDescription as Vehicle, Currency.Name as CurrencyName, VehicleDetail.CoverStartDate, VehicleDetail.CoverEndDate,VehicleDetail.RenewalDate, VehicleDetail.isLapsed, ";
        //    query += " VehicleDetail.SumInsured as VehicleDetail_SumInsured, VehicleDetail.id as VehicleDetailid, VehicleDetail.IsActive, SummaryDetail.id as SummaryDetailId ";
        //    query += " from VehicleDetail join PolicyDetail on VehicleDetail.PolicyId= PolicyDetail.id ";
        //    query += " join SummaryVehicleDetail on VehicleDetail.id =SummaryVehicleDetail.VehicleDetailsId ";
        //    query += " join SummaryDetail on SummaryVehicleDetail.SummaryDetailId=SummaryDetail.id ";
        //    query += " join Customer on VehicleDetail.CustomerId=Customer.id ";
        //    query += " join VehicleMake on VehicleDetail.MakeId=VehicleMake.MakeCode ";
        //    query += " join VehicleModel on VehicleDetail.ModelId=VehicleModel.ModelCode ";
        //    query += " join PaymentMethod on SummaryDetail.PaymentMethodId=PaymentMethod.id ";
        //    query += " join Currency on VehicleDetail.CurrencyId=Currency.id  order by CreatedOn desc ";


        //    SummaryList = InsuranceContext.Query(query).Select(x => new PolicyListViewModel()
        //    {
        //        PolicyNumber = x.PolicyNumber,
        //        CustomerName = x.CustomerName,
        //        PaymentMethod = x.PaymentMethod,
        //        TotalSumInsured = x.TotalSumInsured,
        //        TotalPremium = x.TotalPremium,
        //        createdOn = x.CreatedOn,
        //        RegisterationNumber = x.RegistrationNo,
        //        VehicleName = x.Vehicle,
        //        Currency = x.CurrencyName,
        //        VehicleSumInsured = x.VehicleDetail_SumInsured,
        //        VehicleDetailId = x.VehicleDetailid,
        //        IsActive = x.IsActive,
        //        SummaryDetailId = x.SummaryDetailId,
        //        CoverStartDate = x.CoverEndDate,
        //        CoverEndDate = x.CoverEndDate,
        //        RenewalDate1 = x.RenewalDate,
        //        isLapsed = x.isLapsed
        //    }).ToList();





        //    foreach (var item in SummaryList.Take(200))
        //    {


        //        policylistviewmodel.TotalPremium = Convert.ToDecimal(item.TotalPremium);
        //        policylistviewmodel.TotalSumInsured = Convert.ToDecimal(item.TotalSumInsured);
        //        policylistviewmodel.PaymentMethodId = Convert.ToInt32(item.PaymentMethodId);
        //        policylistviewmodel.CustomerId = Convert.ToInt32(item.CustomerId);
        //        policylistviewmodel.SummaryId = item.SummaryDetailId;
        //        policylistviewmodel.createdOn = Convert.ToDateTime(item.createdOn);
        //        policylistviewmodel.PaymentMethod = item.PaymentMethod;
        //        policylistviewmodel.CustomerName = item.CustomerName;
        //        policylistviewmodel.PolicyNumber = item.PolicyNumber;

        //        VehicleReinsuranceViewModel obj = new VehicleReinsuranceViewModel();

        //        if (item.VehicleDetailId != 0)
        //        {
        //            //   var _reinsurenaceTrans = InsuranceContext.ReinsuranceTransactions.All(where: $"SummaryDetailId={item.Id} and VehicleId={_item.VehicleDetailsId}").ToList();

        //            var _reinsurenaceTrans = reInsuranceList.Where(c => c.SummaryDetailId == item.SummaryDetailId && c.VehicleId == item.VehicleDetailId).ToList();

        //            obj.RegistrationNo = item.RegisterationNumber;
        //            obj.Vehicle = item.VehicleName;
        //            obj.VehicleId = item.VehicleDetailId;
        //            obj.SumInsured = Convert.ToDecimal(item.VehicleSumInsured);
        //            obj.VehicleId = item.VehicleDetailId;
        //            obj.startdate = Convert.ToDateTime(item.CoverStartDate);
        //            obj.enddate = Convert.ToDateTime(item.CoverEndDate);
        //            obj.RenewalDate = Convert.ToDateTime(item.RenewalDate1);
        //            obj.isLapsed = item.isLapsed;
        //            obj.isActive = Convert.ToBoolean(item.IsActive);
        //            obj.currency = item.Currency;
        //            if (_reinsurenaceTrans != null && _reinsurenaceTrans.Count > 0)
        //            {
        //                obj.BrokerCommission = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceCommission);
        //                obj.AutoFacPremium = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsurancePremium);
        //                obj.AutoFacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceAmount);

        //                if (_reinsurenaceTrans.Count > 1)
        //                {
        //                    obj.FacultativeCommission = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsuranceCommission);
        //                    obj.FacPremium = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsurancePremium);
        //                    obj.FacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceAmount);
        //                }
        //            }
        //            policylistviewmodel.Vehicles.Add(obj);
        //        }

        //        policylist.listpolicy.Add(policylistviewmodel);
        //    }






        //    return View(policylist);
        //}






        [Authorize(Roles = "Staff,Administrator")]
        public ActionResult RenewPolicyList()
        {


            ListPolicy policylist = new ListPolicy();
            policylist.listpolicy = new List<PolicyListViewModel>();

            var CoverTypeList = InsuranceContext.CoverTypes.All().ToList();
            var SummaryList = new List<SummaryDetail>();

            var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
            var role = UserManager.GetRoles(_User.Id.ToString()).FirstOrDefault();

            //var RoleId= UserManager

            var cutomerList = InsuranceContext.Customers.All();

            var customerID = InsuranceContext.Customers.Single(where: $"userid='{User.Identity.GetUserId().ToString()}'").Id;


            if (role == "Staff")
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"CreatedBy={customerID} and isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();
            }
            else if (role == "Administrator")
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();
            }
            else
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"customerid={customerID} and isQuotation = '0'").OrderByDescending(x => x.Id).ToList();
            }


            var currenyList = _summaryDetailService.GetAllCurrency();


            foreach (var item in SummaryList.Take(50))
            {

                var paymentDetails = InsuranceContext.PaymentInformations.Single(where: $"SummaryDetailId =" + item.Id);

                if (paymentDetails == null)
                {
                    continue;
                }


                var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={item.Id}").ToList();

                if (SummaryVehicleDetails != null && SummaryVehicleDetails.Count > 0)
                {


                    PolicyListViewModel policylistviewmodel = new PolicyListViewModel();
                    foreach (var _item in SummaryVehicleDetails)
                    {
                        VehicleReinsuranceViewModel obj = new VehicleReinsuranceViewModel();

                        var _vehicle = InsuranceContext.VehicleDetails.Single(where: $" Id='{_item.VehicleDetailsId}'  ");


                        if (_vehicle != null && _vehicle.RenewalDate.Value.Year == DateTime.Now.Year && _vehicle.RenewalDate.Value.Month == DateTime.Now.Month && _vehicle.RenewalDate.Value.Day <= DateTime.Now.Day && _vehicle.IsActive != false)
                        {
                            var policy = InsuranceContext.PolicyDetails.Single(_vehicle.PolicyId);
                            //var customerDetails = InsuranceContext.Customers.Single(_vehicle.CustomerId);

                            var customerDetails = cutomerList.FirstOrDefault(c => c.Id == _vehicle.CustomerId);

                            var agentDetails = cutomerList.FirstOrDefault(c => c.Id == item.CreatedBy);

                            //Added on 24th April
                            //var StaffDetails = InsuranceContext.Customers.Single(_vehicle.CustomerId);

                            //End
                            policylistviewmodel.AgentName = agentDetails == null ? "" : agentDetails.FirstName + " " + agentDetails.LastName;
                            policylistviewmodel.CustomerName = customerDetails.FirstName + " " + customerDetails.LastName;
                            policylistviewmodel.CustomerContactNumber = customerDetails.PhoneNumber;

                            policylistviewmodel.Vehicles = new List<VehicleReinsuranceViewModel>();
                            policylistviewmodel.TotalPremium = Convert.ToDecimal(item.TotalPremium);
                            policylistviewmodel.TotalSumInsured = Convert.ToDecimal(item.TotalSumInsured);
                            policylistviewmodel.PaymentMethodId = Convert.ToInt32(item.PaymentMethodId);
                            policylistviewmodel.CustomerId = Convert.ToInt32(item.CustomerId);
                            policylistviewmodel.SummaryId = item.Id;
                            policylistviewmodel.createdOn = Convert.ToDateTime(item.CreatedOn);
                            policylistviewmodel.PolicyNumber = policy.PolicyNumber;



                            //obj.RegistrationNo = _vehicle.RegistrationNo;
                            //obj.CoverType = Convert.ToInt32(_vehicle.CoverTypeId);

                            var CoverTypeDetials = CoverTypeList.FirstOrDefault(c => c.Id == _vehicle.CoverTypeId);
                            if (CoverTypeDetials != null)
                                policylistviewmodel.CoverTypeName = CoverTypeDetials.Name;

                            if (_vehicle.PaymentTermId == 1)
                                policylistviewmodel.PaymentTerm = "Annually";
                            else
                                policylistviewmodel.PaymentTerm = _vehicle.PaymentTermId + " Months";


                            //obj.isReinsurance = (_vehicle.SumInsured > 100000 ? true : false);
                            //obj.MakeId = _vehicle.MakeId;
                            //obj.ModelId = _vehicle.ModelId;

                            var MakeDetails = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{_vehicle.MakeId}'");
                            if (MakeDetails != null)
                                policylistviewmodel.Make = MakeDetails.MakeDescription;

                            var ModelDetials = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{_vehicle.ModelId}'");

                            if (ModelDetials != null)
                                policylistviewmodel.Model = ModelDetials.ModelDescription;


                            policylistviewmodel.RegisterationNumber = _vehicle.RegistrationNo;


                            policylistviewmodel.startdate = _vehicle.CoverStartDate.Value.ToShortDateString();
                            policylistviewmodel.enddate = _vehicle.CoverEndDate.Value.ToShortDateString();
                            policylistviewmodel.RenewalDate = _vehicle.RenewalDate.Value.ToShortDateString();
                            policylistviewmodel.Currency = _summaryDetailService.GetCurrencyName(currenyList, _vehicle.CurrencyId);

                            policylist.listpolicy.Add(policylistviewmodel);
                        }
                    }



                }



            }


            return View(policylist);
        }


        [Authorize(Roles = "Staff,Administrator")]
        [HttpGet]
        public ActionResult SearchRenewPolicyList()
        {
            return RedirectToAction("RenewPolicyList");

        }


        [Authorize(Roles = "Staff,Administrator")]
        [HttpPost]
        public ActionResult SearchRenewPolicyList(ListPolicy Model)
        {



            ListPolicy policylist = new ListPolicy();
            policylist.listpolicy = new List<PolicyListViewModel>();

            var CoverTypeList = InsuranceContext.CoverTypes.All().ToList();
            var SummaryList = new List<SummaryDetail>();

            var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
            var role = UserManager.GetRoles(_User.Id.ToString()).FirstOrDefault();

            var customerID = InsuranceContext.Customers.Single(where: $"userid='{User.Identity.GetUserId().ToString()}'").Id;


            if (role == "Staff")
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"CreatedBy={customerID} and isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();
            }
            else if (role == "Administrator")
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();
            }
            else
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"customerid={customerID} and isQuotation = '0'").OrderByDescending(x => x.Id).ToList();
            }


            foreach (var item in SummaryList)
            {

                var paymentDetails = InsuranceContext.PaymentInformations.Single(where: $"SummaryDetailId =" + item.Id);

                if (paymentDetails == null)
                {
                    continue;
                }



                var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={item.Id}").ToList();

                var currencyList = _summaryDetailService.GetAllCurrency();


                if (SummaryVehicleDetails != null && SummaryVehicleDetails.Count > 0)
                {


                    PolicyListViewModel policylistviewmodel = new PolicyListViewModel();
                    foreach (var _item in SummaryVehicleDetails)
                    {
                        VehicleReinsuranceViewModel obj = new VehicleReinsuranceViewModel();


                        var _vehicle = InsuranceContext.VehicleDetails.Single(where: $" Id='{_item.VehicleDetailsId}'  ");




                        if (_vehicle != null && _vehicle.RenewalDate.Value >= Convert.ToDateTime(Model.FromDate) && _vehicle.RenewalDate.Value <= Convert.ToDateTime(Model.EndDate) && _vehicle.IsActive != false)
                        {
                            var policy = InsuranceContext.PolicyDetails.Single(_vehicle.PolicyId);
                            var customerDetails = InsuranceContext.Customers.Single(_vehicle.CustomerId);

                            policylistviewmodel.CustomerName = customerDetails.FirstName + " " + customerDetails.LastName;
                            policylistviewmodel.CustomerContactNumber = customerDetails.PhoneNumber;

                            policylistviewmodel.Vehicles = new List<VehicleReinsuranceViewModel>();
                            policylistviewmodel.TotalPremium = Convert.ToDecimal(item.TotalPremium);
                            policylistviewmodel.TotalSumInsured = Convert.ToDecimal(item.TotalSumInsured);
                            policylistviewmodel.PaymentMethodId = Convert.ToInt32(item.PaymentMethodId);
                            policylistviewmodel.CustomerId = Convert.ToInt32(item.CustomerId);
                            policylistviewmodel.SummaryId = item.Id;
                            policylistviewmodel.createdOn = Convert.ToDateTime(item.CreatedOn);
                            policylistviewmodel.PolicyNumber = policy.PolicyNumber;

                            ViewBag.fromdate = Model.FromDate;
                            ViewBag.enddate = Model.EndDate;



                            //obj.RegistrationNo = _vehicle.RegistrationNo;
                            //obj.CoverType = Convert.ToInt32(_vehicle.CoverTypeId);

                            var CoverTypeDetials = CoverTypeList.FirstOrDefault(c => c.Id == _vehicle.CoverTypeId);
                            if (CoverTypeDetials != null)
                                policylistviewmodel.CoverTypeName = CoverTypeDetials.Name;

                            if (_vehicle.PaymentTermId == 1)
                                policylistviewmodel.PaymentTerm = "Annually";
                            else
                                policylistviewmodel.PaymentTerm = _vehicle.PaymentTermId + " Months";


                            //obj.isReinsurance = (_vehicle.SumInsured > 100000 ? true : false);
                            //obj.MakeId = _vehicle.MakeId;
                            //obj.ModelId = _vehicle.ModelId;

                            var MakeDetails = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{_vehicle.MakeId}'");
                            if (MakeDetails != null)
                                policylistviewmodel.Make = MakeDetails.MakeDescription;

                            var ModelDetials = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{_vehicle.ModelId}'");

                            if (ModelDetials != null)
                                policylistviewmodel.Model = ModelDetials.ModelDescription;


                            policylistviewmodel.RegisterationNumber = _vehicle.RegistrationNo;


                            policylistviewmodel.startdate = _vehicle.CoverStartDate.Value.ToShortDateString();
                            policylistviewmodel.enddate = _vehicle.CoverEndDate.Value.ToShortDateString();
                            policylistviewmodel.RenewalDate = _vehicle.RenewalDate.Value.ToShortDateString();
                            policylistviewmodel.Currency = _summaryDetailService.GetCurrencyName(currencyList, _vehicle.CurrencyId);

                            policylist.listpolicy.Add(policylistviewmodel);
                        }

                    }

                    //if (policylistviewmodel.SummaryId != 0)
                    //    policylist.listpolicy.Add(policylistviewmodel);


                }

            }



            return View("RenewPolicyList", policylist);


        }




        private void ClearRenewSession()
        {
            Session.Remove("RenewVehicleId");
            Session.Remove("RenewPaymentId");
            Session.Remove("RenewInvoiceId");
            Session.Remove("RenewVehicleSummary");
            Session.Remove("RenewVehiclePolicy");
            Session.Remove("RenewVehicle");
            Session.Remove("RenewVehicleDetails");
            Session.Remove("RenewCardDetail");
            Session.Remove("ReSummaryDetailed");
            Session.Remove("CheckRenewVehicleDetails");
            Session.Remove("ReCustomerDataModal");
        }

        // GET: Dashboard
        [Authorize(Roles = "Administrator, AgentStaff, Agent, SuperUser")]
        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult SearchPolicy(string searchText)
        {
            ListPolicy policylist = new ListPolicy();

            try
            {

                var SummaryList = new List<SummaryDetail>();

                policylist.listpolicy = new List<PolicyListViewModel>();
                if (searchText != null && searchText != "")
                {

                    var custom = searchText.Split(' ');
                  
                    var customers = new List<Customer>();
                    if (custom.Length == 2)
                    {
                        var searchtext1 = Convert.ToString(custom[0]);
                        var searchtext2 = Convert.ToString(custom[1]);

                        customers = InsuranceContext.Customers.All(where: $"FirstName like '%{searchtext1}%' and LastName like '%{searchtext2}%' ").ToList();
                    }
                    if (custom.Length == 1)
                    {
                        customers = InsuranceContext.Customers.All(where: $"FirstName like '%{searchText}%' or LastName like '%{searchText}%' ").ToList();

                        if(customers.Count==0)
                        {
                            var vehicleDetails = InsuranceContext.VehicleDetails.All(where: $"RegistrationNo like '%{searchText}%' and IsActive=1 ");

                            foreach(var item in vehicleDetails)
                            {
                                var summaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId= '{item.Id}' ");

                                if(summaryVehicleDetails!=null)
                                {
                                    var summaryDetails = InsuranceContext.SummaryDetails.Single(where: $"Id= '{summaryVehicleDetails.SummaryDetailId}' ");
                                    if(summaryDetails!=null)
                                    {
                                        SummaryList.Add(summaryDetails);
                                    }
                                }


                            }
                        }


                    }
                    if (customers != null && customers.Count > 0)
                    {
                        var commaSeperatedCustomerIds = "";
                        foreach (var item in customers)
                        {
                            if (commaSeperatedCustomerIds == "")
                            {
                                commaSeperatedCustomerIds = item.Id.ToString();
                            }
                            else
                            {
                                commaSeperatedCustomerIds += "," + item.Id.ToString();
                            }
                        }

                        SummaryList = InsuranceContext.SummaryDetails.All(where: $"CustomerId in ({commaSeperatedCustomerIds})").ToList();
                    }
                    else if(SummaryList.Count>0)
                    {
                        
                    }                 
                    else
                    {
                        //var policye = InsuranceContext.PolicyDetails.Query("Select * From PolicyDetail Where PolicyNumber Like '%" + searchText + "%'").FirstOrDefault();
                        var policye = InsuranceContext.PolicyDetails.Query("Select * From PolicyDetail Where PolicyNumber Like '%" + searchText + "%'").FirstOrDefault();
                        if (policye != null)
                        {


                            var policyId = policye.Id;
                            var vehicle = InsuranceContext.VehicleDetails.Single(where: $"PolicyId = '" + policyId + "'");

                            if (vehicle != null)
                            {
                                if (vehicle.Id != 0)
                                {
                                    var vehiclesummaryid = vehicle.Id;

                                    var SummaryVehicleDetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId =" + vehiclesummaryid);

                                    if (SummaryVehicleDetail.SummaryDetailId != 0)
                                    {
                                        SummaryList = InsuranceContext.SummaryDetails.All(Convert.ToString(SummaryVehicleDetail.SummaryDetailId)).ToList();
                                    }
                                }
                            }
                        }
                    }


                    var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                    var role = UserManager.GetRoles(_User.Id.ToString()).FirstOrDefault();

                    var customerID = InsuranceContext.Customers.Single(where: $"userid='{User.Identity.GetUserId().ToString()}'").Id;




                    if (role == "Staff")
                    {
                        // SummaryList = InsuranceContext.SummaryDetails.All(where: $"CreatedBy={customerID} and isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();

                        SummaryList = SummaryList.Where(c => c.CreatedBy == customerID && c.isQuotation == false).OrderByDescending(c => c.Id).ToList();
                    }
                    else if (role == "Administrator" || role == "Renewals")
                    {
                        //  SummaryList = InsuranceContext.SummaryDetails.All(where: $"isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();
                        SummaryList = SummaryList.Where(c => c.isQuotation == false).OrderByDescending(c => c.Id).ToList();
                    }
                    else
                    {
                        SummaryList = SummaryList.Where(c => c.CreatedBy == customerID && c.isQuotation == false).OrderByDescending(c => c.Id).ToList();
                    }


                    var paymentlList = InsuranceContext.PaymentInformations.All();


                    if (SummaryList != null && SummaryList.Count > 0)
                    {
                        foreach (var item in SummaryList)
                        {

                            var paymentDetails = paymentlList.FirstOrDefault(c => c.SummaryDetailId == item.Id);
                            if (paymentDetails == null)
                            {
                                continue;
                            }

                            PolicyListViewModel policylistviewmodel = new PolicyListViewModel();

                            policylistviewmodel.Vehicles = new List<VehicleReinsuranceViewModel>();
                            policylistviewmodel.TotalPremium = Convert.ToDecimal(item.TotalPremium);
                            policylistviewmodel.TotalSumInsured = Convert.ToDecimal(item.TotalSumInsured);
                            policylistviewmodel.PaymentMethodId = Convert.ToInt32(item.PaymentMethodId);
                            policylistviewmodel.CustomerId = Convert.ToInt32(item.CustomerId);
                            policylistviewmodel.SummaryId = item.Id;

                            policylistviewmodel.PaymentMethod = MiscellaneousService.GetPaymentMethodNamebyID(item.PaymentMethodId == null ? 1 : item.PaymentMethodId.Value);



                            var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={item.Id}").ToList();
                            var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
                            if (vehicle != null)
                            {
                                policylistviewmodel.CustomerName = MiscellaneousService.GetCustomerNamebyID(vehicle.CustomerId.Value);


                                if (vehicle.PolicyId != 0)
                                {
                                    var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);

                                    //if (policy.PolicyName!="")
                                    //{
                                    var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));
                                    policylistviewmodel.PolicyNumber = policy.PolicyNumber;
                                    //}

                                }
                            }




                            foreach (var _item in SummaryVehicleDetails)
                            {
                                VehicleReinsuranceViewModel obj = new VehicleReinsuranceViewModel();
                                var _vehicle = InsuranceContext.VehicleDetails.Single(_item.VehicleDetailsId);
                                if (_vehicle != null)
                                {




                                    if (_vehicle.Id != 0)
                                    {
                                        // var _reinsurenaceTrans = InsuranceContext.ReinsuranceTransactions.All(where: $"SummaryDetailId={item.Id} and VehicleId={_item.VehicleDetailsId}").ToList();
                                        var _reinsurenaceTrans = InsuranceContext.ReinsuranceTransactions.All(where: $"SummaryDetailId={item.Id} and VehicleId={_item.VehicleDetailsId}").ToList();
                                        obj.CoverType = Convert.ToInt32(_vehicle.CoverTypeId);
                                        obj.isReinsurance = (_vehicle.SumInsured > 100000 ? true : false);
                                        obj.MakeId = _vehicle.MakeId;
                                        obj.ModelId = _vehicle.ModelId;

                                        obj.Make = MiscellaneousService.GetMakeNamebyMakeCode(_vehicle.MakeId);
                                        obj.Model = MiscellaneousService.GetModelNamebyModelCode(_vehicle.ModelId);


                                        //obj.Premium = Convert.ToDecimal(_vehicle.Premium);
                                        obj.RegistrationNo = _vehicle.RegistrationNo;
                                        obj.SumInsured = Convert.ToDecimal(_vehicle.SumInsured);
                                        obj.VehicleId = _vehicle.Id;
                                        obj.startdate = Convert.ToDateTime(_vehicle.CoverStartDate);
                                        obj.enddate = Convert.ToDateTime(_vehicle.CoverEndDate);
                                        obj.isActive = Convert.ToBoolean(_vehicle.IsActive);
                                        obj.RenewalDate = Convert.ToDateTime(_vehicle.RenewalDate);
                                        if (_reinsurenaceTrans != null && _reinsurenaceTrans.Count > 0)
                                        {
                                            obj.BrokerCommission = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceCommission);
                                            obj.AutoFacPremium = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsurancePremium);
                                            obj.AutoFacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceAmount);
                                            if (_reinsurenaceTrans.Count > 1)
                                            {
                                                obj.FacultativeCommission = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsuranceCommission);
                                                obj.FacPremium = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsurancePremium);
                                                obj.FacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceAmount);
                                            }
                                        }
                                    }
                                    policylistviewmodel.Vehicles.Add(obj);

                                }

                                //else
                                //{
                                //    return View("PolicyList", policylist);
                                //}
                            }

                            policylist.listpolicy.Add(policylistviewmodel);
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return View("PolicyList", policylist);
        }

        // Setting Methods

        // GET: Setting
        public ActionResult Index()
        {
            InsuranceClaim.Models.SettingModel obj = new InsuranceClaim.Models.SettingModel();
            List<Insurance.Domain.Setting> objList = new List<Insurance.Domain.Setting>();

            var eSettingValueTypedata = from eSettingValueType e in Enum.GetValues(typeof(eSettingValueType))
                                        select new
                                        {
                                            ID = (int)e,
                                            Name = e.ToString()
                                        };

            ViewBag.eSettingValueType = new SelectList(eSettingValueTypedata, "ID", "Name");




            objList = InsuranceContext.Settings.All().ToList();
            return View(obj);
        }

        [HttpPost]
        public ActionResult SaveSetting(SettingModel model)
        {

            //model.CreatedBy = 1;
            //model.CreatedDate = DateTime.Now;
            var _customerData = InsuranceContext.Customers.Single(where: $"UserId ='{User.Identity.GetUserId().ToString()}'");
            model.CreatedBy = _customerData.Id;
            model.CreatedDate = DateTime.Now;
            var dbModel = Mapper.Map<SettingModel, Setting>(model);
            InsuranceContext.Settings.Insert(dbModel);

            return RedirectToAction("SettingList");
        }
        [Authorize(Roles = "Administrator")]
        public ActionResult SettingList()
        {

            var db = InsuranceContext.Settings.All().OrderByDescending(x => x.Id).ToList();
            return View(db);
        }

        public ActionResult EditSetting(int Id)
        {
            var record = InsuranceContext.Settings.All(where: $"Id ={Id}").FirstOrDefault();
            var model = Mapper.Map<Setting, SettingModel>(record);



            var eSettingValueTypedata = from eSettingValueType e in Enum.GetValues(typeof(eSettingValueType))
                                        select new
                                        {
                                            ID = (int)e,
                                            Name = e.ToString()
                                        };

            ViewBag.eSettingValueType = new SelectList(eSettingValueTypedata, "ID", "Name");

            return View(model);
        }
        [HttpPost]
        public ActionResult EditSetting(SettingModel model, int Id)
        {

            if (ModelState.IsValid)
            {
                var _customerData = InsuranceContext.Customers.Single(where: $"UserId ='{User.Identity.GetUserId().ToString()}'");
                model.ModifiedBy = _customerData.Id;
                model.ModifiedDate = DateTime.Now;
                model.CreatedBy = _customerData.Id;

                var data = Mapper.Map<SettingModel, Setting>(model);
                InsuranceContext.Settings.Update(data);
            }
            return RedirectToAction("SettingList");
        }

        public ActionResult DeleteSetting(int Id)
        {
            string query = $"Delete Setting  where Id = {Id}";
            InsuranceContext.Settings.Execute(query);

            return RedirectToAction("SettingList");
        }
        [Authorize(Roles = "Administrator")]
        public ActionResult ListReinsuranceBroker()
        {


            var list = InsuranceContext.ReinsuranceBrokers.All().OrderByDescending(x => x.Id).ToList();
            return View(list);
        }
        public ActionResult AddReinsuranceBroker(int? id = 0)
        {
            InsuranceClaim.Models.ReinsuranceBrokerModel obj = new ReinsuranceBrokerModel();
            if (id > 0)
            {
                var model = InsuranceContext.ReinsuranceBrokers.Single(id);
                obj = Mapper.Map<ReinsuranceBroker, ReinsuranceBrokerModel>(model);
            }

            return View(obj);
        }
        [HttpPost]
        public ActionResult SaveReinsuranceBroker(ReinsuranceBrokerModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0 || model.Id == null)
                {
                    var dbModel = Mapper.Map<ReinsuranceBrokerModel, ReinsuranceBroker>(model);
                    InsuranceContext.ReinsuranceBrokers.Insert(dbModel);
                    return RedirectToAction("ListReinsuranceBroker");
                }
                else
                {
                    var data = Mapper.Map<ReinsuranceBrokerModel, ReinsuranceBroker>(model);
                    InsuranceContext.ReinsuranceBrokers.Update(data);
                }
            }
            return RedirectToAction("ListReinsuranceBroker");
        }
        public ActionResult DeleteReinsuranceBroker(int Id)
        {
            string query = $"Delete ReinsuranceBroker  where Id = {Id}";
            InsuranceContext.ReinsuranceBrokers.Execute(query);
            return RedirectToAction("ListReinsuranceBroker");
        }
        public ActionResult RenewPolicy(int Id)
        {
            var summaryDetail = InsuranceContext.SummaryDetails.Single(Id);
            var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={Id}").ToList();
            var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
            var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
            var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));
            var Cusotmer = InsuranceContext.Customers.Single(vehicle.CustomerId);

            CustomerModel custModel = Mapper.Map<Customer, CustomerModel>(Cusotmer);
            Session["CustomerDataModal"] = custModel;

            Session["PolicyData"] = policy;

            List<RiskDetailModel> listRiskDetail = new List<RiskDetailModel>();
            foreach (var item in SummaryVehicleDetails)
            {
                var _vehicle = InsuranceContext.VehicleDetails.Single(item.VehicleDetailsId);
                RiskDetailModel riskDetail = Mapper.Map<VehicleDetail, RiskDetailModel>(_vehicle);
                listRiskDetail.Add(riskDetail);
            }
            Session["VehicleDetails"] = listRiskDetail;

            SummaryDetailModel summarymodel = Mapper.Map<SummaryDetail, SummaryDetailModel>(summaryDetail);
            Session["SummaryDetailed"] = summarymodel;

            return RedirectToAction("RiskDetail", "CustomerRegistration");
        }



        //public ActionResult MyPolicies()
        //{
        //    TempData["RedirectedFrom"] = "MyPolicy";
        //    Session["ViewlistVehicles"] = null;
        //    ListPolicy policylist = new ListPolicy();
        //    policylist.listpolicy = new List<PolicyListViewModel>();


        //    var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
        //    var role = UserManager.GetRoles(_User.Id.ToString()).FirstOrDefault();

        //    // var currencyList = InsuranceContext.Currencies.All();

        //    SummaryDetailService detailDervice = new SummaryDetailService();
        //    var currencyList = detailDervice.GetAllCurrency();


        //    var customerID = InsuranceContext.Customers.Single(where: $"userid='{User.Identity.GetUserId().ToString()}'").Id;

        //    var SummaryList = new List<SummaryDetail>();

        //    if (role == "Staff")
        //    {
        //        SummaryList = InsuranceContext.SummaryDetails.All(where: $"CreatedBy={customerID} and isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();
        //    }
        //    else if (role == "Administrator" || role== "Renewals")
        //    {
        //        SummaryList = InsuranceContext.SummaryDetails.All(where: $"isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();
        //    }
        //    else
        //    {
        //        SummaryList = InsuranceContext.SummaryDetails.All(where: $"customerid={customerID} and isQuotation = '0'").OrderByDescending(x => x.Id).ToList();
        //    }


        //    var SummaryVehicleDetailslist = InsuranceContext.SummaryVehicleDetails.All().ToList();
        //    var vehiclelist = InsuranceContext.VehicleDetails.All().ToList();
        //    var policy_list = InsuranceContext.PolicyDetails.All().ToList();

        //    foreach (var item in SummaryList.Take(100))
        //    {
        //        PolicyListViewModel policylistviewmodel = new PolicyListViewModel();

        //        var paymentDetails = InsuranceContext.PaymentInformations.Single(where: $"SummaryDetailId =" + item.Id);



        //        if (paymentDetails == null)
        //        {
        //            continue;
        //        }



        //        policylistviewmodel.Vehicles = new List<VehicleReinsuranceViewModel>();
        //        policylistviewmodel.TotalPremium = Convert.ToDecimal(item.TotalPremium);
        //        policylistviewmodel.TotalSumInsured = Convert.ToDecimal(item.TotalSumInsured);
        //        policylistviewmodel.PaymentMethodId = Convert.ToInt32(item.PaymentMethodId);
        //        policylistviewmodel.CustomerId = Convert.ToInt32(item.CustomerId);
        //        policylistviewmodel.CustomerEmail = GetCustomerEmailbyCustomerID(item.CustomerId);
        //        policylistviewmodel.SummaryId = item.Id;
        //        policylistviewmodel.createdOn = Convert.ToDateTime(item.CreatedOn);

        //        // var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={item.Id}").ToList();

        //        var SummaryVehicleDetails = SummaryVehicleDetailslist.Where(x => x.SummaryDetailId == item.Id).ToList();


        //        // var vehicle = InsuranceContext.VehicleDetails.Single(where: $" Id='{SummaryVehicleDetails[0].VehicleDetailsId}'");
        //        var vehicle = vehiclelist.FirstOrDefault(y => y.Id == SummaryVehicleDetails[0].VehicleDetailsId);


        //        if (vehicle != null)
        //        {
        //            // var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
        //            var policy = policy_list.FirstOrDefault(x => x.Id == vehicle.PolicyId);

        //            //   var product = InsuranceContext.Products.Single(Convert.ToInt32(vehicle.ProductId));

        //            policylistviewmodel.PolicyNumber = policy.PolicyNumber;
        //            policylistviewmodel.PolicyStatus = policy.Status;

        //            policylistviewmodel.Currency = detailDervice.GetCurrencyName(currencyList, vehicle.CurrencyId);


        //            //foreach (var _item in SummaryVehicleDetails)
        //            //{

        //            //}


        //            VehicleReinsuranceViewModel obj = new VehicleReinsuranceViewModel();
        //            var _vehicle = vehicle;// InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);

        //            if (_vehicle != null)
        //            {

        //                var _reinsurenaceTrans = InsuranceContext.ReinsuranceTransactions.All(where: $"SummaryDetailId={item.Id} and VehicleId={SummaryVehicleDetails[0].VehicleDetailsId}").ToList();
        //                obj.CoverType = Convert.ToInt32(_vehicle.CoverTypeId);
        //                obj.isReinsurance = (_vehicle.SumInsured > 100000 ? true : false);
        //                obj.MakeId = _vehicle.MakeId;
        //                obj.ModelId = _vehicle.ModelId;
        //                //obj.Premium = Convert.ToDecimal(_vehicle.Premium);
        //                obj.RegisterationNumber = _vehicle.RegistrationNo;
        //                obj.SumInsured = Convert.ToDecimal(_vehicle.SumInsured);
        //                obj.VehicleId = _vehicle.Id;
        //                obj.startdate = Convert.ToDateTime(_vehicle.CoverStartDate);
        //                obj.enddate = Convert.ToDateTime(_vehicle.CoverEndDate);
        //                obj.RenewalDate = Convert.ToDateTime(_vehicle.RenewalDate);
        //                obj.isLapsed = _vehicle.isLapsed;
        //                obj.BalanceAmount = Convert.ToDecimal(_vehicle.BalanceAmount);
        //                obj.isActive = Convert.ToBoolean(_vehicle.IsActive);
        //                obj.Premium = Convert.ToDecimal(_vehicle.Premium + _vehicle.StampDuty + _vehicle.ZTSCLevy + (Convert.ToBoolean(_vehicle.IncludeRadioLicenseCost) ? Convert.ToDecimal(_vehicle.RadioLicenseCost) : 0.00m));
        //                if (_reinsurenaceTrans != null && _reinsurenaceTrans.Count > 0)
        //                {
        //                    obj.BrokerCommission = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceCommission);
        //                    obj.AutoFacPremium = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsurancePremium);
        //                    obj.AutoFacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceAmount);

        //                    if (_reinsurenaceTrans.Count > 1)
        //                    {
        //                        obj.FacultativeCommission = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsuranceCommission);
        //                        obj.FacPremium = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsurancePremium);
        //                        obj.FacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsuranceAmount);
        //                    }

        //                    policylistviewmodel.Vehicles.Add(obj);
        //                }

        //                policylist.listpolicy.Add(policylistviewmodel);

        //            }
        //        }
        //    }

        //    return View(policylist);
        //}


        public ActionResult MyPolicies()
        {
            TempData["RedirectedFrom"] = "MyPolicy";
            Session["ViewlistVehicles"] = null;
            ListPolicy policylist = new ListPolicy();
            policylist.listpolicy = new List<PolicyListViewModel>();


            var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
            var role = UserManager.GetRoles(_User.Id.ToString()).FirstOrDefault();

            // var currencyList = InsuranceContext.Currencies.All();

            //SummaryDetailService detailDervice = new SummaryDetailService();
            List<Currency> currencyList = InsuranceContext.Currencies.All().ToList();


            var customerID = InsuranceContext.Customers.Single(where: $"userid='{User.Identity.GetUserId().ToString()}'").Id;

            var SummaryList = new List<SummaryDetail>();

            if (role == "Staff")
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"CreatedBy={customerID} and isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();
            }
            else if (role == "Administrator" || role == "Renewals")
            {
                var query = " Select SD.* from SummaryDetail SD Join PaymentInformation pa on SD.Id =pa.SummaryDetailId where SD.isQuotation=0 ";
                SummaryList = InsuranceContext.Query(query).Select(x => new SummaryDetail()
                {
                    TotalPremium = x.TotalPremium,
                    TotalSumInsured = x.TotalSumInsured,
                    PaymentMethodId = x.PaymentMethodId,
                    CustomerId = x.CustomerId,
                    Id = x.Id,
                    CreatedOn = x.CreatedOn
                }).OrderByDescending(x => x.Id).ToList();
                //   SummaryList = InsuranceContext.SummaryDetails.All(where: $"isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();
            }
            else if (role == "Agent")
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"AgentId={customerID} and isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();
            }
            else if (role == "AgentStaff")
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"CreatedBy={customerID} and isQuotation = '0'  ").OrderByDescending(x => x.Id).ToList();
            }
            else
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"customerid={customerID} and isQuotation = '0'").OrderByDescending(x => x.Id).ToList();
            }


            var SummaryVehicleDetailslist = InsuranceContext.SummaryVehicleDetails.All().ToList();
            var vehiclelist = InsuranceContext.VehicleDetails.All().ToList();
            var policy_list = InsuranceContext.PolicyDetails.All().ToList();
            var ReinsuranceTransactionslist = InsuranceContext.ReinsuranceTransactions.All().ToList();
            foreach (var item in SummaryList.Take(100))
            {
                //var paymentDetails = InsuranceContext.PaymentInformations.Single(where: $"SummaryDetailId =" + item.Id);
                //if (paymentDetails == null)
                //{
                //    continue;
                //}

                // var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={item.Id}").ToList();

                var SummaryVehicleDetails = SummaryVehicleDetailslist.FirstOrDefault(x => x.SummaryDetailId == item.Id);


                // var vehicle = InsuranceContext.VehicleDetails.Single(where: $" Id='{SummaryVehicleDetails[0].VehicleDetailsId}'");
                var vehicle = vehiclelist.FirstOrDefault(y => y.Id == SummaryVehicleDetails.VehicleDetailsId);


                if (vehicle != null)
                {
                    var _vehicle = vehicle;// InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);


                    PolicyListViewModel policylistviewmodel = new PolicyListViewModel();


                    policylistviewmodel.Vehicles = new List<VehicleReinsuranceViewModel>();
                    policylistviewmodel.TotalPremium = Convert.ToDecimal(item.TotalPremium);
                    policylistviewmodel.TotalSumInsured = Convert.ToDecimal(item.TotalSumInsured);
                    policylistviewmodel.PaymentMethodId = Convert.ToInt32(item.PaymentMethodId);
                    policylistviewmodel.CustomerId = Convert.ToInt32(item.CustomerId);
                    policylistviewmodel.CustomerEmail = GetCustomerEmailbyCustomerID(item.CustomerId);
                    policylistviewmodel.SummaryId = item.Id;
                    policylistviewmodel.createdOn = Convert.ToDateTime(item.CreatedOn);

                    // var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
                    var policy = policy_list.FirstOrDefault(x => x.Id == vehicle.PolicyId);

                    //   var product = InsuranceContext.Products.Single(Convert.ToInt32(vehicle.ProductId));

                    policylistviewmodel.PolicyNumber = policy.PolicyNumber;
                    policylistviewmodel.PolicyStatus = policy.Status;

                    var currencyDetails = currencyList.FirstOrDefault(c => c.Id == vehicle.CurrencyId);

                    if (currencyDetails != null)
                        policylistviewmodel.Currency = currencyDetails.Name;
                    else
                        policylistviewmodel.Currency = "USD";




                    //string Cureency = currencyList.FirstOrDefault(x => x.Id == vehicle.CurrencyId).Name.ToString();

                    //if (Cureency == null)
                    //    policylistviewmodel.Currency = "USD";
                    //else
                    //    policylistviewmodel.Currency = Cureency;
                    // detailDervice.GetCurrencyName(currencyList, vehicle.CurrencyId);



                    VehicleReinsuranceViewModel obj = new VehicleReinsuranceViewModel();


                    var _reinsurenaceTrans = ReinsuranceTransactionslist.Where(x => x.SummaryDetailId == item.Id && x.VehicleId == SummaryVehicleDetails.VehicleDetailsId).ToList(); //InsuranceContext.ReinsuranceTransactions.All(where: $"SummaryDetailId={item.Id} and VehicleId={SummaryVehicleDetails[0].VehicleDetailsId}").ToList();
                    obj.CoverType = Convert.ToInt32(_vehicle.CoverTypeId);
                    obj.isReinsurance = (_vehicle.SumInsured > 100000 ? true : false);
                    obj.MakeId = _vehicle.MakeId;
                    obj.ModelId = _vehicle.ModelId;
                    //obj.Premium = Convert.ToDecimal(_vehicle.Premium);
                    obj.RegisterationNumber = _vehicle.RegistrationNo;
                    obj.SumInsured = Convert.ToDecimal(_vehicle.SumInsured);
                    obj.VehicleId = _vehicle.Id;
                    obj.startdate = Convert.ToDateTime(_vehicle.CoverStartDate);
                    obj.enddate = Convert.ToDateTime(_vehicle.CoverEndDate);
                    obj.RenewalDate = Convert.ToDateTime(_vehicle.RenewalDate);
                    obj.isLapsed = _vehicle.isLapsed;
                    obj.BalanceAmount = Convert.ToDecimal(_vehicle.BalanceAmount);
                    obj.isActive = Convert.ToBoolean(_vehicle.IsActive);
                    obj.Premium = Convert.ToDecimal(_vehicle.Premium + _vehicle.StampDuty + _vehicle.ZTSCLevy + (Convert.ToBoolean(_vehicle.IncludeRadioLicenseCost) ? Convert.ToDecimal(_vehicle.RadioLicenseCost) : 0.00m));
                    if (_reinsurenaceTrans != null && _reinsurenaceTrans.Count > 0)
                    {
                        obj.BrokerCommission = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceCommission);
                        obj.AutoFacPremium = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsurancePremium);
                        obj.AutoFacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceAmount);

                        if (_reinsurenaceTrans.Count > 1)
                        {
                            obj.FacultativeCommission = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsuranceCommission);
                            obj.FacPremium = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsurancePremium);
                            obj.FacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsuranceAmount);
                        }

                        policylistviewmodel.Vehicles.Add(obj);
                    }

                    policylist.listpolicy.Add(policylistviewmodel);


                }
            }

            return View(policylist);
        }














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

        public ActionResult SearchMyPolicy(string searchText)
        {
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

            ListPolicy policylist = new ListPolicy();
            policylist.listpolicy = new List<PolicyListViewModel>();
            if (searchText != null && searchText != "")
            {

                var custom = searchText.Split(' ');
                var SummaryList = new List<SummaryDetail>();
                var SummaryNewList = new List<SummaryDetail>();
                var customers = new List<Customer>();
                if (custom.Length == 2)
                {
                    var searchtext1 = Convert.ToString(custom[0]);
                    var searchtext2 = Convert.ToString(custom[1]);


                    customers = InsuranceContext.Customers.All(where: $"FirstName like '%{searchtext1}%' and LastName like '%{searchtext2}%' ").ToList();
                }
                if (custom.Length == 1)
                {
                    customers = InsuranceContext.Customers.All(where: $"FirstName like '%{searchText}%' or LastName like '%{searchText}%' ").ToList();
                }
                if (customers != null && customers.Count > 0)
                {
                    var commaSeperatedCustomerIds = "";
                    foreach (var item in customers)
                    {
                        if (commaSeperatedCustomerIds == "")
                        {
                            commaSeperatedCustomerIds = item.Id.ToString();
                        }
                        else
                        {
                            commaSeperatedCustomerIds += "," + item.Id.ToString();
                        }
                    }
                    //var customerID = InsuranceContext.Customers.Single(where: $"userid='{User.Identity.GetUserId().ToString()}'").Id;
                    //if (commaSeperatedCustomerIds == Convert.ToString(customerID))
                    //{
                    SummaryNewList = InsuranceContext.SummaryDetails.All(where: $"CustomerId in ({commaSeperatedCustomerIds})  and isQuotation<>{1}").ToList();



                    //}
                }
                else
                {
                    //var policye = InsuranceContext.PolicyDetails.Query("Select * From PolicyDetail Where PolicyNumber Like '%" + searchText + "%'").FirstOrDefault();
                    var policye = InsuranceContext.PolicyDetails.Query("Select * From PolicyDetail Where PolicyNumber Like '%" + searchText + "%'");
                    if (policye.Count() > 0)
                    {

                        foreach (var item in policye)
                        {
                            var policyId = item.Id;
                            ////
                            var customerid = item.CustomerId;
                            //var customerID = InsuranceContext.Customers.Single(where: $"userid='{User.Identity.GetUserId().ToString()}'").Id;
                            //if (customerid == customerID)
                            //{
                            var vehicle = InsuranceContext.VehicleDetails.Single(where: $"PolicyId = '" + policyId + "'");

                            if (vehicle != null)
                            {
                                var vehiclesummaryid = vehicle.Id;
                                var SummaryVehicleDetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId =" + vehiclesummaryid);
                                if (SummaryVehicleDetail != null)
                                {
                                    // SummaryList = InsuranceContext.SummaryDetails.All(Convert.ToString(SummaryVehicleDetail.SummaryDetailId)).ToList();

                                    // where: $"SummaryDetailId={item.Id} and VehicleId={_item.VehicleDetailsId}"


                                    SummaryList = InsuranceContext.SummaryDetails.All(where: $"Id={SummaryVehicleDetail.SummaryDetailId} and isQuotation<>{1}").ToList();



                                    SummaryNewList.AddRange(SummaryList);
                                }


                            }


                        }


                    }
                }

                if (SummaryNewList != null && SummaryNewList.Count > 0)
                {
                    foreach (var item in SummaryNewList)
                    {
                        PolicyListViewModel policylistviewmodel = new PolicyListViewModel();

                        policylistviewmodel.Vehicles = new List<VehicleReinsuranceViewModel>();
                        policylistviewmodel.TotalPremium = Convert.ToDecimal(item.TotalPremium);
                        policylistviewmodel.TotalSumInsured = Convert.ToDecimal(item.TotalSumInsured);
                        policylistviewmodel.PaymentMethodId = Convert.ToInt32(item.PaymentMethodId);
                        policylistviewmodel.CustomerId = Convert.ToInt32(item.CustomerId);
                        policylistviewmodel.SummaryId = item.Id;

                        var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={item.Id}").ToList();

                        if (SummaryVehicleDetails == null)
                        {
                            continue;
                        }

                        var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);

                        if (vehicle != null)
                        {


                            var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
                            var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));

                            policylistviewmodel.PolicyNumber = policy.PolicyNumber;

                            foreach (var _item in SummaryVehicleDetails)
                            {
                                VehicleReinsuranceViewModel obj = new VehicleReinsuranceViewModel();
                                var _vehicle = InsuranceContext.VehicleDetails.Single(_item.VehicleDetailsId);

                                if (_vehicle != null)
                                {
                                    var _reinsurenaceTrans = InsuranceContext.ReinsuranceTransactions.All(where: $"SummaryDetailId={item.Id} and VehicleId={_item.VehicleDetailsId}").ToList();

                                    obj.CoverType = Convert.ToInt32(_vehicle.CoverTypeId);
                                    obj.isReinsurance = (_vehicle.SumInsured > 100000 ? true : false);
                                    obj.MakeId = _vehicle.MakeId;
                                    obj.ModelId = _vehicle.ModelId;
                                    //obj.Premium = Convert.ToDecimal(_vehicle.Premium);
                                    obj.RegisterationNumber = _vehicle.RegistrationNo;
                                    obj.SumInsured = Convert.ToDecimal(_vehicle.SumInsured);
                                    obj.VehicleId = _vehicle.Id;
                                    obj.startdate = Convert.ToDateTime(_vehicle.CoverStartDate);
                                    obj.enddate = Convert.ToDateTime(_vehicle.CoverEndDate);
                                    obj.RenewalDate = Convert.ToDateTime(_vehicle.RenewalDate);
                                    if (_reinsurenaceTrans != null && _reinsurenaceTrans.Count > 0)
                                    {
                                        obj.BrokerCommission = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceCommission);
                                        obj.AutoFacPremium = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsurancePremium);
                                        obj.AutoFacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceAmount);

                                        if (_reinsurenaceTrans.Count > 1)
                                        {
                                            obj.FacultativeCommission = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsuranceCommission);
                                            obj.FacPremium = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsurancePremium);
                                            obj.FacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceAmount);
                                        }
                                    }

                                }


                                policylistviewmodel.Vehicles.Add(obj);
                            }
                        }
                        policylist.listpolicy.Add(policylistviewmodel);
                    }
                }
            }

            return View("MyPolicies", policylist);
        }



        [Authorize(Roles = "Licence Disk Delivery Manager,Administrator")]
        public ActionResult LicenceTickets()
        {
            var ListLicence = new ListLicenceTickets();
            var LicenceTickets = new List<LicenceTicketViewModel>();
            var List = InsuranceContext.LicenceTickets.All().ToList();

            foreach (var item in List)
            {
                //var id = InsuranceContext.LicenceTickets.Single(item.Id);

                LicenceTickets.Add(new LicenceTicketViewModel
                {
                    IsClosed = Convert.ToBoolean(item.IsClosed),
                    Id = item.Id,
                    TicketNo = item.TicketNo,
                    PolicyNumber = item.PolicyNumber,
                    CloseComments = item.CloseComments,
                    ReopenComments = item.ReopenComments,
                    DeliveredTo = item.DeliveredTo


                });
            }
            return View(LicenceTickets.ToList());
        }

        public ActionResult SaveComments(string CloseComments, string hdnSelectedTicket, string DeliveredTo)
        {
            if (ModelState.IsValid)
            {
                var ticket = InsuranceContext.LicenceTickets.Single(Convert.ToInt32(hdnSelectedTicket));
                ticket.CloseComments = CloseComments;
                ticket.DeliveredTo = DeliveredTo;
                ticket.IsClosed = true;
                InsuranceContext.LicenceTickets.Update(ticket);
                //LicenceTickets();
            }
            return RedirectToAction("LicenceTickets");
        }


        public ActionResult ReopenTicket(string ReopenComments, int Id)
        {
            var ticket = InsuranceContext.LicenceTickets.Single(Convert.ToInt32(Id));
            ticket.IsClosed = false;
            ticket.ReopenComments = ReopenComments;
            InsuranceContext.LicenceTickets.Update(ticket);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UploadFile()
        {
            if (Request.Files.Count > 0)
            {
                try
                {
                    var PolicyNumber = System.Web.HttpContext.Current.Request.Params["PolicyNumber"];
                    var CustomerId = System.Web.HttpContext.Current.Request.Params["CustomerId"];
                    var vehicleId = System.Web.HttpContext.Current.Request.Params["vehicleId"];

                    var Title = System.Web.HttpContext.Current.Request.Params["Title"];
                    var Description = System.Web.HttpContext.Current.Request.Params["Description"];

                    //var customerid = "";
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = Title + "-" + DateTime.Now.ToString("yyyyMMddhhmmss") + "." + testfiles[testfiles.Length - 1].Split('.')[1];
                        }
                        else
                        {
                            fname = Title + "-" + DateTime.Now.ToString("yyyyMMddhhmmss") + "." + file.FileName.Split('.')[1];
                        }


                        //if folder exist : folder name : customer id eg 1,2,3 etc
                        string custfolderpath = @Server.MapPath("~/Documents/" + CustomerId + "/");
                        string policyfolderpath = @Server.MapPath("~/Documents/" + CustomerId + "/" + PolicyNumber + "/");
                        string vehiclefolderpath = @Server.MapPath("~/Documents/" + CustomerId + "/" + PolicyNumber + "/" + vehicleId + "/");

                        if (!Directory.Exists(custfolderpath))
                        {
                            Directory.CreateDirectory(custfolderpath);
                            Directory.CreateDirectory(policyfolderpath);
                            Directory.CreateDirectory(vehiclefolderpath);
                        }
                        else
                        {
                            if (!Directory.Exists(policyfolderpath))
                            {
                                Directory.CreateDirectory(policyfolderpath);
                                Directory.CreateDirectory(vehiclefolderpath);
                            }
                            else
                            {
                                if (!Directory.Exists(vehiclefolderpath))
                                {
                                    Directory.CreateDirectory(vehiclefolderpath);
                                }
                            }
                        }

                        fname = "/Documents/" + CustomerId + "/" + PolicyNumber + "/" + vehicleId + "/" + fname;
                        file.SaveAs(Server.MapPath(fname));

                        PolicyDocument doc = new PolicyDocument();
                        doc.PolicyNumber = PolicyNumber;
                        doc.Title = Title;
                        doc.Description = Description;
                        doc.CustomerId = Convert.ToInt32(CustomerId);
                        doc.FilePath = fname;
                        doc.vehicleId = Convert.ToInt32(vehicleId);
                        InsuranceContext.PolicyDocuments.Insert(doc);
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
            var list = new List<InsuranceClaim.Models.PolicyDocumentModels>();

            try
            {
                var PolicyNumber = System.Web.HttpContext.Current.Request.Params["PolicyNumber"];
                var CustomerId = System.Web.HttpContext.Current.Request.Params["CustomerId"];
                var vehicleId = System.Web.HttpContext.Current.Request.Params["vehicleId"];

                //string[] filePaths = Directory.GetFiles(Server.MapPath("~/Documents/" + CustomerId + "/" + PolicyNumber + "/" + vehicleId + "/"));

                var FileList = InsuranceContext.PolicyDocuments.All(where: $"CustomerId={CustomerId} and PolicyNumber='{PolicyNumber}' and vehicleId={vehicleId}");

                foreach (var item in FileList)
                {
                    var obj = new InsuranceClaim.Models.PolicyDocumentModels();
                    obj.Title = item.Title;
                    obj.Decription = item.Description;
                    obj.FilePath = item.FilePath;
                    obj.id = item.Id;
                    list.Add(obj);
                }

                string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority +
      Request.ApplicationPath.TrimEnd('/') + "/";


                var customerDetails = InsuranceContext.Customers.Single(where: "id=" + CustomerId);
                if (customerDetails != null && customerDetails.BranchId != 0)
                {
                    baseUrl = System.Configuration.ConfigurationManager.AppSettings["SignaturePath"];
                }


                string path = "/Documents/" + CustomerId + "/" + PolicyNumber + "/";
                string filePath = Server.MapPath(path);

                foreach (var files in Directory.GetFiles(filePath))
                {
                    FileInfo info = new FileInfo(files);
                    var fileName = Path.GetFileName(info.FullName);
                    var obj = new InsuranceClaim.Models.PolicyDocumentModels();
                    obj.Title = fileName;
                    obj.Decription = "";
                    string documentPath = baseUrl + path + fileName;
                    obj.FilePath = documentPath;
                    list.Add(obj);
                }

            }
            catch (Exception ex)
            {

            }


            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangePassword()
        {
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            var UserModel = new UserModel();
            var user = UserManager.FindById(User.Identity.GetUserId().ToString());

            UserModel.Email = user.Email;



            ViewBag.message = 0;


            return View(UserModel);
        }

        [HttpPost]
        public ActionResult ChangePassword(UserModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = UserManager.FindByEmail(model.Email);

            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = UserManager.ChangePassword(user.Id, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                ViewBag.message = 1;
                //return RedirectToAction("ChangePassword", "Account");
            }
            else
            {
                var errorMsg = "";

                foreach (var item in result.Errors)
                {
                    errorMsg += item;
                }
                TempData["ErrorMsg"] = errorMsg;

                // ViewBag.message = 2;
                //return RedirectToAction("ChangePassword", "Account");
            }

            return View();
        }

        public ActionResult CustomerDetials(int id = 0)
        {

            CustomerModel custModel = new CustomerModel();
            if (id != 0)
            {


                var summaryDetail = InsuranceContext.SummaryDetails.Single(id);

                string path = Server.MapPath("~/Content/Countries.txt");
                var countries = System.IO.File.ReadAllText(path);
                var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(countries);
                ViewBag.Countries = resultt.countries.OrderBy(x => x.code.Replace("+", ""));

                ViewBag.Cities = InsuranceContext.Cities.All();

                var Cusotmer = InsuranceContext.Customers.Single(summaryDetail.CustomerId);

                custModel = AutoMapper.Mapper.Map<Customer, CustomerModel>(Cusotmer);

                if (Cusotmer != null)
                {
                    var dbUser = UserManager.Users.FirstOrDefault(c => c.Id == Cusotmer.UserID);
                    if (dbUser != null)
                    {
                        custModel.EmailAddress = dbUser.Email; ;
                    }
                }
                Session["SummaryDetailIdView"] = id;

            }
            return View(custModel);

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
            var startDate = Request.Form["CoverStartDate"];
            var endDate = Request.Form["CoverEndDate"];
            if (!string.IsNullOrEmpty(startDate))
            {
                ModelState.Remove("CoverStartDate");
                model.CoverStartDate = Convert.ToDateTime(startDate, usDtfi);
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                ModelState.Remove("CoverEndDate");
                model.CoverEndDate = Convert.ToDateTime(endDate, usDtfi);
            }
            if (ModelState.IsValid)
            {
                model.Id = 0;

                //if (!model.IncludeRadioLicenseCost)
                //{
                //    model.RadioLicenseCost = 0.00m;
                //}


                List<RiskDetailModel> listriskdetailmodel = new List<RiskDetailModel>();
                if (Session["VehicleDetails"] != null)
                {
                    List<RiskDetailModel> listriskdetails = (List<RiskDetailModel>)Session["VehicleDetails"];
                    if (listriskdetails != null && listriskdetails.Count > 0)
                    {
                        listriskdetailmodel = listriskdetails;
                    }
                }
                model.Id = 0;
                listriskdetailmodel.Add(model);
                Session["VehicleDetails"] = listriskdetailmodel;

            }

            return RedirectToAction("SummaryDetail", "Account");


        }

        [HttpPost]
        public async Task<JsonResult> SaveCustomerData(CustomerModel model, string buttonUpdate)
        {
            ModelState.Remove("City");
            ModelState.Remove("CountryCode");

            if (ModelState.IsValid)
            {
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                if (userLoggedin)
                {

                    Session["CustomerDataModal"] = model;
                    return Json(new { IsError = true, error = "" }, JsonRequestBehavior.AllowGet);
                }


            }
            return Json(new { IsError = false, error = TempData["ErrorMessage"].ToString() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProductDetail()
        {

            return RedirectToAction("RiskDetail");
        }

        public ActionResult RiskDetail(int? id = 1)
        {
            if (TempData["RedirectedFrom"] != null && Convert.ToString(TempData["RedirectedFrom"]) == "MyPolicy")
            {
                ViewBag.RedirectedFrom = "MyPolicy";
            }
            else if (TempData["RedirectedFrom"] != null && Convert.ToString(TempData["RedirectedFrom"]) == "PolicyList")
            {
                Session["ViewlistVehicles"] = null;
                ViewBag.RedirectedFrom = "PolicyList";
            }
            else if (TempData["RedirectedFrom"] != null && Convert.ToString(TempData["RedirectedFrom"]) == "QuotationList")
            {
                ViewBag.RedirectedFrom = "QuotationList";
            }
            else
            {
                ViewBag.RedirectedFrom = "PolicyList";
            }
            var viewModel = new RiskDetailModel();

            if (Session["SummaryDetailIdView"] != null)
            {
                SetValueIntoSession(Convert.ToInt32(Session["SummaryDetailIdView"]));
                // id = Convert.ToInt32(Session["SummaryDetailIdView"]);

                viewModel.SummaryId = Convert.ToInt32(Session["SummaryDetailIdView"]);
                // Session["SummaryDetailIdView"] = null;
            }
            else if (Session["SummaryDetailIdView"] == null)
            {
                Session["SummaryDetailIdView"] = id;
                SetValueIntoSession(Convert.ToInt32(Session["SummaryDetailIdView"]));
                viewModel.SummaryId = Convert.ToInt32(Session["SummaryDetailIdView"]);
            }




            ViewBag.Products = InsuranceContext.Products.All().ToList();
            var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm))
                                   select new
                                   {
                                       ID = (int)e,
                                       Name = e.ToString()
                                   };

            ViewBag.ePaymentTermData = new SelectList(ePaymentTermData, "ID", "Name");
            int RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
            var PolicyData = (PolicyDetail)Session["PolicyDataView"];
            // Id is policyid from Policy detail table

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
            viewModel.VehicleUsage = 0;
            //  TempData["Policy"] = service.GetPolicy(id);
            TempData["Policy"] = PolicyData;
            if (makers.Count > 0)
            {
                // If 
                var model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = model;
            }

            //if (Session["ViewlistVehicles"] == null || (Session["ViewlistVehicles"] != null && ((List<RiskDetailModel>)Session["ViewlistVehicles"]).Count == 0))
            //{

            //    var InsService = new InsurerService();
            //    var summaryDetail = InsuranceContext.SummaryDetails.Single(id);
            //    var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={id}").ToList();
            //    var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
            //    var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
            //    List<RiskDetailModel> listVehicles = new List<RiskDetailModel>();


            //    foreach (var item in SummaryVehicleDetails)
            //    {
            //        RiskDetailModel _vehicle = new RiskDetailModel();
            //        VehicleDetail __vehicle = new VehicleDetail();
            //        __vehicle = InsuranceContext.VehicleDetails.Single(item.VehicleDetailsId);

            //        _vehicle = Mapper.Map<VehicleDetail, RiskDetailModel>(__vehicle);

            //        listVehicles.Add(_vehicle);
            //    }
            //    Session["ViewSummaryDetail"] = summaryDetail;
            //    Session["ViewPolicy"] = policy;
            //    Session["ViewlistVehicles"] = listVehicles;

            //}


            viewModel.NoOfCarsCovered = 1;

            if (Session["ViewlistVehicles"] != null)
            {
                var list = (List<RiskDetailModel>)Session["ViewlistVehicles"];
                if (list.Count > 0)
                {
                    viewModel.NoOfCarsCovered = list.Count + 1;
                }

            }


            if (id > 0)
            {
                SummaryDetailService _summaryDetailService = new SummaryDetailService();

                var currenyList = _summaryDetailService.GetAllCurrency();
                var list = (List<RiskDetailModel>)Session["ViewlistVehicles"];
                if (list != null && list.Count > 0 && (list.Count >= id)) // 29_jan_2019
                {
                    var data = (RiskDetailModel)list[Convert.ToInt32(id - 1)];
                    if (data != null)
                    {


                        viewModel.AgentCommissionId = data.AgentCommissionId;
                        viewModel.currency = _summaryDetailService.GetCurrencyName(currenyList, data.CurrencyId);
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

                        viewModel.VehicleUsage = data.VehicleUsage;


                        viewModel.isUpdate = true;
                        viewModel.vehicleindex = Convert.ToInt32(id);

                        var ser = new VehicleService();
                        var model = ser.GetModel(data.MakeId);
                        ViewBag.Model = model;
                    }
                }
            }
            return View(viewModel);
        }


        public void SetValueIntoSession(int summaryId)
        {
            //Session["ICEcashToken"] = null;
            Session["SummaryDetailIdView"] = summaryId;

            var summaryDetail = InsuranceContext.SummaryDetails.Single(summaryId);
            var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summaryId}").ToList();
            var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
            var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
            var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));



            Session["PolicyDataView"] = policy;

            List<RiskDetailModel> listRiskDetail = new List<RiskDetailModel>();
            foreach (var item in SummaryVehicleDetails)
            {
                var _vehicle = InsuranceContext.VehicleDetails.Single(where: $"Id={item.VehicleDetailsId} and IsActive<>0 ");

                if (_vehicle != null)
                {
                    RiskDetailModel riskDetail = Mapper.Map<VehicleDetail, RiskDetailModel>(_vehicle);

                    listRiskDetail.Add(riskDetail);
                }
            }


            // Session["VehicleDetails"] = listRiskDetail;
            Session["ViewlistVehicles"] = listRiskDetail;

            SummaryDetailModel summarymodel = Mapper.Map<SummaryDetail, SummaryDetailModel>(summaryDetail);
            summarymodel.Id = summaryDetail.Id;
            //Session["SummaryDetailed"] = summarymodel;

            Session["ViewSummaryDetail"] = summaryDetail;



        }

        [HttpGet]
        public ActionResult RenewPolicies(int policyId = 0)
        {



            return View();
        }


        [HttpGet]
        public JsonResult getVehicleList()
        {
            try
            {
                if (Session["ViewlistVehicles"] != null)
                {
                    var list = (List<RiskDetailModel>)Session["ViewlistVehicles"];
                    List<VehicleListModel> vehiclelist = new List<VehicleListModel>();
                    SummaryDetailService detailService = new SummaryDetailService();

                    var currencyList = detailService.GetAllCurrency();

                    foreach (var item in list)
                    {
                        VehicleListModel obj = new VehicleListModel();
                        obj.make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'").ShortDescription;
                        obj.model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ShortDescription;
                        obj.covertype = InsuranceContext.CoverTypes.Single(item.CoverTypeId).Name;
                        obj.excess = item.Premium.ToString();
                        obj.premium = Convert.ToString(list.Sum(items => items.Premium + items.ZTSCLevy + items.StampDuty + items.VehicleLicenceFee + (items.IncludeRadioLicenseCost ? items.RadioLicenseCost : 0.00m)));
                        obj.suminsured = item.SumInsured == null ? "0" : item.SumInsured.ToString();
                        obj.CurrencyName = detailService.GetCurrencyName(currencyList, item.CurrencyId);
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

        public ActionResult SummaryDetail(int Id = 0)
        {

            //  var currenyList = _summaryDetailService.GetAllCurrency();
            var _model = new SummaryDetailModel();
            var summaryDetail = Session["ViewSummaryDetail"];
            var listVehicles = Session["ViewlistVehicles"];
            var vehicle = (List<RiskDetailModel>)Session["ViewlistVehicles"];// summary.GetVehicleInformation(id);
            var summarydetail = (SummaryDetail)Session["ViewSummaryDetail"];
            SummaryDetailService SummaryDetailServiceObj = new SummaryDetailService();

            if (summarydetail != null)
            {


                var model = Mapper.Map<SummaryDetail, SummaryDetailModel>(summarydetail);
                model.CarInsuredCount = vehicle.Count;
                model.DebitNote = "INV" + Convert.ToString(SummaryDetailServiceObj.getNewDebitNote());
                model.PaymentMethodId = summarydetail.PaymentMethodId;
                //  model.Currency = _summaryDetailService.GetCurrencyName(currenyList, _vehicle.CustomerId);
                model.PaymentTermId = 1;
                model.ReceiptNumber = "";
                model.SMSConfirmation = false;




                model.TotalPremium = vehicle.Sum(item => item.Premium + item.ZTSCLevy + item.StampDuty + item.VehicleLicenceFee + (item.IncludeRadioLicenseCost ? item.RadioLicenseCost : 0.00m));// + vehicle.StampDuty + vehicle.ZTSCLevy;
                                                                                                                                                                                                  //  model.Currency = _summaryDetailService.GetCurrencyName(currenyList,vehicle.Sum(item=>item.CurrencyId));                                                                                                                                                                                  //model.TotalRadioLicenseCost = vehicle.Sum(item => item.RadioLicenseCost);
                model.TotalStampDuty = vehicle.Sum(item => item.StampDuty);
                model.TotalSumInsured = vehicle.Sum(item => item.SumInsured);
                model.TotalZTSCLevies = vehicle.Sum(item => item.ZTSCLevy);
                model.ExcessBuyBackAmount = vehicle.Sum(item => item.ExcessBuyBackAmount);
                model.MedicalExpensesAmount = vehicle.Sum(item => item.MedicalExpensesAmount);
                model.PassengerAccidentCoverAmount = vehicle.Sum(item => item.PassengerAccidentCoverAmount);
                model.RoadsideAssistanceAmount = vehicle.Sum(item => item.RoadsideAssistanceAmount);
                model.ExcessAmount = vehicle.Sum(item => item.ExcessAmount);
                model.Discount = vehicle.Sum(item => item.Discount);
                decimal radio = 0.00m;
                foreach (var item in vehicle)
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
            return View(_model);
        }

        [HttpGet]
        public async Task<JsonResult> DisablePolicy(int VehicleId)
        {
            try
            {
                string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
                var _vehicle = InsuranceContext.VehicleDetails.Single(VehicleId);
                var costomerId = _vehicle.CustomerId;
                var _customer = InsuranceContext.Customers.Single(costomerId);

                Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
                var policylist = InsuranceContext.PolicyDetails.Single(where: $"CustomerId = {_customer.Id}");
                var _user = UserManager.FindById(_customer.UserID);

                _vehicle.IsActive = false;

                string DeActivePolicyPath = "/Views/Shared/EmaiTemplates/PolicyDeActivation.cshtml";
                string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(DeActivePolicyPath));
                var body = EmailBody2.Replace("##RenewDate##", _vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##path##", filepath).Replace("##FirstName##", _customer.FirstName)
                    .Replace("##LastName##", _customer.LastName).Replace("##Address1##", _customer.AddressLine1).Replace("##Address2##", _customer.AddressLine2)
                    .Replace("##PolicyNumber##", policylist.PolicyNumber).Replace("##RegistrationNo##", _vehicle.RegistrationNo);

                objEmailService.SendEmail(_user.Email, "", "", "Policy DeActivation", body, null);


                string Body = "Hello " + _customer.FirstName + "\nYour Policy Number " + policylist.PolicyNumber + " And Vehicle  " + _vehicle.RegistrationNo + " has been DeActivated ." + "\nThank you .";
                var result = await objsmsService.SendSMS(_customer.Countrycode.Replace("+", "") + _user.PhoneNumber, Body);

                SmsLog objsmslog = new SmsLog()
                {
                    Sendto = _user.PhoneNumber,
                    Body = Body,
                    Response = result,
                    CreatedBy = _customer.Id,
                    CreatedOn = DateTime.Now
                };

                InsuranceContext.SmsLogs.Insert(objsmslog);


                InsuranceContext.VehicleDetails.Update(_vehicle);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
                throw;
            }

        }

        public async Task<JsonResult> ActivateVehicle(int VehicleId)
        {
            try
            {
                string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
                var vehicle = InsuranceContext.VehicleDetails.Single(VehicleId);
                var _customerid = vehicle.CustomerId;
                var customer = InsuranceContext.Customers.Single(_customerid);

                Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
                var policylist = InsuranceContext.PolicyDetails.Single(where: $"CustomerId = {customer.Id}");
                var user = UserManager.FindById(customer.UserID);


                vehicle.IsActive = true;

                string ActivePolicyPath = "/Views/Shared/EmaiTemplates/PolicyActivation.cshtml";
                string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(ActivePolicyPath));
                var body = EmailBody2.Replace("##RenewDate##", vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##path##", filepath).Replace("##FirstName##", customer.FirstName)
                    .Replace("##LastName##", customer.LastName).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2)
                    .Replace("##PolicyNumber##", policylist.PolicyNumber).Replace("##RegistrationNo##", vehicle.RegistrationNo);
                objEmailService.SendEmail(user.Email, "", "", "Policy Activation", body, null);


                string Body = "Hello " + customer.FirstName + "\nYour Policy Number " + policylist.PolicyNumber + " And Vehicle  " + vehicle.RegistrationNo + " has been Activated ." + "\nThank you .";
                var result = await objsmsService.SendSMS(customer.Countrycode.Replace("+", "") + user.PhoneNumber, Body);

                SmsLog objsmslog = new SmsLog()
                {
                    Sendto = user.PhoneNumber,
                    Body = Body,
                    Response = result,
                    CreatedBy = customer.Id,
                    CreatedOn = DateTime.Now
                };

                InsuranceContext.SmsLogs.Insert(objsmslog);

                InsuranceContext.VehicleDetails.Update(vehicle);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
                throw;
            }

        }

        public JsonResult DeleteDocument()
        {
            try
            {
                var docid = System.Web.HttpContext.Current.Request.Params["docid"];

                var document = InsuranceContext.PolicyDocuments.Single(Convert.ToInt32(docid));

                string filelocation = document.FilePath;

                if (System.IO.File.Exists(Server.MapPath(filelocation)))
                {
                    System.IO.File.Delete(Server.MapPath(filelocation));
                }

                InsuranceContext.PolicyDocuments.Delete(document);

                return Json(true, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult PolicyRenewReminderSetting(int? id = 0)
        {
            InsuranceClaim.Models.PolicyRenewReminderSettingViewModel data = new InsuranceClaim.Models.PolicyRenewReminderSettingViewModel();
            var eRemainderType = from ePolicyRenewReminderType e in Enum.GetValues(typeof(ePolicyRenewReminderType))
                                 select new
                                 {
                                     ID = (int)e,
                                     Name = e.ToString()
                                 };

            ViewBag.ePolicyRenewReminder = new SelectList(eRemainderType, "ID", "Name");
            if (id > 0)
            {
                var listofRemindersetting = InsuranceContext.PolicyRenewReminderSettings.Single(id);
                data = new PolicyRenewReminderSettingViewModel() { CreatedBy = listofRemindersetting.CreatedBy, CreatedOn = listofRemindersetting.CreatedOn, Id = listofRemindersetting.Id, ModifiedBy = listofRemindersetting.ModifiedBy, ModifiedOn = listofRemindersetting.ModifiedOn, NoOfDays = listofRemindersetting.NoOfDays, NotificationType = listofRemindersetting.NotificationType, Email = listofRemindersetting.Email, SMS = listofRemindersetting.SMS };
            }

            return View(data);
        }
        [HttpPost]
        public ActionResult SavePolicyRenewReminderSetting(PolicyRenewReminderSettingViewModel model)
        {
            PolicyRenewReminderSetting obje = new Insurance.Domain.PolicyRenewReminderSetting();
            var _customerData = InsuranceContext.Customers.Single(where: $"UserId ='{User.Identity.GetUserId().ToString()}'");

            if (ModelState.IsValid)
            {
                if (model.Id == 0 || model.Id == null)
                {
                    obje.NoOfDays = model.NoOfDays;
                    obje.NotificationType = 0;
                    obje.SMS = model.SMS;
                    obje.Email = model.Email;
                    obje.CreatedBy = _customerData.Id;
                    obje.CreatedOn = DateTime.Now;
                    obje.ModifiedBy = _customerData.Id;
                    obje.ModifiedOn = DateTime.Now;
                    InsuranceContext.PolicyRenewReminderSettings.Insert(obje);
                }
                else
                {
                    var data = InsuranceContext.PolicyRenewReminderSettings.Single(model.Id);
                    //obje.Id = model.Id;
                    data.NoOfDays = model.NoOfDays;
                    data.NotificationType = 0;
                    data.SMS = model.SMS;
                    data.Email = model.Email;
                    data.ModifiedBy = _customerData.Id;
                    data.ModifiedOn = DateTime.Now;
                    InsuranceContext.PolicyRenewReminderSettings.Update(data);
                }

            }

            return RedirectToAction("ListPolicyRenewReminderSetting");
        }
        public ActionResult ListPolicyRenewReminderSetting()
        {
            var PolicySettingList = InsuranceContext.PolicyRenewReminderSettings.All().OrderByDescending(x => x.Id).ToList();

            return View(PolicySettingList);
        }
        public ActionResult DeletePolicyRenewReminderSetting(int Id)
        {
            string query = $"Delete PolicyRenewReminderSetting  where Id = {Id}";
            InsuranceContext.PolicyRenewReminderSettings.Execute(query);



            return RedirectToAction("ListPolicyRenewReminderSetting");
        }
        public async Task<JsonResult> ReminderEmailsSmsLogic()
        {
            string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
            var EmailsContant = InsuranceContext.PolicyRenewReminderSettings.All(where: $"NotificationType = {Convert.ToInt32(ePolicyRenewReminderType.Email)}");
            var SmsContant = InsuranceContext.PolicyRenewReminderSettings.All(where: $"NotificationType = {Convert.ToInt32(ePolicyRenewReminderType.Email)}");
            var LicenceTickets = InsuranceContext.LicenceTickets.All(where: $"CAST(CreatedDate as date) <= '{DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")}'");
            var VehicleMakes = InsuranceContext.VehicleMakes.All();
            var VehicleModels = InsuranceContext.VehicleModels.All();

            var customerData = InsuranceContext.Customers.Single(where: $"UserId ='{User.Identity.GetUserId().ToString()}'");
            var policylist = InsuranceContext.PolicyDetails.Single(where: $"CustomerId = {customerData.Id}");
            var user = UserManager.FindById(customerData.UserID);
            Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
            var now = DateTime.Now;
            foreach (var item in EmailsContant)
            {
                var _vehicleList = InsuranceContext.VehicleDetails.All(where: $"RenewalDate = '{new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddDays(item.NoOfDays)}'").ToList();
                foreach (var _item in _vehicleList)
                {
                    string make = VehicleMakes.Where(x => x.MakeCode == _item.MakeId).Select(x => x.ShortDescription).FirstOrDefault();
                    string model = VehicleModels.Where(x => x.ModelCode == _item.ModelId).Select(x => x.ShortDescription).FirstOrDefault();

                    string ReminderEmailPath = "/Views/Shared/EmaiTemplates/RenewReminderEmail.cshtml";
                    string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(ReminderEmailPath));
                    var body = EmailBody2.Replace("##RenewDate##", _item.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##path##", filepath)
                        .Replace("##FirstName##", customerData.FirstName).Replace("##LastName##", customerData.LastName).Replace("##Address1##", customerData.AddressLine1)
                        .Replace("##Address2##", customerData.AddressLine2).Replace("##numberofDays##", item.NoOfDays.ToString()).Replace("##PolicyNumber##", policylist.PolicyNumber)
                        .Replace("##Make##", make).Replace("##Model##", model);
                    try
                    {
                        objEmailService.SendEmail(user.Email, "", "", "Renew/Repay Next Term Premium of Your Policy | 21 Days Left", body, null);
                    }
                    catch (Exception ex)
                    {
                        ReminderFailed(body, user.Email, "Renew/Repay Next Term Premium of Your Policy | 21 Days Left", Convert.ToInt32(ePolicyRenewReminderType.Email));


                    }
                }
            }
            foreach (var item in SmsContant)
            {
                var vehicleList = InsuranceContext.VehicleDetails.All(where: $"RenewalDate = '{new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddDays(item.NoOfDays)}'").ToList();
                foreach (var items in vehicleList)
                {
                    string make = VehicleMakes.Where(x => x.MakeCode == items.MakeId).Select(x => x.ShortDescription).FirstOrDefault();
                    string model = VehicleModels.Where(x => x.ModelCode == items.ModelId).Select(x => x.ShortDescription).FirstOrDefault();


                    string body = "Hello " + customerData.FirstName + "\nYour Vehicle " + make + " " + model + " will Expire in " + item.NoOfDays + " days i.e on " + items.RenewalDate.Value.ToString("dd/MM/yyyy") + ". Please Renew/Repay for your next Payment Term before the Renewal date of " + items.RenewalDate.Value.ToString("dd/MM/yyyy") + " to continue your services otherwise your vehicle will get Lapsed." + "\nThank you.";

                    try
                    {
                        var result = await objsmsService.SendSMS(customerData.Countrycode.Replace("+", "") + user.PhoneNumber, body);
                        SmsLog objsmslog = new SmsLog()
                        {
                            Sendto = user.PhoneNumber,
                            Body = body,
                            Response = result,
                            CreatedBy = customerData.Id,
                            CreatedOn = DateTime.Now
                        };

                        InsuranceContext.SmsLogs.Insert(objsmslog);

                    }
                    catch (Exception ex)
                    {
                        ReminderFailed(body, user.PhoneNumber, "", Convert.ToInt32(ePolicyRenewReminderType.SMS));

                    }
                }

            }

            foreach (var _item in LicenceTickets)
            {
                //string _filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
                var vehicle = InsuranceContext.VehicleDetails.Single(_item.VehicleId);

                string make = VehicleMakes.Where(x => x.MakeCode == vehicle.MakeId).Select(x => x.ShortDescription).FirstOrDefault();
                string model = VehicleModels.Where(x => x.ModelCode == vehicle.ModelId).Select(x => x.ShortDescription).FirstOrDefault();

                string ReminderEmailPath = "/Views/Shared/EmaiTemplates/LicenceDiskReminder.cshtml";
                string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(ReminderEmailPath));
                var body = EmailBody2.Replace("##PolicyNumber##", policylist.PolicyNumber).Replace("##path##", filepath).Replace("##Make##", make).Replace("##Model##", model)
                    .Replace("##TransactionDate##", vehicle.TransactionDate.Value.ToString("dd/MM/yyyy"));
                try
                {
                    objEmailService.SendEmail(AdminEmail, "", "", "Licence Disk Not Delivered | Policy NUmber - " + _item.PolicyNumber, body, null);
                }
                catch (Exception ex)
                {
                    ReminderFailed(body, AdminEmail, "Licence Disk Not Delivered | Policy NUmber - " + _item.PolicyNumber, Convert.ToInt32(ePolicyRenewReminderType.Email));
                }
            }

            var FailedEmailsList = InsuranceContext.ReminderFaileds.All().ToList();

            if (FailedEmailsList != null && FailedEmailsList.Count > 0)
            {
                foreach (var item in FailedEmailsList)
                {
                    try
                    {
                        objEmailService.SendEmail(item.SendTo, "", "", item.EmailSubject, item.EmailBody, null);
                        InsuranceContext.ReminderFaileds.Delete(item);
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public void ReminderFailed(string body, string SendTo, string subject, int NotificationType)
        {
            ReminderFailed obj = new Insurance.Domain.ReminderFailed();
            obj.EmailBody = body;
            obj.SendTo = SendTo;
            obj.EmailSubject = subject;
            obj.NotificationType = NotificationType;
            obj.ModifiedBy = 0;
            obj.ModifiedOn = DateTime.Now;
            obj.CreatedBy = 0;
            obj.CreatedOn = DateTime.Now;
            InsuranceContext.ReminderFaileds.Insert(obj);
        }

        public ActionResult QuotationList()
        {

            //bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
            var role = UserManager.GetRoles(_User.Id.ToString()).FirstOrDefault();
            TempData["RedirectedFrom"] = "QuotationList";
            Session["ViewlistVehicles"] = null;
            ListPolicy policylist = new ListPolicy();
            policylist.listpolicy = new List<PolicyListViewModel>();
            var customerID = InsuranceContext.Customers.Single(where: $"userid='{User.Identity.GetUserId().ToString()}'").Id;



            var SummaryList = new List<SummaryDetail>();

            if (role == "Staff")
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"CreatedBy={customerID} and isQuotation = 'True'").OrderByDescending(x => x.Id).ToList();
            }
            else if (role == "Administrator")
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: "isQuotation = 'True'").OrderByDescending(x => x.Id).ToList();
            }
            else
            {
                SummaryList = InsuranceContext.SummaryDetails.All(where: $"customerid={customerID} and isQuotation = 'True'").OrderByDescending(x => x.Id).ToList();
            }

            SummaryDetailService detailService = new SummaryDetailService();

            var currencyList = detailService.GetAllCurrency();

            foreach (var item in SummaryList.Take(100))
            {
                PolicyListViewModel policylistviewmodel = new PolicyListViewModel();
                if (item.isQuotation == true)
                {
                    policylistviewmodel.Vehicles = new List<VehicleReinsuranceViewModel>();
                    policylistviewmodel.TotalPremium = Convert.ToDecimal(item.TotalPremium);
                    policylistviewmodel.TotalSumInsured = Convert.ToDecimal(item.TotalSumInsured);
                    policylistviewmodel.PaymentMethodId = Convert.ToInt32(item.PaymentMethodId);
                    policylistviewmodel.CustomerId = Convert.ToInt32(item.CustomerId);
                    policylistviewmodel.CustomerEmail = GetCustomerEmailbyCustomerID(item.CustomerId);
                    policylistviewmodel.SummaryId = item.Id;
                    policylistviewmodel.createdOn = Convert.ToDateTime(item.CreatedOn);

                    var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={item.Id}").ToList();

                    if (SummaryVehicleDetails.Count > 0)
                    {

                        var vehicle = InsuranceContext.VehicleDetails.Single(where: $" Id='{SummaryVehicleDetails[0].VehicleDetailsId}'");


                        if (vehicle != null)
                        {
                            var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
                            var product = InsuranceContext.Products.Single(Convert.ToInt32(vehicle.ProductId));
                            policylistviewmodel.PolicyNumber = policy.PolicyNumber;

                            policylistviewmodel.Currency = detailService.GetCurrencyName(currencyList, vehicle.CurrencyId);

                            int increment = 0;
                            var _vehicle = new VehicleDetail();
                            decimal premiumAmount = 0;
                            decimal sumInsured = 0;

                            foreach (var _item in SummaryVehicleDetails)
                            {
                                _vehicle = InsuranceContext.VehicleDetails.Single(_item.VehicleDetailsId);

                                if (_vehicle != null)
                                {
                                    premiumAmount = premiumAmount + Convert.ToDecimal(_vehicle.Premium + _vehicle.StampDuty + _vehicle.ZTSCLevy + (Convert.ToBoolean(_vehicle.IncludeRadioLicenseCost) ? Convert.ToDecimal(_vehicle.RadioLicenseCost) : 0.00m));
                                    sumInsured = sumInsured + Convert.ToDecimal(_vehicle.SumInsured);
                                }
                            }

                            if (_vehicle != null)
                            {
                                VehicleReinsuranceViewModel obj = new VehicleReinsuranceViewModel();
                                //  var _reinsurenaceTrans = InsuranceContext.ReinsuranceTransactions.All(where: $"SummaryDetailId={item.Id} and VehicleId={_item.VehicleDetailsId}").ToList();

                                obj.CoverType = Convert.ToInt32(_vehicle.CoverTypeId);
                                obj.isReinsurance = (_vehicle.SumInsured > 100000 ? true : false);
                                obj.MakeId = _vehicle.MakeId;
                                obj.ModelId = _vehicle.ModelId;
                                //obj.Premium = Convert.ToDecimal(_vehicle.Premium);
                                obj.RegisterationNumber = _vehicle.RegistrationNo;
                                obj.SumInsured = sumInsured;
                                obj.VehicleId = _vehicle.Id;
                                obj.startdate = Convert.ToDateTime(_vehicle.CoverStartDate);
                                obj.enddate = Convert.ToDateTime(_vehicle.CoverEndDate);
                                obj.RenewalDate = Convert.ToDateTime(_vehicle.RenewalDate);
                                obj.isLapsed = _vehicle.isLapsed;
                                obj.BalanceAmount = Convert.ToDecimal(_vehicle.BalanceAmount);
                                obj.isActive = Convert.ToBoolean(_vehicle.IsActive);
                                obj.Premium = premiumAmount;
                                policylistviewmodel.Vehicles.Add(obj);
                                policylist.listpolicy.Add(policylistviewmodel);
                            }
                        }
                    }
                }
            }


            return View(policylist);
        }

        //public ActionResult EndorsementDetials(int id = 0)
        //{
        //    CustomerModel custModel = new CustomerModel();
        //    if (id != 0)
        //    {
        //        var endorseSummaryDetail = InsuranceContext.EndorsementSummaryDetails.All(where: $"SummaryId={id}").FirstOrDefault();
        //        if (endorseSummaryDetail == null)
        //        {
        //            var summaryDetail = InsuranceContext.SummaryDetails.Single(id);
        //            var Cusotmer = InsuranceContext.Customers.Single(summaryDetail.CustomerId);
        //            custModel = AutoMapper.Mapper.Map<Customer, CustomerModel>(Cusotmer);

        //            if (Cusotmer != null)
        //            {
        //                var dbUser = UserManager.Users.FirstOrDefault(c => c.Id == Cusotmer.UserID);
        //                if (dbUser != null)
        //                {
        //                    custModel.EmailAddress = dbUser.Email; ;
        //                }
        //            }
        //            Session["SummaryDetailIdView"] = id;
        //        }
        //        else
        //        {
        //            var Cusotmer = InsuranceContext.Customers.Single(endorseSummaryDetail.CustomerId);
        //            custModel = AutoMapper.Mapper.Map<Customer, CustomerModel>(Cusotmer);

        //            if (Cusotmer != null)
        //            {
        //                var dbUser = UserManager.Users.FirstOrDefault(c => c.Id == Cusotmer.UserID);
        //                if (dbUser != null)
        //                {
        //                    custModel.EmailAddress = dbUser.Email; ;
        //                }
        //            }
        //            Session["SummaryDetailIdView"] = id;
        //        }

        //        string path = Server.MapPath("~/Content/Countries.txt");
        //        var countries = System.IO.File.ReadAllText(path);
        //        var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(countries);
        //        ViewBag.Countries = resultt.countries.OrderBy(x => x.code.Replace("+", ""));

        //        ViewBag.Cities = InsuranceContext.Cities.All();

        //    }
        //    return View(custModel);
        //}


        //public ActionResult EndorsementRiskDetails(int? id = 1)
        //{
        //    if (TempData["RedirectedFrom"] != null && Convert.ToString(TempData["RedirectedFrom"]) == "MyPolicy")
        //    {
        //        ViewBag.RedirectedFrom = "MyPolicy";
        //    }
        //    else if (TempData["RedirectedFrom"] != null && Convert.ToString(TempData["RedirectedFrom"]) == "PolicyList")
        //    {
        //        ViewBag.RedirectedFrom = "PolicyList";
        //    }
        //    else if (TempData["RedirectedFrom"] != null && Convert.ToString(TempData["RedirectedFrom"]) == "QuotationList")
        //    {
        //        ViewBag.RedirectedFrom = "QuotationList";
        //    }
        //    else
        //    {
        //        ViewBag.RedirectedFrom = "PolicyList";
        //    }
        //    var viewModel = new EndorsementRiskDetailModel();

        //    if (Session["SummaryDetailIdView"] != null)
        //    {

        //        InsertEndersoment(Convert.ToInt32(Session["SummaryDetailIdView"]));

        //        SetEndorsementValueIntoSession(Convert.ToInt32(Session["SummaryDetailIdView"]));
        //        // id = Convert.ToInt32(Session["SummaryDetailIdView"]);

        //        viewModel.SummaryId = Convert.ToInt32(Session["SummaryDetailIdView"]);
        //        // Session["SummaryDetailIdView"] = null;
        //    }


        //    var eExcessTypeData = from eExcessType e in Enum.GetValues(typeof(eExcessType))
        //                          select new
        //                          {
        //                              ID = (int)e,
        //                              Name = e.ToString()
        //                          };

        //    ViewBag.eExcessTypeData = new SelectList(eExcessTypeData, "ID", "Name");

        //    ViewBag.Products = InsuranceContext.Products.All().ToList();
        //    var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm))
        //                           select new
        //                           {
        //                               ID = (int)e,
        //                               Name = e.ToString()
        //                           };

        //    ViewBag.ePaymentTermData = new SelectList(ePaymentTermData, "ID", "Name");
        //    int RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
        //    var PolicyData = (PolicyDetail)Session["PolicyDataView"];
        //    // Id is policyid from Policy detail table

        //    var service = new VehicleService();

        //    ViewBag.VehicleUsage = service.GetAllVehicleUsage();
        //    viewModel.NumberofPersons = 0;
        //    viewModel.AddThirdPartyAmount = 0.00m;
        //    viewModel.RadioLicenseCost = Convert.ToDecimal(RadioLicenseCosts);
        //    var makers = service.GetMakers();
        //    ViewBag.CoverType = service.GetCoverType();
        //    ViewBag.AgentCommission = service.GetAgentCommission();
        //    ViewBag.Sources = InsuranceContext.BusinessSources.All();
        //    ViewBag.Makers = makers;
        //    viewModel.isUpdate = false;
        //    viewModel.VehicleUsage = 0;
        //    //  TempData["Policy"] = service.GetPolicy(id);
        //    TempData["Policy"] = PolicyData;
        //    if (makers.Count > 0)
        //    {
        //        var model = service.GetModel(makers.FirstOrDefault().MakeCode);
        //        ViewBag.Model = model;
        //    }

        //    viewModel.NoOfCarsCovered = 1;

        //    if (Session["ViewlistVehicles"] != null)
        //    {
        //        var list = (List<EndorsementRiskDetailModel>)Session["ViewlistVehicles"];
        //        if (list.Count > 0)
        //        {
        //            viewModel.NoOfCarsCovered = list.Count + 1;
        //        }
        //    }


        //    if (id > 0)
        //    {
        //        var list = (List<EndorsementRiskDetailModel>)Session["ViewlistVehicles"];
        //        if (list != null && list.Count > 0 && (list.Count >= id))
        //        {
        //            var data = (EndorsementRiskDetailModel)list[Convert.ToInt32(id - 1)];
        //            if (data != null)
        //            {
        //                viewModel.AgentCommissionId = data.AgentCommissionId;
        //                viewModel.ChasisNumber = data.ChasisNumber;
        //                viewModel.CoverEndDate = data.CoverEndDate;
        //                viewModel.CoverNoteNo = data.CoverNoteNo;
        //                viewModel.CoverStartDate = data.CoverStartDate;
        //                viewModel.CoverTypeId = data.CoverTypeId;
        //                viewModel.CubicCapacity = data.CubicCapacity==null ? 0 :(int)Math.Round(data.CubicCapacity.Value, 0);
        //                viewModel.CustomerId = data.CustomerId;
        //                viewModel.EngineNumber = data.EngineNumber;
        //                // viewModel.Equals = data.Equals;
        //                viewModel.Excess = (int)Math.Round(data.Excess, 0);
        //                viewModel.ExcessType = data.ExcessType;
        //                viewModel.MakeId = data.MakeId;
        //                viewModel.ModelId = data.ModelId;
        //                viewModel.NoOfCarsCovered = id;
        //                viewModel.OptionalCovers = data.OptionalCovers;
        //                viewModel.PolicyId = data.PolicyId;
        //                viewModel.Premium = data.Premium;
        //                viewModel.RadioLicenseCost = (int)Math.Round(data.RadioLicenseCost == null ? 0 : data.RadioLicenseCost.Value, 0);
        //                viewModel.Rate = data.Rate;
        //                viewModel.RegistrationNo = data.RegistrationNo;
        //                viewModel.StampDuty = data.StampDuty;
        //                viewModel.SumInsured = (int)Math.Round(data.SumInsured == null ? 0 : data.SumInsured.Value, 0);
        //                viewModel.VehicleColor = data.VehicleColor;
        //                viewModel.VehicleUsage = data.VehicleUsage;
        //                viewModel.VehicleYear = data.VehicleYear;
        //                viewModel.Id = data.Id;
        //                viewModel.ZTSCLevy = data.ZTSCLevy;
        //                viewModel.NumberofPersons = data.NumberofPersons;
        //                viewModel.PassengerAccidentCover = data.PassengerAccidentCover;
        //                viewModel.IsLicenseDiskNeeded = data.IsLicenseDiskNeeded;
        //                viewModel.ExcessBuyBack = data.ExcessBuyBack;
        //                viewModel.RoadsideAssistance = data.RoadsideAssistance;
        //                viewModel.MedicalExpenses = data.MedicalExpenses;
        //                viewModel.Addthirdparty = data.Addthirdparty;
        //                viewModel.AddThirdPartyAmount = data.AddThirdPartyAmount;
        //                viewModel.ExcessAmount = data.ExcessAmount;
        //                viewModel.ProductId = data.ProductId;
        //                viewModel.PaymentTermId = data.PaymentTermId;
        //                viewModel.IncludeRadioLicenseCost = data.IncludeRadioLicenseCost;
        //                viewModel.Discount = data.Discount;
        //                viewModel.VehicleLicenceFee = Convert.ToDecimal(data.VehicleLicenceFee);
        //                viewModel.BusinessSourceId = data.BusinessSourceId;
        //                viewModel.VehicleUsage = data.VehicleUsage;
        //                viewModel.isUpdate = true;
        //                viewModel.vehicleindex = Convert.ToInt32(id);
        //                viewModel.VehicleId = data.VehicleId;

        //                var ser = new VehicleService();
        //                var model = ser.GetModel(data.MakeId);
        //                ViewBag.Model = model;
        //            }
        //        }
        //    }
        //    return View(viewModel);
        //}

        //public void InsertEndersoment(int summaryId)
        //{

        //    var summaryDetail = InsuranceContext.SummaryDetails.Single(summaryId);
        //    var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summaryId}").ToList();

        //    int vehicalId = 0;
        //    if (SummaryVehicleDetails.Count > 0)
        //    {
        //        vehicalId = SummaryVehicleDetails[0].VehicleDetailsId;
        //    }

        //    bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

        //    var VehicelDetail = InsuranceContext.EndorsementVehicleDetails.Single(where: $"VehicleId={vehicalId}");


        //    if (VehicelDetail == null)
        //    {


        //        EndorsementSummaryDetail endorsementSummaryDetail = new EndorsementSummaryDetail();
        //        endorsementSummaryDetail.SummaryId = summaryDetail.Id;
        //        endorsementSummaryDetail.VehicleDetailId = summaryDetail.VehicleDetailId;
        //        endorsementSummaryDetail.CustomerId = summaryDetail.CustomerId;
        //        endorsementSummaryDetail.PaymentTermId = summaryDetail.PaymentTermId;
        //        endorsementSummaryDetail.PaymentMethodId = summaryDetail.PaymentMethodId;
        //        endorsementSummaryDetail.TotalSumInsured = summaryDetail.TotalSumInsured;
        //        endorsementSummaryDetail.TotalPremium = summaryDetail.TotalPremium;
        //        endorsementSummaryDetail.TotalStampDuty = summaryDetail.TotalStampDuty;
        //        endorsementSummaryDetail.TotalZTSCLevies = summaryDetail.TotalZTSCLevies;
        //        endorsementSummaryDetail.TotalRadioLicenseCost = summaryDetail.TotalRadioLicenseCost;
        //        endorsementSummaryDetail.AmountPaid = summaryDetail.AmountPaid;
        //        endorsementSummaryDetail.DebitNote = summaryDetail.DebitNote;
        //        endorsementSummaryDetail.ReceiptNumber = summaryDetail.ReceiptNumber;
        //        endorsementSummaryDetail.SMSConfirmation = summaryDetail.SMSConfirmation;
        //        endorsementSummaryDetail.CreatedOn = summaryDetail.CreatedOn;
        //        var _customerData = new Customer();
        //        if (_userLoggedin)
        //        {
        //            var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
        //             _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();
        //            endorsementSummaryDetail.CreatedBy = _customerData.Id;
        //        }

        //        endorsementSummaryDetail.ModifiedOn = summaryDetail.ModifiedOn;
        //        endorsementSummaryDetail.ModifiedBy = summaryDetail.ModifiedBy;
        //        endorsementSummaryDetail.IsActive = summaryDetail.IsActive;
        //        endorsementSummaryDetail.BalancePaidDate = summaryDetail.BalancePaidDate;

        //        endorsementSummaryDetail.Notes = summaryDetail.Notes;
        //        endorsementSummaryDetail.isQuotation = summaryDetail.isQuotation;

        //        InsuranceContext.EndorsementSummaryDetails.Insert(endorsementSummaryDetail);


        //        foreach (var item in SummaryVehicleDetails)
        //        {
        //            var vehicle = InsuranceContext.VehicleDetails.Single(where: $"Id={item.VehicleDetailsId}");
        //            var VelicleDetailEndersoment = InsuranceContext.EndorsementVehicleDetails.Single(where: $"VehicleId={vehicle.Id}");

        //            if (VelicleDetailEndersoment == null)
        //            {
        //                var vehicleInsert = new EndorsementVehicleDetail();
        //                vehicleInsert.VehicleId = vehicle.Id;
        //                vehicleInsert.NoOfCarsCovered = vehicle.NoOfCarsCovered;
        //                vehicleInsert.PolicyId = vehicle.PolicyId;
        //                vehicleInsert.RegistrationNo = vehicle.RegistrationNo;
        //                vehicleInsert.CustomerId = vehicle.CustomerId;
        //                vehicleInsert.MakeId = vehicle.MakeId;
        //                vehicleInsert.ModelId = vehicle.ModelId;
        //                vehicleInsert.ModelId = vehicle.ModelId;
        //                vehicleInsert.CubicCapacity = vehicle.CubicCapacity;
        //                vehicleInsert.VehicleYear = vehicle.VehicleYear;
        //                 vehicleInsert.EngineNumber = vehicle.EngineNumber;
        //                vehicleInsert.ChasisNumber = vehicle.ChasisNumber;
        //                vehicleInsert.VehicleColor = vehicle.VehicleColor;
        //                vehicleInsert.VehicleUsage = vehicle.VehicleUsage;
        //                vehicleInsert.CoverTypeId = vehicle.CoverTypeId;
        //                vehicleInsert.CoverTypeId = vehicle.CoverTypeId;
        //                vehicleInsert.CoverStartDate = vehicle.CoverStartDate;
        //                vehicleInsert.CoverEndDate = vehicle.CoverEndDate;

        //                vehicleInsert.SumInsured = vehicle.SumInsured;
        //                vehicleInsert.Premium = vehicle.Premium;
        //                vehicleInsert.AgentCommissionId = vehicle.AgentCommissionId;
        //                vehicleInsert.Rate = vehicle.Rate;
        //                vehicleInsert.StampDuty = vehicle.StampDuty;
        //                vehicleInsert.ZTSCLevy = vehicle.ZTSCLevy;
        //                vehicleInsert.RadioLicenseCost = vehicle.RadioLicenseCost;
        //                vehicleInsert.OptionalCovers = vehicle.OptionalCovers;
        //                vehicleInsert.Excess = vehicle.Excess;
        //                vehicleInsert.CoverNoteNo = vehicle.CoverNoteNo;
        //                vehicleInsert.ExcessType = vehicle.ExcessType;
        //                vehicleInsert.ExcessType = vehicle.ExcessType;
        //                vehicleInsert.CreatedOn = DateTime.Now;
        //                if (_userLoggedin)
        //                {
        //                    var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
        //                    _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();
        //                    vehicleInsert.CreatedBy = _customerData.Id;
        //                }

        //                vehicleInsert.IsActive = vehicle.IsActive;
        //                vehicleInsert.Addthirdparty = vehicle.Addthirdparty;
        //                vehicleInsert.Addthirdparty = vehicle.Addthirdparty;
        //                vehicleInsert.AddThirdPartyAmount = vehicle.AddThirdPartyAmount;




        //                vehicleInsert.PassengerAccidentCoverAmount = vehicle.PassengerAccidentCoverAmount == null ? 0 : vehicle.PassengerAccidentCoverAmount;
        //                vehicleInsert.ExcessBuyBackAmount = vehicle.ExcessBuyBackAmount == null ? 0 : vehicle.ExcessBuyBackAmount;

        //                vehicleInsert.NumberofPersons = vehicle.NumberofPersons;
        //                vehicleInsert.IsLicenseDiskNeeded = vehicle.IsLicenseDiskNeeded;


        //                vehicleInsert.PassengerAccidentCoverAmountPerPerson = vehicle.PassengerAccidentCoverAmountPerPerson == null ? 0 : vehicle.PassengerAccidentCoverAmountPerPerson;
        //                vehicleInsert.ExcessBuyBackPercentage = vehicle.ExcessBuyBackPercentage == null ? 0 : vehicle.ExcessBuyBackPercentage;

        //                vehicleInsert.PaymentTermId = vehicle.PaymentTermId;
        //                vehicleInsert.ProductId = vehicle.ProductId;
        //                vehicleInsert.RoadsideAssistanceAmount = vehicle.RoadsideAssistanceAmount == null ? 0 : vehicle.RoadsideAssistanceAmount;
        //                vehicleInsert.MedicalExpensesAmount = vehicle.MedicalExpensesAmount == null ? 0 : vehicle.MedicalExpensesAmount;

        //                vehicleInsert.ExcessAmount = vehicle.ExcessAmount;

        //                vehicleInsert.TransactionDate = DateTime.Now;
        //                vehicleInsert.RenewalDate = vehicle.RenewalDate;
        //                vehicleInsert.IncludeRadioLicenseCost = vehicle.IncludeRadioLicenseCost;
        //                vehicleInsert.InsuranceId = vehicle.InsuranceId;
        //                vehicleInsert.InsuranceId = vehicle.InsuranceId;
        //                vehicleInsert.AnnualRiskPremium = vehicle.AnnualRiskPremium == null ? 0 : vehicle.AnnualRiskPremium;
        //                vehicleInsert.TermlyRiskPremium = vehicle.TermlyRiskPremium == null ? 0 : vehicle.TermlyRiskPremium;
        //                vehicleInsert.QuaterlyRiskPremium = vehicle.QuaterlyRiskPremium == null ? 0 : vehicle.QuaterlyRiskPremium;
        //                vehicleInsert.Discount = vehicle.Discount;

        //                vehicleInsert.isLapsed = vehicle.isLapsed;
        //                vehicleInsert.BalanceAmount = vehicle.BalanceAmount;
        //                vehicleInsert.VehicleLicenceFee = vehicle.VehicleLicenceFee;
        //                vehicleInsert.BusinessSourceId = vehicle.BusinessSourceDetailId;
        //                vehicleInsert.CurrencyId = vehicle.CurrencyId;

        //                vehicleInsert.RoadsideAssistancePercentage = vehicle.RoadsideAssistancePercentage == null ? 0 : vehicle.RoadsideAssistancePercentage;
        //                vehicleInsert.MedicalExpensesPercentage = vehicle.MedicalExpensesPercentage == null ? 0 : vehicle.MedicalExpensesPercentage;

        //                InsuranceContext.EndorsementVehicleDetails.Insert(vehicleInsert);

        //            }



        //            EndorsementSummaryVehicleDetail summaryVehicalDetials = new EndorsementSummaryVehicleDetail();

        //            summaryVehicalDetials.SummaryDetailId = summaryId;
        //            summaryVehicalDetials.VehicleDetailsId = item.VehicleDetailsId;
        //            summaryVehicalDetials.CreatedOn = DateTime.Now;
        //            summaryVehicalDetials.CreatedBy = _customerData.Id;
        //            summaryVehicalDetials.ModifiedOn = DateTime.Now;
        //            summaryVehicalDetials.ModifiedBy = _customerData.Id;

        //            InsuranceContext.EndorsementSummaryVehicleDetails.Insert(summaryVehicalDetials);

        //        }
        //    }
        //}

        //[HttpGet]
        //public JsonResult getEndorsementVehicleList()
        //{
        //    try
        //    {
        //        if (Session["ViewlistVehicles"] != null)
        //        {
        //            var list = (List<EndorsementRiskDetailModel>)Session["ViewlistVehicles"];
        //            List<VehicleListModel> vehiclelist = new List<VehicleListModel>();

        //            foreach (var item in list)
        //            {
        //                VehicleListModel obj = new VehicleListModel();
        //                obj.make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'").ShortDescription;
        //                obj.model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ShortDescription;
        //                obj.covertype = InsuranceContext.CoverTypes.Single(item.CoverTypeId).Name;
        //                obj.premium = item.Premium.ToString();
        //                obj.suminsured = item.SumInsured.ToString();
        //                obj.ZTSCLevy = item.ZTSCLevy == null ? "0" : item.ZTSCLevy.ToString();
        //                vehiclelist.Add(obj);
        //            }

        //            return Json(vehiclelist, JsonRequestBehavior.AllowGet);
        //        }
        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {

        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }

        //}

        //[HttpGet]
        //public JsonResult GetEndorsementLicenseAddress()
        //{
        //    var customerData = (CustomerModel)Session["CustomerDataModal"];
        //    //LicenseAddress licenseAddress = new LicenseAddress();
        //    EndorsementRiskDetailModel riskDetailModel = new EndorsementRiskDetailModel();
        //    riskDetailModel.LicenseAddress1 = customerData.AddressLine1;
        //    riskDetailModel.LicenseAddress2 = customerData.AddressLine2;
        //    riskDetailModel.LicenseCity = customerData.City;
        //    return Json(riskDetailModel, JsonRequestBehavior.AllowGet);
        //}

        //public void SetEndorsementValueIntoSession(int summaryId)
        //{
        //    Session["SummaryDetailIdView"] = summaryId;

        //    var endorsesummaryDetail = InsuranceContext.EndorsementSummaryDetails.Single(where: $"SummaryId={summaryId}");
        //    var endorseSummaryVehicleDetails = InsuranceContext.EndorsementSummaryVehicleDetails.All(where: $"SummaryDetailId={summaryId}").ToList();


        //    var endorsevehicle = InsuranceContext.EndorsementVehicleDetails.All(where: $"VehicleId={endorseSummaryVehicleDetails[0].VehicleDetailsId}").FirstOrDefault();
        //    var endorsepolicy = InsuranceContext.PolicyDetails.Single(endorsevehicle.PolicyId);
        //    var endorseproduct = InsuranceContext.Products.Single(Convert.ToInt32(endorsepolicy.PolicyName));
        //    Session["PolicyDataView"] = endorsepolicy;

        //    List<EndorsementRiskDetailModel> listRiskDetail = new List<EndorsementRiskDetailModel>();
        //    foreach (var item in endorseSummaryVehicleDetails)
        //    {
        //        var _vehicle = InsuranceContext.EndorsementVehicleDetails.Single(where: $"VehicleId={item.VehicleDetailsId}");
        //        EndorsementRiskDetailModel riskDetail = Mapper.Map<EndorsementVehicleDetail, EndorsementRiskDetailModel>(_vehicle);
        //        riskDetail.VehicleId =Convert.ToInt32(_vehicle.VehicleId);
        //        listRiskDetail.Add(riskDetail);
        //    }
        //    Session["ViewlistVehicles"] = listRiskDetail;

        //    EndorsementSummaryDetailModel summarymodel = Mapper.Map<EndorsementSummaryDetail, EndorsementSummaryDetailModel>(endorsesummaryDetail);
        //    summarymodel.Id = endorsesummaryDetail.Id;
        //    Session["ViewSummaryDetail"] = endorsesummaryDetail;


        //}

        //public ActionResult SaveEndorsementRiskDetails(EndorsementRiskDetailModel model)
        //{

        //    // update endersoment details 

        //    var dbVehicle = InsuranceContext.VehicleDetails.Single(where: $"Id={model.VehicleId}");
        //    var EnderSomentVehical = InsuranceContext.EndorsementVehicleDetails.Single(where: $"VehicleId={model.VehicleId}");
        //    bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;


        //    var vehicleUpdate = Mapper.Map<EndorsementRiskDetailModel, EndorsementVehicleDetail>(model);

        //    EnderSomentVehical.VehicleId = dbVehicle.Id;
        //    EnderSomentVehical.NoOfCarsCovered = vehicleUpdate.NoOfCarsCovered;
        //    EnderSomentVehical.PolicyId = vehicleUpdate.PolicyId;
        //    EnderSomentVehical.RegistrationNo = vehicleUpdate.RegistrationNo;
        //    EnderSomentVehical.CustomerId = dbVehicle.CustomerId;
        //    EnderSomentVehical.MakeId = vehicleUpdate.MakeId;
        //    EnderSomentVehical.ModelId = vehicleUpdate.ModelId;
        //    EnderSomentVehical.CubicCapacity = vehicleUpdate.CubicCapacity;
        //    EnderSomentVehical.VehicleYear = vehicleUpdate.VehicleYear;
        //    EnderSomentVehical.EngineNumber = vehicleUpdate.EngineNumber;
        //    EnderSomentVehical.ChasisNumber = vehicleUpdate.ChasisNumber;
        //    EnderSomentVehical.VehicleColor = vehicleUpdate.VehicleColor;
        //    EnderSomentVehical.VehicleUsage = vehicleUpdate.VehicleUsage;
        //    EnderSomentVehical.CoverTypeId = vehicleUpdate.CoverTypeId;
        //    EnderSomentVehical.CoverStartDate = vehicleUpdate.CoverStartDate;
        //    EnderSomentVehical.CoverEndDate = vehicleUpdate.CoverEndDate;
        //    EnderSomentVehical.SumInsured = vehicleUpdate.SumInsured;
        //    EnderSomentVehical.Premium = vehicleUpdate.Premium;
        //    EnderSomentVehical.AgentCommissionId = vehicleUpdate.AgentCommissionId;
        //    EnderSomentVehical.Rate = vehicleUpdate.Rate;
        //    EnderSomentVehical.StampDuty = vehicleUpdate.StampDuty;
        //    EnderSomentVehical.ZTSCLevy = vehicleUpdate.ZTSCLevy;
        //    EnderSomentVehical.RadioLicenseCost = vehicleUpdate.RadioLicenseCost;
        //    EnderSomentVehical.OptionalCovers = vehicleUpdate.OptionalCovers;
        //    EnderSomentVehical.Excess = vehicleUpdate.Excess;
        //    EnderSomentVehical.CoverNoteNo = vehicleUpdate.CoverNoteNo;
        //    EnderSomentVehical.ExcessType = vehicleUpdate.ExcessType;
        //    EnderSomentVehical.CreatedOn = vehicleUpdate.CreatedOn;
        //    EnderSomentVehical.CreatedBy = vehicleUpdate.CreatedBy;
        //    EnderSomentVehical.ModifiedOn = DateTime.Now;
        //    EnderSomentVehical.ModifiedBy = vehicleUpdate.ModifiedBy;
        //    if (_userLoggedin)
        //    {
        //        var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
        //        var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();

        //        EnderSomentVehical.ModifiedBy = _customerData.Id;
        //    }
        //    EnderSomentVehical.IsActive = dbVehicle.IsActive;
        //    EnderSomentVehical.Addthirdparty = vehicleUpdate.Addthirdparty;
        //    EnderSomentVehical.AddThirdPartyAmount = vehicleUpdate.AddThirdPartyAmount;
        //    EnderSomentVehical.PassengerAccidentCover = vehicleUpdate.PassengerAccidentCover;
        //    EnderSomentVehical.ExcessBuyBack = vehicleUpdate.ExcessBuyBack;
        //    EnderSomentVehical.RoadsideAssistance = vehicleUpdate.RoadsideAssistance;
        //    EnderSomentVehical.MedicalExpenses = vehicleUpdate.MedicalExpenses;
        //    EnderSomentVehical.NumberofPersons = vehicleUpdate.NumberofPersons;
        //    EnderSomentVehical.IsLicenseDiskNeeded = vehicleUpdate.IsLicenseDiskNeeded;
        //    EnderSomentVehical.PassengerAccidentCoverAmount = vehicleUpdate.PassengerAccidentCoverAmount == null ? 0 : vehicleUpdate.PassengerAccidentCoverAmount;
        //    EnderSomentVehical.ExcessBuyBackAmount = vehicleUpdate.ExcessBuyBackAmount == null ? 0 : vehicleUpdate.ExcessBuyBackAmount;
        //    EnderSomentVehical.PaymentTermId = vehicleUpdate.PaymentTermId;
        //    EnderSomentVehical.ProductId = vehicleUpdate.ProductId;
        //    EnderSomentVehical.RoadsideAssistanceAmount = vehicleUpdate.RoadsideAssistanceAmount == null ? 0 : vehicleUpdate.RoadsideAssistanceAmount;
        //    EnderSomentVehical.MedicalExpensesAmount = vehicleUpdate.MedicalExpensesAmount == null ? 0 : vehicleUpdate.MedicalExpensesAmount;
        //    EnderSomentVehical.PassengerAccidentCoverAmountPerPerson = vehicleUpdate.PassengerAccidentCoverAmountPerPerson == null ? 0 : vehicleUpdate.PassengerAccidentCoverAmountPerPerson;
        //    EnderSomentVehical.ExcessBuyBackPercentage = vehicleUpdate.ExcessBuyBackPercentage == null ? 0 : vehicleUpdate.ExcessBuyBackPercentage;
        //    EnderSomentVehical.RoadsideAssistancePercentage = vehicleUpdate.RoadsideAssistancePercentage == null ? 0 : vehicleUpdate.RoadsideAssistancePercentage;
        //    EnderSomentVehical.MedicalExpensesPercentage = vehicleUpdate.MedicalExpensesPercentage == null ? 0 : vehicleUpdate.MedicalExpensesPercentage;
        //    EnderSomentVehical.ExcessAmount = vehicleUpdate.ExcessAmount;
        //    EnderSomentVehical.RenewalDate = vehicleUpdate.RenewalDate;
        //    EnderSomentVehical.TransactionDate = DateTime.Now;
        //    EnderSomentVehical.IncludeRadioLicenseCost = vehicleUpdate.IncludeRadioLicenseCost;
        //    EnderSomentVehical.InsuranceId = vehicleUpdate.InsuranceId;
        //    EnderSomentVehical.AnnualRiskPremium = vehicleUpdate.AnnualRiskPremium == null ? 0 : vehicleUpdate.AnnualRiskPremium;
        //    EnderSomentVehical.TermlyRiskPremium = vehicleUpdate.TermlyRiskPremium == null ? 0 : vehicleUpdate.TermlyRiskPremium;
        //    EnderSomentVehical.QuaterlyRiskPremium = vehicleUpdate.QuaterlyRiskPremium == null ? 0 : vehicleUpdate.QuaterlyRiskPremium;
        //    EnderSomentVehical.Discount = vehicleUpdate.Discount;
        //    EnderSomentVehical.isLapsed = vehicleUpdate.isLapsed;
        //    EnderSomentVehical.BalanceAmount = dbVehicle.BalanceAmount;
        //    EnderSomentVehical.VehicleLicenceFee = vehicleUpdate.VehicleLicenceFee;
        //    EnderSomentVehical.BusinessSourceId = vehicleUpdate.BusinessSourceId;

        //    InsuranceContext.EndorsementVehicleDetails.Update(EnderSomentVehical);




        //    return RedirectToAction("EndorsementSummaryDetail", "Account");
        //}

        //public ActionResult EndorsementSummaryDetail(int? Id = 0)
        //{
        //    var _model = new EndorsementSummaryDetailModel();
        //    var summaryDetail = Session["ViewSummaryDetail"];
        //    var listVehicles = Session["ViewlistVehicles"];
        //    var vehicle = (List<EndorsementRiskDetailModel>)Session["ViewlistVehicles"];// summary.GetVehicleInformation(id);


        //    var endorsesummaryDetail = InsuranceContext.EndorsementSummaryDetails.All(where: $"SummaryId={Convert.ToInt32(Session["SummaryDetailIdView"])}").FirstOrDefault();
        //    if (endorsesummaryDetail == null)
        //    {
        //        var summarydetail = (SummaryDetail)Session["ViewSummaryDetail"];

        //        SummaryDetailService SummaryDetailServiceObj = new SummaryDetailService();

        //        if (summarydetail != null)
        //        {
        //            var model = Mapper.Map<SummaryDetail, EndorsementSummaryDetailModel>(summarydetail);
        //            model.CarInsuredCount = vehicle.Count;
        //            model.DebitNote = "INV" + Convert.ToString(SummaryDetailServiceObj.getNewDebitNote());
        //            model.PaymentMethodId = summarydetail.PaymentMethodId;
        //            model.PaymentTermId = 1;
        //            model.ReceiptNumber = "";
        //            model.SMSConfirmation = false;
        //            model.TotalPremium = vehicle.Sum(item => item.Premium + item.ZTSCLevy + item.StampDuty + item.VehicleLicenceFee + (item.IncludeRadioLicenseCost ? item.RadioLicenseCost : 0.00m));// + vehicle.StampDuty + vehicle.ZTSCLevy;
        //            model.TotalStampDuty = vehicle.Sum(item => item.StampDuty);
        //            model.TotalSumInsured = vehicle.Sum(item => item.SumInsured);
        //            model.TotalZTSCLevies = vehicle.Sum(item => item.ZTSCLevy);
        //            model.ExcessBuyBackAmount = vehicle.Sum(item => item.ExcessBuyBackAmount);
        //            model.MedicalExpensesAmount = vehicle.Sum(item => item.MedicalExpensesAmount);
        //            model.PassengerAccidentCoverAmount = vehicle.Sum(item => item.PassengerAccidentCoverAmount);
        //            model.RoadsideAssistanceAmount = vehicle.Sum(item => item.RoadsideAssistanceAmount);
        //            model.ExcessAmount = vehicle.Sum(item => item.ExcessAmount);
        //            model.Discount = vehicle.Sum(item => item.Discount);
        //            decimal radio = 0.00m;
        //            foreach (var item in vehicle)
        //            {
        //                if (item.IncludeRadioLicenseCost)
        //                {
        //                    radio += Convert.ToDecimal(item.RadioLicenseCost);
        //                }
        //            }
        //            model.TotalRadioLicenseCost = radio;

        //            return View(model);
        //        }
        //    }
        //    else
        //    {
        //        var smrydetail = (EndorsementSummaryDetail)Session["ViewSummaryDetail"];

        //        EndorsementSummaryDetailService endorsementService = new EndorsementSummaryDetailService();

        //        if (smrydetail != null)
        //        {
        //            var model = Mapper.Map<EndorsementSummaryDetail, EndorsementSummaryDetailModel>(smrydetail);
        //            model.CarInsuredCount = vehicle.Count;
        //            model.DebitNote = "INV" + Convert.ToString(endorsementService.getNewDebitNote());
        //            model.PaymentMethodId = smrydetail.PaymentMethodId;
        //            model.PaymentTermId = 1;
        //            model.ReceiptNumber = "";
        //            model.SMSConfirmation = false;
        //            model.TotalPremium = vehicle.Sum(item => item.Premium + item.ZTSCLevy + item.StampDuty + item.VehicleLicenceFee + (item.IncludeRadioLicenseCost ? item.RadioLicenseCost : 0.00m));// + vehicle.StampDuty + vehicle.ZTSCLevy;
        //            model.TotalStampDuty = vehicle.Sum(item => item.StampDuty);
        //            model.TotalSumInsured = vehicle.Sum(item => item.SumInsured);
        //            model.TotalZTSCLevies = vehicle.Sum(item => item.ZTSCLevy);
        //            model.ExcessBuyBackAmount = vehicle.Sum(item => item.ExcessBuyBackAmount);
        //            model.MedicalExpensesAmount = vehicle.Sum(item => item.MedicalExpensesAmount);
        //            model.PassengerAccidentCoverAmount = vehicle.Sum(item => item.PassengerAccidentCoverAmount);
        //            model.RoadsideAssistanceAmount = vehicle.Sum(item => item.RoadsideAssistanceAmount);
        //            model.ExcessAmount = vehicle.Sum(item => item.ExcessAmount);
        //            model.Discount = vehicle.Sum(item => item.Discount);
        //            decimal radio = 0.00m;
        //            foreach (var item in vehicle)
        //            {
        //                if (item.IncludeRadioLicenseCost)
        //                {
        //                    radio += Convert.ToDecimal(item.RadioLicenseCost);
        //                }
        //            }
        //            model.TotalRadioLicenseCost = radio;

        //            return View(model);
        //        }
        //    }


        //    return View(_model);
        //}

        //public ActionResult SaveEndorsementSummaryDetails(EndorsementSummaryDetailModel model)
        //{
        //    int summaryId = Convert.ToInt32(Session["SummaryDetailIdView"]);
        //    var summaryDetail = InsuranceContext.SummaryDetails.Single(summaryId);
        //    var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summaryId}").ToList();
        //    var endorseSummryVehicle = InsuranceContext.EndorsementSummaryDetails.All(where: $"SummaryId={summaryId}").FirstOrDefault();
        //    var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
        //    var vehicleDetail = Mapper.Map<EndorsementSummaryDetailModel, EndorsementSummaryDetail>(model);
        //    bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

        //    if (endorseSummryVehicle == null)
        //    {
        //        vehicleDetail.SummaryId = summaryDetail.Id;
        //        vehicleDetail.CustomerId = vehicle.CustomerId;
        //        vehicleDetail.CreatedOn = DateTime.Now;
        //        vehicleDetail.BalancePaidDate = DateTime.Now;
        //        if (_userLoggedin)
        //        {
        //            var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
        //            var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();

        //            vehicleDetail.CreatedBy = _customerData.Id;
        //        }

        //        InsuranceContext.EndorsementSummaryDetails.Insert(vehicleDetail);
        //    }
        //    else
        //    {
        //        endorseSummryVehicle.SummaryId = summaryDetail.Id;
        //        endorseSummryVehicle.CustomerId = vehicle.CustomerId;
        //        endorseSummryVehicle.PaymentTermId = model.PaymentTermId;
        //        endorseSummryVehicle.PaymentMethodId = model.PaymentMethodId;
        //        endorseSummryVehicle.TotalSumInsured = model.TotalSumInsured;
        //        endorseSummryVehicle.TotalPremium = model.TotalPremium;
        //        endorseSummryVehicle.TotalStampDuty = model.TotalStampDuty;
        //        endorseSummryVehicle.TotalZTSCLevies = model.TotalZTSCLevies;
        //        endorseSummryVehicle.TotalRadioLicenseCost = model.TotalRadioLicenseCost;
        //        endorseSummryVehicle.DebitNote = model.DebitNote;
        //        endorseSummryVehicle.ReceiptNumber = model.ReceiptNumber;
        //        endorseSummryVehicle.SMSConfirmation = model.SMSConfirmation;
        //        endorseSummryVehicle.CreatedOn = DateTime.Now;
        //        endorseSummryVehicle.CreatedBy = summaryDetail.CreatedBy;
        //        endorseSummryVehicle.ModifiedOn = DateTime.Now;
        //        if (_userLoggedin)
        //        {
        //            var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
        //            var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();

        //            endorseSummryVehicle.ModifiedBy = _customerData.Id;
        //        }
        //        endorseSummryVehicle.IsActive = summaryDetail.IsActive;
        //        endorseSummryVehicle.AmountPaid = model.AmountPaid;
        //        endorseSummryVehicle.BalancePaidDate = DateTime.Now;
        //        endorseSummryVehicle.Notes = model.Notes;
        //        endorseSummryVehicle.isQuotation = summaryDetail.isQuotation;

        //        InsuranceContext.EndorsementSummaryDetails.Update(endorseSummryVehicle);

        //    }

        //    var summaryvehicle = InsuranceContext.EndorsementSummaryVehicleDetails.All(where: $"SummaryDetailId={summaryId}").FirstOrDefault();
        //    EndorsementSummaryVehicleDetailModel endorsemodel = new EndorsementSummaryVehicleDetailModel();
        //    endorsemodel.SummaryDetailId = summaryId;
        //    endorsemodel.VehicleDetailsId = Convert.ToInt32(vehicle.Id);

        //    if (summaryvehicle == null)
        //    {
        //        endorsemodel.CreatedOn = DateTime.Now;
        //        var insertSummaryVehicle = Mapper.Map<EndorsementSummaryVehicleDetailModel, EndorsementSummaryVehicleDetail>(endorsemodel);
        //        if (_userLoggedin)
        //        {
        //            var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
        //            var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();

        //            insertSummaryVehicle.CreatedBy = _customerData.Id;
        //        }
        //        InsuranceContext.EndorsementSummaryVehicleDetails.Insert(insertSummaryVehicle);
        //    }
        //    else
        //    {
        //        var updateSummaryVehicle = Mapper.Map<EndorsementSummaryVehicleDetailModel, EndorsementSummaryVehicleDetail>(endorsemodel);
        //        updateSummaryVehicle.Id = summaryvehicle.Id;
        //        updateSummaryVehicle.CreatedOn = summaryvehicle.CreatedOn;
        //        updateSummaryVehicle.CreatedBy = summaryvehicle.CreatedBy;
        //        updateSummaryVehicle.ModifiedOn = DateTime.Now;
        //        if (_userLoggedin)
        //        {
        //            var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
        //            var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();

        //            updateSummaryVehicle.ModifiedBy = _customerData.Id;
        //        }
        //        InsuranceContext.EndorsementSummaryVehicleDetails.Update(updateSummaryVehicle);
        //    }

        //    return RedirectToAction("MyPolicies", "Account");
        //}
    }
}