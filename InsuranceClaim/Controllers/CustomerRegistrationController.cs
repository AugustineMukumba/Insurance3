using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Insurance.Domain;

namespace InsuranceClaim.Controllers
{
    public class CustomerRegistrationController : Controller
    {
        // GET: CustomerRegistration
        public ActionResult Index()
        {
            GetUserData();
            return View();
        }

        public void GetUserData()
        {
            var data = InsuranceContext.Currencies.All().ToList();
        }
    }
}