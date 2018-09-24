using AutoMapper;
using Insurance.Domain;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class VehicleMakeController : Controller
    {
        // GET: VehicleMake
        public ActionResult Index()
        {


            InsuranceClaim.Models.VehiclesMakeModel obj = new InsuranceClaim.Models.VehiclesMakeModel();
            List<Insurance.Domain.VehicleMake> objList = new List<Insurance.Domain.VehicleMake>();
            objList = InsuranceContext.VehicleMakes.All().ToList();

            return View(obj);
        }

        // GET: VehicleMake/Details/5
      [HttpPost]
        public ActionResult SaveVehicleMake(VehiclesMakeModel Model )
        {
            var dbModel = Mapper.Map<VehiclesMakeModel, VehicleMake>(Model);
            dbModel.CreatedOn = DateTime.Now;
            dbModel.ModifiedOn = DateTime.Now;
            dbModel.MakeDescription = Model.MakeDescription.ToUpper();
            InsuranceContext.VehicleMakes.Insert(dbModel);
            return RedirectToAction("VehicleMakeList");
        }

        // GET: VehicleMake/Create
        public ActionResult VehicleMakeList()
        {

            var makelist = InsuranceContext.VehicleMakes.All(where: "IsActive = 'True' or IsActive is Null").OrderByDescending(x=>x.Id).ToList();
            return View(makelist);
        }

        public ActionResult VehicleMakeEdit(int Id)
        {
            var record = InsuranceContext.VehicleMakes.All(where: $"Id ={Id}").FirstOrDefault();

            var model = Mapper.Map<VehicleMake, VehiclesMakeModel>(record);
            return View(model);
        }
        [HttpPost]
        public ActionResult VehicleMakeEdit(VehiclesMakeModel model)
        {
            if (ModelState.IsValid)
            {
                var makeid = model.Id;
                //var data = Mapper.Map<VehiclesMakeModel, VehicleMake>(model);
                var data = InsuranceContext.VehicleMakes.Single(where: $"Id = {makeid}");
                data.MakeDescription = model.MakeDescription.ToUpper();
                data.MakeCode = model.MakeCode;
                data.ShortDescription = model.ShortDescription;
                //data.CreatedOn = model.CreatedOn;
                data.ModifiedOn = DateTime.Now;
                InsuranceContext.VehicleMakes.Update(data);

            }
            return RedirectToAction("VehicleMakeList");
        }
       
        public ActionResult DeleteMake(int id)
        {
            string query = $"update VehicleMake set IsActive =0 where Id={id}";
            InsuranceContext.VehicleMakes.Execute(query);

            return RedirectToAction("VehicleMakeList");

           
        }

      
        
    }
}
