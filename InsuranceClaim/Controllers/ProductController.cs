using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Insurance.Domain;
using InsuranceClaim.Models;
using AutoMapper;
namespace InsuranceClaim.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product 


        public ActionResult Index()
        {
            //InsuranceContext.Products.Insert();
            InsuranceClaim.Models.ProductModel obj = new InsuranceClaim.Models.ProductModel();
            List<Insurance.Domain.Product> objList = new List<Insurance.Domain.Product>();
            objList = InsuranceContext.Products.All().ToList();

            return View(obj);
        }
        [HttpPost]
        public ActionResult ProductSave(ProductModel model)
        {
            var dbModel = Mapper.Map<ProductModel, Product>(model);
            InsuranceContext.Products.Insert(dbModel);

            //  InsuranceContext.Products.Insert(db);
            // InsuranceContext.Products.a

            return RedirectToAction("ProductList");
        }
        public ActionResult ProductList()
        {
            var db = InsuranceContext.Products.All().ToList();


            return View(db);
        }
        public ActionResult ProductEdit(int Id)
        {
            var record = InsuranceContext.Products.All(where: $"Id ={Id}").FirstOrDefault();
            ProductModel obj = new ProductModel();
            obj.Id = record.Id;
            obj.ProductName = record.ProductName;
            obj.ProductCode = record.ProductCode;


            return View(obj);
        }
        [HttpPost]
        public ActionResult ProductEdit(ProductModel model)
        {
            if (ModelState.IsValid)
            {
                var db = InsuranceContext.Products.Single(where: $"Id = {model.Id}");
                db.ProductName = model.ProductName;
                db.ProductCode = model.ProductCode;
                InsuranceContext.Products.Update(db);

            }

            return RedirectToAction("ProductList");
        }
    }
}
