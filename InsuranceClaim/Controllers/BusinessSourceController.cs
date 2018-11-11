using Insurance.Domain;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class BusinessSourceController : Controller
    {
        // GET: BusinessSource
        public ActionResult Index()
        {
            var result = (from business in InsuranceContext.BusinessSources.All().ToList()
                          select new BusinessSourceModel { Id = business.Id, Source = business.Source, CreatedOn = business.CreatedOn }).ToList();

            return View(result);
        }

        // GET: BusinessSource/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: BusinessSource/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BusinessSource/Create
        [HttpPost]
        public ActionResult Create(BusinessSourceModel model)
        {
            try
            {
                // TODO: Add insert logic here

                ModelState.Remove("Id");

                if (ModelState.IsValid)
                {
                    BusinessSource buesiness = new BusinessSource { Source = model.Source, CreatedOn = DateTime.Now };
                    InsuranceContext.BusinessSources.Insert(buesiness);
                }


                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: BusinessSource/Edit/5
        public ActionResult Edit(int id)
        {
            BusinessSourceModel model = new BusinessSourceModel();

            var businessResource = InsuranceContext.BusinessSources.Single(id);

            if (businessResource != null)
            {
                model.Id = businessResource.Id;
                model.Source = businessResource.Source;
            }

            return View(model);
        }

        // POST: BusinessSource/Edit/5
        [HttpPost]
        public ActionResult Edit(BusinessSourceModel model)
        {
            try
            {
                // TODO: Add update logic here

                if (ModelState.IsValid)
                {
                    var businessResource = InsuranceContext.BusinessSources.Single(model.Id);

                    if (businessResource != null)
                    {
                        businessResource.Source = businessResource.Source;
                        InsuranceContext.BusinessSources.Update(businessResource);
                    }
                }

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: BusinessSource/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: BusinessSource/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
