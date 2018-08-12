using Insurance.Service;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class InsuranceHomeController : Controller
    {
        // GET: InsuranceHome
        public ActionResult Index()
        {

            List<RiskDetailModel> objVehicles = new List<RiskDetailModel>();

            objVehicles.Add(new RiskDetailModel { RegistrationNo = "AEM5376", CoverTypeId = 1, SumInsured = 10000.00m, PaymentTermId = 4, VehicleYear = 2012 });

            ICEcashService obj = new ICEcashService();
            //obj.getToken();
            //obj.RequestQuote(objVehicles);
            return View();
        }
    }
}