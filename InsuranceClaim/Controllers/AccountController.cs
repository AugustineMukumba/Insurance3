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

namespace InsuranceClaim.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
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
                    //var customer = InsuranceContext.Customers.All(where: $"UserId ='{User.Identity.GetUserId().ToString()}'").FirstOrDefault();
                    if (role == "Administrator" || role == "Staff")
                    {
                        return RedirectToAction("Dashboard", "Account");
                    }
                    else
                    {
                        return RedirectToAction("index", "CustomerRegistration");
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

            return RedirectToAction("Index", "CustomerRegistration");
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
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
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
                if (!roleManager.RoleExists(model.RoleName))
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
            var user = InsuranceContext.Customers.All().ToList();


            foreach (var item in user)
            {
                CustomerModel cstmrModel = new CustomerModel();
                cstmrModel.Id = item.Id;
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
            InsuranceContext.Customers.Delete(data);

            var currentUser = UserManager.FindById(userid);
            UserManager.Delete(currentUser);



            return RedirectToAction("UserManagementList");
        }

        public ActionResult PolicyList()
        {
            ListPolicy policylist = new ListPolicy();
            policylist.listpolicy = new List<PolicyListViewModel>();
            var SummaryList = InsuranceContext.SummaryDetails.All().ToList();

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
                    var _reinsurenaceTrans = InsuranceContext.ReinsuranceTransactions.Single(where: $"SummaryDetailId={item.Id} and VehicleId={_item.VehicleDetailsId}");

                    obj.CoverType = Convert.ToInt32(_vehicle.CoverTypeId);
                    obj.isReinsurance = (_vehicle.SumInsured > 100000 ? true : false);
                    obj.MakeId = _vehicle.MakeId;
                    obj.ModelId = _vehicle.ModelId;
                    obj.Premium = Convert.ToDecimal(_vehicle.Premium);
                    obj.RegisterationNumber = _vehicle.RegistrationNo;
                    obj.SumInsured = Convert.ToDecimal(_vehicle.SumInsured);
                    obj.VehicleId = _vehicle.Id;
                    if (_reinsurenaceTrans != null)
                    {
                        obj.ReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans.ReinsuranceAmount);
                        obj.ReinsurerBrokerId = _reinsurenaceTrans.ReinsuranceBrokerId;
                    }
                   
                    
                    policylistviewmodel.Vehicles.Add(obj);
                }

                policylist.listpolicy.Add(policylistviewmodel);
            }


            return View(policylist);
        }

  
        // GET: Dashboard
        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult SearchPolicy()
        {



            return View();
        }

        // Setting Methods

       // GET: Setting
        public ActionResult Index()
        {
            InsuranceClaim.Models.SettingModel obj = new InsuranceClaim.Models.SettingModel();
            List<Insurance.Domain.Setting> objList = new List<Insurance.Domain.Setting>();
            objList = InsuranceContext.Settings.All().ToList();
            return View(obj);
        }

        [HttpPost]
        public ActionResult SaveSetting(SettingModel model)
        {

            //model.CreatedBy = 1;
            //model.CreatedDate = DateTime.Now;
            var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{User.Identity.GetUserId().ToString()}'").FirstOrDefault();
            model.CreatedBy = _customerData.Id;
            model.CreatedDate = DateTime.Now;
            var dbModel = Mapper.Map<SettingModel, Setting>(model);
            InsuranceContext.Settings.Insert(dbModel);

            return RedirectToAction("SettingList");
        }

        public ActionResult SettingList()
        {
            
            var db = InsuranceContext.Settings.All().ToList();
            return View(db);
        }

        public ActionResult EditSetting(int Id)
        {
            var record = InsuranceContext.Settings.All(where: $"Id ={Id}").FirstOrDefault();
            var model = Mapper.Map<Setting, SettingModel>(record);
            return View(model);
        }
        [HttpPost]
        public ActionResult EditSetting(SettingModel model,int Id)
        {

            if (ModelState.IsValid)
            {
                model.ModifiedBy = 2;
                model.ModifiedDate = DateTime.Now;

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
        public ActionResult ListReinsuranceBroker()
        {


            var list = InsuranceContext.ReinsuranceBrokers.All().ToList();
            return View(list);
        }
        public ActionResult AddReinsuranceBroker( int? id = 0 )
        {
            InsuranceClaim.Models.ReinsuranceBrokerModel obj = new ReinsuranceBrokerModel();
            if (id > 0)
            {

                var model = InsuranceContext.ReinsuranceBrokers.Single(id);
                obj = Mapper.Map< ReinsuranceBroker, ReinsuranceBrokerModel>(model);
                

            }
          
            return View(obj);


        }
        [HttpPost]
        public ActionResult SaveReinsuranceBroker(ReinsuranceBrokerModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id ==0 ||model.Id==null)
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
            ListPolicy policylist = new ListPolicy();
            policylist.listpolicy = new List<PolicyListViewModel>();
            var customerID = InsuranceContext.Customers.Single(where: $"userid='{User.Identity.GetUserId().ToString()}'").Id;
            var SummaryList = InsuranceContext.SummaryDetails.All(where: $"customerid={customerID}").ToList();


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
                    var _reinsurenaceTrans = InsuranceContext.ReinsuranceTransactions.Single(where: $"SummaryDetailId={item.Id} and VehicleId={_item.VehicleDetailsId}");

                    obj.CoverType = Convert.ToInt32(_vehicle.CoverTypeId);
                    obj.isReinsurance = (_vehicle.SumInsured > 100000 ? true : false);
                    obj.MakeId = _vehicle.MakeId;
                    obj.ModelId = _vehicle.ModelId;
                    obj.Premium = Convert.ToDecimal(_vehicle.Premium);
                    obj.RegisterationNumber = _vehicle.RegistrationNo;
                    obj.SumInsured = Convert.ToDecimal(_vehicle.SumInsured);
                    obj.VehicleId = _vehicle.Id;
                    obj.startdate = Convert.ToDateTime(_vehicle.CoverStartDate);
                    obj.enddate = Convert.ToDateTime(_vehicle.CoverEndDate);
                    if (_reinsurenaceTrans != null)
                    {
                        obj.ReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans.ReinsuranceAmount);
                        obj.ReinsurerBrokerId = _reinsurenaceTrans.ReinsuranceBrokerId;
                    }


                    policylistviewmodel.Vehicles.Add(obj);
                }

                policylist.listpolicy.Add(policylistviewmodel);
            }


            return View(policylist);
        }
    }


}