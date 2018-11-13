using AutoMapper;
using Insurance.Domain;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class BirthdayMessageController : Controller
    {
        // GET: BirthdayMessage
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SendBirthdayMessage()
        {

            return View();
        }

        [HttpPost]
        public ActionResult SendBirthdayMessage(BirthdayMessageModel Model)
        {
            if (ModelState.IsValid)
            {
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                string userid = "";
                if (userLoggedin)
                {
                    userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");
                    var dbModel = Mapper.Map<BirthdayMessageModel, BirthdayMessage>(Model);
                    dbModel.CreatedBy = customer.Id;
                    dbModel.CreatedOn = DateTime.Now;
                    InsuranceContext.BirthdayMessages.Insert(dbModel);
                    return RedirectToAction("SendBirthdayMessage");
                }
            }
            return View();
        }
    }
}