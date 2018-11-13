using System;
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

                    var customer = InsuranceContext.Customers.All(where: $"UserId ='{_user.Id.ToString()}'").FirstOrDefault();
                    Session["firstname"] = customer.FirstName;
                    Session["lastname"] = customer.LastName;


                    if (role == "Administrator")
                    {

                        return RedirectToAction("Dashboard", "Account");
                    }
                    else
                    {

                        return Redirect("/CustomerRegistration/index");
                        // return RedirectToAction("Index", "CustomerRegistration");
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
                Summeryofcover += "<tr> <td style='padding: 7px 10px; font - size:15px;'>" + item.RegistrationNo + " </td>  <td style='padding: 7px 10px; font - size:15px;'>" + vehicledescription + "</td><td style='padding: 7px 10px; font - size:15px;'>$" + item.SumInsured + "</td><td style='padding: 7px 10px; font - size:15px;'>" + converType + "</td><td style='padding: 7px 10px; font - size:15px;'>" + InsuranceContext.VehicleUsages.All(Convert.ToString(item.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</td><td style='padding: 7px 10px; font - size:15px;'>" + policyPeriod + "</td><td style='padding: 7px 10px; font - size:15px;'>" + paymentTermsName + "</td><td style='padding: 7px 10px; font - size:15px;'>$" + Convert.ToString(item.Premium) + "</td></tr>";
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
                Replace("##NINumber##", customerDetails.NationalIdentificationNumber).Replace("##VehicleLicenceFee##", Convert.ToString(ListOfVehicles.Sum(x => x.VehicleLicenceFee)));

            #region Invoice PDF
            var attacehmetn_File = MiscellaneousService.EmailPdf(Bodyy, policyDetials.CustomerId, policyDetials.PolicyNumber, "Policy schedule");
            #endregion

            #region Invoice EMail
            //var _yAtter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
            List<string> _attachementss = new List<string>();
            _attachementss.Add(attacehmetn_File);
            //_attachementss.Add(_yAtter);
            #endregion

            if (customerDetails.IsCustomEmail)
            {
                objEmailService.SendEmail(LoggedUserEmail(), "", "", "Policy schedule", Bodyy, _attachementss);
            }
            else
            {
                objEmailService.SendEmail(user.Email, "", "", "Policy schedule", Bodyy, _attachementss);
            }


            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public string LoggedUserEmail()
        {
            string email = "";
            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            if (_userLoggedin)
            {
                var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                email = _User.Email;
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


        public ActionResult UserManagement(int id = 0)
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

            CustomerModel obj = new CustomerModel();
            List<IdentityRole> roles = roleManager.Roles.ToList();
            InsuranceClaim.Models.RoleManagementListViewModel _roles = new RoleManagementListViewModel();

            _roles.RoleList = roles;
            ViewBag.Adduser = _roles.RoleList;



            if (id != 0)
            {
                var data = InsuranceContext.Customers.Single(id);
                var user = UserManager.FindById(data.UserID);
                var email = user.Email;
                var phone = user.PhoneNumber;
                var role = UserManager.GetRoles(data.UserID).FirstOrDefault();




                obj.FirstName = data.FirstName;
                obj.LastName = data.LastName;
                obj.AddressLine1 = data.AddressLine1;
                obj.AddressLine2 = data.AddressLine2;
                obj.City = data.City;
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
            var user = InsuranceContext.Customers.All(where: "IsActive = 'True' or IsActive is null").OrderByDescending(x => x.Id).ToList();


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

                ListUserViewModel.Add(cstmrModel);
            }

            ListUserViewModel lstUserModel = new ListUserViewModel();
            lstUserModel.ListUsers = ListUserViewModel;

            return View(lstUserModel);


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



        [Authorize(Roles = "Staff,Administrator")]
        public ActionResult PolicyList()
        {
            TempData["RedirectedFrom"] = "PolicyList";
            Session["ViewlistVehicles"] = null;
            ListPolicy policylist = new ListPolicy();
            policylist.listpolicy = new List<PolicyListViewModel>();
            //   var SummaryList = InsuranceContext.SummaryDetails.All().OrderByDescending(x => x.Id).ToList(); // commented 

            var SummaryList = InsuranceContext.SummaryDetails.All().OrderByDescending(x => x.Id).ToList().Take(50);

            foreach (var item in SummaryList)
            {
                PolicyListViewModel policylistviewmodel = new PolicyListViewModel();

                var paymentDetails = InsuranceContext.PaymentInformations.Single(where: $"SummaryDetailId =" + item.Id);

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

                var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={item.Id}").ToList();
                if (SummaryVehicleDetails != null && SummaryVehicleDetails.Count > 0)
                {
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
                            obj.isLapsed = _vehicle.isLapsed;
                            obj.isActive = Convert.ToBoolean(_vehicle.IsActive);
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

                policylist.listpolicy.Add(policylistviewmodel);
            }


            return View(policylist);
        }


        // GET: Dashboard
        [Authorize(Roles = "Administrator")]
        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult SearchPolicy(string searchText)
        {
            ListPolicy policylist = new ListPolicy();
            policylist.listpolicy = new List<PolicyListViewModel>();
            if (searchText != null && searchText != "")
            {

                var custom = searchText.Split(' ');
                var SummaryList = new List<SummaryDetail>();
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

                    SummaryList = InsuranceContext.SummaryDetails.All(where: $"CustomerId in ({commaSeperatedCustomerIds})").ToList();
                }
                else
                {
                    //var policye = InsuranceContext.PolicyDetails.Query("Select * From PolicyDetail Where PolicyNumber Like '%" + searchText + "%'").FirstOrDefault();
                    var policye = InsuranceContext.PolicyDetails.Query("Select * From PolicyDetail Where PolicyNumber Like '%" + searchText + "%'").FirstOrDefault();
                    if (policye != null)
                    {


                        var policyId = policye.Id;
                        var vehicle = InsuranceContext.VehicleDetails.Single(where: $"PolicyId = '" + policyId + "'");

                        var vehiclesummaryid = vehicle.Id;

                        var SummaryVehicleDetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId =" + vehiclesummaryid);

                        SummaryList = InsuranceContext.SummaryDetails.All(Convert.ToString(SummaryVehicleDetail.SummaryDetailId)).ToList();
                    }
                }

                if (SummaryList != null && SummaryList.Count > 0)
                {
                    foreach (var item in SummaryList)
                    {
                        PolicyListViewModel policylistviewmodel = new PolicyListViewModel();

                        policylistviewmodel.Vehicles = new List<VehicleReinsuranceViewModel>();
                        policylistviewmodel.TotalPremium = Convert.ToDecimal(item.TotalPremium);
                        policylistviewmodel.TotalSumInsured = Convert.ToDecimal(item.TotalSumInsured);
                        policylistviewmodel.PaymentMethodId = Convert.ToInt32(item.PaymentMethodId);
                        policylistviewmodel.CustomerId = Convert.ToInt32(item.CustomerId);
                        policylistviewmodel.SummaryId = item.Id;

                        var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={item.Id}").ToList();
                        var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
                        var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
                        var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));

                        policylistviewmodel.PolicyNumber = policy.PolicyNumber;

                        foreach (var _item in SummaryVehicleDetails)
                        {
                            VehicleReinsuranceViewModel obj = new VehicleReinsuranceViewModel();
                            var _vehicle = InsuranceContext.VehicleDetails.Single(_item.VehicleDetailsId);
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


                            policylistviewmodel.Vehicles.Add(obj);
                        }

                        policylist.listpolicy.Add(policylistviewmodel);
                    }
                }
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
        public ActionResult MyPolicies()
        {
            TempData["RedirectedFrom"] = "MyPolicy";
            Session["ViewlistVehicles"] = null;
            ListPolicy policylist = new ListPolicy();
            policylist.listpolicy = new List<PolicyListViewModel>();



            var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
            var role = UserManager.GetRoles(_User.Id.ToString()).FirstOrDefault();

            var customerID = InsuranceContext.Customers.Single(where: $"userid='{User.Identity.GetUserId().ToString()}'").Id;


            var SummaryList = new List<SummaryDetail>();



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


            foreach (var item in SummaryList.Take(50))
            {
                PolicyListViewModel policylistviewmodel = new PolicyListViewModel();

                var paymentDetails = InsuranceContext.PaymentInformations.Single(where: $"SummaryDetailId =" + item.Id);

                if (paymentDetails == null)
                {
                    continue;
                }

                policylistviewmodel.Vehicles = new List<VehicleReinsuranceViewModel>();
                policylistviewmodel.TotalPremium = Convert.ToDecimal(item.TotalPremium);
                policylistviewmodel.TotalSumInsured = Convert.ToDecimal(item.TotalSumInsured);
                policylistviewmodel.PaymentMethodId = Convert.ToInt32(item.PaymentMethodId);
                policylistviewmodel.CustomerId = Convert.ToInt32(item.CustomerId);
                policylistviewmodel.CustomerEmail = GetCustomerEmailbyCustomerID(item.CustomerId);
                policylistviewmodel.SummaryId = item.Id;
                policylistviewmodel.createdOn = Convert.ToDateTime(item.CreatedOn);

                var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={item.Id}").ToList();
                var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);


                if (vehicle != null)
                {
                    var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
                    var product = InsuranceContext.Products.Single(Convert.ToInt32(vehicle.ProductId));

                    policylistviewmodel.PolicyNumber = policy.PolicyNumber;

                    int i = 0;

                    //foreach (var _item in SummaryVehicleDetails)
                    //{

                    //}


                    VehicleReinsuranceViewModel obj = new VehicleReinsuranceViewModel();
                    var _vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);

                    if (_vehicle != null)
                    {

                        var _reinsurenaceTrans = InsuranceContext.ReinsuranceTransactions.All(where: $"SummaryDetailId={item.Id} and VehicleId={SummaryVehicleDetails[0].VehicleDetailsId}").ToList();
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
            }

            return View(policylist);
        }


        //public ActionResult EndorsementDetials(int summaryId)
        //{
        //    var summaryDetials = InsuranceContext.SummaryDetails.Single(where: $"id='{summaryId}'");

        //    if(summaryDetials!=null)
        //    {

        //    }

        //    return View();
        //}

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
            var PolicyNumber = System.Web.HttpContext.Current.Request.Params["PolicyNumber"];
            var CustomerId = System.Web.HttpContext.Current.Request.Params["CustomerId"];
            var vehicleId = System.Web.HttpContext.Current.Request.Params["vehicleId"];

            //string[] filePaths = Directory.GetFiles(Server.MapPath("~/Documents/" + CustomerId + "/" + PolicyNumber + "/" + vehicleId + "/"));

            var FileList = InsuranceContext.PolicyDocuments.All(where: $"CustomerId={CustomerId} and PolicyNumber='{PolicyNumber}' and vehicleId={vehicleId}");
            var list = new List<InsuranceClaim.Models.PolicyDocumentModels>();
            foreach (var item in FileList)
            {
                var obj = new InsuranceClaim.Models.PolicyDocumentModels();
                obj.Title = item.Title;
                obj.Decription = item.Description;
                obj.FilePath = item.FilePath;
                obj.id = item.Id;
                list.Add(obj);
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
            //var model = new PolicyDetailModel();
            //var InsService = new InsurerService();
            //model.CurrencyId = InsuranceContext.Currencies.All().FirstOrDefault().Id;
            //model.PolicyStatusId = InsuranceContext.PolicyStatuses.All().FirstOrDefault().Id;
            //model.BusinessSourceId = InsuranceContext.BusinessSources.All().FirstOrDefault().Id;
            ////model.Products = InsuranceContext.Products.All().ToList();
            //model.InsurerId = InsService.GetInsurers().FirstOrDefault().Id;
            //var objList = InsuranceContext.PolicyDetails.All(orderBy: "Id desc").FirstOrDefault();
            //if (objList != null)
            //{
            //    string number = objList.PolicyNumber.Split('-')[0].Substring(4, objList.PolicyNumber.Length - 6);
            //    long pNumber = Convert.ToInt64(number.Substring(2, number.Length - 2)) + 1;
            //    string policyNumber = string.Empty;
            //    int length = 7;
            //    length = length - pNumber.ToString().Length;
            //    for (int i = 0; i < length; i++)
            //    {
            //        policyNumber += "0";
            //    }
            //    policyNumber += pNumber;
            //    ViewBag.PolicyNumber = "GMCC" + DateTime.Now.Year.ToString().Substring(2, 2) + policyNumber + "-1";
            //    model.PolicyNumber = ViewBag.PolicyNumber;
            //}
            //else
            //{
            //    ViewBag.PolicyNumber = ConfigurationManager.AppSettings["PolicyNumber"] + "-1";
            //    model.PolicyNumber = ViewBag.PolicyNumber;
            //}

            //model.BusinessSourceId = 3;

            //Session["PolicyDataView"] = Mapper.Map<PolicyDetailModel, PolicyDetail>(model);




            return RedirectToAction("RiskDetail");
        }


        //public ActionResult RiskDetail(int? id = 1)
        //{



        //    if (Session["SummaryDetailId"] != null)
        //    {
        //        SetValueIntoSession(Convert.ToInt32(Session["SummaryDetailId"]));
        //        Session["SummaryDetailId"] = null;
        //    }


        //    if (Session["CustomerDataModal"] == null)
        //    {
        //        return RedirectToAction("Index", "CustomerRegistration");
        //        return Redirect("/CustomerRegistration/Index");
        //    }


        //    ViewBag.Products = InsuranceContext.Products.All(where: "Active = 'True' or Active is Null").ToList();
        //    var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm))
        //                           select new
        //                           {
        //                               ID = (int)e,
        //                               Name = e.ToString()
        //                           };

        //    ViewBag.ePaymentTermData = new SelectList(ePaymentTermData, "ID", "Name");
        //    ViewBag.PaymentTermId = InsuranceContext.PaymentTerms.All().ToList();
        //    ViewBag.PaymentTermId = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is Null").ToList();

        //    var eExcessTypeData = from eExcessType e in Enum.GetValues(typeof(eExcessType))
        //                          select new
        //                          {
        //                              ID = (int)e,
        //                              Name = e.ToString()
        //                          };

        //    ViewBag.eExcessTypeData = new SelectList(eExcessTypeData, "ID", "Name");

        //    int RadioLicenseCosts = 0;
        //  //  int RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
        //    var PolicyData = (PolicyDetail)Session["PolicyData"];
        //   // Id is policyid from Policy detail table
        //    var viewModel = new RiskDetailModel();
        //    var service = new VehicleService();

        //    ViewBag.VehicleUsage = service.GetAllVehicleUsage();

        //    viewModel.VehicleUsage = 0;
        //    viewModel.NumberofPersons = 0;
        //    viewModel.AddThirdPartyAmount = 0.00m;
        //    viewModel.RadioLicenseCost = Convert.ToDecimal(RadioLicenseCosts);
        //    var makers = service.GetMakers();
        //    ViewBag.CoverType = service.GetCoverType();
        //    ViewBag.AgentCommission = service.GetAgentCommission();
        //    ViewBag.Makers = makers;
        //    viewModel.isUpdate = false;
        //    TempData["Policy"] = service.GetPolicy(id);
        //    if (makers.Count > 0)
        //    {
        //        var model = service.GetModel(makers.FirstOrDefault().MakeCode);
        //        ViewBag.Model = model;

        //    }

        //    viewModel.NoOfCarsCovered = 1;
        //    if (Session["VehicleDetails"] != null)
        //    {
        //        var list = (List<RiskDetailModel>)Session["VehicleDetails"];
        //        viewModel.NoOfCarsCovered = list.Count + 1;
        //    }

        //    if (id > 0)
        //    {
        //        var list = (List<RiskDetailModel>)Session["VehicleDetails"];
        //        if (list != null && list.Count > 0 && (list.Count >= id))
        //        {
        //            var data = (RiskDetailModel)list[Convert.ToInt32(id - 1)];
        //            if (data != null)
        //            {
        //                viewModel.AgentCommissionId = data.AgentCommissionId;
        //                viewModel.ChasisNumber = data.ChasisNumber;
        //                viewModel.CoverEndDate = data.CoverEndDate;
        //                viewModel.CoverNoteNo = data.CoverNoteNo;
        //                viewModel.CoverStartDate = data.CoverStartDate;
        //                viewModel.CoverTypeId = data.CoverTypeId;
        //                viewModel.CubicCapacity = (int)Math.Round(data.CubicCapacity.Value, 0);
        //                viewModel.CustomerId = data.CustomerId;
        //                viewModel.EngineNumber = data.EngineNumber;
        //               // viewModel.Equals = data.Equals;
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
        //                viewModel.ExcessAmount = data.ExcessAmount;
        //                viewModel.ExcessBuyBackAmount = data.ExcessBuyBackAmount;
        //                viewModel.MedicalExpensesAmount = data.MedicalExpensesAmount;
        //                viewModel.MedicalExpensesPercentage = data.MedicalExpensesPercentage;
        //                viewModel.PassengerAccidentCoverAmount = data.PassengerAccidentCoverAmount;
        //                viewModel.PassengerAccidentCoverAmountPerPerson = data.PassengerAccidentCoverAmountPerPerson;
        //                viewModel.PaymentTermId = data.PaymentTermId;
        //                viewModel.ProductId = data.ProductId;
        //                viewModel.IncludeRadioLicenseCost = data.IncludeRadioLicenseCost;
        //                viewModel.RenewalDate = data.RenewalDate;
        //                viewModel.TransactionDate = data.TransactionDate;
        //                viewModel.AnnualRiskPremium = data.AnnualRiskPremium;
        //                viewModel.TermlyRiskPremium = data.TermlyRiskPremium;
        //                viewModel.QuaterlyRiskPremium = data.QuaterlyRiskPremium;
        //                viewModel.Discount = data.Discount;
        //                viewModel.VehicleLicenceFee = Convert.ToDecimal(data.VehicleLicenceFee);
        //                viewModel.InsuranceId = data.InsuranceId;
        //                viewModel.isUpdate = true;

        //                viewModel.vehicleindex = Convert.ToInt32(id);

        //                var ser = new VehicleService();
        //                var model = ser.GetModel(data.MakeId);
        //                ViewBag.Model = model;
        //            }
        //        }
        //    }

        //    if (Session["ViewlistVehicles"] == null || (Session["ViewlistVehicles"] != null && ((List<RiskDetailModel>)Session["ViewlistVehicles"]).Count == 0))
        //    {

        //        var InsService = new InsurerService();
        //        var summaryDetail = InsuranceContext.SummaryDetails.Single(id);
        //        var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={id}").ToList();
        //        var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
        //        var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
        //        List<RiskDetailModel> listVehicles = new List<RiskDetailModel>();


        //        foreach (var item in SummaryVehicleDetails)
        //        {
        //            RiskDetailModel _vehicle = new RiskDetailModel();
        //            VehicleDetail __vehicle = new VehicleDetail();
        //            __vehicle = InsuranceContext.VehicleDetails.Single(item.VehicleDetailsId);

        //            _vehicle = Mapper.Map<VehicleDetail, RiskDetailModel>(__vehicle);

        //            listVehicles.Add(_vehicle);
        //        }
        //        Session["ViewSummaryDetail"] = summaryDetail;
        //        Session["ViewPolicy"] = policy;
        //        Session["ViewlistVehicles"] = listVehicles;

        //    }






        //    return View(viewModel);
        //}


        public ActionResult RiskDetail(int? id = 1)
        {
            if (TempData["RedirectedFrom"] != null && Convert.ToString(TempData["RedirectedFrom"]) == "MyPolicy")
            {
                ViewBag.RedirectedFrom = "MyPolicy";
            }
            else if (TempData["RedirectedFrom"] != null && Convert.ToString(TempData["RedirectedFrom"]) == "PolicyList")
            {
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
                var list = (List<RiskDetailModel>)Session["ViewlistVehicles"];
                if (list != null && list.Count > 0 && (list.Count >= id))
                {
                    var data = (RiskDetailModel)list[Convert.ToInt32(id - 1)];
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
                var _vehicle = InsuranceContext.VehicleDetails.Single(item.VehicleDetailsId);
                RiskDetailModel riskDetail = Mapper.Map<VehicleDetail, RiskDetailModel>(_vehicle);
                listRiskDetail.Add(riskDetail);
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

        public ActionResult SummaryDetail(int? Id = 0)
        {
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
                model.PaymentTermId = 1;
                model.ReceiptNumber = "";
                model.SMSConfirmation = false;

                model.TotalPremium = vehicle.Sum(item => item.Premium + item.ZTSCLevy + item.StampDuty + item.VehicleLicenceFee + (item.IncludeRadioLicenseCost ? item.RadioLicenseCost : 0.00m));// + vehicle.StampDuty + vehicle.ZTSCLevy;
                                                                                                                                                                                                  //model.TotalRadioLicenseCost = vehicle.Sum(item => item.RadioLicenseCost);
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
                var body = EmailBody2.Replace("##RenewDate##", _vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##path##", filepath).Replace("##FirstName##", _customer.FirstName).Replace("##LastName##", _customer.LastName).Replace("##Address1##", _customer.AddressLine1).Replace("##Address2##", _customer.AddressLine2).Replace("##PolicyNumber##", policylist.PolicyNumber).Replace("##RegistrationNo##", _vehicle.RegistrationNo);

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
                var body = EmailBody2.Replace("##RenewDate##", vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##path##", filepath).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##PolicyNumber##", policylist.PolicyNumber).Replace("##RegistrationNo##", vehicle.RegistrationNo);
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
                    var body = EmailBody2.Replace("##RenewDate##", _item.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##path##", filepath).Replace("##FirstName##", customerData.FirstName).Replace("##LastName##", customerData.LastName).Replace("##Address1##", customerData.AddressLine1).Replace("##Address2##", customerData.AddressLine2).Replace("##numberofDays##", item.NoOfDays.ToString()).Replace("##PolicyNumber##", policylist.PolicyNumber).Replace("##Make##", make).Replace("##Model##", model);
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
                var body = EmailBody2.Replace("##PolicyNumber##", policylist.PolicyNumber).Replace("##path##", filepath).Replace("##Make##", make).Replace("##Model##", model).Replace("##TransactionDate##", vehicle.TransactionDate.Value.ToString("dd/MM/yyyy"));
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

            foreach (var item in SummaryList)
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
                        var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
                        if (vehicle != null)
                        {
                            var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
                            var product = InsuranceContext.Products.Single(Convert.ToInt32(vehicle.ProductId));

                            policylistviewmodel.PolicyNumber = policy.PolicyNumber;

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

        public ActionResult EndorsementDetials(int id = 0)
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

    }
}